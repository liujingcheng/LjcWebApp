using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LjcWebApp.Helper
{
    public static class DbHelper
    {
        public static string DbConnectionString;
        public static DbContextOptionsBuilder Builder = new DbContextOptionsBuilder();

        private static LjcDbContext _dbContext;
        public static LjcDbContext GetDbContext()
        {
            return _dbContext ?? (_dbContext = new LjcDbContext());
        }
    }
}
