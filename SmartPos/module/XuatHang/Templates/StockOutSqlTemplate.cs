namespace SmartPos.Module.XuatHang.Templates
{
    internal static class StockOutSqlTemplate
    {
        public const string EnsureSchema = @"
IF OBJECT_ID('dbo.StockOuts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockOuts (
        StockOutID      INT           PRIMARY KEY IDENTITY(1,1),
        StockOutCode    VARCHAR(20)   NOT NULL UNIQUE,
        StockOutDate    DATETIME2     NOT NULL DEFAULT GETDATE(),
        WarehouseID     INT           NOT NULL,
        Reason          NVARCHAR(100) NOT NULL,
        Notes           NVARCHAR(500) NULL,
        CreatedByUserID INT           NULL,
        CreatedAt       DATETIME2     NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_StockOuts_Warehouse FOREIGN KEY (WarehouseID) REFERENCES dbo.Warehouses(WarehouseID),
        CONSTRAINT FK_StockOuts_User      FOREIGN KEY (CreatedByUserID) REFERENCES dbo.Users(UserID)
    );
END;

IF OBJECT_ID('dbo.StockOutDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockOutDetails (
        DetailID     INT           PRIMARY KEY IDENTITY(1,1),
        StockOutID   INT           NOT NULL,
        ProductID    INT           NOT NULL,
        BatchNumber  VARCHAR(50)   NULL,
        ExpiryDate   DATE          NULL,
        Quantity     DECIMAL(18,4) NOT NULL,
        CONSTRAINT FK_StockOutDetails_Header  FOREIGN KEY (StockOutID) REFERENCES dbo.StockOuts(StockOutID),
        CONSTRAINT FK_StockOutDetails_Product FOREIGN KEY (ProductID) REFERENCES dbo.Products(ProductID)
    );
    CREATE INDEX IX_StockOutDetails_HeaderID ON dbo.StockOutDetails(StockOutID);
END;";

        public const string GetWarehouses = @"
SELECT WarehouseID, WarehouseName
FROM dbo.Warehouses
WHERE IsActive = 1
ORDER BY WarehouseName;";

        public const string GetProductInventory = @"
SELECT 
    i.ProductID, 
    p.ProductCode, 
    p.ProductName, 
    u.UnitName,
    i.BatchNumber, 
    i.ExpiryDate, 
    i.Quantity
FROM dbo.Inventory i
INNER JOIN dbo.Products p ON p.ProductID = i.ProductID
LEFT JOIN dbo.Units u ON u.UnitID = p.BaseUnitID
WHERE i.WarehouseID = @WarehouseID 
  AND i.Quantity > 0
  AND (@Search IS NULL OR p.ProductName LIKE @Search OR p.ProductCode LIKE @Search)
ORDER BY p.ProductName, i.ExpiryDate;";

        public const string InsertStockOut = @"
INSERT INTO dbo.StockOuts (StockOutCode, StockOutDate, WarehouseID, Reason, Notes, CreatedByUserID)
VALUES (@StockOutCode, GETDATE(), @WarehouseID, @Reason, @Notes, @CreatedByUserID);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public const string InsertStockOutDetail = @"
INSERT INTO dbo.StockOutDetails (StockOutID, ProductID, BatchNumber, ExpiryDate, Quantity)
VALUES (@StockOutID, @ProductID, @BatchNumber, @ExpiryDate, @Quantity);";

        public const string UpdateInventory = @"
UPDATE dbo.Inventory
SET Quantity = Quantity - @Quantity,
    UpdatedAt = GETDATE()
WHERE WarehouseID = @WarehouseID
  AND ProductID = @ProductID
  AND ISNULL(BatchNumber, '') = ISNULL(@BatchNumber, '');";

        public const string InsertStockTransaction = @"
INSERT INTO dbo.StockTransactions 
    (WarehouseID, ProductID, TransactionType, Quantity, BatchNumber, ExpiryDate, ReferenceType, ReferenceID, Note, CreatedByUserID, CreatedAt)
VALUES 
    (@WarehouseID, @ProductID, 4, -@Quantity, @BatchNumber, @ExpiryDate, 'StockOut', @ReferenceID, @Note, @CreatedByUserID, GETDATE());";

        public const string GetNextCode = @"
SELECT ISNULL(MAX(StockOutID), 0) + 1 FROM dbo.StockOuts;";
    }
}
