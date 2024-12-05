using HRMs.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;

public class HomeController : Controller
{
    private string connectionString = "Data Source=(local);Initial Catalog=HRMsDB;Integrated Security=True";

public ActionResult Index()
{
    var dashboard = new DashboardViewModel();
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        conn.Open();

        // Get total employees
        string empQuery = "SELECT COUNT(*) FROM Employee WHERE Status = 1";
        using (SqlCommand cmd = new SqlCommand(empQuery, conn))
        {
            dashboard.TotalEmployees = (int)cmd.ExecuteScalar();
        }

        // Get department count
        string deptQuery = "SELECT COUNT(*) FROM Department";
        using (SqlCommand cmd = new SqlCommand(deptQuery, conn))
        {
            dashboard.TotalDepartments = (int)cmd.ExecuteScalar();
        }

        // Get today's attendance
        string attQuery = @"SELECT COUNT(DISTINCT EmployeeID) 
                          FROM Attendance 
                          WHERE CAST(CheckIn AS DATE) = CAST(GETDATE() AS DATE)";
        using (SqlCommand cmd = new SqlCommand(attQuery, conn))
        {
            dashboard.TodayPresent = (int)cmd.ExecuteScalar();
        }

        // Get pending leave requests
        string leaveQuery = "SELECT COUNT(*) FROM Leave WHERE Status = 'Pending'";
        using (SqlCommand cmd = new SqlCommand(leaveQuery, conn))
        {
            dashboard.PendingLeaves = (int)cmd.ExecuteScalar();
        }

            // Get department-wise employee count
        string deptEmpQuery = @"
    SELECT 
        d.DepartmentName, 
        COUNT(e.EmployeeID) as TotalEmployees,
        (SELECT COUNT(DISTINCT a.EmployeeID) 
         FROM Attendance a 
         JOIN Employee e2 ON a.EmployeeID = e2.EmployeeID 
         WHERE e2.DepartmentID = d.DepartmentID 
         AND CAST(a.CheckIn AS DATE) = CAST(GETDATE() AS DATE)) as PresentToday
    FROM Department d
    LEFT JOIN Employee e ON d.DepartmentID = e.DepartmentID
    GROUP BY d.DepartmentID, d.DepartmentName";

            dashboard.DepartmentStats = new List<DashboardViewModel.DepartmentStat>();
            using (SqlCommand cmd = new SqlCommand(deptEmpQuery, conn))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dashboard.DepartmentStats.Add(new DashboardViewModel.DepartmentStat
                        {
                            Department = reader["DepartmentName"].ToString(),
                            Total = (int)reader["TotalEmployees"],
                            Present = reader["PresentToday"] == DBNull.Value ? 0 : (int)reader["PresentToday"],
                            EmployeeCount = (int)reader["TotalEmployees"]
                        });
                    }
                }
            }
        }

    return View(dashboard);
}

    public ActionResult About()
    {
        return View();
    }

    public ActionResult Contact()
    {
        return View();
    }

    [Route("Dashboard")]
    public ActionResult Dashboard()
    {
        return RedirectToAction("Index");
    }

    [Route("Error/{code?}")]
    public ActionResult Error(int? code)
    {
        return View("Error");
    }
}