using System;

namespace BookStoreManagement.ViewModels
{
    public class ReturnReceiptDetailFullViewModel
    {
        public int ReturnReceiptId { get; set; }
        public string ReturnCode { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RefundAmount { get; set; }
        public string? ReturnReason { get; set; }
        public bool IsRestock { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public string? Note { get; set; }
        public string ReturnStatus { get; set; } = string.Empty;
    }
}
