using System;
using System.Collections.Generic;
using System.Linq;
using LjcWebApp;
using MvcWords.Domain;
using  LjcWebApp.Services.ConfigStatic;

namespace LjcWebApp.Services.Introspection
{
    public class QuestionService
    {
        /// <summary>
        /// 获取所有列表
        /// </summary>
        /// <returns></returns>
        public List<question> GetList()
        {
            var words = new List<question>();
            try
            {
                using (var context = new LjcDbContext())
                {
                    words = context.question.OrderBy(p=>p.Sort).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }

            return words;
        }

        /// <summary>
        /// 模糊查找
        /// </summary>
        /// <returns></returns>
        public List<question> Search(string likeStr)
        {
            var words = new List<question>();
            try
            {
                using (var context = new LjcDbContext())
                {
                    words = context.question.Where(p => p.QuestionMember.Contains(likeStr)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }

            return words;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public bool Add(question question)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    question.Id = Guid.NewGuid().ToString().Replace("-", "");
                    question.ModifiedOn = question.CreatedOn = DateTime.Now;

                    context.question.Add(question);
                    context.SaveChanges();
                    UpdateCache();
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
        /// 更新
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public bool Update(question question)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.question.First(p=>p.Id==question.Id);
                    entity.QuestionMember = question.QuestionMember;
                    entity.FullScore = question.FullScore;
                    entity.Weight = question.Weight;
                    entity.IsYes = question.IsYes;
                    entity.IsPositive = question.IsPositive;
                    entity.ModifiedOn = DateTime.Now;


                    context.SaveChanges();
                    UpdateCache();
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
        /// 查找单个实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public question Get(string id)
        {
            question entity = null;
            try
            {
                using (var context = new LjcDbContext())
                {
                    entity = context.question.First(p=>p.Id==id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return entity;
        }

        /// <summary>
        /// 删除问题
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(string id)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.question.First(p=>p.Id==id);
                    context.question.Remove(entity);
                    context.SaveChanges();
                    UpdateCache();
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
        /// 更新缓存
        /// </summary>
        public void UpdateCache()
        {
            IntrospectStaticData.QuestionCount = null;
        }

        public bool DeleteAll()
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var all = context.question.ToList();
                    context.question.RemoveRange(all);
                    context.SaveChanges();
                    UpdateCache();
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
