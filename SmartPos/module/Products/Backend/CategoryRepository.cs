using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.Products.Models;
using SmartPos.Module.Products.Templates;

namespace SmartPos.Module.Products.Backend
{
    public class CategoryRepository
    {
        private readonly string _connectionString;

        public CategoryRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(ProductSqlTemplate.EnsureCategorySchema, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<CategoryDTO> GetAll()
        {
            var list = new List<CategoryDTO>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(ProductSqlTemplate.GetCategories, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CategoryDTO
                            {
                                CategoryID = (int)reader["CategoryID"],
                                CategoryName = reader["CategoryName"].ToString(),
                                ParentID = reader["ParentID"] as int?,
                                Description = reader["Description"]?.ToString(),
                                IsActive = (bool)reader["IsActive"],
                                ParentName = reader["ParentName"]?.ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }

        public bool Save(CategoryDTO dto)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = dto.CategoryID == 0 ? ProductSqlTemplate.InsertCategory : ProductSqlTemplate.UpdateCategory;
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (dto.CategoryID > 0) cmd.Parameters.AddWithValue("@CategoryID", dto.CategoryID);
                    cmd.Parameters.AddWithValue("@CategoryName", dto.CategoryName);
                    cmd.Parameters.AddWithValue("@ParentID", (object)dto.ParentID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)dto.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", dto.IsActive);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool HasProducts(int categoryId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(ProductSqlTemplate.HasProducts, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    return cmd.ExecuteScalar() != null;
                }
            }
        }

        public bool Delete(int categoryId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(ProductSqlTemplate.DeleteCategory, conn))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
