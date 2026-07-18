using System;

namespace BookStoreManagement.ViewModels
{
    public class ExportReceiptListViewModel
    {
        public int Id { get; set; }
        public string ReceiptCode { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime ExportDate { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
