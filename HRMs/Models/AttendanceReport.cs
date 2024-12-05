using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMs.Models
{
    public class AttendanceReport
    {
        public string EmployeeName { get; set; }
        public System.DateTime CheckIn { get; set; }
        public System.DateTime? CheckOut { get; set; }
        public int MinutesWorked { get; set; }

        public string WorkDuration
        {
            get
            {
                int hours = MinutesWorked / 60;
                int minutes = MinutesWorked % 60;
                return $"{hours}h {minutes}m";
            }
        }
    }
}