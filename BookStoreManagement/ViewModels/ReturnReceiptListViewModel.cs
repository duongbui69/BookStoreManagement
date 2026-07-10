using System;

namespace BookStoreManagement.ViewModels
{
    public class ReturnReceiptListViewModel
    {
        public int Id { get; set; }
        public string ReturnCode { get; set; } = string.Empty;
        public int? SalesOrderId { get; set; }
        public string? OrderCode { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public string? Note { get; set; }
    }
}
