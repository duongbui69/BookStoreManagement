using System;

namespace BookStoreManagement.ViewModels
{
    public class InventoryHistoryViewModel
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int QuantityChange { get; set; }
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
