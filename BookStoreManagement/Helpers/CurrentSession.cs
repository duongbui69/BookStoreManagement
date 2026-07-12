using System;
using BookStoreManagement.Models;

namespace BookStoreManagement.Helpers
{
    public static class CurrentSession
    {
        public static int UserId { get; private set; }
        public static int RoleId { get; private set; }
        public static string RoleName { get; private set; } = string.Empty;
        public static string Username { get; private set; } = string.Empty;
        public static string FullName { get; private set; } = string.Empty;

        public static bool IsLoggedIn => UserId > 0;
        public static bool IsAdmin => RoleId == 1; // Assuming 1 is Admin
        public static bool IsStaff => RoleId == 2; // Assuming 2 is Staff

        public static void SetCurrentUser(User user)
        {
            UserId = user.Id;
            Username = user.Username;
            FullName = user.FullName;
            RoleId = user.RoleId;
        }

        public static void Clear()
        {
            UserId = 0;
            RoleId = 0;
            RoleName = string.Empty;
            Username = string.Empty;
            FullName = string.Empty;
        }
    }
}
