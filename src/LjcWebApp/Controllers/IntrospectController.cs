using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using MvcWords.Domain;
using Newtonsoft.Json;
using  LjcWebApp.Services.ConfigStatic;
using  LjcWebApp.Services.Introspection;

namespace LjcWebApp.Controllers
{
    [Authorize]
    public class IntrospectController : Controller
    {
        //
        // GET: /introspect/
        QuestionService _questionService = new QuestionService();
        IntrospectService _introspectService = new IntrospectService();

        public ActionResult Index()
        {
            ViewBag.QuestionCount = IntrospectStaticData.QuestionCount;
            return View();
        }

        [HttpPost]
        public string SaveIntorspect(Question question, double score)
        {
            var introspect = new introspect();
            introspect.QuestionId = question.Id;
            introspect.Question = question.QuestionMember;
            introspect.Score = score;
            introspect.Weight = question.Weight;
            introspect.IsYes = question.IsYes;
            introspect.IsPositive = question.IsPositive;
            introspect.Date = DateTime.Now.Date;

            var success = false;
            if (_introspectService.IsExist(introspect))
            {
                var entity = _introspectService.GetByQuestionAndDate(introspect.Question, introspect.Date.Value);
                introspect.Id = entity.Id;
                success = _introspectService.Update(introspect);
            }
            else
            {
                success = _introspectService.Add(introspect);
            }
            if (success)
            {
                return "true";
            }
            return "false";
        }

        [HttpPost]
        public string GetNextQuestion(Question currentQuestion)
        {
            if (currentQuestion != null && currentQuestion.Id == null)
            //当前端传null过来时，这里接收到的结果会序列化成一个各字段内容都为空的实例，而不是直接null，所以这里处理下
            {
                currentQuestion = null;
            }

            string result = null;

            var nextQuestion = _introspectService.GetNextQuestion(currentQuestion);
            result = nextQuestion != null ? JsonConvert.SerializeObject(nextQuestion) : "end";

            return result;
        }



        [HttpPost]
        public string GetPreviousQuestion(Question currentQuestion)
        {
            if (currentQuestion != null && currentQuestion.Id == null)
            //当前端传null过来时，这里接收到的结果会序列化成一个各字段内容都为空的实例，而不是直接null，所以这里处理下
            {
                currentQuestion = null;
            }

            string result = null;

            var previousQuestion = _introspectService.GetPreviousQuestion(currentQuestion);
            result = previousQuestion != null ? JsonConvert.SerializeObject(previousQuestion) : "end";

            return result;
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public string GetStatisticInfo()
        {
            var yesterdayList = _introspectService.GetListByDate(DateTime.Now.AddDays(-1).Date);
            var todayList = _introspectService.GetListByDate(DateTime.Now.Date);

            double yesterdayTotalScore = yesterdayList.Sum(p => p.Score) ?? 0;
            var yesterdayAverageScore = yesterdayList.Count == 0 ? 0 : Math.Round(yesterdayTotalScore / yesterdayList.Count, 2);
            double todayTotalScore = todayList.Sum(p => p.Score) ?? 0;
            var todayAverageScore = todayList.Count == 0 ? 0 : Math.Round(todayTotalScore / todayList.Count, 2);

            var result = string.Format("昨天:{1}/{0}/{4}；今天:{3}/{2}/{5}",
                yesterdayTotalScore, yesterdayAverageScore, todayTotalScore, todayAverageScore,
                yesterdayList.Count, todayList.Count);

            return result;

        }

        /// <summary>
        /// 获取从当前问题开始往前逆推，第一个未处理的问题
        /// </summary>
        /// <param name="currentQuestion"></param>
        /// <returns></returns>
        [HttpPost]
        public string GetPreviousUnhandledQuestion(Question currentQuestion)
        {
            if (currentQuestion != null && currentQuestion.Id == null)
            //当前端传null过来时，这里接收到的结果会序列化成一个各字段内容都为空的实例，而不是直接null，所以这里处理下
            {
                currentQuestion = null;
            }

            string result = null;

            var question = _introspectService.GetPreviousUnhandledQuestion(currentQuestion);
            result = question != null ? JsonConvert.SerializeObject(question) : "end";

            return result;
        }

    }
}
