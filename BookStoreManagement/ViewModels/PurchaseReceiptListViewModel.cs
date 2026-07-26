using System;

namespace BookStoreManagement.ViewModels
{
    public class PurchaseReceiptListViewModel
    {
        public int Id { get; set; }
        public string ReceiptCode { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public DateTime ImportDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = "Đã nhập";
    }
}
