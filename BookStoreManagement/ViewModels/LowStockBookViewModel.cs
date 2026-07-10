namespace BookStoreManagement.ViewModels
{
    public class LowStockBookViewModel
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string BookCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinStock { get; set; }
    }
}
