using System;

namespace BookStoreManagement.Models
{
    public class PurchaseReceipt
    {
        public int Id { get; set; }
        public string ReceiptCode { get; set; } = string.Empty;
        public int StoreId { get; set; } = 1;
        public int SupplierId { get; set; }
        public int UserId { get; set; }
        public DateTime ImportDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = "Đã nhập";
    }
}
