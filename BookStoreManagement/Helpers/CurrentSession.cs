using System;
using BookStoreManagement.Models;

namespace BookStoreManagement.Helpers
{
    public static class CurrentSession
    {
        public static int UserId { get; private set; }
        public static string UserCode { get; private set; } = string.Empty;
        public static int? StoreId { get; private set; }
        public static string? StoreName { get; private set; }
        public static int RoleId { get; private set; }
        public static string RoleName { get; private set; } = string.Empty;
        public static string Username { get; private set; } = string.Empty;
        public static string FullName { get; private set; } = string.Empty;

        public static bool IsLoggedIn => UserId > 0;
        public static bool IsAdmin => RoleName.Equals(AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase);
        public static bool IsStaff => RoleName.Equals(AppConstants.Roles.Staff, StringComparison.OrdinalIgnoreCase);

        public static void SetCurrentUser(User user)
        {
            UserId = user.Id;
            UserCode = user.UserCode;
            StoreId = user.StoreId;
            StoreName = user.StoreName;
            RoleId = user.RoleId;
            RoleName = user.RoleName;
            Username = user.Username;
            FullName = user.FullName;
        }

        public static void Clear()
        {
            UserId = 0;
            UserCode = string.Empty;
            StoreId = null;
            StoreName = null;
            RoleId = 0;
            RoleName = string.Empty;
            Username = string.Empty;
            FullName = string.Empty;
        }
    }
}
