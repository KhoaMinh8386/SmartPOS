namespace SmartPos.Module.Reports.Templates
{
    internal static class ReportSqlTemplate
    {
        public const string GetDashboardKpis = @"
DECLARE @TodayStart DATETIME = CAST(GETDATE() AS DATE);
DECLARE @MonthStart DATETIME = DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0);

SELECT 
    (SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices WHERE InvoiceDate >= @TodayStart AND Status = 1) as TodayRevenue,
    (SELECT ISNULL(SUM(TotalAmount), 0) FROM Invoices WHERE InvoiceDate >= @MonthStart AND Status = 1) as MonthRevenue,
    (SELECT COUNT(*) FROM Invoices WHERE InvoiceDate >= @TodayStart AND Status = 1) as TodayOrders,
    (SELECT ISNULL(SUM(i.TotalAmount - cogs.TotalCost), 0) 
     FROM Invoices i
     CROSS APPLY (
        SELECT SUM(ii.Quantity * p.CostPrice) as TotalCost
        FROM InvoiceItems ii
        JOIN Products p ON ii.ProductID = p.ProductID
        WHERE ii.InvoiceID = i.InvoiceID
     ) cogs
     WHERE i.InvoiceDate >= @TodayStart AND i.Status = 1) as TodayProfit,
    (SELECT COUNT(*) FROM Inventory WHERE Quantity <= (SELECT MinStockLevel FROM Products WHERE ProductID = Inventory.ProductID)) as LowStockCount,
    (SELECT COUNT(*) FROM Inventory WHERE ExpiryDate <= DATEADD(day, 30, GETDATE()) AND ExpiryDate >= GETDATE()) as NearExpiryCount;";

        public const string GetRevenueChart = @"
SELECT 
    FORMAT(InvoiceDate, 'dd/MM') as Label,
    SUM(TotalAmount) as Value
FROM Invoices
WHERE InvoiceDate >= DATEADD(day, -@Days, GETDATE()) AND Status = 1
GROUP BY FORMAT(InvoiceDate, 'dd/MM'), CAST(InvoiceDate AS DATE)
ORDER BY CAST(InvoiceDate AS DATE);";

        public const string GetTopProducts = @"
SELECT TOP 5 
    p.ProductName as Label,
    SUM(ii.Quantity) as Value
FROM InvoiceItems ii
JOIN Invoices i ON ii.InvoiceID = i.InvoiceID
JOIN Products p ON ii.ProductID = p.ProductID
WHERE i.InvoiceDate >= DATEADD(month, -1, GETDATE()) AND i.Status = 1
GROUP BY p.ProductName
ORDER BY Value DESC;";

        public const string GetPaymentMethods = @"
SELECT 
    CASE PaymentMethod WHEN 1 THEN 'Tien mat' WHEN 2 THEN 'Chuyen khoan' ELSE 'Khac' END as Label,
    SUM(TotalAmount) as Value
FROM Invoices
WHERE Status = 1
GROUP BY PaymentMethod;";

        public const string GetRecentInvoices = @"
SELECT TOP 10 
    InvoiceCode, InvoiceDate, TotalAmount, Status
FROM Invoices
ORDER BY InvoiceDate DESC;";

        public const string GetLowStockAlert = @"
SELECT TOP 10
    p.ProductCode, p.ProductName, i.Quantity as CurrentStock, p.MinStockLevel
FROM Inventory i
JOIN Products p ON i.ProductID = p.ProductID
WHERE i.Quantity <= p.MinStockLevel
ORDER BY i.Quantity ASC;";

        public const string GetProductPerformance = @"
SELECT 
    p.ProductID, p.ProductCode, p.ProductName,
    ISNULL(sales.TotalQty, 0) as SoldQuantity,
    ISNULL(sales.TotalRev, 0) as Revenue,
    ISNULL(inv.TotalStock, 0) as CurrentStock,
    p.CostPrice, p.MinStockLevel as MinStockAlert
FROM Products p
LEFT JOIN (
    SELECT ii.ProductID, SUM(ii.Quantity) as TotalQty, SUM(ii.Quantity * ii.UnitPrice) as TotalRev
    FROM InvoiceItems ii
    JOIN Invoices i ON ii.InvoiceID = i.InvoiceID
    WHERE i.Status = 1 AND i.InvoiceDate >= @FromDate AND i.InvoiceDate <= @ToDate
    GROUP BY ii.ProductID
) sales ON p.ProductID = sales.ProductID
LEFT JOIN (
    SELECT ProductID, SUM(Quantity) as TotalStock
    FROM Inventory
    GROUP BY ProductID
) inv ON p.ProductID = inv.ProductID
ORDER BY Revenue DESC;";

        public const string GetNearExpiryItems = @"
SELECT 
    p.ProductID, p.ProductCode, p.ProductName, i.Quantity as CurrentStock, i.ExpiryDate
FROM Inventory i
JOIN Products p ON i.ProductID = p.ProductID
WHERE i.ExpiryDate <= DATEADD(day, @Days, GETDATE()) AND i.ExpiryDate >= GETDATE()
ORDER BY i.ExpiryDate ASC;";

        public const string GetCustomerReport = @"
SELECT 
    CustomerID, FullName, CustomerType as Rank, TotalSpent, TotalPoints as LoyaltyPoints,
    (SELECT COUNT(*) FROM Invoices WHERE CustomerID = Customers.CustomerID AND Status = 1) as OrderCount
FROM Customers
ORDER BY TotalSpent DESC;";

        public const string GetProfitReport = @"
SELECT 
    cat.CategoryName,
    SUM(ii.Quantity * ii.UnitPrice) as Revenue,
    SUM(ii.Quantity * p.CostPrice) as Cost
FROM InvoiceItems ii
JOIN Invoices i ON ii.InvoiceID = i.InvoiceID
JOIN Products p ON ii.ProductID = p.ProductID
JOIN Categories cat ON p.CategoryID = cat.CategoryID
WHERE i.Status = 1 AND i.InvoiceDate >= @FromDate AND i.InvoiceDate <= @ToDate
GROUP BY cat.CategoryName;";

        public const string GetAllBatches = @"
SELECT 
    p.ProductID, p.ProductCode, p.ProductName,
    i.BatchNumber, i.ManufactureDate, i.ExpiryDate,
    i.Quantity, '' AS ShelfLocation, w.WarehouseName,
    DATEDIFF(DAY, GETDATE(), ISNULL(i.ExpiryDate, GETDATE() + 9999)) as DaysToExpiry
FROM Inventory i
JOIN Products p ON i.ProductID = p.ProductID
LEFT JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
WHERE (@WarehouseID = 0 OR i.WarehouseID = @WarehouseID)
ORDER BY i.ExpiryDate ASC;";

        public const string GetBatchesByProduct = @"
SELECT 
    p.ProductID, p.ProductCode, p.ProductName,
    i.BatchNumber, i.ManufactureDate, i.ExpiryDate,
    i.Quantity, '' AS ShelfLocation, w.WarehouseName,
    DATEDIFF(DAY, GETDATE(), ISNULL(i.ExpiryDate, GETDATE() + 9999)) as DaysToExpiry
FROM Inventory i
JOIN Products p ON i.ProductID = p.ProductID
LEFT JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
WHERE i.ProductID = @ProductID
ORDER BY i.ExpiryDate ASC;";
    }
}
