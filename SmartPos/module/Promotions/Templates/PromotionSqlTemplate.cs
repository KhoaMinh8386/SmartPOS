namespace SmartPos.Module.Promotions.Templates
{
    internal static class PromotionSqlTemplate
    {
        public const string EnsureSchema = @"
-- =============================================
-- Tạo bảng Users nếu chưa tồn tại
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Users (
        UserID          INT IDENTITY(1,1) PRIMARY KEY,
        Username        NVARCHAR(50)   NOT NULL UNIQUE,
        PasswordHash    NVARCHAR(255)  NOT NULL,
        FullName        NVARCHAR(100)  NOT NULL,
        Email           NVARCHAR(100)  NULL,
        RoleID          INT            NOT NULL DEFAULT 3, -- 1:Admin, 2:Manager, 3:Cashier
        IsActive        BIT            NOT NULL DEFAULT 1,
        FailedAttempts  INT            NOT NULL DEFAULT 0,
        LockoutEnd      DATETIME       NULL,
        CreatedAt       DATETIME       NOT NULL DEFAULT GETDATE()
    );
    
    -- Seed admin account (password: admin123)
    INSERT INTO dbo.Users (Username, PasswordHash, FullName, RoleID, IsActive)
    VALUES ('admin', '$2a$11$qR7jW1D7n6X7X7X7X7X7XuG5j5j5j5j5j5j5j5j5j5j5j5j5j5j5j', 'Hệ thống Admin', 1, 1);
END;

-- Đảm bảo cột RoleID tồn tại
IF COL_LENGTH('dbo.Users', 'RoleID') IS NULL
    ALTER TABLE dbo.Users ADD RoleID INT NOT NULL DEFAULT 3;

-- Đảm bảo các cột lockout tồn tại
IF COL_LENGTH('dbo.Users', 'FailedAttempts') IS NULL
    ALTER TABLE dbo.Users ADD FailedAttempts INT NOT NULL DEFAULT 0;

IF COL_LENGTH('dbo.Users', 'LockoutEnd') IS NULL
    ALTER TABLE dbo.Users ADD LockoutEnd DATETIME NULL;

-- =============================================
-- Tạo bảng Vouchers nếu chưa tồn tại
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Vouchers' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Vouchers (
        VoucherID          INT IDENTITY(1,1) PRIMARY KEY,
        VoucherCode        NVARCHAR(50)   NOT NULL UNIQUE,
        Description        NVARCHAR(255)  NULL,
        DiscountType       TINYINT        NOT NULL DEFAULT 1,  -- 1=%, 2=Số tiền
        DiscountValue      DECIMAL(18,2)  NOT NULL DEFAULT 0,
        MinOrderValue      DECIMAL(18,2)  NOT NULL DEFAULT 0,
        MaxDiscount        DECIMAL(18,2)  NULL,
        AllowStackDiscount BIT            NOT NULL DEFAULT 0,
        Priority           INT            NOT NULL DEFAULT 100,
        IsActive           BIT            NOT NULL DEFAULT 1,
        StartDate          DATETIME       NOT NULL,
        EndDate            DATETIME       NOT NULL,
        CreatedAt          DATETIME       NOT NULL DEFAULT GETDATE()
    );
END;

-- Bổ sung cột còn thiếu nếu bảng đã tồn tại từ phiên bản cũ
IF COL_LENGTH('dbo.Vouchers', 'AllowStackDiscount') IS NULL
    ALTER TABLE dbo.Vouchers ADD AllowStackDiscount BIT NOT NULL CONSTRAINT DF_Vouchers_AllowStackDiscount DEFAULT 0;

IF COL_LENGTH('dbo.Vouchers', 'Priority') IS NULL
    ALTER TABLE dbo.Vouchers ADD Priority INT NOT NULL CONSTRAINT DF_Vouchers_Priority DEFAULT 100;

-- =============================================
-- Tạo bảng ProductSales nếu chưa tồn tại
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductSales' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ProductSales (
        SaleID            INT IDENTITY(1,1) PRIMARY KEY,
        ProductID         INT            NOT NULL,
        SaleName          NVARCHAR(200)  NOT NULL,
        DiscountType      TINYINT        NOT NULL DEFAULT 1,  -- 1=%, 2=Số tiền
        DiscountValue     DECIMAL(18,2)  NOT NULL DEFAULT 0,
        SalePrice         DECIMAL(18,2)  NULL,
        AllowStackVoucher BIT            NOT NULL DEFAULT 0,
        Priority          INT            NOT NULL DEFAULT 100,
        IsActive          BIT            NOT NULL DEFAULT 1,
        StartDate         DATETIME       NOT NULL,
        EndDate           DATETIME       NOT NULL,
        CreatedAt         DATETIME       NOT NULL DEFAULT GETDATE()
    );
END;

-- Bổ sung cột còn thiếu nếu bảng đã tồn tại từ phiên bản cũ
IF COL_LENGTH('dbo.ProductSales', 'Priority') IS NULL
    ALTER TABLE dbo.ProductSales ADD Priority INT NOT NULL CONSTRAINT DF_ProductSales_Priority DEFAULT 100;

IF COL_LENGTH('dbo.ProductSales', 'AllowStackVoucher') IS NULL
    ALTER TABLE dbo.ProductSales ADD AllowStackVoucher BIT NOT NULL CONSTRAINT DF_ProductSales_AllowStackVoucher DEFAULT 0;";

        public const string GetProducts = @"
SELECT ProductID, ProductCode, ProductName
FROM dbo.Products
WHERE IsActive = 1
ORDER BY ProductName;";

        public const string GetVouchers = @"
SELECT
    VoucherID,
    VoucherCode,
    Description,
    DiscountType,
    DiscountValue,
    MinOrderValue,
    MaxDiscount,
    AllowStackDiscount,
    Priority,
    IsActive,
    StartDate,
    EndDate
FROM dbo.Vouchers
ORDER BY CreatedAt DESC, VoucherID DESC;";

        public const string GetProductSales = @"
SELECT
    ps.SaleID,
    ps.ProductID,
    p.ProductCode,
    p.ProductName,
    ps.SaleName,
    ps.DiscountType,
    ps.DiscountValue,
    ps.SalePrice,
    ps.AllowStackVoucher,
    ps.Priority,
    ps.IsActive,
    ps.StartDate,
    ps.EndDate
FROM dbo.ProductSales ps
INNER JOIN dbo.Products p ON p.ProductID = ps.ProductID
ORDER BY ps.CreatedAt DESC, ps.SaleID DESC;";

        public const string InsertVoucher = @"
INSERT INTO dbo.Vouchers
    (VoucherCode, Description, DiscountType, DiscountValue, MinOrderValue, MaxDiscount, StartDate, EndDate, AllowStackDiscount, Priority, IsActive, CreatedAt)
VALUES
    (@VoucherCode, @Description, @DiscountType, @DiscountValue, @MinOrderValue, @MaxDiscount, @StartDate, @EndDate, @AllowStackDiscount, @Priority, @IsActive, GETDATE());";

        public const string UpdateVoucher = @"
UPDATE dbo.Vouchers
SET Description = @Description,
    DiscountType = @DiscountType,
    DiscountValue = @DiscountValue,
    MinOrderValue = @MinOrderValue,
    MaxDiscount = @MaxDiscount,
    StartDate = @StartDate,
    EndDate = @EndDate,
    AllowStackDiscount = @AllowStackDiscount,
    Priority = @Priority,
    IsActive = @IsActive
WHERE VoucherID = @VoucherID;";

        public const string InsertProductSale = @"
INSERT INTO dbo.ProductSales
    (ProductID, SaleName, DiscountType, DiscountValue, SalePrice, StartDate, EndDate, AllowStackVoucher, Priority, IsActive, CreatedAt)
VALUES
    (@ProductID, @SaleName, @DiscountType, @DiscountValue, @SalePrice, @StartDate, @EndDate, @AllowStackVoucher, @Priority, @IsActive, GETDATE());";

        public const string UpdateProductSale = @"
UPDATE dbo.ProductSales
SET ProductID = @ProductID,
    SaleName = @SaleName,
    DiscountType = @DiscountType,
    DiscountValue = @DiscountValue,
    SalePrice = @SalePrice,
    StartDate = @StartDate,
    EndDate = @EndDate,
    AllowStackVoucher = @AllowStackVoucher,
    Priority = @Priority,
    IsActive = @IsActive
WHERE SaleID = @SaleID;";

        public const string DeleteVoucher = @"
DELETE FROM dbo.Vouchers WHERE VoucherID = @VoucherID;";

        public const string DeleteProductSale = @"
DELETE FROM dbo.ProductSales WHERE SaleID = @SaleID;";

        public const string CheckVoucherCodeExists = @"
SELECT COUNT(1) FROM dbo.Vouchers WHERE VoucherCode = @VoucherCode AND VoucherID <> @VoucherID;";
    }
}
