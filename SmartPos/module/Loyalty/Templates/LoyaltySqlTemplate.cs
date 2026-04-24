namespace SmartPos.Module.Loyalty.Templates
{
    public static class LoyaltySqlTemplate
    {
        public const string GetCustomersByTier = @"
            SELECT CustomerID, CustomerCode, FullName, Phone, Email, TotalPoints, TotalSpent, CustomerType, CreatedAt
            FROM dbo.Customers
            WHERE CustomerType = @Tier
            ORDER BY TotalPoints DESC";

        public const string GetCustomersNearTier = @"
            SELECT CustomerID, CustomerCode, FullName, Phone, Email, TotalPoints, TotalSpent, CustomerType, CreatedAt
            FROM dbo.Customers
            WHERE (CustomerType = N'Thường' AND TotalPoints >= @ThanThietPointsThreshold - 2 AND TotalPoints < @ThanThietPointsThreshold)
               OR (CustomerType = N'Thân Thiết' AND TotalPoints >= @VipPointsThreshold - 2 AND TotalPoints < @VipPointsThreshold)
            ORDER BY TotalPoints DESC";

        public const string UpdateCustomerPoints = @"
            UPDATE dbo.Customers
            SET TotalPoints = TotalPoints + @PointsAdded,
                TotalSpent = TotalSpent + @AmountSpent
            WHERE CustomerID = @CustomerID";

        public const string UpdateCustomerTier = @"
            UPDATE dbo.Customers
            SET CustomerType = @NewTier
            WHERE CustomerID = @CustomerID";

        public const string GetCustomerById = @"
            SELECT CustomerID, CustomerCode, FullName, Phone, Email, TotalPoints, TotalSpent, CustomerType, CreatedAt
            FROM dbo.Customers
            WHERE CustomerID = @CustomerID";

        public const string GetSmtpSettings = @"
            SELECT SettingKey, SettingValue
            FROM dbo.Settings
            WHERE SettingKey IN ('SmtpServer', 'SmtpPort', 'SmtpUser', 'SmtpPass', 'SmtpEnableSsl')";
    }
}
