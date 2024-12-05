using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Web;

namespace HRMs.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime HireDate { get; set; }
        public int DepartmentID { get; set; }
        public int PositionID { get; set; }
        public bool Status { get; set; }

        public virtual Department Department { get; set; }
        public virtual Position Position { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
        public virtual ICollection<Leave> Leaves { get; set; }
    }
}