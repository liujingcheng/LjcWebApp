using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LjcWebApp;
using LjcWebApp.Helper;
using Microsoft.EntityFrameworkCore;

namespace LjcWebApp.Services.ConfigStatic
{
    public class IntrospectStaticData
    {
        private static int? _questionCount;
        public static int? QuestionCount
        {
            get
            {
                if (_questionCount == null)
                {
                    using (var context = new LjcDbContext())
                    {
                        _questionCount = context.question.Count();
                    }
                }
                return _questionCount ?? 0;
            }
            set { _questionCount = value; }
        }
    }
}
