//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace LjcWebApp
{
    using System;
    using System.Collections.Generic;
    
    public partial class timestatistic
    {
        [Key]
        public string EventId { get; set; }
        public string EventName { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
        public Nullable<System.DateTime> EndTime { get; set; }
        public Nullable<int> EffectiveTime { get; set; }
        public Nullable<int> PlanTime { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime ModifiedOn { get; set; }
        public long OrderValue { get; set; }
        public Nullable<int> Quadrant { get; set; }
        public Nullable<short> InQuadrant { get; set; }
        public string Remark { get; set; }
    }
}
