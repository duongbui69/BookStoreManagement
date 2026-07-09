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
        public static bool IsAdmin => RoleName == AppConstants.Roles.Admin;
        public static bool IsStaff => RoleName == AppConstants.Roles.Staff;

        public static void Set(User user, string roleName)
        {
            UserId = user.Id;
            RoleId = user.RoleId;
            RoleName = roleName;
            Username = user.Username;
            FullName = user.FullName;
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
