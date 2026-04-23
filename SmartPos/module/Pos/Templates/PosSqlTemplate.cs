namespace SmartPos.Module.Pos.Templates
{
    internal static class PosSqlTemplate
    {
        public const string FindProduct = @"
SELECT TOP 10 
    p.ProductID, p.ProductCode, p.ProductName, p.RetailPrice, u.UnitName
FROM dbo.Products p
LEFT JOIN dbo.Units u ON p.BaseUnitID = u.UnitID
WHERE p.IsActive = 1 
  AND (p.ProductCode = @Term OR p.Barcode = @Term OR p.ProductName LIKE @SearchTerm)
ORDER BY p.ProductName;";

        public const string FindCustomerByPhone = @"
SELECT CustomerID, CustomerName, Phone, Address, ISNULL(Points, 0) as Points
FROM dbo.Customers
WHERE Phone = @Phone AND IsActive = 1;";

        public const string InsertCustomer = @"
INSERT INTO dbo.Customers (CustomerName, Phone, Address, Points, IsActive, CreatedAt)
VALUES (@Name, @Phone, @Address, 0, 1, GETDATE());
SELECT SCOPE_IDENTITY();";

        public const string InsertInvoice = @"
INSERT INTO dbo.Invoices (
    InvoiceCode, CustomerID, CashierUserID, WarehouseID, SubTotal, TotalAmount, DiscountAmount, 
    PaidAmount, PaymentMethod, Status, Notes, VoucherCode, VoucherDiscount, InvoiceDate
) VALUES (
    @InvoiceCode, @CustomerID, @UserID, @WarehouseID, @SubTotal, @TotalAmount, @VoucherDiscount, 
    @PaidAmount, @PaymentMethod, 1, @Note, @VoucherCode, @VoucherDiscount, GETDATE()
);
SELECT SCOPE_IDENTITY();";

        public const string InsertInvoiceItem = @"
INSERT INTO dbo.InvoiceItems (InvoiceID, ProductID, Quantity, UnitPrice, UnitID)
VALUES (@InvoiceID, @ProductID, @Quantity, @UnitPrice, @UnitID);";

        public const string GetInvoices = @"
SELECT 
    i.InvoiceID, i.InvoiceCode, i.InvoiceDate, 
    ISNULL(c.CustomerName, 'Khach le') as CustomerName,
    u.FullName as StaffName,
    i.TotalAmount,
    CASE i.PaymentMethod WHEN 1 THEN 'Tien mat' WHEN 2 THEN 'Chuyen khoan' ELSE 'Khac' END as PaymentMethodText
FROM dbo.Invoices i
LEFT JOIN dbo.Customers c ON i.CustomerID = c.CustomerID
INNER JOIN dbo.Users u ON i.CashierUserID = u.UserID
WHERE (@Search IS NULL OR i.InvoiceCode LIKE @Search OR c.Phone LIKE @Search)
ORDER BY i.InvoiceDate DESC;";

        public const string GetInvoiceDetail = @"
SELECT 
    i.InvoiceID, i.InvoiceCode, i.InvoiceDate, 
    ISNULL(c.CustomerName, 'Khach le') as CustomerName,
    c.Phone,
    u.FullName as StaffName,
    i.TotalAmount, i.PaidAmount, i.ChangeAmount, i.PaymentMethod
FROM dbo.Invoices i
LEFT JOIN dbo.Customers c ON i.CustomerID = c.CustomerID
INNER JOIN dbo.Users u ON i.CashierUserID = u.UserID
WHERE i.InvoiceID = @InvoiceID;";

        public const string GetInvoiceItems = @"
SELECT 
    ii.ProductID, p.ProductCode, p.ProductName, 
    ii.UnitPrice, ii.Quantity, (ii.UnitPrice * ii.Quantity) as SubTotal,
    u.UnitName
FROM dbo.InvoiceItems ii
INNER JOIN dbo.Products p ON ii.ProductID = p.ProductID
LEFT JOIN dbo.Units u ON p.BaseUnitID = u.UnitID
WHERE ii.InvoiceID = @InvoiceID;";

        public const string GetNextInvoiceCode = @"SELECT ISNULL(MAX(InvoiceID), 0) + 1 FROM dbo.Invoices;";
    }
}
