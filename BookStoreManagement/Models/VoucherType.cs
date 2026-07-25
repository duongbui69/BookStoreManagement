using System;

namespace BookStoreManagement.Models
{
    public class VoucherType
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string GroupType { get; set; } = "Other";
        public string Status { get; set; } = "Active";
    }
}
