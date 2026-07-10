namespace BookStoreManagement.Models
{
    public class SalesOrderDetail
    {
        public int Id { get; set; }
        public int SalesOrderId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }

        // Cot tinh trong SQL, chi dung de doc du lieu, khong INSERT truc tiep.
        public decimal LineTotal { get; set; }
    }
}
