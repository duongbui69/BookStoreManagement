using System;

namespace BookStoreManagement.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}