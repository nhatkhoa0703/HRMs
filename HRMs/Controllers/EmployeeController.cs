using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using HRMs.Models;

namespace HRMs.Controllers
{
    public class EmployeeController : Controller
    {
        private string connectionString = "Data Source=(local);Initial Catalog=HRMsDB;Integrated Security=True";

        // GET: Employee
        public ActionResult Index()
        {
            List<Employee> employees = new List<Employee>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT e.*, d.DepartmentName, p.PositionName 
                    FROM Employee e
                    LEFT JOIN Department d ON e.DepartmentID = d.DepartmentID
                    LEFT JOIN Position p ON e.PositionID = p.PositionID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new Employee
                            {
                                EmployeeID = (int)reader["EmployeeID"],
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                HireDate = Convert.ToDateTime(reader["HireDate"]),
                                DepartmentID = (int)reader["DepartmentID"],
                                PositionID = (int)reader["PositionID"],
                                Status = (bool)reader["Status"],
                                Department = new Department { DepartmentName = reader["DepartmentName"].ToString() },
                                Position = new Position { PositionName = reader["PositionName"].ToString() }
                            });
                        }
                    }
                }
            }
            return View(employees);
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            // Load departments and positions for dropdowns
            ViewBag.Departments = GetDepartmentList();
            ViewBag.Positions = GetPositionList();
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO Employee (FirstName, LastName, Email, Phone, HireDate, DepartmentID, PositionID, Status) 
                        VALUES (@FirstName, @LastName, @Email, @Phone, @HireDate, @DepartmentID, @PositionID, @Status)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", employee.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", employee.LastName);
                        cmd.Parameters.AddWithValue("@Email", employee.Email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Phone", employee.Phone ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HireDate", employee.HireDate);
                        cmd.Parameters.AddWithValue("@DepartmentID", employee.DepartmentID);
                        cmd.Parameters.AddWithValue("@PositionID", employee.PositionID);
                        cmd.Parameters.AddWithValue("@Status", employee.Status);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }

            ViewBag.Departments = GetDepartmentList();
            ViewBag.Positions = GetPositionList();
            return View(employee);
        }

        // GET: Employee/Edit/5
        public ActionResult Edit(int id)
        {
            Employee employee = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Employee WHERE EmployeeID = @ID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            employee = new Employee
                            {
                                EmployeeID = (int)reader["EmployeeID"],
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                HireDate = Convert.ToDateTime(reader["HireDate"]),
                                DepartmentID = (int)reader["DepartmentID"],
                                PositionID = (int)reader["PositionID"],
                                Status = (bool)reader["Status"]
                            };
                        }
                    }
                }
            }

            if (employee == null)
            {
                return HttpNotFound();
            }

            ViewBag.Departments = GetDepartmentList();
            ViewBag.Positions = GetPositionList();
            return View(employee);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = @"UPDATE Employee 
                               SET FirstName = @FirstName, 
                                   LastName = @LastName, 
                                   Email = @Email, 
                                   Phone = @Phone, 
                                   HireDate = @HireDate, 
                                   DepartmentID = @DepartmentID, 
                                   PositionID = @PositionID, 
                                   Status = @Status 
                               WHERE EmployeeID = @EmployeeID";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                            cmd.Parameters.AddWithValue("@FirstName", employee.FirstName);
                            cmd.Parameters.AddWithValue("@LastName", employee.LastName);
                            cmd.Parameters.AddWithValue("@Email", (object)employee.Email ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Phone", (object)employee.Phone ?? DBNull.Value);
                            // Add date validation
                            if (employee.HireDate == DateTime.MinValue || employee.HireDate == default(DateTime))
                            {
                                cmd.Parameters.AddWithValue("@HireDate", DateTime.Now);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@HireDate", employee.HireDate);
                            }
                            cmd.Parameters.AddWithValue("@DepartmentID", employee.DepartmentID);
                            cmd.Parameters.AddWithValue("@PositionID", employee.PositionID);
                            cmd.Parameters.AddWithValue("@Status", employee.Status);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the error and add to ModelState
                    ModelState.AddModelError("", "Error saving employee: " + ex.Message);
                }
            }

            // If we got this far, something failed, redisplay form
            // Reload dropdowns
            ViewBag.Departments = GetDepartmentList();  // Assuming you have this method
            ViewBag.Positions = GetPositionList();      // Assuming you have this method
            return View(employee);
        }

        // GET: Employee/Delete/5
        public ActionResult Delete(int id)
        {
            Employee employee = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT e.*, d.DepartmentName, p.PositionName 
                    FROM Employee e
                    LEFT JOIN Department d ON e.DepartmentID = d.DepartmentID
                    LEFT JOIN Position p ON e.PositionID = p.PositionID
                    WHERE e.EmployeeID = @ID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            employee = new Employee
                            {
                                EmployeeID = (int)reader["EmployeeID"],
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Phone = reader["Phone"].ToString(),
                                HireDate = Convert.ToDateTime(reader["HireDate"]),
                                Department = new Department { DepartmentName = reader["DepartmentName"].ToString() },
                                Position = new Position { PositionName = reader["PositionName"].ToString() }
                            };
                        }
                    }
                }
            }

            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Employee WHERE EmployeeID = @ID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }

        // Helper methods for dropdowns
        private SelectList GetDepartmentList()
        {
            List<Department> departments = new List<Department>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT DepartmentID, DepartmentName FROM Department";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departments.Add(new Department
                            {
                                DepartmentID = (int)reader["DepartmentID"],
                                DepartmentName = reader["DepartmentName"].ToString()
                            });
                        }
                    }
                }
            }
            return new SelectList(departments, "DepartmentID", "DepartmentName");
        }

        private SelectList GetPositionList()
        {
            List<Position> positions = new List<Position>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT PositionID, PositionName FROM Position";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            positions.Add(new Position
                            {
                                PositionID = (int)reader["PositionID"],
                                PositionName = reader["PositionName"].ToString()
                            });
                        }
                    }
                }
            }
            return new SelectList(positions, "PositionID", "PositionName");
        }
    }
}