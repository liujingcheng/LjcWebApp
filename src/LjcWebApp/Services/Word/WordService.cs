using System;
using System.Collections.Generic;
using System.Linq;
using LjcWebApp;
using LjcWebApp.Helper;

namespace LjcWebApp.Services.Word
{
    public class WordService
    {
        /// <summary>
        /// 模糊查找（拼写或解释）
        /// </summary>
        /// <returns></returns>
        public List<word_tb> SearchWords(string likeStr)
        {
            var words = new List<word_tb>();
            try
            {
                using (var context = new LjcDbContext())
                {
                    words = context.word_tb.Where(p => p.Spelling.Contains(likeStr)
                        || p.Paraphrase.Contains(likeStr)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }

            return words;
        }

        /// <summary>
        /// 更新单词
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public bool UpdateWord(word_tb word)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.word_tb.First(p=>p.WordId==word.WordId);
                    entity.Spelling = word.Spelling;
                    entity.Paraphrase = word.Paraphrase;
                    entity.Phonetic = word.Phonetic;
                    entity.Classs = word.Classs;

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 查找单词
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public word_tb GetWord(int id)
        {
            word_tb word = null;
            try
            {
                using (var context = new LjcDbContext())
                {
                    word = context.word_tb.First(p => p.WordId == id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return word;
        }

        /// <summary>
        /// 删除单词
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteWord(int id)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.word_tb.First(p => p.WordId == id);
                    context.word_tb.Remove(entity);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return false;
            }
            return true;
        }

    }
}
