using System;

namespace BookStoreManagement.Models
{
    public class Store
    {
        public int Id { get; set; }
        public string StoreCode { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
