using System;

namespace BookStoreManagement.ViewModels
{
    public class SalesOrderListViewModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
