using System;

namespace BookStoreManagement.ViewModels
{
    public class SalesOrderDetailFullViewModel
    {
        public int SalesOrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineTotal { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
