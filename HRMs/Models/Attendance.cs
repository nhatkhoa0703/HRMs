using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMs.Models
{
    public class Attendance
    {
        public int AttendanceID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        public virtual Employee Employee { get; set; }
        public string EmployeeName { get; set; }
    }
}