using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LjcWebApp.Helper
{
    public static class DbHelper
    {
        public const string DbConnectionString = "";
        public static DbContextOptionsBuilder Builder = new DbContextOptionsBuilder();
    }
}
