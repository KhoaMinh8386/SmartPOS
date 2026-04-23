namespace SmartPos.Module.Customers.Templates
{
    internal static class CustomerSqlTemplate
    {
        // ─── Schema Migration ──────────────────────────────────────────────────
        public const string EnsureSchema = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE dbo.Customers (
        CustomerID      INT IDENTITY(1,1) PRIMARY KEY,
        CustomerCode    NVARCHAR(20) UNIQUE,
        FullName        NVARCHAR(100) NOT NULL,
        Phone           NVARCHAR(15),
        Email           NVARCHAR(100),
        Address         NVARCHAR(255),
        Gender          NVARCHAR(10),
        DateOfBirth     DATE,
        TotalPoints     INT           DEFAULT 0,
        TotalSpent      DECIMAL(18,2) DEFAULT 0,
        CustomerType    NVARCHAR(50)  DEFAULT N'Thường',
        Note            NVARCHAR(255),
        CreatedAt       DATETIME      DEFAULT GETDATE(),
        UpdatedAt       DATETIME
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CustomerPointsHistory')
BEGIN
    CREATE TABLE dbo.CustomerPointsHistory (
        ID          INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID  INT NOT NULL,
        Points      INT NOT NULL,
        Type        NVARCHAR(50),
        Description NVARCHAR(255),
        CreatedAt   DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (CustomerID) REFERENCES dbo.Customers(CustomerID)
    );
END;";

        // ─── LIST ─────────────────────────────────────────────────────────────
        public const string GetCustomerList = @"
SELECT c.CustomerID,
       ISNULL(c.CustomerCode, N'')         AS CustomerCode,
       ISNULL(c.FullName, N'')             AS FullName,
       ISNULL(c.Phone, N'')               AS Phone,
       ISNULL(c.Email, N'')               AS Email,
       ISNULL(c.TotalPoints, 0)           AS TotalPoints,
       ISNULL(c.TotalSpent, 0)            AS TotalSpent,
       ISNULL(c.CustomerType, N'Thường')  AS CustomerType,
       ISNULL(c.CreatedAt, GETDATE())     AS CreatedAt
FROM   dbo.Customers c
WHERE  (   @Search IS NULL
        OR c.FullName     LIKE N'%' + @Search + N'%'
        OR c.Phone        LIKE N'%' + @Search + N'%'
        OR c.CustomerCode LIKE N'%' + @Search + N'%')
  AND  (@TypeFilter IS NULL OR ISNULL(c.CustomerType, N'Thường') = @TypeFilter)
ORDER BY ISNULL(c.TotalPoints, 0) DESC, ISNULL(c.FullName, N'');";

        // ─── DETAIL ───────────────────────────────────────────────────────────
        public const string GetCustomerDetail = @"
SELECT  CustomerID,
        ISNULL(CustomerCode, N'')        AS CustomerCode,
        ISNULL(FullName, N'')            AS FullName,
        Phone, Email, Address, Gender, DateOfBirth,
        ISNULL(TotalPoints, 0)           AS TotalPoints,
        ISNULL(TotalSpent, 0)            AS TotalSpent,
        ISNULL(CustomerType, N'Thường')  AS CustomerType,
        Note,
        ISNULL(CreatedAt, GETDATE())     AS CreatedAt,
        UpdatedAt
FROM    dbo.Customers
WHERE   CustomerID = @CustomerID;";

        // ─── INSERT ───────────────────────────────────────────────────────────
        public const string InsertCustomer = @"
DECLARE @Code NVARCHAR(20) = 'KH' + FORMAT(NEXT VALUE FOR OBJECT_ID('dbo.Customers'), '0000');
-- fallback code
SELECT @Code = 'KH' + RIGHT('0000' + CAST((ISNULL(MAX(CustomerID),0)+1) AS NVARCHAR), 4)
FROM dbo.Customers;

INSERT INTO dbo.Customers (CustomerCode, FullName, Phone, Email, Address, Gender, DateOfBirth, Note, CreatedAt)
VALUES (@Code, @FullName, @Phone, @Email, @Address, @Gender, @DateOfBirth, @Note, GETDATE());
SELECT SCOPE_IDENTITY();";

        // ─── UPDATE ───────────────────────────────────────────────────────────
        public const string UpdateCustomer = @"
UPDATE dbo.Customers
SET  FullName = @FullName, Phone = @Phone, Email = @Email,
     Address  = @Address,  Gender = @Gender, DateOfBirth = @DateOfBirth,
     Note = @Note, UpdatedAt = GETDATE()
WHERE CustomerID = @CustomerID;";

        // ─── DELETE ───────────────────────────────────────────────────────────
        public const string DeleteCustomer = @"
DELETE FROM dbo.CustomerPointsHistory WHERE CustomerID = @CustomerID;
DELETE FROM dbo.Customers WHERE CustomerID = @CustomerID;";

        // ─── POINTS HISTORY ───────────────────────────────────────────────────
        public const string GetPointsHistory = @"
SELECT ID, Points, Type, Description, CreatedAt
FROM   dbo.CustomerPointsHistory
WHERE  CustomerID = @CustomerID
ORDER  BY CreatedAt DESC;";

        public const string AdjustPoints = @"
UPDATE dbo.Customers
SET TotalPoints = TotalPoints + @Points,
    CustomerType = CASE
        WHEN TotalPoints + @Points >= 5000 THEN N'VIP'
        WHEN TotalPoints + @Points >= 1000 THEN N'Thân thiết'
        ELSE N'Thường' END,
    UpdatedAt = GETDATE()
WHERE CustomerID = @CustomerID;

INSERT INTO dbo.CustomerPointsHistory (CustomerID, Points, Type, Description, CreatedAt)
VALUES (@CustomerID, @Points, @Type, @Description, GETDATE());";

        // ─── INVOICE HISTORY ──────────────────────────────────────────────────
        public const string GetCustomerInvoices = @"
SELECT i.InvoiceID, i.InvoiceCode, i.InvoiceDate, i.TotalAmount,
       CASE i.PaymentMethod WHEN 1 THEN N'Tiền mặt' WHEN 2 THEN N'Chuyển khoản' ELSE N'Kết hợp' END AS PaymentMethodText
FROM   dbo.Invoices i
WHERE  i.CustomerID = @CustomerID
ORDER  BY i.InvoiceDate DESC;";

        // ─── AUTO CODE ────────────────────────────────────────────────────────
        public const string GetNextCustomerCode = @"
SELECT 'KH' + RIGHT('0000' + CAST(ISNULL(MAX(CustomerID), 0) + 1 AS NVARCHAR), 4)
FROM dbo.Customers;";
    }
}
