using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using HRMs.Models;

namespace HRMs.Controllers
{
    public class LeaveController : Controller
    {
        private string connectionString = "Data Source=(local);Initial Catalog=HRMsDB;Integrated Security=True";

        // GET: Leave
        public ActionResult Index()
        {
            List<Leave> leaves = new List<Leave>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT l.*, 
                           e.FirstName + ' ' + e.LastName as EmployeeName
                    FROM Leave l
                    JOIN Employee e ON l.EmployeeID = e.EmployeeID
                    ORDER BY l.StartDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            leaves.Add(new Leave
                            {
                                LeaveID = (int)reader["LeaveID"],
                                EmployeeID = (int)reader["EmployeeID"],
                                StartDate = Convert.ToDateTime(reader["StartDate"]),
                                EndDate = Convert.ToDateTime(reader["EndDate"]),
                                LeaveType = reader["LeaveType"].ToString(),
                                Status = reader["Status"].ToString(),
                                Reason = reader["Reason"].ToString(),
                                EmployeeName = reader["EmployeeName"].ToString()
                            });
                        }
                    }
                }
            }
            return View(leaves);
        }

        // GET: Leave/Create
        public ActionResult Create()
        {
            ViewBag.Employees = GetEmployeeList();
            ViewBag.LeaveTypes = new SelectList(new[]
            {
                "Annual Leave",
                "Sick Leave",
                "Personal Leave",
                "Emergency Leave"
            });
            return View();
        }

        // POST: Leave/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Leave leave)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO Leave (EmployeeID, StartDate, EndDate, LeaveType, Status, Reason)
                        VALUES (@EmployeeID, @StartDate, @EndDate, @LeaveType, @Status, @Reason)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", leave.EmployeeID);
                        cmd.Parameters.AddWithValue("@StartDate", leave.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", leave.EndDate);
                        cmd.Parameters.AddWithValue("@LeaveType", leave.LeaveType);
                        cmd.Parameters.AddWithValue("@Status", "Pending");
                        cmd.Parameters.AddWithValue("@Reason", leave.Reason ?? (object)DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }

            ViewBag.Employees = GetEmployeeList();
            ViewBag.LeaveTypes = new SelectList(new[]
            {
                "Annual Leave",
                "Sick Leave",
                "Personal Leave",
                "Emergency Leave"
            });
            return View(leave);
        }

        // GET: Leave/Edit/5
        public ActionResult Edit(int id)
        {
            Leave leave = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT l.*, e.FirstName + ' ' + e.LastName as EmployeeName
                    FROM Leave l
                    JOIN Employee e ON l.EmployeeID = e.EmployeeID
                    WHERE l.LeaveID = @ID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            leave = new Leave
                            {
                                LeaveID = (int)reader["LeaveID"],
                                EmployeeID = (int)reader["EmployeeID"],
                                StartDate = Convert.ToDateTime(reader["StartDate"]),
                                EndDate = Convert.ToDateTime(reader["EndDate"]),
                                LeaveType = reader["LeaveType"].ToString(),
                                Status = reader["Status"].ToString(),
                                Reason = reader["Reason"].ToString(),
                                EmployeeName = reader["EmployeeName"].ToString()
                            };
                        }
                    }
                }
            }

            if (leave == null)
            {
                return HttpNotFound();
            }

            ViewBag.LeaveTypes = new SelectList(new[]
            {
                "Annual Leave",
                "Sick Leave",
                "Personal Leave",
                "Emergency Leave"
            }, leave.LeaveType);

            ViewBag.Statuses = new SelectList(new[]
            {
                "Pending",
                "Approved",
                "Rejected"
            }, leave.Status);

            return View(leave);
        }

        // POST: Leave/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Leave leave)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        UPDATE Leave 
                        SET StartDate = @StartDate, 
                            EndDate = @EndDate,
                            LeaveType = @LeaveType,
                            Status = @Status,
                            Reason = @Reason
                        WHERE LeaveID = @ID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", leave.LeaveID);
                        cmd.Parameters.AddWithValue("@StartDate", leave.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", leave.EndDate);
                        cmd.Parameters.AddWithValue("@LeaveType", leave.LeaveType);
                        cmd.Parameters.AddWithValue("@Status", leave.Status);
                        cmd.Parameters.AddWithValue("@Reason", leave.Reason ?? (object)DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }

            ViewBag.LeaveTypes = new SelectList(new[]
            {
                "Annual Leave",
                "Sick Leave",
                "Personal Leave",
                "Emergency Leave"
            }, leave.LeaveType);

            ViewBag.Statuses = new SelectList(new[]
            {
                "Pending",
                "Approved",
                "Rejected"
            }, leave.Status);

            return View(leave);
        }

        private SelectList GetEmployeeList()
        {
            List<Employee> employees = new List<Employee>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT EmployeeID, FirstName + ' ' + LastName as FullName FROM Employee WHERE Status = 1";
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
                                FirstName = reader["FullName"].ToString()
                            });
                        }
                    }
                }
            }
            return new SelectList(employees, "EmployeeID", "FirstName");
        }
    }
}