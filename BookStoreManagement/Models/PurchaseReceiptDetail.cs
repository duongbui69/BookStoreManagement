namespace BookStoreManagement.Models
{
    public class PurchaseReceiptDetail
    {
        public int Id { get; set; }

        public int PurchaseReceiptId { get; set; }

        public int BookId { get; set; }

        public int Quantity { get; set; }

        public decimal ImportPrice { get; set; }

        public decimal LineTotal { get; set; }
    }
}