namespace SmartPos.Module.PurchaseOrders.Templates
{
    internal static class PurchaseOrderSqlTemplate
    {
        public const string EnsureSchema = @"
IF OBJECT_ID('dbo.SupplierPayment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SupplierPayment (
        PaymentID       INT           PRIMARY KEY IDENTITY(1,1),
        SupplierID      INT           NOT NULL,
        PurchaseOrderID INT           NOT NULL,
        Amount          DECIMAL(18,2) NOT NULL,
        PaymentMethod   TINYINT       NOT NULL DEFAULT 1,
        PaymentDate     DATETIME2     NOT NULL DEFAULT GETDATE(),
        Note            NVARCHAR(255) NULL,
        CreatedByUserID INT           NULL,
        CreatedAt       DATETIME2     NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_SupplierPayment_Supplier FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers(SupplierID),
        CONSTRAINT FK_SupplierPayment_PO       FOREIGN KEY (PurchaseOrderID) REFERENCES dbo.PurchaseOrders(PurchaseOrderID),
        CONSTRAINT CK_SupplierPayment_Amount   CHECK (Amount > 0)
    );

    CREATE INDEX IX_SupplierPayment_SupplierID ON dbo.SupplierPayment(SupplierID);
    CREATE INDEX IX_SupplierPayment_POID       ON dbo.SupplierPayment(PurchaseOrderID);
END;

IF COL_LENGTH('dbo.PurchaseOrderItems', 'ShelfLocation') IS NULL
BEGIN
    ALTER TABLE dbo.PurchaseOrderItems ADD ShelfLocation NVARCHAR(50) NULL;
END

IF COL_LENGTH('dbo.Inventory', 'ShelfLocation') IS NULL
BEGIN
    ALTER TABLE dbo.Inventory ADD ShelfLocation NVARCHAR(50) NULL;
END

IF COL_LENGTH('dbo.StockOutDetails', 'ShelfLocation') IS NULL
BEGIN
    ALTER TABLE dbo.StockOutDetails ADD ShelfLocation NVARCHAR(50) NULL;
END;";

        public const string GetSuppliers = @"
SELECT SupplierID, SupplierName
FROM dbo.Suppliers
WHERE IsActive = 1
ORDER BY SupplierName;";

        public const string GetUsers = @"
SELECT UserID, FullName
FROM dbo.Users
WHERE IsActive = 1
ORDER BY FullName;";

        public const string GetWarehouses = @"
SELECT WarehouseID, WarehouseName
FROM dbo.Warehouses
WHERE IsActive = 1
ORDER BY WarehouseName;";

        public const string GetProducts = @"
SELECT ProductID, BaseUnitID, ProductCode, ProductName
FROM dbo.Products
WHERE IsActive = 1
ORDER BY ProductName;";

        public const string CreatePurchaseOrder = @"
INSERT INTO dbo.PurchaseOrders
    (POCode, SupplierID, WarehouseID, CreatedByUserID, OrderDate, TotalAmount, PaidAmount, PaymentStatus, Status, Notes, CreatedAt)
VALUES
    (@POCode, @SupplierID, @WarehouseID, @CreatedByUserID, @OrderDate, @TotalAmount, 0, @PaymentStatus, 2, @Notes, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);";

        public const string AddPurchaseOrderItem = @"
INSERT INTO dbo.PurchaseOrderItems
    (PurchaseOrderID, ProductID, UnitID, ConversionRate, Quantity, CostPrice, BatchNumber, ShelfLocation, ManufactureDate, ExpiryDate)
VALUES
    (@PurchaseOrderID, @ProductID, @UnitID, 1, @Quantity, @CostPrice, @BatchNumber, @ShelfLocation, @ManufactureDate, @ExpiryDate);";

        public const string UpsertInventory = @"
IF EXISTS (
    SELECT 1
    FROM dbo.Inventory
    WHERE WarehouseID = @WarehouseID
      AND ProductID = @ProductID
      AND ISNULL(BatchNumber, '') = ISNULL(@BatchNumber, '')
      AND ISNULL(ShelfLocation, '') = ISNULL(@ShelfLocation, '')
)
BEGIN
    UPDATE dbo.Inventory
    SET Quantity = Quantity + @Quantity,
        CostPrice = @CostPrice,
        ManufactureDate = @ManufactureDate,
        ExpiryDate = @ExpiryDate,
        UpdatedAt = GETDATE()
    WHERE WarehouseID = @WarehouseID
      AND ProductID = @ProductID
      AND ISNULL(BatchNumber, '') = ISNULL(@BatchNumber, '')
      AND ISNULL(ShelfLocation, '') = ISNULL(@ShelfLocation, '');
END
ELSE
BEGIN
    INSERT INTO dbo.Inventory
        (WarehouseID, ProductID, BatchNumber, ShelfLocation, ManufactureDate, ExpiryDate, Quantity, CostPrice, UpdatedAt)
    VALUES
        (@WarehouseID, @ProductID, @BatchNumber, @ShelfLocation, @ManufactureDate, @ExpiryDate, @Quantity, @CostPrice, GETDATE());
END;";

        public const string GetFefoBatches = @"
SELECT
    i.InventoryID,
    i.ProductID,
    p.ProductCode,
    p.ProductName,
    i.BatchNumber,
    i.ShelfLocation,
    i.ExpiryDate,
    i.Quantity,
    w.WarehouseName
FROM dbo.Inventory i
INNER JOIN dbo.Products p ON p.ProductID = i.ProductID
INNER JOIN dbo.Warehouses w ON w.WarehouseID = i.WarehouseID
WHERE i.ProductID = @ProductID
  AND i.Quantity > 0
ORDER BY
    CASE WHEN i.ExpiryDate IS NULL THEN 1 ELSE 0 END,
    i.ExpiryDate ASC,
    i.InventoryID ASC;";
    }
}
