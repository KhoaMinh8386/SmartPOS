using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.Products.Models;
using SmartPos.Module.Products.Templates;

namespace SmartPos.Module.Products.Backend
{
    public class ProductBackend
    {
        private readonly string _connectionString;

        public ProductBackend()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Missing SmartPosDb connection string in App.config.");
            }
        }

        public List<CategoryListItem> GetCategories()
        {
            var result = new List<CategoryListItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ProductSqlTemplate.GetCategories, conn))
            {
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new CategoryListItem
                        {
                            CategoryID = (int)rdr["CategoryID"],
                            CategoryName = rdr["CategoryName"].ToString(),
                            Description = rdr["Description"]?.ToString(),
                            IsActive = (bool)rdr["IsActive"]
                        });
                    }
                }
            }
            return result;
        }

        public List<ProductListItem> GetProducts(string search, int categoryId)
        {
            var result = new List<ProductListItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ProductSqlTemplate.GetProducts, conn))
            {
                cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : "%" + search + "%");
                cmd.Parameters.AddWithValue("@CategoryID", categoryId);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new ProductListItem
                        {
                            ProductID = (int)rdr["ProductID"],
                            ProductCode = rdr["ProductCode"].ToString(),
                            ProductName = rdr["ProductName"].ToString(),
                            CategoryName = rdr["CategoryName"].ToString(),
                            RetailPrice = (decimal)rdr["RetailPrice"],
                            Location = rdr["Location"]?.ToString(),
                            StockQuantity = (decimal)rdr["StockQuantity"],
                            IsActive = (bool)rdr["IsActive"]
                        });
                    }
                }
            }
            return result;
        }

        public ProductDetail GetProductDetail(int productId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ProductSqlTemplate.GetProductDetail, conn))
            {
                cmd.Parameters.AddWithValue("@ProductID", productId);
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new ProductDetail
                        {
                            ProductID = (int)rdr["ProductID"],
                            CategoryID = (int)rdr["CategoryID"],
                            SupplierID = rdr["SupplierID"] == DBNull.Value ? (int?)null : (int)rdr["SupplierID"],
                            BaseUnitID = (int)rdr["BaseUnitID"],
                            ProductCode = rdr["ProductCode"].ToString(),
                            Barcode = rdr["Barcode"]?.ToString(),
                            ProductName = rdr["ProductName"].ToString(),
                            Description = rdr["Description"]?.ToString(),
                            ImageUrl = rdr["ImageUrl"]?.ToString(),
                            CostPrice = (decimal)rdr["CostPrice"],
                            RetailPrice = (decimal)rdr["RetailPrice"],
                            WholesalePrice = rdr["WholesalePrice"] == DBNull.Value ? (decimal?)null : (decimal)rdr["WholesalePrice"],
                            Weight = rdr["Weight"] == DBNull.Value ? (decimal?)null : (decimal)rdr["Weight"],
                            Location = rdr["Location"]?.ToString(),
                            IsActive = (bool)rdr["IsActive"],
                            HasExpiry = (bool)rdr["HasExpiry"]
                        };
                    }
                }
            }
            return null;
        }

        public void SaveProduct(ProductDetail product)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = product.ProductID == 0 ? ProductSqlTemplate.InsertProduct : ProductSqlTemplate.UpdateProduct;
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (product.ProductID > 0) cmd.Parameters.AddWithValue("@ProductID", product.ProductID);
                    cmd.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                    cmd.Parameters.AddWithValue("@SupplierID", (object)product.SupplierID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BaseUnitID", product.BaseUnitID);
                    cmd.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    cmd.Parameters.AddWithValue("@Barcode", (object)product.Barcode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@Description", (object)product.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ImageUrl", (object)product.ImageUrl ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CostPrice", product.CostPrice);
                    cmd.Parameters.AddWithValue("@RetailPrice", product.RetailPrice);
                    cmd.Parameters.AddWithValue("@WholesalePrice", (object)product.WholesalePrice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Weight", (object)product.Weight ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Location", (object)product.Location ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", product.IsActive);
                    cmd.Parameters.AddWithValue("@HasExpiry", product.HasExpiry);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProduct(int productId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ProductSqlTemplate.DeleteProduct, conn))
            {
                cmd.Parameters.AddWithValue("@ProductID", productId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveCategory(CategoryListItem category)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = category.CategoryID == 0 ? ProductSqlTemplate.InsertCategory : ProductSqlTemplate.UpdateCategory;
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (category.CategoryID > 0) cmd.Parameters.AddWithValue("@CategoryID", category.CategoryID);
                    cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    cmd.Parameters.AddWithValue("@Description", (object)category.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", category.IsActive);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<SupplierLookupItem> GetSuppliers()
        {
            var result = new List<SupplierLookupItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ProductSqlTemplate.GetSuppliersLookup, conn))
            {
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new SupplierLookupItem { SupplierID = (int)rdr["SupplierID"], SupplierName = rdr["SupplierName"].ToString() });
                    }
                }
            }
            return result;
        }

        public List<UnitLookupItem> GetUnits()
        {
            var result = new List<UnitLookupItem>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(ProductSqlTemplate.GetUnitsLookup, conn))
            {
                conn.Open();
                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(new UnitLookupItem { UnitID = (int)rdr["UnitID"], UnitName = rdr["UnitName"].ToString() });
                    }
                }
            }
            return result;
        }
    }
}
