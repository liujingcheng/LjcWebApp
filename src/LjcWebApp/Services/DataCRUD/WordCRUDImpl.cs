using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LjcWebApp;
using MvcWords.Domain;

namespace LjcWebApp.Services.DataCRUD
{
    public class WordCRUDImpl 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="word"></param>
        /// <returns>0:删除成功；2：发生异常</returns>
        public int TrDeleteWordService(word_tb word)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.word_tb.First(p => p.WordId==word.WordId);
                    context.word_tb.Remove(entity);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("删除单词错误：", ex);
                return 2;
            }

            return 0;
        }
    }
}
