using System;

namespace BookStoreManagement.Models
{
    public class InventoryTransaction
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public int? UserId { get; set; }

        public string TransactionType { get; set; } = string.Empty;

        public int QuantityChange { get; set; }

        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}