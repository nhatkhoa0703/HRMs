using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using HRMs.Models;

namespace HRMs.Controllers
{
    public class PositionController : Controller
    {
        private string connectionString = "Data Source=.;Initial Catalog=HRMsDB;Integrated Security=True";

        // GET: Position
        public ActionResult Index()
        {
            List<Position> positions = new List<Position>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT p.*, 
                           (SELECT COUNT(*) FROM Employee e WHERE e.PositionID = p.PositionID) as EmployeeCount 
                    FROM Position p";

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
                                PositionName = reader["PositionName"].ToString(),
                                Description = reader["Description"].ToString(),
                                BasicSalary = (decimal)reader["BasicSalary"],
                                EmployeeCount = (int)reader["EmployeeCount"]
                            });
                        }
                    }
                }
            }
            return View(positions);
        }

        // GET: Position/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Position/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Position position)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO Position (PositionName, Description, BasicSalary) 
                                   VALUES (@Name, @Description, @Salary)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", position.PositionName);
                        cmd.Parameters.AddWithValue("@Description", position.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Salary", position.BasicSalary);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(position);
        }

        // GET: Position/Edit/5
        public ActionResult Edit(int id)
        {
            Position position = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Position WHERE PositionID = @ID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            position = new Position
                            {
                                PositionID = (int)reader["PositionID"],
                                PositionName = reader["PositionName"].ToString(),
                                Description = reader["Description"].ToString(),
                                BasicSalary = (decimal)reader["BasicSalary"]
                            };
                        }
                    }
                }
            }

            if (position == null)
            {
                return HttpNotFound();
            }
            return View(position);
        }

        // POST: Position/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Position position)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE Position 
                                   SET PositionName = @Name, 
                                       Description = @Description, 
                                       BasicSalary = @Salary 
                                   WHERE PositionID = @ID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", position.PositionID);
                        cmd.Parameters.AddWithValue("@Name", position.PositionName);
                        cmd.Parameters.AddWithValue("@Description", position.Description ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Salary", position.BasicSalary);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(position);
        }

        // GET: Position/Delete/5
        public ActionResult Delete(int id)
        {
            Position position = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT p.*, 
                               (SELECT COUNT(*) FROM Employee e WHERE e.PositionID = p.PositionID) as EmployeeCount 
                               FROM Position p WHERE p.PositionID = @ID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            position = new Position
                            {
                                PositionID = (int)reader["PositionID"],
                                PositionName = reader["PositionName"].ToString(),
                                Description = reader["Description"].ToString(),
                                BasicSalary = (decimal)reader["BasicSalary"],
                                EmployeeCount = (int)reader["EmployeeCount"]
                            };
                        }
                    }
                }
            }

            if (position == null)
            {
                return HttpNotFound();
            }

            if (position.EmployeeCount > 0)
            {
                TempData["ErrorMessage"] = "Cannot delete this position as it has employees assigned to it.";
                return RedirectToAction("Index");
            }

            return View(position);
        }

        // POST: Position/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Position WHERE PositionID = @ID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }
    }
}