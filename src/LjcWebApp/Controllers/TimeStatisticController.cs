using System;
using System.Collections.Generic;
using System.Linq;
using LjcWebApp.Helper;
using LjcWebApp.Models.ViewModels;
using LjcWebApp.Services.Account;
using Microsoft.AspNetCore.Mvc;
using LjcWebApp.Services.DataCRUD;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Sakura.AspNetCore;

namespace LjcWebApp.Controllers
{
    [Authorize]
    public class TimeStatisticController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;
        //
        // GET: /TimeStatistic/
        TimeStatisticService _timeStatisticService;

        private TimeStatisticService TimeStatisticService
        {
            get
            {
                if (_timeStatisticService == null)
                {
                    return _timeStatisticService = new TimeStatisticService
                    {
                        CurrentUser = new MyUserService().GetByUserName(HttpContext.User.Identity.Name)
                    };
                }
                return _timeStatisticService;
            }
        }

        public TimeStatisticController(IOptions<AppSettings> settings)
        {
            _appSettings = settings;
        }

        public ActionResult Control()
        {
            //HttpContext.Response.Buffer = true;
            //HttpContext.Response.Expires = 0;
            //HttpContext.Response.ExpiresAbsolute = DateTime.UtcNow.AddHours(8).AddDays(-1);
            //HttpContext.Response.AddHeader("pragma", "no-cache");
            //HttpContext.Response.AddHeader("cache-control", "private");
            //HttpContext.Response.CacheControl = "no-cache";

            try
            {
                ViewBag.EventList = TimeStatisticService.GetUnFinishedList();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return View();
        }

        [HttpPost]
        public string SaveEvent(string eventId, string eventName, int? effectiveTime, int? planMinutes, string status,
            string lastticks)
        {
            try
            {
                var planTime = planMinutes == null ? null : planMinutes * 60;
                return TimeStatisticService.SaveEvent(eventId, eventName.Trim(), effectiveTime, planTime, status, lastticks);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return ex.Message;
            }
        }

        [HttpPost]
        public string SaveEvent1(string eventId, string eventName, int? effectiveTime, int? planMinutes, string status, string lastticks, int quadrant)
        {
            try
            {
                return TimeStatisticService.SaveEvent(eventId, eventName.Trim(), null, null, status, lastticks, quadrant, 1);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return ex.Message;
            }
        }

        public ActionResult Index(int? page, string startDateStr, string endDateStr, string searchText)
        {
            try
            {
                var pageNumber = page ?? 1;
                var today = DateTime.UtcNow.AddHours(8);
                //if (string.IsNullOrEmpty(startDateStr))
                ////若起始日期为空则默认为本周一
                //{
                //    int i = today.DayOfWeek - DayOfWeek.Monday;
                //    if (i == -1)
                //        // i值 > = 0 ，因为枚举原因，Sunday排在最前，此时Sunday-Monday=-1，必须+7=6。 
                //        i = 6;
                //    var ts = new TimeSpan(i, 0, 0, 0);
                //    startDateStr = today.Subtract(ts).ToString("yyyy-MM-dd");
                //}

                //主了配合显示出1天以上任务，先取今天
                if (string.IsNullOrEmpty(startDateStr))
                {
                    startDateStr = today.ToString("yyyy-MM-dd");
                }

                if (string.IsNullOrEmpty(endDateStr))
                //若结束日期为空则默认为今天
                {
                    endDateStr = today.ToString("yyyy-MM-dd");
                }
                var startDate = Convert.ToDateTime(startDateStr);
                var endDate = Convert.ToDateTime(endDateStr);

                ViewBag.StartDateStr = startDateStr;
                ViewBag.EndDateStr = endDateStr;
                ViewBag.SearchText = searchText;

                var list = TimeStatisticService.GetFinishedList(startDate, endDate);
                if (!string.IsNullOrEmpty(searchText))
                {
                    list = list.Where(p => p.EventName.Contains(searchText)).ToList();
                }
                var days = list.Select(p => p.StartTime.Value.Date).Distinct();
                var ondayDataModelList = new List<OneDayDataModel>();
                foreach (var day in days)
                {
                    var ondayEventModelList = new List<EventModel>();
                    var ondayEventList = list.Where(p => p.StartTime.Value.Date == day).ToList();
                    foreach (var item in ondayEventList)
                    {
                        if (item.Status == "Started")//还在进行中的任务要特殊处理,要计算出上次结束时间到当前时间的间隔，然后加上总有效时间上
                        {
                            if (item.EndTime == null)
                            {
                                item.EndTime = item.StartTime;
                            }
                            var span = DateTime.UtcNow.AddHours(8) - item.EndTime.Value;
                            var seconds = (int)Math.Ceiling(span.TotalSeconds);
                            item.EffectiveTime += seconds;
                        }

                        var eventModel = new EventModel();

                        eventModel.EventId = item.EventId;
                        eventModel.Date = item.StartTime == null ? null : item.StartTime.Value.Date.ToString("MM-dd");
                        eventModel.EventName = item.EventName;
                        eventModel.StartTime = item.StartTime == null ? null : item.StartTime.Value.ToString("HH:mm:ss");
                        eventModel.EndTime = item.EndTime == null ? null : item.EndTime.Value.ToString("HH:mm:ss");
                        eventModel.EffectiveTime = item.EffectiveTime == null ? null : EventModel.TransferToDateTime(item.EffectiveTime);
                        eventModel.PlanTime = item.PlanTime == null ? null : EventModel.TransferToDateTime(item.PlanTime);
                        eventModel.Status = item.Status;
                        eventModel.Remark = item.Remark;

                        ondayEventModelList.Add(eventModel);
                    }
                    var oneDayTotalEffectiveTime = EventModel.TransferToDateTime(ondayEventList.Sum(p => p.EffectiveTime));

                    ondayDataModelList.Add(new OneDayDataModel()
                    {
                        Date = day.ToString("MM-dd"),
                        EventModelList = ondayEventModelList,
                        TotalEffectiveTime = oneDayTotalEffectiveTime
                    });
                }

                //注意，这里的查询方式使用的是假分页，若要使用真分页得看具体使用的ORM而定
                var pagedList = ondayDataModelList.ToPagedList(_appSettings.Value.DaysOnOnePage, pageNumber);

                ViewBag.OnePageOfHistoryList = pagedList;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }
            return View();
        }

        [HttpPost]
        public int Delete(string eventId)
        {
            return TimeStatisticService.Delete(eventId);
        }

        [HttpPost]
        public int UpdateOrder(string eventId1, string eventId2)
        {
            return TimeStatisticService.UpdateOrder(eventId1, eventId2);
        }

        [HttpPost]
        public string UpdateOrderAfterInsert(string beforNewLingeEventId, string insertedEventId)
        {
            if (string.IsNullOrEmpty(beforNewLingeEventId)) return "true";//说明要最后一行新增，没有点Insert
            if (string.IsNullOrEmpty(insertedEventId)) return "更新排序失败！参数insertedEventId为空！";
            return TimeStatisticService.UpdateOrderAfterInsert(beforNewLingeEventId, insertedEventId) ? "true" : "更新排序失败！";
        }

        /// <summary>
        /// 获取四象限里的列表
        /// </summary>
        public ActionResult FourQuadrant(int? pageSize)
        {
            ViewBag.PageSize = pageSize;//防止分页后丢失PageSize add by ljc 2015-12-05
            //页面首次加载没有使用异步分页访问时需要填充四个列表
            GetQuadrantList(1, 1, pageSize);
            GetQuadrantList(1, 2, pageSize);
            GetQuadrantList(1, 3, pageSize);
            GetQuadrantList(1, 4, pageSize);

            return View();
        }

        /// <summary>
        /// 获取四象限里的列表
        /// </summary>
        /// <param name="page">分页</param>
        /// <param name="quadrant">所在象限</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult GetQuadrantList(int? page, int quadrant, int? pageSize)
        {
            ViewBag.PageSize = pageSize;//防止分页后丢失PageSize add by ljc 2015-12-05
            if (pageSize == null)//如果为空，则从配置文件读取
            {
                int pageSize1 = _appSettings.Value.PageSize;
                pageSize = pageSize1;
            }

            ActionResult partial = new ContentResult();
            try
            {
                var pageNumber = page ?? 1;

                var list = TimeStatisticService.GetQuadrantList(quadrant);

                //注意，这里的查询方式使用的是假分页，若要使用真分页得看具体使用的ORM而定
                var pagedList = list.ToPagedList(pageNumber, pageSize.Value);

                switch (quadrant)
                {
                    case 1:
                        ViewBag.OnePageOfQuadrantList1 = pagedList;
                        partial = PartialView("QuadrantPartial1");
                        break;
                    case 2:
                        ViewBag.OnePageOfQuadrantList2 = pagedList;
                        partial = PartialView("QuadrantPartial2");
                        break;
                    case 3:
                        ViewBag.OnePageOfQuadrantList3 = pagedList;
                        partial = PartialView("QuadrantPartial3");
                        break;
                    case 4:
                        ViewBag.OnePageOfQuadrantList4 = pagedList;
                        partial = PartialView("QuadrantPartial4");
                        break;
                }

            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
            }

            return partial;
        }

        /// <summary>
        /// 改变是否在象限里的状态
        /// </summary>
        /// <param name="eventId">Id</param>
        /// <param name="inQuadrant">是否在象限里</param>
        /// <param name="status">状态</param>
        [HttpPost]
        public int ChangeInQuadrant(string eventId, short inQuadrant, string status = null)
        {
            return TimeStatisticService.ChangeInQuadrant(eventId, inQuadrant, status);
        }

        [HttpPost]
        public int ChangeQuadrant(string eventId, short quadrant, short? inQuadrant = null)
        {
            if (inQuadrant.HasValue)
            {
                var result = TimeStatisticService.ChangeInQuadrant(eventId, inQuadrant.Value);
                if (result != 0)
                {
                    return result;
                }
            }
            return TimeStatisticService.ChangeQuadrant(eventId, quadrant);
        }

        [HttpPost]
        public int UpdateRemark(string eventId, string remark)
        {
            return TimeStatisticService.UpdateRemark(eventId, remark);
        }

        public IActionResult QuickAdd(EventModel model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult QuickAdd([FromServices]IHostingEnvironment env, EventModel eventModel)
        {
            if (ModelState.IsValid)
            {
                var result = SaveEvent(null, eventModel.EventName, null, null, "New", null);
                var eventId = result.Split('|')[0];
                var success = UpdateOrderAfterInsert("heqadLineId", eventId);
                if (success == "true")
                {
                    return RedirectToAction(nameof(QuickAdd), new EventModel());
                }
                return RedirectToAction(nameof(QuickAdd), new EventModel() { EventName = success });
            }
            return View();
        }
    }
}
