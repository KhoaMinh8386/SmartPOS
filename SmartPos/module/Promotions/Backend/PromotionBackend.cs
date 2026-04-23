using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.Promotions.Models;
using SmartPos.Module.Promotions.Templates;

namespace SmartPos.Module.Promotions.Backend
{
    public class PromotionBackend
    {
        private readonly string _connectionString;

        public PromotionBackend()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["SmartPosDb"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Thiếu chuỗi kết nối SmartPosDb trong App.config.");
            }
        }

        public void EnsureSchema()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PromotionSqlTemplate.EnsureSchema, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public PromotionDataBundle LoadData()
        {
            return new PromotionDataBundle
            {
                Products = GetProducts(),
                Vouchers = GetVouchers(),
                ProductSales = GetProductSales()
            };
        }

        public void SaveVoucher(VoucherItem voucher)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(voucher.VoucherID > 0 ? PromotionSqlTemplate.UpdateVoucher : PromotionSqlTemplate.InsertVoucher, connection))
            {
                if (voucher.VoucherID > 0)
                {
                    command.Parameters.AddWithValue("@VoucherID", voucher.VoucherID);
                }
                else
                {
                    command.Parameters.AddWithValue("@VoucherCode", voucher.VoucherCode);
                }

                command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(voucher.Description) ? (object)DBNull.Value : voucher.Description);
                command.Parameters.AddWithValue("@DiscountType", voucher.DiscountType);
                command.Parameters.AddWithValue("@DiscountValue", voucher.DiscountValue);
                command.Parameters.AddWithValue("@MinOrderValue", voucher.MinOrderValue);
                command.Parameters.AddWithValue("@MaxDiscount", voucher.MaxDiscount.HasValue ? (object)voucher.MaxDiscount.Value : DBNull.Value);
                command.Parameters.AddWithValue("@StartDate", voucher.StartDate);
                command.Parameters.AddWithValue("@EndDate", voucher.EndDate);
                command.Parameters.AddWithValue("@AllowStackDiscount", voucher.AllowStackDiscount);
                command.Parameters.AddWithValue("@Priority", voucher.Priority);
                command.Parameters.AddWithValue("@IsActive", voucher.IsActive);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void SaveProductSale(ProductSaleItem sale)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(sale.SaleID > 0 ? PromotionSqlTemplate.UpdateProductSale : PromotionSqlTemplate.InsertProductSale, connection))
            {
                if (sale.SaleID > 0)
                {
                    command.Parameters.AddWithValue("@SaleID", sale.SaleID);
                }

                command.Parameters.AddWithValue("@ProductID", sale.ProductID);
                command.Parameters.AddWithValue("@SaleName", sale.SaleName);
                command.Parameters.AddWithValue("@DiscountType", sale.DiscountType);
                command.Parameters.AddWithValue("@DiscountValue", sale.DiscountValue);
                command.Parameters.AddWithValue("@SalePrice", sale.SalePrice.HasValue ? (object)sale.SalePrice.Value : DBNull.Value);
                command.Parameters.AddWithValue("@StartDate", sale.StartDate);
                command.Parameters.AddWithValue("@EndDate", sale.EndDate);
                command.Parameters.AddWithValue("@AllowStackVoucher", sale.AllowStackVoucher);
                command.Parameters.AddWithValue("@Priority", sale.Priority);
                command.Parameters.AddWithValue("@IsActive", sale.IsActive);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private List<ProductOption> GetProducts()
        {
            var result = new List<ProductOption>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PromotionSqlTemplate.GetProducts, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ProductOption
                        {
                            ProductID = Convert.ToInt32(reader["ProductID"]),
                            ProductCode = Convert.ToString(reader["ProductCode"]),
                            ProductName = Convert.ToString(reader["ProductName"])
                        });
                    }
                }
            }
            return result;
        }

        private List<VoucherItem> GetVouchers()
        {
            var result = new List<VoucherItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PromotionSqlTemplate.GetVouchers, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new VoucherItem
                        {
                            VoucherID = Convert.ToInt32(reader["VoucherID"]),
                            VoucherCode = Convert.ToString(reader["VoucherCode"]),
                            Description = Convert.ToString(reader["Description"]),
                            DiscountType = Convert.ToByte(reader["DiscountType"]),
                            DiscountValue = Convert.ToDecimal(reader["DiscountValue"]),
                            MinOrderValue = Convert.ToDecimal(reader["MinOrderValue"]),
                            MaxDiscount = reader["MaxDiscount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["MaxDiscount"]),
                            AllowStackDiscount = Convert.ToBoolean(reader["AllowStackDiscount"]),
                            Priority = Convert.ToInt32(reader["Priority"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            StartDate = Convert.ToDateTime(reader["StartDate"]),
                            EndDate = Convert.ToDateTime(reader["EndDate"])
                        });
                    }
                }
            }
            return result;
        }

        private List<ProductSaleItem> GetProductSales()
        {
            var result = new List<ProductSaleItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PromotionSqlTemplate.GetProductSales, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ProductSaleItem
                        {
                            SaleID = Convert.ToInt32(reader["SaleID"]),
                            ProductID = Convert.ToInt32(reader["ProductID"]),
                            ProductCode = Convert.ToString(reader["ProductCode"]),
                            ProductName = Convert.ToString(reader["ProductName"]),
                            SaleName = Convert.ToString(reader["SaleName"]),
                            DiscountType = Convert.ToByte(reader["DiscountType"]),
                            DiscountValue = Convert.ToDecimal(reader["DiscountValue"]),
                            SalePrice = reader["SalePrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["SalePrice"]),
                            AllowStackVoucher = Convert.ToBoolean(reader["AllowStackVoucher"]),
                            Priority = Convert.ToInt32(reader["Priority"]),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            StartDate = Convert.ToDateTime(reader["StartDate"]),
                            EndDate = Convert.ToDateTime(reader["EndDate"])
                        });
                    }
                }
            }
            return result;
        }

        public bool IsVoucherCodeDuplicate(string voucherCode, int excludeId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PromotionSqlTemplate.CheckVoucherCodeExists, connection))
            {
                command.Parameters.AddWithValue("@VoucherCode", voucherCode);
                command.Parameters.AddWithValue("@VoucherID", excludeId);
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public void DeleteVoucher(int voucherId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PromotionSqlTemplate.DeleteVoucher, connection))
            {
                command.Parameters.AddWithValue("@VoucherID", voucherId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void DeleteProductSale(int saleId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(PromotionSqlTemplate.DeleteProductSale, connection))
            {
                command.Parameters.AddWithValue("@SaleID", saleId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
