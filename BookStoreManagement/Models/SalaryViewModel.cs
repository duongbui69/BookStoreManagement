using System;

namespace BookStoreManagement.Models
{
    public class SalaryViewModel
    {
        public int StaffId { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalSalary => HourlyRate * (decimal)Math.Round(TotalHours, 2);
    }
}
