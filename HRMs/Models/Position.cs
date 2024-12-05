using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMs.Models
{
    public class Position
    {
        public int PositionID { get; set; }
        public string PositionName { get; set; }
        public string Description { get; set; }
        public decimal BasicSalary { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        // Add this property
        public int EmployeeCount { get; set; }
    }
}