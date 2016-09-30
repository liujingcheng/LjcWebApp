using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LjcWebApp.Models.ViewModels
{
    public class EventModel
    {
        public string EventId { get; set; }
        public string Date { get; set; }
        [Required]
        [StringLength(1000, ErrorMessage = "请输入1-255个字符内！", MinimumLength = 1)]
        [Display(Name = "事件名")]
        public string EventName { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string EffectiveTime { get; set; }
        public string PlanTime { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }

        public static string TransferToDateTime(int? pSeconds)
        {
            int seconds;
            if (pSeconds == null)
            {
                return null;
            }
            else
            {
                seconds = pSeconds.Value;
            }
            int hour = 0;
            int minu = 0;
            int sec = 0;
            if (seconds > 3600)
            {
                hour = seconds / 3600;
                seconds = seconds % 3600;
            }
            if (seconds > 60)
            {
                minu = seconds / 60;
                seconds = seconds % 60;
            }
            sec = seconds;
            return hour + ":" + minu + ":" + sec;
        }
    }

}