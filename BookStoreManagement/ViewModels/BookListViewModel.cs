using System;

namespace BookStoreManagement.ViewModels
{
    public class BookListViewModel
    {
        public int Id { get; set; }
        public string BookCode { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? PublishYear { get; set; }
        public int? PageCount { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public int? PublisherId { get; set; }
        public string? PublisherName { get; set; }
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
        public int MinStock { get; set; }
        public string StockStatus { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
