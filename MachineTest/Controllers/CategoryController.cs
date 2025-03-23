using MachineTest.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

public class CategoryController : Controller
{
    private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

    public ActionResult Index()
    {
        List<Category> categories = new List<Category>();
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Category";
            SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            categories = dt.AsEnumerable().Select(row => new Category
            {
                CategoryId = Convert.ToInt32(row["CategoryId"]),
                CategoryName = row["CategoryName"].ToString()
            }).ToList();
        }
        return View(categories);
    }

    public ActionResult Create() => View();

    [HttpPost]
    public ActionResult Create(Category category)
    {
        if (ModelState.IsValid)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Category (CategoryName) VALUES (@CategoryName)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
        return View(category);
    }

    public ActionResult Edit(int id)
    {
        Category category = new Category();
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Category WHERE CategoryId = @CategoryId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@CategoryId", id);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                category.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                category.CategoryName = reader["CategoryName"].ToString();
            }
        }
        return View(category);
    }

    [HttpPost]
    public ActionResult Edit(Category category)
    {
        if (ModelState.IsValid)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Category SET CategoryName = @CategoryName WHERE CategoryId = @CategoryId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                cmd.Parameters.AddWithValue("@CategoryId", category.CategoryId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
        return View(category);
    }

    public ActionResult Delete(int id)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

          
            string checkQuery = "SELECT COUNT(*) FROM Product WHERE CategoryId = @CategoryId";
            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@CategoryId", id);
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    TempData["ErrorMessage"] = "Cannot delete this category because products are associated with it.";
                    return RedirectToAction("Index");
                }
            }

         
            string deleteQuery = "DELETE FROM Category WHERE CategoryId = @CategoryId";
            using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
            {
                cmd.Parameters.AddWithValue("@CategoryId", id);
                cmd.ExecuteNonQuery();
            }
        }
        return RedirectToAction("Index");
    }

}
