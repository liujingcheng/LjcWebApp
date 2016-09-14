using System;
using System.Collections.Generic;
using System.Linq;
using LjcWebApp;
using LjcWebApp.Helper;
using LjcWebApp.Services.ConfigStatic;

namespace LjcWebApp.Services.Introspection
{
    public class IntrospectService : BaseService
    {
        /// <summary>
        /// 返回今天下一个需要反省的问题
        /// </summary>
        /// <returns></returns>
        public question GetNextQuestion(question currentQuestion)
        {
            using (var context = new LjcDbContext())
            {
                int? nextSort = 1;
                if (currentQuestion != null && currentQuestion.Sort != null)
                {
                    nextSort = currentQuestion.Sort + 1;
                }
                else
                //当当前问题currentQuestion为空时(比如首次加载页面或刷新页面时)，就紧接上次回答的问题继续往下
                {
                    var lastCreatedIntrospect =
                        context.introspect.Where(p => p.Date == DateTime.Now.Date)
                            .OrderByDescending(p => p.CreatedOn)
                            .FirstOrDefault();
                    if (lastCreatedIntrospect != null)
                    {
                        var lastHandledQuestion =
                            context.question.FirstOrDefault(p => p.QuestionMember == lastCreatedIntrospect.question);
                        if (lastHandledQuestion != null)
                        {
                            nextSort = lastHandledQuestion.Sort + 1;
                            if (nextSort > IntrospectStaticData.QuestionCount)
                            //如果超出范围（比如上次处理的已是最后一个问题，但最后一个问题之前还有一些问题没处理时，就适用这种情况）
                            {
                                return GetPreviousUnhandledQuestion(lastHandledQuestion);
                            }
                        }
                    }
                }

                var nextQuestion = context.question.FirstOrDefault(p => p.Sort == nextSort);

                if (nextQuestion != null)
                //寻找今天上次回答这个问题时的得分
                {
                    var introspect =
                        context.introspect.FirstOrDefault(
                            p => p.Date == DateTime.Now.Date && p.question == nextQuestion.QuestionMember);
                    if (introspect != null)
                    {
                        nextQuestion.Score = introspect.Score;
                    }
                }
                return nextQuestion;
            }
        }

        /// <summary>
        /// 返回昨天的当前问题的Introspect对象
        /// </summary>
        /// <returns></returns>
        public introspect GetYesterdayCurrentIntrospect(question currentQuestion)
        {
            if (currentQuestion == null) return null;
            using (var context = new LjcDbContext())
            {
                var yesterdayIntrospect =
                    context.introspect.FirstOrDefault(p => p.QuestionId == currentQuestion.Id && p.Date == DateTime.Now.Date.AddDays(-1));
                return yesterdayIntrospect;
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Add(introspect entity)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    entity.Id = Guid.NewGuid().ToString().Replace("-", "");
                    entity.ModifiedOn = entity.CreatedOn = DateTime.Now;

                    context.introspect.Add(entity);
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
        /// 更新
        /// </summary>
        /// <param name="pEntity"></param>
        /// <returns></returns>
        public bool Update(introspect pEntity)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.introspect.First(p => p.Id == pEntity.Id);
                    entity.QuestionId = pEntity.QuestionId;
                    entity.question = pEntity.question;
                    entity.Score = pEntity.Score;
                    entity.Weight = pEntity.Weight;
                    entity.IsYes = pEntity.IsYes;
                    entity.IsPositive = pEntity.IsPositive;
                    entity.Date = pEntity.Date;
                    entity.ModifiedOn = DateTime.Now;

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
        /// 指定的introspect是否存在（只比较question和Date）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool IsExist(introspect entity)
        {
            using (var context = new LjcDbContext())
            {
                return context.introspect.Any(p => p.question == entity.question && p.Date == entity.Date);
            }
        }

        /// <summary>
        /// 查找单个实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public introspect GetByKey(string id)
        {
            introspect entity = null;
            try
            {
                using (var context = new LjcDbContext())
                {
                    entity = context.introspect.First(p => p.Id == id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return entity;
        }

        /// <summary>
        /// 查找单个实体
        /// </summary>
        /// <param name="question"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public introspect GetByQuestionAndDate(string question, DateTime date)
        {
            introspect entity = null;
            try
            {
                using (var context = new LjcDbContext())
                {
                    entity = context.introspect.FirstOrDefault(p => p.question == question && p.Date == date);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return entity;
        }

        /// <summary>
        /// 返回上一个需要反省的问题
        /// </summary>
        /// <param name="currentQuestion">当前问题</param>
        /// <returns></returns>
        public question GetPreviousQuestion(question currentQuestion)
        {
            if (currentQuestion == null) return null;
            using (var context = new LjcDbContext())
            {
                var previousQuestion = context.question.FirstOrDefault(p => p.Sort == currentQuestion.Sort - 1);

                if (previousQuestion != null)
                //寻找今天上次回答这个问题时的得分
                {
                    var introspect =
                        context.introspect.FirstOrDefault(
                            p => p.Date == DateTime.Now.Date && p.question == previousQuestion.QuestionMember);
                    if (introspect != null)
                    {
                        previousQuestion.Score = introspect.Score;
                    }
                }
                return previousQuestion;
            }
        }

        /// <summary>
        /// 根据日期找出所有introspect对象
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<introspect> GetListByDate(DateTime date)
        {
            using (var context = new LjcDbContext())
            {
                return context.introspect.Where(p => p.Date == date).ToList();
            }

        }

        /// <summary>
        /// 获取从当前问题开始往前逆推，第一个未处理的问题
        /// </summary>
        /// <param name="currentQuestion"></param>
        /// <returns></returns>
        public question GetPreviousUnhandledQuestion(question currentQuestion)
        {
            if (currentQuestion == null || currentQuestion.Sort == null) return null;

            using (var context = new LjcDbContext())
            {
                for (int i = currentQuestion.Sort.Value - 1; i > 0; i--)
                {
                    var question = context.question.First(p => p.Sort == i);
                    var date = DateTime.Now.Date;
                    if (
                        context.introspect.Any(
                            p => (p.Date == date && p.question == question.QuestionMember)))
                    {
                        continue;
                    }
                    return question;

                }
                return null;
            }
        }

    }
}
