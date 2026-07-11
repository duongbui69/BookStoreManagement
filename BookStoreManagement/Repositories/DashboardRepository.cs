using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class LowStockItem
    {
        public string BookCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
    }

    public class DashboardStats
    {
        public decimal TotalRevenue { get; set; }
        public decimal RevenueGrowth { get; set; }

        public decimal NetProfit { get; set; }
        public decimal ProfitGrowth { get; set; }

        public int TotalOrders { get; set; }
        public decimal OrdersGrowth { get; set; }

        public int LowStockCount { get; set; }
        
        public Dictionary<string, decimal> SalesByCategory { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<int, decimal> MonthlyRevenue { get; set; } = new Dictionary<int, decimal>();
        public List<LowStockItem> LowStockItems { get; set; } = new List<LowStockItem>();
    }

    public class DashboardRepository
    {
        public DashboardStats GetStats()
        {
            var stats = new DashboardStats();
            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            DateTime now = DateTime.Now;
            DateTime currentMonthStart = new DateTime(now.Year, now.Month, 1);
            DateTime lastMonthStart = currentMonthStart.AddMonths(-1);
            
            // Helper to get sum
            decimal GetTotalRevenue(DateTime start, DateTime end)
            {
                using var cmd = new SqlCommand("SELECT ISNULL(SUM(TotalAmount), 0) FROM SalesOrders WHERE OrderDate >= @Start AND OrderDate < @End", connection);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }

            int GetTotalOrders(DateTime start, DateTime end)
            {
                using var cmd = new SqlCommand("SELECT COUNT(*) FROM SalesOrders WHERE OrderDate >= @Start AND OrderDate < @End", connection);
                cmd.Parameters.AddWithValue("@Start", start);
                cmd.Parameters.AddWithValue("@End", end);
                return (int)cmd.ExecuteScalar();
            }

            decimal currentRevenue = GetTotalRevenue(currentMonthStart, now);
            decimal lastRevenue = GetTotalRevenue(lastMonthStart, currentMonthStart);
            
            int currentOrders = GetTotalOrders(currentMonthStart, now);
            int lastOrders = GetTotalOrders(lastMonthStart, currentMonthStart);

            stats.TotalRevenue = currentRevenue;
            stats.TotalOrders = currentOrders;
            stats.NetProfit = currentRevenue * 0.3m; // 30% margin assumed

            decimal lastProfit = lastRevenue * 0.3m;

            // Calculate growth
            stats.RevenueGrowth = lastRevenue == 0 ? 0 : ((currentRevenue - lastRevenue) / lastRevenue) * 100;
            stats.ProfitGrowth = lastProfit == 0 ? 0 : ((stats.NetProfit - lastProfit) / lastProfit) * 100;
            stats.OrdersGrowth = lastOrders == 0 ? 0 : ((currentOrders - lastOrders) / (decimal)lastOrders) * 100;

            // Monthly Revenue for Line Chart (Past 12 months)
            for (int i = 11; i >= 0; i--)
            {
                DateTime mStart = currentMonthStart.AddMonths(-i);
                DateTime mEnd = mStart.AddMonths(1);
                stats.MonthlyRevenue.Add(mStart.Month, GetTotalRevenue(mStart, mEnd));
            }

            // Pie Chart Data (Sales By Category)
            using (var cmd = new SqlCommand(@"
                SELECT c.CategoryName, ISNULL(SUM(sd.Quantity * sd.UnitPrice), 0)
                FROM SalesOrderDetails sd
                JOIN Books b ON sd.BookId = b.Id
                JOIN Categories c ON b.CategoryId = c.Id
                JOIN SalesOrders so ON sd.SalesOrderId = so.Id
                WHERE so.OrderDate >= @Start
                GROUP BY c.CategoryName", connection))
            {
                cmd.Parameters.AddWithValue("@Start", currentMonthStart);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    stats.SalesByCategory.Add(reader.GetString(0), reader.GetDecimal(1));
                }
            }

            // Low Stock Items
            using (var cmd = new SqlCommand(@"
                SELECT b.BookCode, b.Title, a.AuthorName, c.CategoryName, b.Quantity, b.MinStock
                FROM Books b
                LEFT JOIN Authors a ON b.AuthorId = a.Id
                LEFT JOIN Categories c ON b.CategoryId = c.Id
                WHERE b.Quantity <= b.MinStock AND b.IsActive = 1
                ORDER BY b.Quantity ASC", connection))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    stats.LowStockItems.Add(new LowStockItem
                    {
                        BookCode = reader.GetString(0),
                        Title = reader.GetString(1),
                        Author = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
                        Category = reader.IsDBNull(3) ? "Unknown" : reader.GetString(3),
                        CurrentStock = reader.GetInt32(4),
                        Threshold = reader.GetInt32(5)
                    });
                }
                stats.LowStockCount = stats.LowStockItems.Count;
            }

            return stats;
        }
    }
}
