using System;

namespace BookStoreManagement.ViewModels
{
    public class UserListViewModel
    {
        public int Id { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? IdentityNumber { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? HireDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
