namespace SmartPos.Module.Products.Templates
{
    internal static class ProductSqlTemplate
    {
        public const string GetProducts = @"
SELECT 
    p.ProductID, 
    p.ProductCode, 
    p.ProductName, 
    c.CategoryName, 
    p.RetailPrice, 
    p.Location, 
    p.IsActive,
    ISNULL(inv.TotalQty, 0) as StockQuantity
FROM dbo.Products p
INNER JOIN dbo.Categories c ON p.CategoryID = c.CategoryID
LEFT JOIN (
    SELECT ProductID, SUM(Quantity) as TotalQty 
    FROM dbo.Inventory 
    GROUP BY ProductID
) inv ON p.ProductID = inv.ProductID
WHERE (@Search IS NULL OR p.ProductName LIKE @Search OR p.ProductCode LIKE @Search OR p.Barcode LIKE @Search)
  AND (@CategoryID = 0 OR p.CategoryID = @CategoryID)
ORDER BY p.ProductName;";

        public const string GetProductDetail = @"
SELECT * FROM dbo.Products WHERE ProductID = @ProductID;";

        public const string InsertProduct = @"
INSERT INTO dbo.Products (
    CategoryID, SupplierID, BaseUnitID, ProductCode, Barcode, 
    ProductName, Description, ImageUrl, CostPrice, RetailPrice, 
    WholesalePrice, MinStockLevel, Weight, Location, IsActive, 
    HasExpiry, CreatedAt
) VALUES (
    @CategoryID, @SupplierID, @BaseUnitID, @ProductCode, @Barcode, 
    @ProductName, @Description, @ImageUrl, @CostPrice, @RetailPrice, 
    @WholesalePrice, 0, @Weight, @Location, @IsActive, 
    @HasExpiry, GETDATE()
);";

        public const string UpdateProduct = @"
UPDATE dbo.Products SET
    CategoryID = @CategoryID,
    SupplierID = @SupplierID,
    BaseUnitID = @BaseUnitID,
    ProductCode = @ProductCode,
    Barcode = @Barcode,
    ProductName = @ProductName,
    Description = @Description,
    ImageUrl = @ImageUrl,
    CostPrice = @CostPrice,
    RetailPrice = @RetailPrice,
    WholesalePrice = @WholesalePrice,
    Weight = @Weight,
    Location = @Location,
    IsActive = @IsActive,
    HasExpiry = @HasExpiry,
    UpdatedAt = GETDATE()
WHERE ProductID = @ProductID;";

        public const string DeleteProduct = @"UPDATE dbo.Products SET IsActive = 0 WHERE ProductID = @ProductID;";

        public const string EnsureCategorySchema = @"
IF COL_LENGTH('dbo.Categories', 'ParentID') IS NULL
    ALTER TABLE dbo.Categories ADD ParentID INT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Categories_Parent')
    ALTER TABLE dbo.Categories ADD CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentID) REFERENCES dbo.Categories(CategoryID);";

        public const string GetCategories = @"
SELECT c.*, p.CategoryName as ParentName
FROM dbo.Categories c
LEFT JOIN dbo.Categories p ON c.ParentID = p.CategoryID
ORDER BY c.ParentID, c.CategoryName;";

        public const string InsertCategory = @"
INSERT INTO dbo.Categories (CategoryName, ParentID, Description, IsActive)
VALUES (@CategoryName, @ParentID, @Description, @IsActive);";

        public const string UpdateCategory = @"
UPDATE dbo.Categories SET
    CategoryName = @CategoryName,
    ParentID = @ParentID,
    Description = @Description,
    IsActive = @IsActive
WHERE CategoryID = @CategoryID;";

        public const string DeleteCategory = @"UPDATE dbo.Categories SET IsActive = 0 WHERE CategoryID = @CategoryID;";

        public const string HasProducts = @"SELECT TOP 1 1 FROM dbo.Products WHERE CategoryID = @CategoryID;";

        public const string GetSuppliersLookup = @"SELECT SupplierID, SupplierName FROM dbo.Suppliers WHERE IsActive = 1 ORDER BY SupplierName;";

        public const string GetUnitsLookup = @"SELECT UnitID, UnitName FROM dbo.Units ORDER BY UnitName;";
    }
}
