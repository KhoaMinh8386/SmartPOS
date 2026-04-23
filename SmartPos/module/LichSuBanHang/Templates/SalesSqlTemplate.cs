namespace SmartPos.Module.SalesHistory.Templates
{
    internal static class SalesSqlTemplate
    {
        public const string GetSalesHistory = @"
SELECT 
    i.InvoiceID, i.InvoiceCode, i.InvoiceDate, 
    ISNULL(c.FullName, 'Khach le') as FullName,
    u.FullName as StaffName,
    i.TotalAmount, i.DiscountAmount, i.PaidAmount, i.PaymentMethod, i.Status,
    CASE i.PaymentMethod WHEN 1 THEN 'Tien mat' WHEN 2 THEN 'Chuyen khoan' ELSE 'Khac' END as PaymentMethodText
FROM dbo.Invoices i
LEFT JOIN dbo.Customers c ON i.CustomerID = c.CustomerID
INNER JOIN dbo.Users u ON i.CashierUserID = u.UserID
WHERE i.InvoiceDate >= @FromDate AND i.InvoiceDate <= @ToDate
  AND (@StaffID IS NULL OR i.CashierUserID = @StaffID)
  AND (@SearchCustomer IS NULL OR c.FullName LIKE @SearchCustomer OR c.Phone LIKE @SearchCustomer)
  AND (@PaymentMethod IS NULL OR i.PaymentMethod = @PaymentMethod)
  AND (@Status IS NULL OR i.Status = @Status)
ORDER BY i.InvoiceDate DESC;";

        public const string GetOrderDetail = @"
SELECT 
    i.InvoiceID, i.InvoiceCode, i.InvoiceDate, 
    ISNULL(c.FullName, 'Khach le') as FullName,
    c.Phone as CustomerPhone,
    u.FullName as StaffName,
    i.TotalAmount, i.DiscountAmount, i.VoucherDiscount, 
    i.LoyaltyPointsUsed, i.LoyaltyDiscount, i.PaidAmount, 
    i.ChangeAmount, i.PaymentMethod, i.LoyaltyPointsEarned, i.Status, i.Notes
FROM dbo.Invoices i
LEFT JOIN dbo.Customers c ON i.CustomerID = c.CustomerID
INNER JOIN dbo.Users u ON i.CashierUserID = u.UserID
WHERE i.InvoiceID = @InvoiceID;";

        public const string GetOrderItems = @"
SELECT 
    ii.ProductID, p.ProductCode, p.ProductName, 
    ii.UnitPrice, ii.Quantity,
    u.UnitName
FROM dbo.InvoiceItems ii
INNER JOIN dbo.Products p ON ii.ProductID = p.ProductID
LEFT JOIN dbo.Units u ON p.BaseUnitID = u.UnitID
WHERE ii.InvoiceID = @InvoiceID;";

        public const string CancelInvoice = @"
UPDATE dbo.Invoices 
SET Status = 2, Notes = @Reason 
WHERE InvoiceID = @InvoiceID;";

        public const string RevertStock = @"
UPDATE dbo.Inventory 
SET Quantity = Quantity + @Quantity, UpdatedAt = GETDATE()
WHERE ProductID = @ProductID AND WarehouseID = @WarehouseID;";

        public const string RevertCustomerLoyalty = @"
UPDATE dbo.Customers 
SET TotalPoints = ISNULL(TotalPoints, 0) - @PointsEarned + @PointsUsed,
    TotalSpent = TotalSpent - @Amount
WHERE CustomerID = @CustomerID;";

        public const string GetUsers = "SELECT UserID, FullName FROM dbo.Users WHERE IsActive = 1;";
    }
}
