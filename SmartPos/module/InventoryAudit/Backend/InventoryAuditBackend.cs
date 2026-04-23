using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SmartPos.Module.InventoryAudit.Models;
using SmartPos.Module.InventoryAudit.Templates;

namespace SmartPos.Module.InventoryAudit.Backend
{
    public class InventoryAuditBackend
    {
        private readonly string _connectionString;

        public InventoryAuditBackend()
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
            using (SqlCommand command = new SqlCommand(InventoryAuditSqlTemplate.EnsureSchema, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public List<InventoryCheckSummary> GetChecksByWarehouse(int warehouseId)
        {
            var result = new List<InventoryCheckSummary>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(InventoryAuditSqlTemplate.GetChecksByWarehouse, connection))
            {
                command.Parameters.AddWithValue("@WarehouseID", warehouseId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new InventoryCheckSummary
                        {
                            CheckID = Convert.ToInt32(reader["CheckID"]),
                            CheckCode = Convert.ToString(reader["CheckCode"]),
                            CheckDate = Convert.ToDateTime(reader["CheckDate"]),
                            Status = Convert.ToInt32(reader["Status"]),
                            StatusText = Convert.ToString(reader["StatusText"]),
                            CreatedByName = Convert.ToString(reader["CreatedByName"]),
                            ApprovedByName = Convert.ToString(reader["ApprovedByName"]),
                            ApprovedAt = reader["ApprovedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ApprovedAt"])
                        });
                    }
                }
            }

            return result;
        }

        public List<WarehouseOption> GetWarehouses()
        {
            var result = new List<WarehouseOption>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(InventoryAuditSqlTemplate.GetWarehouses, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new WarehouseOption
                        {
                            WarehouseID = Convert.ToInt32(reader["WarehouseID"]),
                            WarehouseName = Convert.ToString(reader["WarehouseName"])
                        });
                    }
                }
            }

            return result;
        }

        public List<StockBatchItem> GetStockByWarehouse(int warehouseId)
        {
            var result = new List<StockBatchItem>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(InventoryAuditSqlTemplate.GetStockByWarehouse, connection))
            {
                command.Parameters.AddWithValue("@WarehouseID", warehouseId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new StockBatchItem
                        {
                            ProductID = Convert.ToInt32(reader["ProductID"]),
                            ProductCode = Convert.ToString(reader["ProductCode"]),
                            ProductName = Convert.ToString(reader["ProductName"]),
                            BatchNumber = Convert.ToString(reader["BatchNumber"]),
                            ExpiryDate = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ExpiryDate"]),
                            SystemQuantity = Convert.ToDecimal(reader["SystemQuantity"])
                        });
                    }
                }
            }

            return result;
        }

        public InventoryCheckDraft CreateCheckDraft(int warehouseId, int createdByUserId, string notes)
        {
            List<StockBatchItem> stocks = GetStockByWarehouse(warehouseId);
            if (stocks.Count == 0)
            {
                throw new InvalidOperationException("Kho hiện không có tồn để kiểm kê.");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string checkCode = "KK" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        int checkId;

                        using (SqlCommand createCommand = new SqlCommand(InventoryAuditSqlTemplate.CreateInventoryCheck, connection, transaction))
                        {
                            createCommand.Parameters.AddWithValue("@CheckCode", checkCode);
                            createCommand.Parameters.AddWithValue("@WarehouseID", warehouseId);
                            createCommand.Parameters.AddWithValue("@CreatedByUserID", createdByUserId);
                            createCommand.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                            checkId = Convert.ToInt32(createCommand.ExecuteScalar());
                        }

                        for (int i = 0; i < stocks.Count; i++)
                        {
                            StockBatchItem stock = stocks[i];
                            using (SqlCommand itemCommand = new SqlCommand(InventoryAuditSqlTemplate.AddInventoryCheckItem, connection, transaction))
                            {
                                itemCommand.Parameters.AddWithValue("@CheckID", checkId);
                                itemCommand.Parameters.AddWithValue("@ProductID", stock.ProductID);
                                itemCommand.Parameters.AddWithValue("@BatchNumber", string.IsNullOrWhiteSpace(stock.BatchNumber) ? (object)DBNull.Value : stock.BatchNumber);
                                itemCommand.Parameters.AddWithValue("@ExpiryDate", stock.ExpiryDate.HasValue ? (object)stock.ExpiryDate.Value.Date : DBNull.Value);
                                itemCommand.Parameters.AddWithValue("@SystemQuantity", stock.SystemQuantity);
                                itemCommand.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        return new InventoryCheckDraft
                        {
                            CheckID = checkId,
                            CheckCode = checkCode
                        };
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<InventoryCheckItemEdit> GetCheckItems(int checkId)
        {
            var result = new List<InventoryCheckItemEdit>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(InventoryAuditSqlTemplate.GetCheckItems, connection))
            {
                command.Parameters.AddWithValue("@CheckID", checkId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new InventoryCheckItemEdit
                        {
                            CheckID = Convert.ToInt32(reader["CheckID"]),
                            ProductID = Convert.ToInt32(reader["ProductID"]),
                            ProductCode = Convert.ToString(reader["ProductCode"]),
                            ProductName = Convert.ToString(reader["ProductName"]),
                            BatchNumber = Convert.ToString(reader["BatchNumber"]),
                            ExpiryDate = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ExpiryDate"]),
                            SystemQuantity = Convert.ToDecimal(reader["SystemQuantity"]),
                            ActualQuantity = reader["ActualQuantity"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["ActualQuantity"]),
                            Reason = Convert.ToString(reader["Note"])
                        });
                    }
                }
            }

            return result;
        }

        public InventoryCheckHeader GetCheckHeader(int checkId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(InventoryAuditSqlTemplate.GetCheckHeader, connection))
            {
                command.Parameters.AddWithValue("@CheckID", checkId);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Phiếu kiểm kê không tồn tại.");
                    }

                    return new InventoryCheckHeader
                    {
                        CheckID = Convert.ToInt32(reader["CheckID"]),
                        CheckCode = Convert.ToString(reader["CheckCode"]),
                        WarehouseID = Convert.ToInt32(reader["WarehouseID"]),
                        WarehouseName = Convert.ToString(reader["WarehouseName"]),
                        CheckDate = Convert.ToDateTime(reader["CheckDate"]),
                        Status = Convert.ToInt32(reader["Status"]),
                        StatusText = Convert.ToString(reader["StatusText"]),
                        Notes = Convert.ToString(reader["Notes"]),
                        CreatedByName = Convert.ToString(reader["CreatedByName"]),
                        ApprovedByName = Convert.ToString(reader["ApprovedByName"]),
                        ApprovedAt = reader["ApprovedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ApprovedAt"])
                    };
                }
            }
        }

        public List<InventoryCheckItemHistory> GetCheckItemHistories(int checkId)
        {
            var result = new List<InventoryCheckItemHistory>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(InventoryAuditSqlTemplate.GetItemHistories, connection))
            {
                command.Parameters.AddWithValue("@CheckID", checkId);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new InventoryCheckItemHistory
                        {
                            HistoryID = Convert.ToInt64(reader["HistoryID"]),
                            CheckID = Convert.ToInt32(reader["CheckID"]),
                            ProductID = Convert.ToInt32(reader["ProductID"]),
                            ProductCode = Convert.ToString(reader["ProductCode"]),
                            ProductName = Convert.ToString(reader["ProductName"]),
                            BatchNumber = Convert.ToString(reader["BatchNumber"]),
                            OldActualQuantity = reader["OldActualQuantity"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["OldActualQuantity"]),
                            NewActualQuantity = reader["NewActualQuantity"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["NewActualQuantity"]),
                            OldReason = Convert.ToString(reader["OldReason"]),
                            NewReason = Convert.ToString(reader["NewReason"]),
                            ChangedByUserID = reader["ChangedByUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ChangedByUserID"]),
                            ChangedByName = Convert.ToString(reader["ChangedByName"]),
                            ChangedAt = Convert.ToDateTime(reader["ChangedAt"])
                        });
                    }
                }
            }

            return result;
        }

        public void SaveCheckDraftItems(int checkId, List<InventoryCheckItemEdit> items, int changedByUserId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        GetWarehouseIdForCheck(connection, transaction, checkId);

                        for (int i = 0; i < items.Count; i++)
                        {
                            InventoryCheckItemEdit item = items[i];
                            decimal actualQty = item.ActualQuantity ?? item.SystemQuantity;

                            decimal? oldActualQty;
                            string oldReason;
                            using (SqlCommand oldValueCommand = new SqlCommand(@"
SELECT TOP 1 ActualQuantity, Note
FROM dbo.InventoryCheckItems
WHERE CheckID = @CheckID
  AND ProductID = @ProductID
  AND ISNULL(BatchNumber, '') = ISNULL(@BatchNumber, '');", connection, transaction))
                            {
                                oldValueCommand.Parameters.AddWithValue("@CheckID", checkId);
                                oldValueCommand.Parameters.AddWithValue("@ProductID", item.ProductID);
                                oldValueCommand.Parameters.AddWithValue("@BatchNumber", string.IsNullOrWhiteSpace(item.BatchNumber) ? (object)DBNull.Value : item.BatchNumber);
                                using (SqlDataReader oldValueReader = oldValueCommand.ExecuteReader())
                                {
                                    if (!oldValueReader.Read())
                                    {
                                        continue;
                                    }

                                    oldActualQty = oldValueReader["ActualQuantity"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(oldValueReader["ActualQuantity"]);
                                    oldReason = Convert.ToString(oldValueReader["Note"]);
                                }
                            }

                            using (SqlCommand updateItem = new SqlCommand(InventoryAuditSqlTemplate.UpdateCheckItemActual, connection, transaction))
                            {
                                updateItem.Parameters.AddWithValue("@CheckID", checkId);
                                updateItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                updateItem.Parameters.AddWithValue("@BatchNumber", string.IsNullOrWhiteSpace(item.BatchNumber) ? (object)DBNull.Value : item.BatchNumber);
                                updateItem.Parameters.AddWithValue("@ActualQuantity", actualQty);
                                updateItem.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(item.Reason) ? (object)DBNull.Value : item.Reason);
                                updateItem.ExecuteNonQuery();
                            }

                            bool isChanged = oldActualQty != actualQty || !string.Equals(oldReason ?? string.Empty, item.Reason ?? string.Empty, StringComparison.Ordinal);
                            if (isChanged)
                            {
                                using (SqlCommand historyCommand = new SqlCommand(InventoryAuditSqlTemplate.InsertItemHistory, connection, transaction))
                                {
                                    historyCommand.Parameters.AddWithValue("@CheckID", checkId);
                                    historyCommand.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    historyCommand.Parameters.AddWithValue("@BatchNumber", string.IsNullOrWhiteSpace(item.BatchNumber) ? (object)DBNull.Value : item.BatchNumber);
                                    historyCommand.Parameters.AddWithValue("@OldActualQuantity", oldActualQty.HasValue ? (object)oldActualQty.Value : DBNull.Value);
                                    historyCommand.Parameters.AddWithValue("@NewActualQuantity", actualQty);
                                    historyCommand.Parameters.AddWithValue("@OldReason", string.IsNullOrWhiteSpace(oldReason) ? (object)DBNull.Value : oldReason);
                                    historyCommand.Parameters.AddWithValue("@NewReason", string.IsNullOrWhiteSpace(item.Reason) ? (object)DBNull.Value : item.Reason);
                                    historyCommand.Parameters.AddWithValue("@ChangedByUserID", changedByUserId > 0 ? (object)changedByUserId : DBNull.Value);
                                    historyCommand.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void ApproveCheck(ApproveInventoryCheckRequest request)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int warehouseId = GetWarehouseIdForCheck(connection, transaction, request.CheckID);

                        for (int i = 0; i < request.Items.Count; i++)
                        {
                            InventoryCheckItemEdit item = request.Items[i];
                            decimal actualQty = item.ActualQuantity ?? item.SystemQuantity;
                            decimal diff = actualQty - item.SystemQuantity;

                            using (SqlCommand updateItem = new SqlCommand(InventoryAuditSqlTemplate.UpdateCheckItemActual, connection, transaction))
                            {
                                updateItem.Parameters.AddWithValue("@CheckID", request.CheckID);
                                updateItem.Parameters.AddWithValue("@ProductID", item.ProductID);
                                updateItem.Parameters.AddWithValue("@BatchNumber", string.IsNullOrWhiteSpace(item.BatchNumber) ? (object)DBNull.Value : item.BatchNumber);
                                updateItem.Parameters.AddWithValue("@ActualQuantity", actualQty);
                                updateItem.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(item.Reason) ? (object)DBNull.Value : item.Reason);
                                updateItem.ExecuteNonQuery();
                            }

                            using (SqlCommand updateInventory = new SqlCommand(InventoryAuditSqlTemplate.UpdateInventoryByBatch, connection, transaction))
                            {
                                updateInventory.Parameters.AddWithValue("@WarehouseID", warehouseId);
                                updateInventory.Parameters.AddWithValue("@ProductID", item.ProductID);
                                updateInventory.Parameters.AddWithValue("@BatchNumber", string.IsNullOrWhiteSpace(item.BatchNumber) ? (object)DBNull.Value : item.BatchNumber);
                                updateInventory.Parameters.AddWithValue("@ActualQuantity", actualQty);
                                updateInventory.ExecuteNonQuery();
                            }

                            if (diff != 0)
                            {
                                using (SqlCommand stockTx = new SqlCommand(InventoryAuditSqlTemplate.InsertAdjustmentTx, connection, transaction))
                                {
                                    stockTx.Parameters.AddWithValue("@WarehouseID", warehouseId);
                                    stockTx.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    stockTx.Parameters.AddWithValue("@QuantityDiff", diff);
                                    stockTx.Parameters.AddWithValue("@BatchNumber", string.IsNullOrWhiteSpace(item.BatchNumber) ? (object)DBNull.Value : item.BatchNumber);
                                    stockTx.Parameters.AddWithValue("@ExpiryDate", item.ExpiryDate.HasValue ? (object)item.ExpiryDate.Value.Date : DBNull.Value);
                                    stockTx.Parameters.AddWithValue("@ReferenceID", request.CheckID);
                                    stockTx.Parameters.AddWithValue("@Note", string.IsNullOrWhiteSpace(item.Reason) ? (object)"Kiem ke dieu chinh" : item.Reason);
                                    stockTx.Parameters.AddWithValue("@CreatedByUserID", request.ApprovedByUserID);
                                    stockTx.ExecuteNonQuery();
                                }
                            }
                        }

                        using (SqlCommand approve = new SqlCommand(InventoryAuditSqlTemplate.ApproveCheck, connection, transaction))
                        {
                            approve.Parameters.AddWithValue("@CheckID", request.CheckID);
                            approve.Parameters.AddWithValue("@ApprovedByUserID", request.ApprovedByUserID);
                            approve.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static int GetWarehouseIdForCheck(SqlConnection connection, SqlTransaction transaction, int checkId)
        {
            using (SqlCommand command = new SqlCommand(InventoryAuditSqlTemplate.GetCheckHeader, connection, transaction))
            {
                command.Parameters.AddWithValue("@CheckID", checkId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Phiếu kiểm kê không tồn tại.");
                    }

                    int status = Convert.ToInt32(reader["Status"]);
                    if (status != 1)
                    {
                        throw new InvalidOperationException("Phiếu kiểm kê đã được duyệt hoặc hủy, không thể cập nhật lại.");
                    }

                    return Convert.ToInt32(reader["WarehouseID"]);
                }
            }
        }
    }
}
