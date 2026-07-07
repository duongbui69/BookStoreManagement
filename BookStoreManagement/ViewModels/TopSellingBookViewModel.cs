namespace BookStoreManagement.ViewModels
{
    public class TopSellingBookViewModel
    {
        public int BookId { get; set; }

        public string BookCode { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public int TotalSold { get; set; }

        public decimal TotalRevenue { get; set; }
    }
}