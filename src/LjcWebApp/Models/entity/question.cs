//------------------------------------------------------------------------------
// <auto-generated>
//    此代码是根据模板生成的。
//
//    手动更改此文件可能会导致应用程序中发生异常行为。
//    如果重新生成代码，则将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace LjcWebApp
{
    using System;
    using System.Collections.Generic;
    
    public partial class question
    {
        public string Id { get; set; }
        public string Question1 { get; set; }
        public Nullable<double> FullScore { get; set; }
        public Nullable<double> Weight { get; set; }
        public Nullable<short> IsYes { get; set; }
        public Nullable<short> IsPositive { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> Sort { get; set; }
        public Nullable<double> Score { get; set; }
    }
}
