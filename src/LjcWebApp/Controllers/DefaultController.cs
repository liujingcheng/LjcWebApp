using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LjcWebApp.Helper;
using LjcWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LjcWebApp.Services.DataCRUD;
using LjcWebApp.Services.DataLoad;
using LjcWebApp.Services.DataStorage;
using LjcWebApp.Services.XMLParse;
using Microsoft.AspNetCore.Hosting;
using Sakura.AspNetCore;

namespace LjcWebApp.Controllers
{
    [Authorize]
    public class DefaultController : Controller
    {
        public const int MAX_PROCESS = 20;
        WordLoadImpl _wordLoadImpl = new WordLoadImpl();
        WordStorageImpl _wordStorageImpl = new WordStorageImpl();
        WordCRUDImpl _wordCRUDImpl = new WordCRUDImpl();

        public ActionResult Index()
        {
            LoadAllReviewWords();
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileUrl">临时文件路径</param>
        /// <param name="fileName">原文件名</param>
        /// <param name="priority">优先级(2:high,1:middle,0:low)</param>
        /// <returns></returns>
        [HttpPost]
        public string ImportYoudaoWords(string fileUrl, string fileName, int priority)
        {
            fileName = fileName.Replace(" ", "");
            string importResult = "导入成功！";
            List<word_tb> listWordTb;
            try
            {
                var fileInfo = new FileInfo(fileUrl);
                if (fileName.ToLower().EndsWith(".txt"))
                {
                    listWordTb = new Txt91Parse().Split91LexiconToWords(fileUrl, fileName.Replace(".txt", ""), priority);
                }
                else
                {
                    listWordTb = new WordsXmlParse(fileUrl).DoParse(priority);
                }

                importResult = _wordStorageImpl.AddWordsList(listWordTb);
                EnableBtn();

                LoadAllReviewWords();//每次导入生词后要重新加载所有要复习的单词

                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
                return importResult;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return "发生异常！请查看日志！";
            }
        }

        /// <summary>
        /// 获取截止当前时间为止要记的单词（包含了过期进间的计算）
        /// </summary>
        /// <returns></returns>
        private List<word_tb> GetWordsToLearn()
        {
            var allWordsToReview = _wordLoadImpl.TrLoadAllLearnWords(DateTime.Now);

            //计算每个单词的过期时间
            allWordsToReview.ForEach(p =>
            {
                var t1 = DateTime.Now.Ticks;
                var t2 = p.Deadline.GetValueOrDefault().Ticks;

                p.ExpireSpanTicks = t1 - t2;
            });

            return allWordsToReview;
        }

        private void LoadAllReviewWords()
        {
            try
            {
                common.LinkedList = null;
                common.CurrentNode = null;
                common.WordsRemembered = null;
                common.WordsNotRemember = null;
                common.LinkedList = new LinkedList<word_tb>();
                common.WordsRemembered = new List<word_tb>();

                common.WordsNotRemember = GetWordsToLearn();

                //var randomIndex = GetRandomIndex(common.WordsNotRemember.Count);
                //common.CurrentNode = new LinkedListNode<word_tb>(common.WordsNotRemember[randomIndex]);
                //common.WordsNotRemember.RemoveAt(randomIndex);
                common.CurrentNode = new LinkedListNode<word_tb>(GetNextNotRememberedWord());
                common.WordsNotRemember.Remove(common.CurrentNode.Value);

                common.LinkedList.AddFirst(common.CurrentNode);

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("LoadAllReviewWords方法异常：", ex);
            }
        }

        private int GetRandomIndex(int count)
        {
            var random = new Random();
            return random.Next(0, count);
        }

        [HttpPost]
        public string GetCurrentWordInfoStr(string pCheckedRadioValue)
        {
            var strList = new List<string>();
            try
            {
                common.rbHideRandomChecked = pCheckedRadioValue == "0";
                common.rbHideSpellingChecked = pCheckedRadioValue == "1";
                common.rbHideParaphraseChecked = pCheckedRadioValue == "2";
                ShowCurrentWord();
                strList.Add(common.lblWordInfoText);
                strList.Add(common.lblRemainText);
                strList.Add(common.lblSpellingText);
                strList.Add(common.lblPhoneticText);
                strList.Add(common.lblParaphraseText);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return strList.Aggregate("%", (current, item) => current + (item + "%"));
        }

        private void ShowCurrentWord()
        {
            if (common.WordsNotRemember != null && common.WordsNotRemember.Count > 0)
            {
                var expire = new TimeSpan(common.CurrentNode.Value.ExpireSpanTicks.GetValueOrDefault());
                //与当前单词同一进度的等记忆单词还剩多少
                string processRemain =
                    common.WordsNotRemember.Count(p => p.Priority == common.CurrentNode.Value.Priority && p.Process == common.CurrentNode.Value.Process
                    && (p.ModifiedOn <= p.LastLearn || (p.FirstLearn != null && p.LastLearn == null))).ToString();
                //显示过期时间   
                var expireDays = expire.Days;
                var sameExpireDays = common.WordsNotRemember.Count(p => p.Priority == common.CurrentNode.Value.Priority && new TimeSpan(p.ExpireSpanTicks.GetValueOrDefault()).Days == expireDays);//与它同一过期时间的单词数
                int priority = common.CurrentNode.Value.Priority;
                var classs = common.CurrentNode.Value.Classs;
                var totalYesCount = common.CurrentNode.Value.YesTotalCount;
                var totalNoCount = common.CurrentNode.Value.NoTotalCount;
                var priorityRemainCountStr = common.WordsNotRemember.Count(p => p.Priority == 2) + "+" + common.WordsNotRemember.Count(p => p.Priority == 1);
                var wordGap = WordLoadImpl.GetWordGap(common.CurrentNode.Value);
                var wordGapWeight = WordLoadImpl.GetWordGapWeight(common.CurrentNode.Value);
                common.lblRemainText = "P" + common.CurrentNode.Value.Process + " PR" + processRemain + " OR:" + priorityRemainCountStr
                    + " O:" + priority + " E:" + expireDays + "(" + sameExpireDays + ") TY:" + totalYesCount + " TN:" + totalNoCount
                    + " SG:" + Math.Round(wordGap, 2) + " RG:" + Math.Round(wordGap * wordGapWeight, 2)
                    + " C:" + classs;
            }

            //显示单词的进度、上次记忆时间和过期时间
            DisplayWordInfo();
            EnableBtn();

            if (common.CurrentNode == null)
            {
                common.lblSpellingText = "";
                common.lblPhoneticText = "";
                common.lblParaphraseText = "";
                if (common.WordsNotRemember != null && common.WordsNotRemember.Count == 0)
                {
                    common.lblRemainText = "Congratulations! No more words to learn!";
                }
                return;
            }

            if (common.CurrentNode.Value.FirstLearn != null && !common.IsFirstNo)
            {
                if (common.rbHideRandomChecked)
                {
                    //if (common.CurrentNode.Value.Process <= 4)
                    //{
                    //    common.lblSpellingText = "";
                    //    common.lblPhoneticText = "";
                    //    common.lblParaphraseText = common.CurrentNode.Value.Paraphrase;
                    //}
                    //else
                    //{
                    var flag = common.CurrentNode.Value.LastForget != null ?
                         common.CurrentNode.Value.LastForget.Value ://上次未记住(0:Spelling;1:Paraphrase)
                         GetRandomIndex(2);
                    switch (flag)
                    {
                        case 0:
                            common.lblSpellingText = "";
                            common.lblPhoneticText = "";
                            common.lblParaphraseText = common.CurrentNode.Value.Paraphrase;
                            break;
                        case 1:
                            common.lblSpellingText = common.CurrentNode.Value.Spelling;
                            common.lblPhoneticText = "";
                            common.lblParaphraseText = "";
                            break;
                    }
                    //}
                }
                if (common.rbHideSpellingChecked)
                {
                    common.lblSpellingText = "";
                    common.lblPhoneticText = "";
                    common.lblParaphraseText = common.CurrentNode.Value.Paraphrase;
                }
                if (common.rbHideParaphraseChecked)
                {
                    common.lblSpellingText = common.CurrentNode.Value.Spelling;
                    common.lblPhoneticText = "";
                    common.lblParaphraseText = "";
                }
            }
            else
            {
                common.lblSpellingText = common.CurrentNode.Value.Spelling;
                common.lblPhoneticText = common.CurrentNode.Value.Phonetic;
                common.lblParaphraseText = common.CurrentNode.Value.Paraphrase;
                if (common.IsFirstNo)
                {
                    common.IsFirstNo = false;
                }
            }

        }

        //显示单词的进度、上次记忆时间和过期时间
        private void DisplayWordInfo()
        {
            if (common.CurrentNode == null) return;

            int remainCount = common.CurrentNode == null ? common.WordsNotRemember.Count : common.WordsNotRemember.Count + 1;
            string LastLearn = common.CurrentNode.Value.LastLearn == null ? "" : common.CurrentNode.Value.LastLearn.Value.ToString("yyyy-MM-dd HH:mm");
            string FirstLearn = common.CurrentNode.Value.FirstLearn == null ? "" : common.CurrentNode.Value.FirstLearn.Value.ToString("yyyy-MM-dd HH:mm");
            string ModifiedOn = common.CurrentNode.Value.ModifiedOn.ToString("yyyy-MM-dd HH:mm");
            //剩余ModifiedOn大于LastLearn的单词个数
            string modifyGTlast = common.WordsNotRemember.Count(p => p.Process != 0 && (p.ModifiedOn > p.LastLearn || (p.FirstLearn != null && p.LastLearn == null))).ToString();
            int notUniqueTimeWords =//上次未记住但又不允许在某段时间内重复出现的单词数
                GetMinutesUniqueWordCount(common.WordsNotRemember);
            common.lblWordInfoText = "R:" + remainCount + " F:" + FirstLearn + " L:" + LastLearn
                + " M:" + ModifiedOn + " M>L" + modifyGTlast + "(" + notUniqueTimeWords + ")";

            //string expireStr;
            //if (common.CurrentNode.Value.LastLearn != null)
            //{
            //    var wordLoadFacade = new WordLoadFacade();
            //    var expire = wordLoadFacade.GetWordExpire(CurrentNode.Value);
            //    //显示时间   
            //    expireStr = expire.Days.ToString() + "d " + expire.Hours.ToString() + "h " + expire.Minutes.ToString() + "m";
            //}
            //else
            //{
            //    expireStr = "0";
            //}
        }

        [HttpPost]
        public string GetEnableBtnStr()
        {
            var btnEnabledList = new List<bool>();
            try
            {
                btnEnabledList.Add(common.BtnNOEnabled);
                btnEnabledList.Add(common.BtnYESEnabled);
                btnEnabledList.Add(common.BtnPreviousEnabled);
                btnEnabledList.Add(common.BtnNextEnabled);
                btnEnabledList.Add(common.BtnDeleteEnabled);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return btnEnabledList.Aggregate("%", (current, item) => current + (item + "%"));
        }
        private void EnableBtn()
        {
            if (common.CurrentNode != null)
            {
                common.BtnNOEnabled = true;
                common.BtnYESEnabled = true;
                common.BtnPreviousEnabled = true;
                common.BtnNextEnabled = true;
                common.BtnDeleteEnabled = true;
            }
            if (common.CurrentNode != null && common.CurrentNode.Next != null) common.BtnNextEnabled = true;
            if (common.CurrentNode != null && common.CurrentNode.Previous != null) common.BtnPreviousEnabled = true;

            if (common.CurrentNode == null)
            {
                common.BtnNOEnabled = false;
                common.BtnYESEnabled = false;
                common.BtnPreviousEnabled = false;
                common.BtnNextEnabled = false;
                common.BtnDeleteEnabled = false;

                common.lblSpellingText = "";
                common.lblPhoneticText = "";
                common.lblParaphraseText = "";
                common.lblWordInfoText = "";
            }
            if (common.CurrentNode != null && common.CurrentNode.Next == null) common.BtnNextEnabled = false;
            if (common.CurrentNode != null && common.CurrentNode.Previous == null) common.BtnPreviousEnabled = false;

        }

        /// <summary>
        /// 返回最难记住的单词
        /// </summary>
        /// <param name="wordsToSelect"></param>
        /// <returns></returns>
        private word_tb GetNextHardToLearnWord(List<word_tb> wordsToSelect)
        {
            if (wordsToSelect == null || wordsToSelect.Count == 0) return null;

            var yesTotalCountGT0List = wordsToSelect.Where(p => p.YesTotalCount > 0).ToList();//先找出分母大于0的单词
            if (yesTotalCountGT0List.Count > 0)//分母不为0的才能进行下面的运算
            {
                var hardWeight = yesTotalCountGT0List.Max(p => ((double)p.NoTotalCount / p.YesTotalCount));
                return
                    GetNextMaxProcessRandomWord(
                        yesTotalCountGT0List.Where(p => ((double)p.NoTotalCount / p.YesTotalCount) == hardWeight).ToList());
            }
            return GetNextMaxProcessRandomWord(wordsToSelect);

        }

        private word_tb GetNextMaxProcessRandomWord(List<word_tb> wordsToSelect)
        {
            if (wordsToSelect == null || wordsToSelect.Count == 0) return null;

            List<long> idList;
            var maxProcess = wordsToSelect.Max(p => p.Process);

            idList = GetIdListByProcess(wordsToSelect, maxProcess);
            var randomIndex = GetRandomIndex(idList.Count);
            return wordsToSelect.First(p => p.WordId == idList[randomIndex]);
        }

        private int GetRandomSizeByProcess(int process)
        {
            int randomSize = 20;
            if (process >= 0 && process <= 2)
            {
                randomSize = 20;
            }
            else if (process >= 3 && process <= 5)
            {
                randomSize = 40;
            }
            else if (process >= 6 && process <= 8)
            {
                randomSize = 60;
            }
            else if (process >= 9 && process <= 11)
            {
                randomSize = 60;
            }
            else if (process >= 9 && process <= 11)
            {
                randomSize = 80;
            }
            else if (process >= 12 && process <= 14)
            {
                randomSize = 100;
            }
            else if (process >= 15 && process <= 17)
            {
                randomSize = 120;
            }
            return randomSize;
        }

        /// <summary>
        /// 根据不同等级的Process返回优先记忆的前n个单词的Id集合
        /// </summary>
        /// <param name="wordsToSelect"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        private List<long> GetIdListByProcess(List<word_tb> wordsToSelect, int process)
        {
            List<long> idList = new List<long>();
            //int randomSize = GetRandomSizeByProcess(process);

            //if (wordsToSelect.Count(p => p.Process == process) >= randomSize)
            //{
            //    idList =
            //        wordsToSelect.Where(p => p.Process == process)
            //            .OrderByDescending(p => p.ModifiedOn)
            //            .Take(randomSize)
            //            .Select(p => p.WordId)
            //            .ToList();
            //}
            //else
            //{
            idList =
                wordsToSelect.Where(p => p.Process == process)
                .Select(p => p.WordId)
                .ToList();

            //}
            return idList;
        }

        private bool WordCanBeLearn(word_tb word)
        {
            return word.ModifiedOn.AddMinutes(GetUniqueMinutesByProcess(word.Process)) < DateTime.Now;
        }

        private int GetUniqueMinutesByProcess(int process)
        {
            if (process == 0) return 20;//第一次记忆的单词，20分钟后就可再次记忆，20分钟为艾宾浩斯记忆曲线的第一个周期
            if (process >= 1 && process <= 3) return 240;
            if (process >= 4 && process <= 6) return 480;
            if (process >= 7) return 1440;
            return 20;
        }

        /// <summary>
        /// 返回某个时间内不能重复记忆的单词数
        /// </summary>
        /// <param name="wordsToSelect"></param>
        /// <returns></returns>
        private int GetMinutesUniqueWordCount(List<word_tb> wordsToSelect)
        {
            return wordsToSelect.Count(p =>
                p.FirstLearn != null
                && (p.ModifiedOn > p.LastLearn || p.LastLearn == null)
                && p.ModifiedOn.AddMinutes(GetUniqueMinutesByProcess(p.Process)) >= DateTime.Now);
        }

        //返回下一个需要记忆的单词
        //记忆过但没记住的单词n分钟内不能重复，高等级的找不到符合此要求的就从低等级找，实在找不到时就放弃该要求，从未记住中随便找一个
        private word_tb GetNextNotRememberedWord()
        {
            word_tb result;

            //上次刚记但没记住的（首次记但没记住的除外）
            var lastModifiedList = common.WordsNotRemember
                .Where(p => p.Process > 0
                    && (p.ModifiedOn > p.LastLearn)
                    && WordCanBeLearn(p)).ToList();//去掉上一行两天内的限制，使上次记忆但没记住的单词优先记忆 modify 2015-08-16

            result = GetNextHardToLearnWord(lastModifiedList);
            if (result != null) return result;

            //从高优先级里面选
            var highPriorityWordList = common.WordsNotRemember.Where(p => p.Priority > 1 && WordCanBeLearn(p)).ToList();
            if (highPriorityWordList.Count > 0)
            {
                var lateMonthList = highPriorityWordList.Where(p => p.Process == 0 && p.Import > DateTime.Now.AddMonths(-1)).ToList();
                if (lateMonthList.Count > 0)//一个月内新添加且从未记忆过的高优先级单词优先记忆
                {
                    result = GetNextHardToLearnWord(lateMonthList);
                    if (result != null) return result;
                }
                result = GetNextHardToLearnWord(highPriorityWordList);
                if (result != null) return result;
            }

            //再从中优先级里面选
            var middlePriorityWordList = common.WordsNotRemember.Where(p => p.Priority == 1 && WordCanBeLearn(p)).ToList();
            if (middlePriorityWordList.Count > 0)
            {
                result = GetNextHardToLearnWord(middlePriorityWordList);
                if (result != null) return result;
            }

            //再从低优先级里面选
            var lowPriorityWordList = common.WordsNotRemember.Where(p => p.Priority < 1 && p.Process > 0 && WordCanBeLearn(p)).ToList();
            if (lowPriorityWordList.Count > 0)
            {
                result = GetNextHardToLearnWord(lowPriorityWordList);
                if (result != null) return result;
            }

            //上次属首次记但没记住的
            var lastModifiedFirstLearnList = common.WordsNotRemember
                .Where(p => p.FirstLearn != null && p.Process == 0
                    && WordCanBeLearn(p)).ToList();
            result = GetNextHardToLearnWord(lastModifiedFirstLearnList);
            if (result != null) return result;

            //直接随机了
            var randomIndex = GetRandomIndex(common.WordsNotRemember.Count);
            return common.WordsNotRemember[randomIndex];

        }

        [HttpPost]
        public void BtnYesClick()
        {
            try
            {
                var nowTime = DateTime.Now;
                if (common.CurrentNode.Value.IsRemembered != true)
                {
                    var wordTb = common.CurrentNode.Value;
                    wordTb.IsRemembered = true;

                    //FirstLearn如果为null的话,只在用户按YES或NO按钮时才对其赋值,也就是用户表态该单词记住或是未记住后该单词才被认为是已经记忆过(而不是第一次记忆),
                    //否则该单词还会被认为是从未记忆过的单词(虽然可能这时用户看到了这个单词)
                    if (wordTb.FirstLearn == null) wordTb.FirstLearn = nowTime;
                    wordTb.ModifiedOn = nowTime;

                    wordTb.Process++;
                    wordTb.YesTotalCount++;//总共记忆并记住了几次
                    var lastTime = wordTb.LastLearn;
                    wordTb.LastLearn = nowTime;

                    wordTb.LastForget = null;//清空状态（上面会根据LastForget来决定该隐藏Spelling还是Paraphrase

                    _wordStorageImpl.UpdateWordsList(new List<word_tb>() { wordTb });
                    wordTb.ModifiedOn = lastTime == null ? DateTime.MinValue : (DateTime)lastTime;//这一句是为了下面点击No按钮时发现该单词之前被点了Yes然后回滚用的
                }

                //EnableBtn();

                //common.CurrentNode.Value.FirstLearn = DateTime.Now;
                if (common.CurrentNode.Value.FirstLearn == null) common.CurrentNode.Value.FirstLearn = nowTime;

                if (common.CurrentNode.Next != null)
                {
                    if (common.WordsNotRemember.Contains(common.CurrentNode.Value))
                    {
                        common.WordsNotRemember.Remove(common.CurrentNode.Value);
                        common.WordsRemembered.Add(common.CurrentNode.Value);
                    }
                    BtnNextClick();
                    return;
                }

                if (common.WordsNotRemember.Count == 0)
                {
                    common.WordsRemembered.Add(common.CurrentNode.Value);
                    common.CurrentNode = null;
                    //EnableBtn();
                    //ShowCurrentWord();
                    //StorageWords();
                    return;
                }

                //var randomIndex = GetRandomIndex(common.WordsNotRemember.Count);
                //var newNode = new LinkedListNode<word_tb>(common.WordsNotRemember[randomIndex]);
                //common.WordsNotRemember.RemoveAt(randomIndex);
                var newNode = new LinkedListNode<word_tb>(GetNextNotRememberedWord());
                common.WordsNotRemember.Remove(newNode.Value);

                common.LinkedList.AddAfter(common.CurrentNode, newNode);
                common.WordsRemembered.Add(common.CurrentNode.Value);
                common.CurrentNode = newNode;

                //ShowCurrentWord();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
        }

        [HttpPost]
        public void BtnNoClick()
        {
            try
            {
                var nowTime = DateTime.Now;
                //EnableBtn();
                if (common.lblSpellingText == "" || common.lblParaphraseText == "")
                {
                    //这次是因为隐藏拼写而记不住的下次还隐藏那个单词的拼写
                    if (common.lblSpellingText == "")
                    {
                        common.CurrentNode.Value.LastForget = 0;
                    }
                    else if (common.lblParaphraseText == "")
                    {
                        common.CurrentNode.Value.LastForget = 1;
                    }
                    _wordStorageImpl.UpdateWordsList(new List<word_tb>() { common.CurrentNode.Value });//这里是为了将LastForget存入数据库

                    common.IsFirstNo = true;
                    common.lblSpellingText = common.CurrentNode.Value.Spelling;
                    common.lblPhoneticText = common.CurrentNode.Value.Phonetic;
                    common.lblParaphraseText = common.CurrentNode.Value.Paraphrase;

                    return;
                }


                if (common.CurrentNode.Value.IsRemembered == true)//此种场景一般是之前按错键把没记住按成了记住，然后现在重新改回来
                {
                    common.CurrentNode.Value.IsRemembered = false;
                    common.CurrentNode.Value.Process--;
                    common.CurrentNode.Value.YesTotalCount--;
                    if (common.CurrentNode.Value.ModifiedOn == DateTime.MinValue)//说明该单词之前是第一次被记忆，原先的lastlearn应为空
                    {
                        common.CurrentNode.Value.LastLearn = null;
                    }
                    else
                    {
                        common.CurrentNode.Value.LastLearn = common.CurrentNode.Value.ModifiedOn;
                    }
                }

                //本来记得挺熟的，若时间长了后忘了，那么该单词就该加强记忆，回退几级（只是一次记忆的首次记不住回退，再次没记住时不能回退）
                if (common.CurrentNode != null && common.CurrentNode.Value.Process > 10 && common.CurrentNode.Value.ModifiedOn == common.CurrentNode.Value.LastLearn)
                {
                    switch (common.CurrentNode.Value.Process)
                    {
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                            common.CurrentNode.Value.Process = 10;
                            break;
                        default:
                            common.CurrentNode.Value.Process = 14;
                            break;
                    }
                }

                //FirstLearn如果为null的话,只在用户按YES或NO按钮时才对其赋值,也就是用户表态该单词记住或是未记住后该单词才被认为是已经记忆过(而不是第一次记忆),
                //否则该单词还会被认为是从未记忆过的单词(虽然可能这时用户看到了这个单词)
                if (common.CurrentNode.Value.FirstLearn == null) common.CurrentNode.Value.FirstLearn = nowTime;

                //记录总共没记住多少次
                common.CurrentNode.Value.NoTotalCount++;

                common.CurrentNode.Value.ModifiedOn = nowTime;

                _wordStorageImpl.UpdateWordsList(new List<word_tb>() { common.CurrentNode.Value });

                //common.CurrentNode.Value.FirstLearn = DateTime.Now;
                if (common.CurrentNode.Value.FirstLearn == null) common.CurrentNode.Value.FirstLearn = nowTime;//为了使记过一次的单词不再把单词、发音和词义都显示出来
                if (common.CurrentNode.Next != null)
                {
                    if (common.WordsRemembered.Contains(common.CurrentNode.Value))
                    {
                        common.WordsRemembered.Remove(common.CurrentNode.Value);
                        common.WordsNotRemember.Add(common.CurrentNode.Value);
                    }
                    BtnNextClick();
                    return;
                }

                common.WordsNotRemember.Add(common.CurrentNode.Value);

                //var randomIndex = GetRandomIndex(common.WordsNotRemember.Count);
                //var newNode = new LinkedListNode<word_tb>(common.WordsNotRemember[randomIndex]);
                //common.WordsNotRemember.RemoveAt(randomIndex);
                var newNode = new LinkedListNode<word_tb>(GetNextNotRememberedWord());
                common.WordsNotRemember.Remove(newNode.Value);

                common.LinkedList.AddAfter(common.CurrentNode, newNode);
                common.CurrentNode = newNode;

                //ShowCurrentWord();
                //EnableBtn();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
        }

        [HttpPost]
        public void BtnNextClick()
        {
            try
            {
                //EnableBtn();

                if (common.CurrentNode.Next == null) return;

                common.CurrentNode = common.CurrentNode.Next;
                //ShowCurrentWord();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
        }

        [HttpPost]
        public void BtnPreviousClick()
        {
            try
            {
                //EnableBtn();

                if (common.CurrentNode.Previous == null) return;

                common.CurrentNode = common.CurrentNode.Previous;
                //ShowCurrentWord();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
        }

        [HttpPost]
        public void BtnDeleteClick()
        {
            try
            {
                int state = _wordCRUDImpl.TrDeleteWordService(common.CurrentNode.Value);
                switch (state)
                {
                    case 0:
                        LogHelper.WriteLog("Delete Success!");
                        break;
                    case 2:
                        LogHelper.WriteLog("Exceptions happened,See the log!");
                        break;
                }

                LoadAllReviewWords();

                //if (common.WordsNotRemember.Contains(common.CurrentNode.Value))
                //{
                //    common.WordsNotRemember.Remove(common.CurrentNode.Value);
                //}
                //if (common.WordsRemembered.Contains(common.CurrentNode.Value))
                //{
                //    common.WordsRemembered.Remove(common.CurrentNode.Value);
                //}

                //var nextDistictValueNode = GetNextDistinctValueNode(common.CurrentNode);
                //if (nextDistictValueNode != null)
                //{
                //    var temp = common.CurrentNode;
                //    common.CurrentNode = nextDistictValueNode;
                //    DeleteLinkedListNode(temp.Value);
                //    ShowCurrentWord();
                //    return;
                //}

                //if (common.WordsNotRemember.Count == 0)
                //{
                //    common.CurrentNode = null;
                //    EnableBtn();
                //    //StorageWords();
                //    return;
                //}

                //DeleteLinkedListNode(common.CurrentNode.Value);
                //var randomIndex = GetRandomIndex(common.WordsNotRemember.Count);
                //var newNode = new LinkedListNode<word_tb>(common.WordsNotRemember[randomIndex]);
                //common.WordsNotRemember.RemoveAt(randomIndex);
                //common.LinkedList.AddLast(newNode);
                //common.CurrentNode = newNode;

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
        }

        private void DeleteLinkedListNode(word_tb wordTb)
        {
            while (common.LinkedList.Contains(wordTb))
            {
                common.LinkedList.Remove(wordTb);
            }
        }

        private LinkedListNode<word_tb> GetNextDistinctValueNode(LinkedListNode<word_tb> node)
        {
            var tempValue = node.Value;
            while (node != null && node.Value.Equals(tempValue))
            {
                node = node.Next;
            }
            return node;
        }

        [HttpPost]
        public int AddWord(string spelling, string phonetic, string paraphrase, string classs, int priority)
        {
            if (string.IsNullOrEmpty(spelling))
            {
                return 3;
            }

            var wordTb = new word_tb();
            wordTb.Spelling = spelling.Trim();
            if (!string.IsNullOrEmpty(phonetic))
            {
                wordTb.Phonetic = phonetic.Trim();
            }
            if (paraphrase == null)
            {
                paraphrase = "I'm null";
            }
            wordTb.Paraphrase = paraphrase.Trim();

            if (wordTb.Spelling.Length < 30)//Spelling长度小于30时替换为~
            {
                wordTb.Paraphrase = wordTb.Paraphrase.Replace(wordTb.Spelling, "~");//解释里面有单词时，以波浪线代替
            }

            wordTb.Classs = string.IsNullOrEmpty(classs) ? "未分类" : classs;//如果分类为空的话就赋值为“未分类”

            wordTb.Import = DateTime.Now;
            wordTb.CreatedOn = wordTb.Import;
            wordTb.ModifiedOn = DateTime.MinValue;//导入单词时ModifiedOn设为最小值，防止跟记忆时ModifiedOn的作用发生混淆

            wordTb.Priority = priority;
            //默认进度为0
            wordTb.Process = 0;

            return _wordStorageImpl.AddWord(wordTb);
        }

        public ActionResult WordsIndex(int? page)
        {
            try
            {
                var pageNumber = page ?? 1;
                ViewBag.OnePageOfHistoryList = new List<word_tb>();
                foreach (var wordTb in common.WordsNotRemember)
                {
                    if (wordTb.LastLearn == null)//从未记过的单词直接加入需要复习的单词行列
                    {
                        wordTb.Deadline = wordTb.Import;
                        continue;
                    }

                    var gap = WordLoadImpl.GetWordGap(wordTb);
                    if (gap.Equals(-1)) continue;
                    wordTb.Deadline = ((DateTime)wordTb.LastLearn).AddDays(gap);
                }

                var list = common.WordsNotRemember.OrderByDescending(p => p.Process).ToList();

                //注意，这里的查询方式使用的是假分页，若要使用真分页得看具体使用的ORM而定
                var pagedList = list.ToPagedList(pageNumber, 100);

                ViewBag.OnePageOfHistoryList = pagedList;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return View();
        }

        public IActionResult Upload(UploadModel model)
        {
            if (model.Tip == null)
            {
                model = new UploadModel()
                {
                    Priority = 2
                };
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Upload([FromServices]IHostingEnvironment env, UploadModel uploadModel)
        {
            var fileName = DateTime.Now.ToString("MMddHHmmss") + ".xml";
            var filePath = Path.Combine(env.WebRootPath + "/upload", fileName);
            try
            {
                using (var stream = new FileStream(filePath, FileMode.CreateNew))
                {
                    uploadModel.UploadedFile.CopyTo(stream);
                }
                uploadModel.Tip = ImportYoudaoWords(filePath, fileName, uploadModel.Priority);
            }
            catch (Exception ex)
            {
                uploadModel.Tip = "发生异常：" + ex.Message;
            }

            return RedirectToAction(nameof(Upload), uploadModel);
        }

    }
}
