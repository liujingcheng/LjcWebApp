using System;
using System.Linq;
using LjcWebApp.Helper;
using LjcWebApp.Services.Account;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using  LjcWebApp.Services.ConfigStatic;
using  LjcWebApp.Services.Introspection;
using Microsoft.AspNetCore.Authorization;

namespace LjcWebApp.Controllers
{
    [Authorize]
    public class IntrospectController : Controller
    {
        //
        // GET: /introspect/
        private IntrospectService _introspectService;
        IntrospectService IntrospectService
        {
            get
            {
                if (HttpContext.User.Identity.Name != "ljcwyc")
                {
                    throw new Exception();
                }
                if (_introspectService == null)
                {
                    return _introspectService = new IntrospectService
                    {
                        CurrentUser = new MyUserService().GetByUserName(HttpContext.User.Identity.Name)
                    };
                }
                return _introspectService;
            }
        }

        public ActionResult Index()
        {
            ViewBag.QuestionCount = IntrospectStaticData.QuestionCount;
            return View();
        }

        [HttpPost]
        public string SaveIntorspect(question question, double score)
        {
            var introspect = new introspect();
            introspect.QuestionId = question.Id;
            introspect.question = question.QuestionMember;
            introspect.Score = score;
            introspect.Weight = question.Weight;
            introspect.IsYes = question.IsYes;
            introspect.IsPositive = question.IsPositive;
            introspect.Date = DateTime.Now.Date;

            var success = false;
            if (IntrospectService.IsExist(introspect))
            {
                var entity = IntrospectService.GetByQuestionAndDate(introspect.question, introspect.Date.Value);
                introspect.Id = entity.Id;
                success = IntrospectService.Update(introspect);
            }
            else
            {
                success = IntrospectService.Add(introspect);
            }
            if (success)
            {
                return "true";
            }
            return "false";
        }

        [HttpPost]
        public string GetNextQuestion(question currentQuestion)
        {
            if (currentQuestion != null && currentQuestion.Id == null)
            //当前端传null过来时，这里接收到的结果会序列化成一个各字段内容都为空的实例，而不是直接null，所以这里处理下
            {
                currentQuestion = null;
            }

            string result = null;

            var nextQuestion = IntrospectService.GetNextQuestion(currentQuestion);
            result = nextQuestion != null ? JsonConvert.SerializeObject(nextQuestion) : "end";

            return result;
        }



        [HttpPost]
        public string GetPreviousQuestion(question currentQuestion)
        {
            if (currentQuestion != null && currentQuestion.Id == null)
            //当前端传null过来时，这里接收到的结果会序列化成一个各字段内容都为空的实例，而不是直接null，所以这里处理下
            {
                currentQuestion = null;
            }

            string result = null;

            var previousQuestion = IntrospectService.GetPreviousQuestion(currentQuestion);
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
            var yesterdayList = IntrospectService.GetListByDate(DateTime.Now.AddDays(-1).Date);
            var todayList = IntrospectService.GetListByDate(DateTime.Now.Date);

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
        public string GetPreviousUnhandledQuestion(question currentQuestion)
        {
            if (currentQuestion != null && currentQuestion.Id == null)
            //当前端传null过来时，这里接收到的结果会序列化成一个各字段内容都为空的实例，而不是直接null，所以这里处理下
            {
                currentQuestion = null;
            }

            string result = null;

            var question = IntrospectService.GetPreviousUnhandledQuestion(currentQuestion);
            result = question != null ? JsonConvert.SerializeObject(question) : "end";

            return result;
        }

    }
}
