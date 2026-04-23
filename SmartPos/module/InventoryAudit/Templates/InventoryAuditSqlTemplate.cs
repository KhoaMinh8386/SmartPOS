namespace SmartPos.Module.InventoryAudit.Templates
{
    internal static class InventoryAuditSqlTemplate
    {
        public const string EnsureSchema = @"
IF OBJECT_ID('dbo.InventoryCheckItemHistories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.InventoryCheckItemHistories (
        HistoryID          BIGINT        PRIMARY KEY IDENTITY(1,1),
        CheckID            INT           NOT NULL,
        ProductID          INT           NOT NULL,
        BatchNumber        VARCHAR(50)   NULL,
        OldActualQuantity  DECIMAL(18,4) NULL,
        NewActualQuantity  DECIMAL(18,4) NULL,
        OldReason          NVARCHAR(255) NULL,
        NewReason          NVARCHAR(255) NULL,
        ChangedByUserID    INT           NULL,
        ChangedAt          DATETIME2     NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_CheckHist_Check   FOREIGN KEY (CheckID) REFERENCES dbo.InventoryChecks(CheckID),
        CONSTRAINT FK_CheckHist_Product FOREIGN KEY (ProductID) REFERENCES dbo.Products(ProductID),
        CONSTRAINT FK_CheckHist_User    FOREIGN KEY (ChangedByUserID) REFERENCES dbo.Users(UserID)
    );

    CREATE INDEX IX_CheckHist_CheckID ON dbo.InventoryCheckItemHistories(CheckID, ChangedAt DESC);
END;";

        public const string GetWarehouses = @"
SELECT WarehouseID, WarehouseName
FROM dbo.Warehouses
WHERE IsActive = 1
ORDER BY WarehouseName;";

        public const string GetStockByWarehouse = @"
SELECT
    i.ProductID,
    p.ProductCode,
    p.ProductName,
    i.BatchNumber,
    i.ExpiryDate,
    i.Quantity AS SystemQuantity
FROM dbo.Inventory i
INNER JOIN dbo.Products p ON p.ProductID = i.ProductID
WHERE i.WarehouseID = @WarehouseID
ORDER BY p.ProductName, i.ExpiryDate, i.BatchNumber;";

        public const string CreateInventoryCheck = @"
INSERT INTO dbo.InventoryChecks
    (CheckCode, WarehouseID, CheckDate, CreatedByUserID, Status, Notes)
VALUES
    (@CheckCode, @WarehouseID, GETDATE(), @CreatedByUserID, 1, @Notes);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public const string AddInventoryCheckItem = @"
INSERT INTO dbo.InventoryCheckItems
    (CheckID, ProductID, BatchNumber, ExpiryDate, SystemQuantity, ActualQuantity, Note)
VALUES
    (@CheckID, @ProductID, @BatchNumber, @ExpiryDate, @SystemQuantity, NULL, NULL);";

        public const string GetCheckItems = @"
SELECT
    ci.CheckID,
    ci.ProductID,
    p.ProductCode,
    p.ProductName,
    ci.BatchNumber,
    ci.ExpiryDate,
    ci.SystemQuantity,
    ci.ActualQuantity,
    ci.Note
FROM dbo.InventoryCheckItems ci
INNER JOIN dbo.Products p ON p.ProductID = ci.ProductID
WHERE ci.CheckID = @CheckID
ORDER BY p.ProductName, ci.ExpiryDate, ci.BatchNumber;";

        public const string GetChecksByWarehouse = @"
SELECT
    c.CheckID,
    c.CheckCode,
    c.CheckDate,
    c.Status,
    CASE c.Status
        WHEN 1 THEN N'Đang kiểm'
        WHEN 2 THEN N'Đã duyệt'
        WHEN 3 THEN N'Đã hủy'
        ELSE N'Không xác định'
    END AS StatusText,
    creator.FullName AS CreatedByName,
    approver.FullName AS ApprovedByName,
    c.ApprovedAt
FROM dbo.InventoryChecks c
LEFT JOIN dbo.Users creator ON creator.UserID = c.CreatedByUserID
LEFT JOIN dbo.Users approver ON approver.UserID = c.ApprovedByUserID
WHERE c.WarehouseID = @WarehouseID
ORDER BY c.CheckDate DESC;";

        public const string GetCheckHeader = @"
SELECT
    c.CheckID,
    c.CheckCode,
    c.WarehouseID,
    w.WarehouseName,
    c.CheckDate,
    c.Status,
    CASE c.Status
        WHEN 1 THEN N'Đang kiểm'
        WHEN 2 THEN N'Đã duyệt'
        WHEN 3 THEN N'Đã hủy'
        ELSE N'Không xác định'
    END AS StatusText,
    c.Notes,
    creator.FullName AS CreatedByName,
    approver.FullName AS ApprovedByName,
    c.ApprovedAt
FROM dbo.InventoryChecks
    c
INNER JOIN dbo.Warehouses w ON w.WarehouseID = c.WarehouseID
LEFT JOIN dbo.Users creator ON creator.UserID = c.CreatedByUserID
LEFT JOIN dbo.Users approver ON approver.UserID = c.ApprovedByUserID
WHERE c.CheckID = @CheckID;";

        public const string InsertItemHistory = @"
INSERT INTO dbo.InventoryCheckItemHistories
    (CheckID, ProductID, BatchNumber, OldActualQuantity, NewActualQuantity, OldReason, NewReason, ChangedByUserID, ChangedAt)
VALUES
    (@CheckID, @ProductID, @BatchNumber, @OldActualQuantity, @NewActualQuantity, @OldReason, @NewReason, @ChangedByUserID, GETDATE());";

        public const string GetItemHistories = @"
SELECT
    h.HistoryID,
    h.CheckID,
    h.ProductID,
    p.ProductCode,
    p.ProductName,
    h.BatchNumber,
    h.OldActualQuantity,
    h.NewActualQuantity,
    h.OldReason,
    h.NewReason,
    h.ChangedByUserID,
    u.FullName AS ChangedByName,
    h.ChangedAt
FROM dbo.InventoryCheckItemHistories h
INNER JOIN dbo.Products p ON p.ProductID = h.ProductID
LEFT JOIN dbo.Users u ON u.UserID = h.ChangedByUserID
WHERE h.CheckID = @CheckID
ORDER BY h.ChangedAt DESC, h.HistoryID DESC;";

        public const string UpdateCheckItemActual = @"
UPDATE dbo.InventoryCheckItems
SET ActualQuantity = @ActualQuantity,
    Note = @Note
WHERE CheckID = @CheckID
  AND ProductID = @ProductID
  AND ISNULL(BatchNumber, '') = ISNULL(@BatchNumber, '');";

        public const string UpdateInventoryByBatch = @"
UPDATE dbo.Inventory
SET Quantity = @ActualQuantity,
    UpdatedAt = GETDATE()
WHERE WarehouseID = @WarehouseID
  AND ProductID = @ProductID
  AND ISNULL(BatchNumber, '') = ISNULL(@BatchNumber, '');";

        public const string InsertAdjustmentTx = @"
INSERT INTO dbo.StockTransactions
    (WarehouseID, ProductID, TransactionType, Quantity, BatchNumber, ExpiryDate, ReferenceType, ReferenceID, Note, CreatedByUserID, CreatedAt)
VALUES
    (@WarehouseID, @ProductID, 3, @QuantityDiff, @BatchNumber, @ExpiryDate, 'InventoryCheck', @ReferenceID, @Note, @CreatedByUserID, GETDATE());";

        public const string ApproveCheck = @"
UPDATE dbo.InventoryChecks
SET Status = 2,
    ApprovedByUserID = @ApprovedByUserID,
    ApprovedAt = GETDATE()
WHERE CheckID = @CheckID;";
    }
}
