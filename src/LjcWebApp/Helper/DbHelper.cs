using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LjcWebApp.Helper
{
    public static class DbHelper
    {
        public const string DbConnectionString = "server=rdsfarfbifarfbi.mysql.rds.aliyuncs.com;database=wordtest;uid=aspuser;pwd=ljc1qazse4";
        public static DbContextOptionsBuilder Builder = new DbContextOptionsBuilder();
    }
}
