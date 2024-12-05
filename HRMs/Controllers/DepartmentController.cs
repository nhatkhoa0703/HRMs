using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using HRMs.Models;

namespace HRMs.Controllers
{
    public class DepartmentController : Controller
    {
        private string connectionString = "Data Source=(local);Initial Catalog=HRMsDB;Integrated Security=True";

        // GET: Department
        public ActionResult Index()
        {
            List<Department> departments = new List<Department>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Department";
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
                                DepartmentName = reader["DepartmentName"].ToString(),
                                Description = reader["Description"].ToString()
                            });
                        }
                    }
                }
            }
            return View(departments);
        }

        // GET: Department/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Department department)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Department (DepartmentName, Description) VALUES (@Name, @Description)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", department.DepartmentName);
                        cmd.Parameters.AddWithValue("@Description", department.Description ?? (object)DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(department);
        }

        // GET: Department/Edit/5
        public ActionResult Edit(int id)
        {
            Department department = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Department WHERE DepartmentID = @ID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            department = new Department
                            {
                                DepartmentID = (int)reader["DepartmentID"],
                                DepartmentName = reader["DepartmentName"].ToString(),
                                Description = reader["Description"].ToString()
                            };
                        }
                    }
                }
            }
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Department department)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Department SET DepartmentName = @Name, Description = @Description WHERE DepartmentID = @ID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@Name", department.DepartmentName);
                        cmd.Parameters.AddWithValue("@Description", department.Description ?? (object)DBNull.Value);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            return View(department);
        }

        // GET: Department/Delete/5
        public ActionResult Delete(int id)
        {
            Department department = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Department WHERE DepartmentID = @ID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            department = new Department
                            {
                                DepartmentID = (int)reader["DepartmentID"],
                                DepartmentName = reader["DepartmentName"].ToString(),
                                Description = reader["Description"].ToString()
                            };
                        }
                    }
                }
            }
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Department/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Department WHERE DepartmentID = @ID";
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