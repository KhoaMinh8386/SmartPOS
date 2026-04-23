using System;

namespace SmartPos
{
    public class UserSessionInfo
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
    }

    public static class UserSession
    {
        public static UserSessionInfo CurrentUser { get; set; }

        public static bool IsLoggedIn
        {
            get { return CurrentUser != null; }
        }

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}
