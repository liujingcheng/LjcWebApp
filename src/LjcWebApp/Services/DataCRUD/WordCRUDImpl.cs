using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LjcWebApp;
using LjcWebApp.Helper;

namespace LjcWebApp.Services.DataCRUD
{
    public class WordCRUDImpl : BaseService
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

        /// <summary>
        /// 设置优先级
        /// </summary>
        /// <param name="id">主键</param>
        /// <param name="priority">优先级</param>
        /// <returns></returns>
        public bool SetPriority(long id, int priority)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.word_tb.First(p => p.WordId == id);
                    entity.Priority = priority;
                    context.word_tb.Update(entity);
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
