using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HRMs.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
    }
}