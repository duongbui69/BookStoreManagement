using System;

namespace BookStoreManagement.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string? IdentityNumber { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public int Points { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
