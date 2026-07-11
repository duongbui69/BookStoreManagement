using System;

namespace BookStoreManagement.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }
        public string BookCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string PublisherName { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
        public int MinStock { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
    }
}
