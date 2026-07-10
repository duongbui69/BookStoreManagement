namespace BookStoreManagement.Models
{
    public class ReturnReceiptDetail
    {
        public int Id { get; set; }
        public int ReturnReceiptId { get; set; }
        public int? SalesOrderDetailId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Cot tinh trong SQL, chi dung de doc du lieu, khong INSERT truc tiep.
        public decimal RefundAmount { get; set; }
        public string? ReturnReason { get; set; }
        public bool IsRestock { get; set; } = true;
    }
}
