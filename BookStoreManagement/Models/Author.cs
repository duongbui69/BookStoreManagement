using System;

namespace BookStoreManagement.Models
{
    public class Author
    {
        public int Id { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}