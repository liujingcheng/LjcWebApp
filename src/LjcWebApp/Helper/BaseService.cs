using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LjcWebApp.Models.entity;
using Microsoft.AspNetCore.Http;

namespace LjcWebApp.Helper
{
    public class BaseService
    {
        public MyUser CurrentUser { get; set; }
    }
}
