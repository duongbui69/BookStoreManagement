using System;

namespace BookStoreManagement.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public int? StoreId { get; set; }
        public int RoleId { get; set; }
        public string? IdentityNumber { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? HireDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Dung khi JOIN Roles / Stores hoac doc tu vw_UserList
        public string RoleName { get; set; } = string.Empty;
        public string? StoreName { get; set; }
    }
}
