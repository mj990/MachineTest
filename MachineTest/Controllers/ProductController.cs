using MachineTest.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace MachineTest.Controllers
{
    public class ProductController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

       
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            List<Product> products = new List<Product>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT p.ProductId, p.ProductName, p.CategoryId, c.CategoryName 
                                 FROM Product p 
                                 INNER JOIN Category c ON p.CategoryId = c.CategoryId";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                products = dt.AsEnumerable().Select(row => new Product
                {
                    ProductId = Convert.ToInt32(row["ProductId"]),
                    ProductName = row["ProductName"].ToString(),
                    CategoryId = row["CategoryId"] != DBNull.Value ? Convert.ToInt32(row["CategoryId"]) : (int?)null,
                    CategoryName = row["CategoryName"].ToString()
                }).ToList();

                int totalRecords = GetTotalProductCount();

                ViewBag.TotalRecords = totalRecords;
                ViewBag.PageSize = pageSize;
                ViewBag.PageNumber = page;
            }

            return View(products);
        }
        public ActionResult Create()
        {
            var categories = GetCategories();
            if (categories == null || !categories.Any())
            {
                ViewBag.ErrorMessage = "No categories found. Please add categories first.";
            }
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        public ActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = GetCategories();
                return View(product);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Product (ProductName, CategoryId) VALUES (@ProductName, @CategoryId)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error: " + ex.Message;
                ViewBag.Categories = GetCategories();
                return View(product);
            }
        }

      
        public ActionResult Edit(int id)
        {
            Product product = GetProductById(id);
            ViewBag.Categories = GetCategories();
            return View(product);
        }

        
        [HttpPost]
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Product SET ProductName = @ProductName, CategoryId = @CategoryId WHERE ProductId = @ProductId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    cmd.Parameters.AddWithValue("@ProductId", product.ProductId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                return RedirectToAction("Index");
            }

            ViewBag.Categories = GetCategories();
            return View(product);
        }

        
        public ActionResult Delete(int id)
        {
            Product product = GetProductById(id);
            return View(product);
        }

      
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM Product WHERE ProductId = @ProductId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProductId", id);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return RedirectToAction("Index");
        }

        private Product GetProductById(int id)
        {
            Product product = new Product();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT ProductId, ProductName, CategoryId FROM Product WHERE ProductId = @ProductId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProductId", id);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    product.ProductId = Convert.ToInt32(reader["ProductId"]);
                    product.ProductName = reader["ProductName"].ToString();
                    product.CategoryId = Convert.ToInt32(reader["CategoryId"]);
                }

                reader.Close();
            }
            return product;
        }

        private List<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT CategoryId, CategoryName FROM Category";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                categories = dt.AsEnumerable().Select(row => new Category
                {
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = row["CategoryName"].ToString()
                }).ToList();
            }

            return categories;
        }

        public int GetTotalProductCount()
        {
            int totalCount = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Product";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    totalCount = (int)cmd.ExecuteScalar();
                }
            }
            return totalCount;
        }

    }
}
