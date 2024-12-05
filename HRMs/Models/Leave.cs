using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMs.Models
{
    public class Leave
    {
        public int LeaveID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }

        public virtual Employee Employee { get; set; }
        public string EmployeeName { get; set; }
    }
}