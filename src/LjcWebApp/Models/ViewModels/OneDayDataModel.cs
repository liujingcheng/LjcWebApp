using System;
using System.Collections.Generic;
using System.Linq;

namespace LjcWebApp.Models.ViewModels
{
    public class OneDayDataModel
    {
        public string Date { get; set; }
        public List<EventModel> EventModelList { get; set; }
        public string TotalEffectiveTime { get; set; }
    }
}