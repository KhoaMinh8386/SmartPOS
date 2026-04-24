namespace SmartPos.Module.Pos
{
    internal static class PosSqlTemplate
    {
        public const string FindProduct = @"
SELECT TOP 10 
    p.ProductID, p.ProductCode, p.ProductName, p.RetailPrice, p.BaseUnitID, u.UnitName, p.ImageUrl,
    ISNULL(ps.SaleID, 0) as SaleID,
    CASE 
        WHEN ps.SaleID IS NULL THEN p.RetailPrice
        WHEN ps.SalePrice > 0 THEN ps.SalePrice
        WHEN ps.DiscountType = 1 THEN p.RetailPrice * (1 - ps.DiscountValue / 100)
        WHEN ps.DiscountType = 2 THEN p.RetailPrice - ps.DiscountValue
        ELSE p.RetailPrice
    END as FinalPrice,
    ISNULL((SELECT SUM(Quantity) FROM dbo.Inventory i WHERE i.ProductID = p.ProductID), 0) AS TotalStock
FROM dbo.Products p
LEFT JOIN dbo.Units u ON p.BaseUnitID = u.UnitID
LEFT JOIN dbo.ProductSales ps ON p.ProductID = ps.ProductID 
    AND ps.IsActive = 1 
    AND (GETDATE() >= ps.StartDate AND GETDATE() <= ps.EndDate)
WHERE p.IsActive = 1 
  AND (p.ProductCode = @Term OR p.Barcode = @Term OR p.ProductName LIKE @SearchTerm)
ORDER BY p.ProductName;";

        public const string FindCustomerByPhone = @"
SELECT TOP 10 CustomerID, FullName, Phone, Address, ISNULL(TotalPoints, 0) as TotalPoints
FROM dbo.Customers
WHERE (Phone LIKE @Term OR FullName LIKE @Term) AND IsActive = 1;";

        public const string InsertCustomer = @"
INSERT INTO dbo.Customers (FullName, Phone, Address, TotalPoints, IsActive, CreatedAt)
VALUES (@Name, @Phone, @Address, 0, 1, GETDATE());
SELECT SCOPE_IDENTITY();";

        public const string InsertInvoice = @"
INSERT INTO dbo.Invoices (
    InvoiceCode, CustomerID, CashierUserID, WarehouseID, SubTotal, TotalAmount, 
    DiscountAmount, VoucherDiscount, PointsDiscount, UsedPoints, EarnedPoints,
    PaidAmount, PaymentMethod, Status, Notes, VoucherCode, InvoiceDate
) VALUES (
    @InvoiceCode, @CustomerID, @UserID, @WarehouseID, @SubTotal, @TotalAmount, 
    @VoucherDiscount + @PointsDiscount, @VoucherDiscount, @PointsDiscount, @UsedPoints, @EarnedPoints,
    @PaidAmount, @PaymentMethod, 1, @Note, @VoucherCode, GETDATE()
);
SELECT SCOPE_IDENTITY();

-- Cập nhật điểm tích lũy cho khách hàng
IF @CustomerID IS NOT NULL
BEGIN
    UPDATE dbo.Customers 
    SET TotalPoints = ISNULL(TotalPoints, 0) - @UsedPoints + @EarnedPoints,
        TotalSpent = ISNULL(TotalSpent, 0) + @TotalAmount
    WHERE CustomerID = @CustomerID;

    -- Lưu lịch sử dùng điểm
    IF @UsedPoints > 0
    BEGIN
        INSERT INTO dbo.CustomerPointsHistory (CustomerID, Points, Type, Description, CreatedAt)
        VALUES (@CustomerID, -@UsedPoints, N'Trừ', N'Dùng điểm cho hóa đơn ' + @InvoiceCode, GETDATE());
    END

    -- Lưu lịch sử tích điểm mới
    IF @EarnedPoints > 0
    BEGIN
        INSERT INTO dbo.CustomerPointsHistory (CustomerID, Points, Type, Description, CreatedAt)
        VALUES (@CustomerID, @EarnedPoints, N'Cộng', N'Tích điểm từ hóa đơn ' + @InvoiceCode, GETDATE());
    END
END;";

        public const string InsertInvoiceItem = @"
INSERT INTO dbo.InvoiceItems (InvoiceID, ProductID, Quantity, UnitPrice, UnitID)
VALUES (@InvoiceID, @ProductID, @Quantity, @UnitPrice, @UnitID);";

        public const string GetInvoices = @"
SELECT 
    i.InvoiceID, i.InvoiceCode, i.InvoiceDate, 
    ISNULL(c.FullName, 'Khach le') as FullName,
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
    ISNULL(c.FullName, 'Khach le') as FullName,
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

        public const string GetNextInvoiceCode = @"
SELECT ISNULL(MAX(CAST(SUBSTRING(InvoiceCode, 9, 4) AS INT)), 0) + 1 
FROM dbo.Invoices 
WHERE InvoiceCode LIKE 'HD' + FORMAT(GETDATE(), 'yyMMdd') + '%';";
    }
}
