using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LjcWebApp
{
    public class AppSettings
    {
        /// <summary>
        /// 一页几条记录
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 一页显示几天的数据（针对EventHistory查询）
        /// </summary>
        public int DaysOnOnePage { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        
    }
}
