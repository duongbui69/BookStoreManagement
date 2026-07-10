using System;

namespace BookStoreManagement.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string BookCode { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? PublishYear { get; set; }
        public int? PageCount { get; set; }
        public int CategoryId { get; set; }
        public int? AuthorId { get; set; }
        public int? PublisherId { get; set; }
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
        public int MinStock { get; set; } = 5;
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
