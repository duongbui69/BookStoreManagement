using System;

namespace BookStoreManagement.Models
{
    public class ReturnReceipt
    {
        public int Id { get; set; }
        public string ReturnCode { get; set; } = string.Empty;
        public int? SalesOrderId { get; set; }
        public int StoreId { get; set; }
        public int? CustomerId { get; set; }
        public int UserId { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public string? Note { get; set; }
        public string ReturnStatus { get; set; } = "Chờ xử lý";
    }
}
