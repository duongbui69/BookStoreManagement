using System;

namespace BookStoreManagement.Models
{
    public class ExportReceiptDetail
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal { get; set; }
    }
}
