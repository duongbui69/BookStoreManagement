using System;

namespace BookStoreManagement.ViewModels
{
    public class StoreBookInventoryViewModel
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public string StoreCode { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookCode { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? AuthorName { get; set; }
        public string? PublisherName { get; set; }
        public int Quantity { get; set; }
        public int MinStock { get; set; }
        public decimal? ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string StockStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? ShelfLocation { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
