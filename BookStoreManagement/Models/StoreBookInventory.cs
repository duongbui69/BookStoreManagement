using System;

namespace BookStoreManagement.Models
{
    public class StoreBookInventory
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public int MinStock { get; set; } = 5;
        public decimal? ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
