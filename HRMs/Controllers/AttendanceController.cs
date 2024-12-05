using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Mvc;
using ClosedXML.Excel;
using HRMs.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace HRMs.Controllers
{
    public class AttendanceController : Controller
    {
        private string connectionString = "Data Source=(local);Initial Catalog=HRMsDB;Integrated Security=True";

        // GET: Attendance
        public ActionResult Index()
        {
            List<Attendance> attendances = new List<Attendance>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT a.*, e.FirstName + ' ' + e.LastName as EmployeeName 
                    FROM Attendance a
                    JOIN Employee e ON a.EmployeeID = e.EmployeeID
                    ORDER BY a.CheckIn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            attendances.Add(new Attendance
                            {
                                AttendanceID = (int)reader["AttendanceID"],
                                EmployeeID = (int)reader["EmployeeID"],
                                CheckIn = Convert.ToDateTime(reader["CheckIn"]),
                                CheckOut = reader["CheckOut"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["CheckOut"])
                                    : (DateTime?)null,
                                EmployeeName = reader["EmployeeName"].ToString()
                            });
                        }
                    }
                }
            }
            ViewBag.Employees = GetEmployeeList();
            return View(attendances);
        }

        // POST: Attendance/CheckIn
        [HttpPost]
        public ActionResult CheckIn(int employeeId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // First check if employee already checked in today
                string checkQuery = @"
                    SELECT COUNT(*) FROM Attendance 
                    WHERE EmployeeID = @EmployeeID 
                    AND CAST(CheckIn AS DATE) = CAST(GETDATE() AS DATE)
                    AND CheckOut IS NULL";

                conn.Open();
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                    int existingCheckins = (int)checkCmd.ExecuteScalar();

                    if (existingCheckins > 0)
                    {
                        return Json(new { success = false, message = "Already checked in today" });
                    }
                }

                // If no existing check-in, create new attendance record
                string insertQuery = "INSERT INTO Attendance (EmployeeID, CheckIn) VALUES (@EmployeeID, @CheckIn)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                    cmd.Parameters.AddWithValue("@CheckIn", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
            return Json(new { success = true });
        }

        // POST: Attendance/CheckOut
        [HttpPost]
        public ActionResult CheckOut(int employeeId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    UPDATE Attendance 
                    SET CheckOut = @CheckOut 
                    WHERE EmployeeID = @EmployeeID 
                    AND CAST(CheckIn AS DATE) = CAST(GETDATE() AS DATE)
                    AND CheckOut IS NULL";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                    cmd.Parameters.AddWithValue("@CheckOut", DateTime.Now);

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return Json(new { success = false, message = "No active check-in found" });
                    }
                }
            }
            return Json(new { success = true });
        }

        // GET: Attendance/Report
        public ActionResult Report(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
                startDate = DateTime.Today.AddDays(-30);
            if (!endDate.HasValue)
                endDate = DateTime.Today;

            List<AttendanceReport> report = new List<AttendanceReport>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        e.FirstName + ' ' + e.LastName as EmployeeName,
                        a.CheckIn,
                        a.CheckOut,
                        DATEDIFF(MINUTE, a.CheckIn, ISNULL(a.CheckOut, GETDATE())) as MinutesWorked
                    FROM Attendance a
                    JOIN Employee e ON a.EmployeeID = e.EmployeeID
                    WHERE CAST(a.CheckIn AS DATE) BETWEEN @StartDate AND @EndDate
                    ORDER BY a.CheckIn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            report.Add(new AttendanceReport
                            {
                                EmployeeName = reader["EmployeeName"].ToString(),
                                CheckIn = Convert.ToDateTime(reader["CheckIn"]),
                                CheckOut = reader["CheckOut"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["CheckOut"])
                                    : (DateTime?)null,
                                MinutesWorked = (int)reader["MinutesWorked"]
                            });
                        }
                    }
                }
            }

            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            return View(report);
        }

        public FileResult ExportToExcel(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue) startDate = DateTime.Today.AddDays(-30);
            if (!endDate.HasValue) endDate = DateTime.Today;

            List<AttendanceReport> report = GetAttendanceData(startDate.Value, endDate.Value);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Attendance Report");

                // Add headers
                worksheet.Cell(1, 1).Value = "Employee Name";
                worksheet.Cell(1, 2).Value = "Date";
                worksheet.Cell(1, 3).Value = "Check In";
                worksheet.Cell(1, 4).Value = "Check Out";
                worksheet.Cell(1, 5).Value = "Work Duration";

                // Style the header
                var headerRange = worksheet.Range("A1:E1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Add data
                int row = 2;
                foreach (var item in report)
                {
                    worksheet.Cell(row, 1).Value = item.EmployeeName;
                    worksheet.Cell(row, 2).Value = item.CheckIn.ToShortDateString();
                    worksheet.Cell(row, 3).Value = item.CheckIn.ToString("HH:mm");
                    worksheet.Cell(row, 4).Value = item.CheckOut?.ToString("HH:mm") ?? "Not checked out";
                    worksheet.Cell(row, 5).Value = item.WorkDuration;
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"AttendanceReport_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.xlsx");
                }
            }
        }

        public FileResult ExportToPDF(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue) startDate = DateTime.Today.AddDays(-30);
            if (!endDate.HasValue) endDate = DateTime.Today;

            List<AttendanceReport> report = GetAttendanceData(startDate.Value, endDate.Value);

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Attendance Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Add date range
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                Paragraph dateRange = new Paragraph(
                    $"Period: {startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy}", normalFont);
                dateRange.SpacingAfter = 20;
                document.Add(dateRange);

                // Create table
                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 2f, 2f, 2f, 2f });

                // Add headers
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                string[] headers = { "Employee Name", "Date", "Check In", "Check Out", "Duration" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                // Add data
                foreach (var item in report)
                {
                    table.AddCell(new Phrase(item.EmployeeName, normalFont));
                    table.AddCell(new Phrase(item.CheckIn.ToShortDateString(), normalFont));
                    table.AddCell(new Phrase(item.CheckIn.ToString("HH:mm"), normalFont));
                    table.AddCell(new Phrase(item.CheckOut?.ToString("HH:mm") ?? "Not checked out", normalFont));
                    table.AddCell(new Phrase(item.WorkDuration, normalFont));
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf",
                    $"AttendanceReport_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf");
            }
        }

        // Helper method to get attendance data
        private List<AttendanceReport> GetAttendanceData(DateTime startDate, DateTime endDate)
        {
            List<AttendanceReport> report = new List<AttendanceReport>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT 
                e.FirstName + ' ' + e.LastName as EmployeeName,
                a.CheckIn,
                a.CheckOut,
                DATEDIFF(MINUTE, a.CheckIn, ISNULL(a.CheckOut, GETDATE())) as MinutesWorked
            FROM Attendance a
            JOIN Employee e ON a.EmployeeID = e.EmployeeID
            WHERE CAST(a.CheckIn AS DATE) BETWEEN @StartDate AND @EndDate
            ORDER BY a.CheckIn DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            report.Add(new AttendanceReport
                            {
                                EmployeeName = reader["EmployeeName"].ToString(),
                                CheckIn = Convert.ToDateTime(reader["CheckIn"]),
                                CheckOut = reader["CheckOut"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["CheckOut"])
                                    : (DateTime?)null,
                                MinutesWorked = (int)reader["MinutesWorked"]
                            });
                        }
                    }
                }
            }
            return report;
        }

        public ActionResult Dashboard()
        {
            var dashboardData = new Dictionary<string, object>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get all summary data in one query
                string summaryQuery = @"
            SELECT 
                (SELECT COUNT(*) FROM Employee WHERE Status = 1) as TotalEmployees,
                (SELECT COUNT(DISTINCT EmployeeID) 
                 FROM Attendance 
                 WHERE CAST(CheckIn AS DATE) = CAST(GETDATE() AS DATE)) as TodayPresent,
                (SELECT COUNT(DISTINCT EmployeeID)
                 FROM Attendance
                 WHERE CAST(CheckIn AS DATE) = CAST(GETDATE() AS DATE)
                 AND DATEPART(HOUR, CheckIn) >= 9) as LateToday,
                (SELECT COUNT(*) FROM Department) as TotalDepartments,
                (SELECT COUNT(*) FROM Leave WHERE Status = 'Pending') as PendingLeaves";

                using (SqlCommand cmd = new SqlCommand(summaryQuery, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dashboardData["TotalEmployees"] = (int)reader["TotalEmployees"];
                            dashboardData["TodayPresent"] = (int)reader["TodayPresent"];
                            dashboardData["LateToday"] = (int)reader["LateToday"];
                            dashboardData["TotalDepartments"] = (int)reader["TotalDepartments"];
                            dashboardData["PendingLeaves"] = (int)reader["PendingLeaves"];
                        }
                    }
                }

                // Weekly trend data
                string trendQuery = @"
            SELECT 
                CAST(a.CheckIn AS DATE) as AttendanceDate,
                COUNT(DISTINCT a.EmployeeID) as EmployeeCount
            FROM Attendance a
            WHERE a.CheckIn >= DATEADD(DAY, -7, GETDATE())
            GROUP BY CAST(a.CheckIn AS DATE)
            ORDER BY AttendanceDate";

                using (SqlCommand cmd = new SqlCommand(trendQuery, conn))
                {
                    var trend = new List<object>();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            trend.Add(new
                            {
                                Date = Convert.ToDateTime(reader["AttendanceDate"]).ToString("MM/dd"),
                                Count = reader["EmployeeCount"]
                            });
                        }
                    }
                    dashboardData["WeeklyTrend"] = trend;
                }

                // Department stats
                string deptQuery = @"
            SELECT 
                d.DepartmentName,
                (SELECT COUNT(*) FROM Employee e WHERE e.DepartmentID = d.DepartmentID AND e.Status = 1) as Total,
                (SELECT COUNT(DISTINCT a.EmployeeID) 
                 FROM Attendance a 
                 JOIN Employee e ON a.EmployeeID = e.EmployeeID 
                 WHERE e.DepartmentID = d.DepartmentID 
                 AND CAST(a.CheckIn AS DATE) = CAST(GETDATE() AS DATE)) as Present
            FROM Department d";

                using (SqlCommand cmd = new SqlCommand(deptQuery, conn))
                {
                    var deptStats = new List<DepartmentAttendance>();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            deptStats.Add(new DepartmentAttendance
                            {
                                Department = reader["DepartmentName"].ToString(),
                                Total = (int)reader["Total"],
                                Present = (int)reader["Present"]
                            });
                        }
                    }
                    dashboardData["DepartmentStats"] = deptStats;
                }
            }
            return View(dashboardData);
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