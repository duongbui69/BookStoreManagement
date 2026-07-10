using System;

namespace BookStoreManagement.ViewModels
{
    public class DailyRevenueViewModel
    {
        public DateTime RevenueDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
