using System;

namespace BookStoreManagement.Models
{
    public class SalesOrder
    {
        public int Id { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public int UserId { get; set; }

        public int? CustomerId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string PaymentMethod { get; set; } = AppConstants.PaymentMethods.Cash;

        public string OrderStatus { get; set; } = AppConstants.OrderStatuses.Completed;

        public string? Note { get; set; }
    }
}