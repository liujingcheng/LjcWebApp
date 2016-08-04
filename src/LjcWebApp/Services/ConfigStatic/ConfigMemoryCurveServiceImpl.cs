using System;
using System.Collections.Generic;
using System.Linq;
using LjcWebApp.Helper;

namespace LjcWebApp.Services.ConfigStatic
{
    public static class ConfigMemoryCurveServiceImpl
    {
        public static List<configmemorycurve_tb> ListConfigMemoryCurveTb = null;
        /// <summary>
        /// 获取记忆曲线配置信息
        /// </summary>
        /// <exception cref="Exception">查询结果对象异常</exception>
        public static IEnumerable<configmemorycurve_tb> GetConfigMemoryCurveAll()
        {
            if (ListConfigMemoryCurveTb == null)
            {
                try
                {
                    var context = DbHelper.GetDbContext();
                    {
                        ListConfigMemoryCurveTb = context.configmemorycurve_tb.ToList();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("获取记忆曲线配置信息异常", ex);
                    return null;
                }
            }

            return ListConfigMemoryCurveTb;
        }
    }
}
