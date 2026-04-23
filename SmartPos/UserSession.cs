using System;

namespace SmartPos
{
    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        Cashier = 3
    }

    public class UserSessionInfo
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }

        public UserRole Role => (UserRole)RoleID;
    }

    public static class UserSession
    {
        public static UserSessionInfo CurrentUser { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public static bool IsManager => CurrentUser?.Role == UserRole.Manager || IsAdmin;
        public static bool IsCashier => CurrentUser?.Role == UserRole.Cashier || IsManager;

        /// <summary>
        /// Kiểm tra quyền dựa trên permission key. 
        /// Có thể mở rộng để check trong database table Permissions nếu cần phức tạp hơn.
        /// </summary>
        public static bool HasPermission(string moduleName)
        {
            if (CurrentUser == null) return false;
            if (IsAdmin) return true; // Admin có mọi quyền

            switch (moduleName)
            {
                case "Reports":
                case "SystemConfig":
                case "UserManagement":
                    return IsAdmin; // Chỉ admin
                
                case "Inventory":
                case "Products":
                case "Suppliers":
                case "PurchaseOrders":
                    return IsManager; // Manager trở lên

                case "POS":
                case "Customers":
                    return IsCashier; // Cashier trở lên

                default:
                    return false;
            }
        }

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}
