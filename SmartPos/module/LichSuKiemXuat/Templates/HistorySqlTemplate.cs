namespace SmartPos.Module.LichSuKiemXuat.Templates
{
    internal static class HistorySqlTemplate
    {
        public const string EnsureSchema = @"
IF COL_LENGTH('dbo.StockOutDetails', 'ShelfLocation') IS NULL
BEGIN
    ALTER TABLE dbo.StockOutDetails ADD ShelfLocation NVARCHAR(50) NULL;
END;";

        // --- Stock Out Queries ---
        public const string GetStockOutHistory = @"
SELECT 
    s.StockOutID, s.StockOutCode, s.StockOutDate, s.Reason, s.Notes,
    w.WarehouseName,
    u.FullName AS CreatedByName,
    (SELECT COUNT(*) FROM dbo.StockOutDetails sd WHERE sd.StockOutID = s.StockOutID) AS ItemCount
FROM dbo.StockOuts s
INNER JOIN dbo.Warehouses w ON w.WarehouseID = s.WarehouseID
LEFT JOIN dbo.Users u ON u.UserID = s.CreatedByUserID
WHERE s.StockOutDate BETWEEN @FromDate AND @ToDate
  AND (@Reason = N'Tất cả' OR s.Reason = @Reason)
  AND (@UserID IS NULL OR s.CreatedByUserID = @UserID)
  AND (@Search IS NULL OR s.StockOutCode LIKE @Search OR s.Notes LIKE @Search)
ORDER BY s.StockOutDate DESC;";

        public const string GetStockOutDetails = @"
SELECT 
    sd.ProductID, p.ProductCode, p.ProductName, p.Barcode, 
    u.UnitName, sd.Quantity, sd.Quantity AS BaseQuantity,
    sd.BatchNumber, sd.ShelfLocation, sd.ExpiryDate,
    ISNULL((SELECT TOP 1 st.Quantity FROM dbo.StockTransactions st WHERE st.ProductID = sd.ProductID AND st.CreatedAt < s.StockOutDate ORDER BY st.CreatedAt DESC), 0) AS StockBefore
FROM dbo.StockOutDetails sd
INNER JOIN dbo.StockOuts s ON s.StockOutID = sd.StockOutID
INNER JOIN dbo.Products p ON p.ProductID = sd.ProductID
LEFT JOIN dbo.Units u ON u.UnitID = p.BaseUnitID
WHERE sd.StockOutID = @StockOutID;";

        public const string GetStockOutStats = @"
SELECT 
    COUNT(*) AS TotalVouchers,
    SUM(CASE WHEN Reason = N'Hư hỏng' THEN 1 ELSE 0 END) AS DamageCount,
    SUM(CASE WHEN Reason = N'Hết hạn' THEN 1 ELSE 0 END) AS ExpiredCount,
    SUM(CASE WHEN Reason = N'Điều chuyển' THEN 1 ELSE 0 END) AS TransferCount,
    SUM(CASE WHEN Reason NOT IN (N'Hư hỏng', N'Hết hạn', N'Điều chuyển') THEN 1 ELSE 0 END) AS OtherCount
FROM dbo.StockOuts
WHERE StockOutDate BETWEEN @FromDate AND @ToDate;";

        // --- Inventory Audit Queries ---
        public const string GetAuditHistory = @"
SELECT 
    c.CheckID, c.CheckCode, c.CheckDate, 
    w.WarehouseName AS CategoryName, -- Using Warehouse as Category for now as per schema
    u1.FullName AS AuditorName,
    u2.FullName AS ApproverName,
    c.ApprovedAt,
    (SELECT COUNT(*) FROM dbo.InventoryCheckItems ci WHERE ci.CheckID = c.CheckID) AS TotalItems,
    (SELECT COUNT(*) FROM dbo.InventoryCheckItems ci WHERE ci.CheckID = c.CheckID AND ci.ActualQuantity = ci.SystemQuantity) AS MatchCount,
    (SELECT COUNT(*) FROM dbo.InventoryCheckItems ci WHERE ci.CheckID = c.CheckID AND ci.ActualQuantity > ci.SystemQuantity) AS OverCount,
    (SELECT COUNT(*) FROM dbo.InventoryCheckItems ci WHERE ci.CheckID = c.CheckID AND ci.ActualQuantity < ci.SystemQuantity) AS UnderCount
FROM dbo.InventoryChecks c
INNER JOIN dbo.Warehouses w ON w.WarehouseID = c.WarehouseID
LEFT JOIN dbo.Users u1 ON u1.UserID = c.CreatedByUserID
LEFT JOIN dbo.Users u2 ON u2.UserID = c.ApprovedByUserID
WHERE c.CheckDate BETWEEN @FromDate AND @ToDate
  AND (@UserID IS NULL OR c.CreatedByUserID = @UserID)
  AND (@Search IS NULL OR c.CheckCode LIKE @Search)
ORDER BY c.CheckDate DESC;";

        public const string GetAuditDetails = @"
SELECT 
    ci.ProductID, p.ProductName, u.UnitName,
    ci.SystemQuantity, ci.ActualQuantity,
    (ci.ActualQuantity - ci.SystemQuantity) AS Difference,
    CASE 
        WHEN ci.ActualQuantity = ci.SystemQuantity THEN N'Khớp'
        WHEN ci.ActualQuantity > ci.SystemQuantity THEN N'Thừa'
        ELSE N'Thiếu'
    END AS ResultText
FROM dbo.InventoryCheckItems ci
INNER JOIN dbo.Products p ON p.ProductID = ci.ProductID
LEFT JOIN dbo.Units u ON u.UnitID = p.BaseUnitID
WHERE ci.CheckID = @CheckID;";

        // --- Purchase History Queries ---
        public const string GetPurchaseHistory = @"
SELECT 
    p.PurchaseOrderID, p.POCode, p.OrderDate, 
    s.SupplierName, w.WarehouseName, 
    p.TotalAmount, p.Status, p.PaymentStatus,
    u.FullName AS CreatedByName,
    (SELECT COUNT(*) FROM dbo.PurchaseOrderItems pi WHERE pi.PurchaseOrderID = p.PurchaseOrderID) AS ItemCount
FROM dbo.PurchaseOrders p
LEFT JOIN dbo.Suppliers s ON s.SupplierID = p.SupplierID
INNER JOIN dbo.Warehouses w ON w.WarehouseID = p.WarehouseID
LEFT JOIN dbo.Users u ON u.UserID = p.CreatedByUserID
WHERE p.OrderDate BETWEEN @FromDate AND @ToDate
  AND (@UserID IS NULL OR p.CreatedByUserID = @UserID)
  AND (@Search IS NULL OR p.POCode LIKE @Search OR p.Notes LIKE @Search)
ORDER BY p.OrderDate DESC;";

        public const string GetPurchaseDetails = @"
SELECT 
    pi.ProductID, pr.ProductCode, pr.ProductName, 
    u.UnitName, pi.Quantity, pi.CostPrice,
    pi.BatchNumber, pi.ShelfLocation, pi.ExpiryDate
FROM dbo.PurchaseOrderItems pi
INNER JOIN dbo.Products pr ON pr.ProductID = pi.ProductID
LEFT JOIN dbo.Units u ON u.UnitID = pi.UnitID
WHERE pi.PurchaseOrderID = @PurchaseOrderID;";

        public const string GetPurchaseStats = @"
SELECT 
    COUNT(*) AS TotalVouchers,
    ISNULL(SUM(TotalAmount), 0) AS TotalAmount
FROM dbo.PurchaseOrders
WHERE OrderDate BETWEEN @FromDate AND @ToDate;";

        public const string GetUsersLookup = "SELECT UserID, FullName FROM dbo.Users WHERE IsActive = 1 ORDER BY FullName;";
        public const string GetCategoriesLookup = "SELECT CategoryID, CategoryName FROM dbo.Categories WHERE IsActive = 1 ORDER BY CategoryName;";
    }
}
