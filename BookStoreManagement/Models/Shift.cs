using System;

namespace BookStoreManagement.Models
{
    public class Shift
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public int StoreId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal InitialCash { get; set; }
        public decimal Revenue { get; set; }
        public int TotalOrders { get; set; }
        public string Status { get; set; } = "Open";
    }
}
