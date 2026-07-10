using System;

namespace BookStoreManagement.ViewModels
{
    public class DailyRevenueByStoreViewModel
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public DateTime RevenueDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
