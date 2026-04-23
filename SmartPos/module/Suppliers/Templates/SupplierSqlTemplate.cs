namespace SmartPos.Module.Suppliers.Templates
{
    internal static class SupplierSqlTemplate
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
END;";

        public const string GetSuppliers = @"
SELECT
    s.SupplierID,
    s.SupplierName,
    s.Phone,
    s.Address,
    s.ImageUrl,
    ISNULL(debt.TotalDebt, 0) AS TotalDebt
FROM dbo.Suppliers s
OUTER APPLY (
    SELECT
        SUM(CASE WHEN po.TotalAmount - ISNULL(paid.PaidAmount, 0) > 0
                 THEN po.TotalAmount - ISNULL(paid.PaidAmount, 0)
                 ELSE 0 END) AS TotalDebt
    FROM dbo.PurchaseOrders po
    OUTER APPLY (
        SELECT SUM(sp.Amount) AS PaidAmount
        FROM dbo.SupplierPayment sp
        WHERE sp.PurchaseOrderID = po.PurchaseOrderID
    ) paid
    WHERE po.SupplierID = s.SupplierID
      AND po.Status <> 3
) debt
WHERE s.IsActive = 1
ORDER BY s.SupplierName;";

        public const string GetSupplierOrders = @"
SELECT
    po.PurchaseOrderID,
    po.POCode,
    po.OrderDate,
    po.TotalAmount,
    ISNULL(paid.PaidAmount, 0) AS PaidAmount,
    po.TotalAmount - ISNULL(paid.PaidAmount, 0) AS DebtAmount,
    CASE
        WHEN po.TotalAmount - ISNULL(paid.PaidAmount, 0) <= 0 THEN N'Đã thanh toán'
        WHEN ISNULL(paid.PaidAmount, 0) = 0 THEN N'Chưa thanh toán'
        ELSE N'Thanh toán một phần'
    END AS StatusText
FROM dbo.PurchaseOrders po
OUTER APPLY (
    SELECT SUM(sp.Amount) AS PaidAmount
    FROM dbo.SupplierPayment sp
    WHERE sp.PurchaseOrderID = po.PurchaseOrderID
) paid
WHERE po.SupplierID = @SupplierID
  AND po.Status <> 3
ORDER BY po.OrderDate DESC;";

        public const string GetCurrentDebtByOrder = @"
SELECT
    po.TotalAmount - ISNULL(paid.PaidAmount, 0) AS DebtAmount
FROM dbo.PurchaseOrders po
OUTER APPLY (
    SELECT SUM(sp.Amount) AS PaidAmount
    FROM dbo.SupplierPayment sp
    WHERE sp.PurchaseOrderID = po.PurchaseOrderID
) paid
WHERE po.PurchaseOrderID = @PurchaseOrderID
  AND po.SupplierID = @SupplierID;";

        public const string InsertPayment = @"
INSERT INTO dbo.SupplierPayment
    (SupplierID, PurchaseOrderID, Amount, PaymentMethod, PaymentDate, Note, CreatedByUserID)
VALUES
    (@SupplierID, @PurchaseOrderID, @Amount, @PaymentMethod, GETDATE(), @Note, @CreatedByUserID);";
    }
}
