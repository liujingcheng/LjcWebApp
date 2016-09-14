using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LjcWebApp;
using LjcWebApp.Helper;

namespace LjcWebApp.Services.DataCRUD
{
    public class TimeStatisticService : BaseService
    {
        public string SaveEvent(string eventId, string eventName, int? effectiveTime, int? planTime, string status,
            string lastticks)
        {
            return SaveEvent(eventId, eventName, effectiveTime, planTime, status,
                lastticks, 1, 0);//直接新建任务时，默认就在第一象限
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>-1:失败</returns>
        public string SaveEvent(string eventId, string eventName, int? effectiveTime, int? planTime, string status, string lastticks, int? quadrant, short inQuadrant)
        {
            string result = "-1";
            try
            {
                using (var context = new LjcDbContext())
                {
                    timestatistic entity;
                    if (status == "New" && eventId == null)
                    {
                        entity = new timestatistic();
                        entity.CreatedOn = DateTime.Now;
                        entity.ModifiedOn = DateTime.Now;
                        entity.EventName = eventName;
                        entity.EffectiveTime = effectiveTime;
                        entity.PlanTime = planTime;
                        entity.Status = status;
                        entity.InQuadrant = inQuadrant;
                        if (quadrant != null) entity.Quadrant = quadrant;
                    }
                    else
                    {
                        entity = context.timestatistic.First(p => p.EventId == eventId);

                        ////正在进行中的事件在新页面打开并操作后要刷新旧页面后才能在旧页面上操作
                        //if (entity.ModifiedOn.Ticks > Convert.ToInt64(lastticks))//由于存入数据库精度有损，故不能用==来判断
                        //{
                        //    return "need refresh";
                        //}
                        entity.ModifiedOn = DateTime.Now;
                        entity.InQuadrant = inQuadrant;
                        if (quadrant != null) entity.Quadrant = quadrant;
                        result = entity.ModifiedOn.Ticks.ToString();
                    }

                    if (status == "Started")
                    {
                        if (entity.StartTime == null)
                        {
                            entity.StartTime = DateTime.Now;
                        }
                        entity.EventName = eventName;
                        entity.EffectiveTime = effectiveTime;
                        entity.PlanTime = planTime;
                        entity.Status = status;
                    }
                    else if (status == "Paused" || status == "Stopped")
                    {
                        entity.EndTime = DateTime.Now;
                        entity.EventName = eventName;
                        entity.EffectiveTime = effectiveTime;
                        entity.PlanTime = planTime;
                        entity.Status = status;
                    }
                    else if (status == "Edit")
                    {
                        entity.EventName = eventName;
                        entity.PlanTime = planTime;
                    }
                    if (entity.EventId == null)
                    {
                        entity.EventId = Guid.NewGuid().ToString();
                        result = entity.EventId + "|" + entity.ModifiedOn.Ticks;
                        context.timestatistic.Add(entity);
                        entity.UserId = CurrentUser.UserId;
                    }
                    else
                    {
                        context.timestatistic.Update(entity);
                    }
                    context.SaveChanges();

                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return "-1";
            }
        }

        /// <summary>
        /// 获得开始到结束日期间所有完成的记录
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<timestatistic> GetFinishedList(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    if (startDate == null && endDate == null)
                    {
                        return context.timestatistic.Where(p => p.UserId == CurrentUser.UserId).OrderByDescending(p => p.CreatedOn).ToList();
                    }
                    if (startDate == null)
                    {
                        return context.timestatistic.OrderByDescending(p => p.Status != "New" && p.StartTime.Value.Date <= endDate.Value.Date).ToList();
                    }
                    if (endDate == null)
                    {
                        return context.timestatistic.OrderByDescending(p => p.Status != "New" && p.StartTime.Value.Date >= startDate.Value.Date).ToList();
                    }
                    return context.timestatistic.Where(p =>
                       p.Status != "New" && p.StartTime.Value.Date >= startDate.Value.Date && p.StartTime.Value.Date <= endDate.Value.Date
                       && p.InQuadrant != 1 && p.Quadrant != 0 && p.UserId == CurrentUser.UserId)
                        .OrderBy(p => p.StartTime).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return new List<timestatistic>();
            }
        }

        /// <summary>
        /// 获取所有未完成记录
        /// </summary>
        /// <returns></returns>
        public List<timestatistic> GetUnFinishedList()
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    return context.timestatistic.Where(p => p.Status != "Stopped" && p.InQuadrant != 1 && p.UserId == CurrentUser.UserId).OrderBy(p => p.OrderValue).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return new List<timestatistic>();
            }
        }



        /// <summary>
        /// 插入事件时，更新排序
        /// </summary>
        /// <param name="beforNewLingeEventId">插入的新行的上一行的EventId</param>
        /// <param name="insertedEventId">插入的新行的EventId</param>
        /// <returns></returns>
        public bool UpdateOrderAfterInsert(string beforNewLingeEventId, string insertedEventId)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var unfinishedList = context.timestatistic.Where(p => p.Status != "Stopped" && p.InQuadrant != 1 && p.UserId == CurrentUser.UserId).OrderBy(p => p.OrderValue).ToList();
                    if (unfinishedList.Count == 0) return true;

                    long tempOrderValue;
                    if (beforNewLingeEventId == "heqadLineId")
                    //如果是从行头往下插入
                    {
                        tempOrderValue = unfinishedList.First().OrderValue - 1;
                    }
                    else
                    {
                        tempOrderValue = context.timestatistic.First(p => p.EventId == beforNewLingeEventId).OrderValue;
                    }
                    var insertedEvent = context.timestatistic.First(p => p.EventId == insertedEventId);
                    foreach (var entity in unfinishedList)
                    {
                        if (entity.OrderValue > tempOrderValue)
                        {
                            entity.OrderValue++;
                        }
                    }
                    insertedEvent.OrderValue = tempOrderValue + 1;
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
        /// 删除记录
        /// </summary>
        /// <returns></returns>
        public int Delete(string eventId)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.timestatistic.First(p => p.EventId == eventId);
                    context.timestatistic.Remove(entity);
                    context.SaveChanges();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return -1;
            }
        }

        /// <summary>
        /// 更新排序字段（交换两记录排序字段的值）
        /// </summary>
        /// <param name="eventId1"></param>
        /// <param name="eventId2"></param>
        /// <returns></returns>
        public int UpdateOrder(string eventId1, string eventId2)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity1 = context.timestatistic.First(p => p.EventId == eventId1);
                    var entity2 = context.timestatistic.First(p => p.EventId == eventId2);
                    var tempOrderValue = entity1.OrderValue;
                    entity1.OrderValue = entity2.OrderValue;
                    entity2.OrderValue = tempOrderValue;

                    context.SaveChanges();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return -1;
            }
        }

        /// <summary>
        /// 获取四象限里的列表
        /// </summary>
        /// <param name="quadrant">所在象限</param>
        /// <returns></returns>
        public List<timestatistic> GetQuadrantList(int quadrant)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    return context.timestatistic.
                        Where(p => p.InQuadrant == 1 && p.Quadrant == quadrant && p.UserId == CurrentUser.UserId).
                        OrderBy(p => p.OrderValue).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return new List<timestatistic>();
            }
        }

        /// <summary>
        /// 改变象限
        /// </summary>
        /// <returns></returns>
        public int ChangeQuadrant(string eventId, int newQuadrant)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.timestatistic.First(p => p.EventId == eventId);
                    entity.Quadrant = newQuadrant;
                    context.SaveChanges();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return -1;
            }
        }

        /// <summary>
        /// 改变是否在象限里的状态
        /// </summary>
        /// <returns></returns>
        public int ChangeInQuadrant(string eventId, short inQuadrant)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.timestatistic.First(p => p.EventId == eventId);
                    entity.InQuadrant = inQuadrant;
                    context.SaveChanges();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return -1;
            }
        }



        /// <summary>
        /// 更新备注
        /// </summary>
        /// <returns></returns>
        public int UpdateRemark(string eventId, string remark)
        {
            try
            {
                using (var context = new LjcDbContext())
                {
                    var entity = context.timestatistic.First(p => p.EventId == eventId);
                    entity.Remark = remark;
                    context.SaveChanges();
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message, ex);
                return -1;
            }
        }
    }
}
