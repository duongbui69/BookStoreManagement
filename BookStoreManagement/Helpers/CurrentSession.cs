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
        public static bool ViewAsStaff { get; set; } = false;
        public static bool IsAdmin => RoleId == 1 && !ViewAsStaff; // Assuming 1 is Admin
        public static bool IsStaff => RoleId == 2 || (RoleId == 1 && ViewAsStaff); // Assuming 2 is Staff

        public static void SetCurrentUser(User user)
        {
            UserId = user.Id;
            UserCode = user.UserCode ?? string.Empty;
            StoreId = user.StoreId ?? 1; // Default to 1 if not assigned
            StoreName = user.StoreName ?? "Main Store";
            RoleId = user.RoleId;
            RoleName = user.RoleName ?? string.Empty;
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
            ViewAsStaff = false;
        }
    }
}
