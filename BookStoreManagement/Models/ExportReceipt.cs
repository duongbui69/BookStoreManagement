using System;

namespace BookStoreManagement.Models
{
    public class ExportReceipt
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public string ReceiptCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime ExportDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Completed";
        public string? Note { get; set; }
    }
}
