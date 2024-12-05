using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMs.Models
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TodayPresent { get; set; }
        public int PendingLeaves { get; set; }
        public int TotalDepartments { get; set; }

        public List<DepartmentStat> DepartmentStats { get; set; }

        public class DepartmentStat
        {
            public string Department { get; set; }
            public int EmployeeCount { get; set; }
            public int Present { get; set; }
            public int Total { get; set; }
        }
    }
}