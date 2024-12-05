using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMs.Models
{
    public class DepartmentAttendance
    {
        public string Department { get; set; }
        public int Present { get; set; }
        public int Total { get; set; }
    }
}