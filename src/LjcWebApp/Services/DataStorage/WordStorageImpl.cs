using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LjcWebApp;
using LjcWebApp.Helper;

namespace LjcWebApp.Services.DataStorage
{
    public class WordStorageImpl
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listWordTb"></param>
        /// <returns>0：所有单词入库成功；1：有部分单词入库失败；</returns>
        public int UpdateWordsList(List<word_tb> listWordTb)
        {
            try
            {
                var context = DbHelper.GetDbContext();
                {
                    foreach (var word in listWordTb)
                    {
                        var entity = context.word_tb.First(p=>p.WordId==word.WordId);
                        entity.Spelling = word.Spelling;
                        entity.Paraphrase = word.Paraphrase;
                        entity.Phonetic = word.Phonetic;
                        entity.Classs = word.Classs;
                        entity.Import = word.Import;
                        entity.Priority = word.Priority;
                        entity.Process = word.Process;
                        entity.FirstLearn = word.FirstLearn;
                        entity.LastLearn = word.LastLearn;
                        //entity.IsRemembered = word.IsRemembered;//该变量不存入数据库，只在记忆的过程中有用（用于标识当时该单词是否已经记住）
                        entity.LastLearnBackup = word.LastLearnBackup;
                        entity.LastProcess = word.LastProcess;
                        entity.LastForget = word.LastForget;
                        entity.YesTotalCount = word.YesTotalCount;
                        entity.NoTotalCount = word.NoTotalCount;
                        entity.CreatedBy = word.CreatedBy;
                        entity.CreatedOn = word.CreatedOn;
                        entity.ModifiedBy = word.ModifiedBy;
                        entity.ModifiedOn = word.ModifiedOn;
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("单词入库出错", ex);
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 同步单词（如果数据库中已有就不存入）
        /// </summary>
        /// <param name="listWordTb"></param>
        /// <returns>0：所有单词同步入库成功；>0：单词入库失败个数；</returns>
        public string AddWordsList(List<word_tb> listWordTb)
        {
            try
            {
                //bool flag = false;
                var errorCount = 0;
                var existCount = 0;
                var successCount = 0;
                foreach (var wordTb in listWordTb)
                {
                    var result = AddWord(wordTb);
                    if (result == 2) existCount++;
                    else if (result == 1) errorCount++;
                    else if (result == 0) successCount++;
                }
                return string.Format("共{0}个单词,成功{1}个,失败{2}个,已存在{3}个单词（不同解释的做了合并）!",
                    listWordTb.Count, successCount, errorCount, existCount);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("单词同步入库出错", ex);
                throw;
            }
        }

        /// <summary>
        /// 如果数据库中已经有一个单词与参数wordTb的拼写相同，那么这个wordTb不入库
        /// </summary>
        /// <param name="wordTb"></param>
        /// <returns>0:成功；1:出错；2:单词已经存在</returns>
        public int AddWord(word_tb wordTb)
        {
            if (wordTb == null) return 1;

            try
            {
                var context = DbHelper.GetDbContext();
                {
                    var entity =
                        context.word_tb.FirstOrDefault(
                            p => p.Spelling == wordTb.Spelling);
                    if (entity != null)
                    {
                        if (wordTb.Priority > entity.Priority)
                        {
                            entity.Classs = wordTb.Classs;
                            entity.Priority = wordTb.Priority;
                            entity.Import = wordTb.Import;
                        }
                        if (entity.Paraphrase != wordTb.Paraphrase && !entity.Paraphrase.Contains(wordTb.Paraphrase))
                        {
                            entity.Paraphrase = wordTb.Paraphrase + Environment.NewLine + "(合)" + entity.Paraphrase;
                            LogHelper.WriteLog(wordTb.Spelling + "单词已经存在！");
                        }
                        context.word_tb.Update(entity);
                        context.SaveChanges();
                        return 2;
                    }
                    context.word_tb.Add(wordTb);
                    context.SaveChanges();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("单词同步入库出错", ex);
                return 1;
            }
        }

    }
}
