using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LjcWebApp;
using LjcWebApp.Helper;
using LjcWebApp.Services.ConfigStatic;

namespace LjcWebApp.Services.DataLoad
{
    public class WordLoadImpl : BaseService
    {
        /// <summary>
        /// 加载所有单词
        /// </summary>
        /// <returns></returns>
        public List<word_tb> LoadAllWords()
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    return context.word_tb.ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("加载所有单词出错：", ex);
                throw;
            }
        }

        /// <summary>
        /// 根据参数time返回需要复习的所有单词
        /// </summary>
        /// <param name="time"></param>
        /// <param name="wordsTobeSelect">待选的词</param>
        /// <returns></returns>
        public List<word_tb> TrLoadAllLearnWords(DateTime time, List<word_tb> wordsTobeSelect)
        {
            var wordsToLearn = new List<word_tb>();
            foreach (var wordTb in wordsTobeSelect)
            {
                if (wordTb.LastLearn == null)
                {
                    wordsToLearn.Add(wordTb);//从未记过的单词直接加入
                    continue;
                }

                var gap = GetWordGap(wordTb);
                if (gap.Equals(-1)) continue;

                var deadline = ((DateTime)wordTb.LastLearn).AddDays(gap * GetWordGapWeight(wordTb));//到期时间
                if (time >= deadline)
                {
                    wordTb.Deadline = deadline;
                    wordsToLearn.Add(wordTb);
                }
            }
            if (wordsToLearn.Count == 0)//当已经没有记过的单词需要复习时，才加载从未记过的单词
            {
                foreach (var wordTb in wordsTobeSelect)
                {
                    if (wordTb.LastLearn == null) //从未记过的单词加入需要复习的单词行列
                    {
                        wordTb.Deadline = wordTb.Import;
                        wordsToLearn.Add(wordTb);
                    }
                }
            }
            return wordsToLearn;
        }

        /// <summary>
        /// 根据参数time返回需要复习的所有单词
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public List<word_tb> TrLoadAllLearnWords(DateTime time)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var listAll = context.word_tb.ToList();
                    return TrLoadAllLearnWords(time, listAll);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("加载需要复习的单词出错：", ex);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>-1：未找到对应曲线的gap</returns>
        public static double GetWordGap(word_tb word)
        {
            var listConfigMemoryCurveTb = ConfigMemoryCurveServiceImpl.GetConfigMemoryCurveAll();
            foreach (var configMemoryCurveTb in listConfigMemoryCurveTb)
            {
                if (configMemoryCurveTb.Process == word.Process)
                {
                    return configMemoryCurveTb.Gap;
                }
            }
            return -1;
        }

        public static double GetWordGapWeight(word_tb word)
        {
            if (word.YesTotalCount == 0)
            {
                return 1;
            }
            return 1 + 1.0 * (word.YesTotalCount - word.NoTotalCount) / (word.YesTotalCount + word.NoTotalCount);
        }

        /// <summary>
        /// 模糊查找
        /// </summary>
        /// <returns></returns>
        public List<word_tb> SearchWords(string likeStr)
        {
            var words = new List<word_tb>();
            try
            {
                using (var context = new LjcDbContext())
                {
                    words = context.word_tb.Where(p => p.Spelling.Contains(likeStr)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }

            return words;
        }

    }
}
