using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class LowStockItem
    {
        public int Id { get; set; }
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
        public Dictionary<DateTime, decimal> MonthlyRevenue { get; set; } = new Dictionary<DateTime, decimal>();
        public List<LowStockItem> LowStockItems { get; set; } = new List<LowStockItem>();
    }

    public class DashboardRepository : RepositoryBase
    {
        public DashboardStats GetStats()
        {
            var stats = new DashboardStats();

            DateTime now = DateTime.Now;
            DateTime currentMonthStart = new DateTime(now.Year, now.Month, 1);
            DateTime lastMonthStart = currentMonthStart.AddMonths(-1);
            
            // Helper to get sum
            decimal GetTotalRevenue(DateTime start, DateTime end)
            {
                return ExecuteScalarDecimal("SELECT ISNULL(SUM(TotalAmount), 0) FROM SalesOrders WHERE OrderDate >= @Start AND OrderDate < @End", 
                    p => {
                        AddParameter(p, "@Start", start);
                        AddParameter(p, "@End", end);
                    });
            }

            int GetTotalOrders(DateTime start, DateTime end)
            {
                return ExecuteScalarInt("SELECT COUNT(*) FROM SalesOrders WHERE OrderDate >= @Start AND OrderDate < @End", 
                    p => {
                        AddParameter(p, "@Start", start);
                        AddParameter(p, "@End", end);
                    });
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
                stats.MonthlyRevenue.Add(mStart, GetTotalRevenue(mStart, mEnd));
            }

            // Pie Chart Data (Sales By Category)
            ExecuteQuery(cmd => {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    stats.SalesByCategory.Add(reader.GetString(0), reader.GetDecimal(1));
                }
                return true;
            }, @"
                SELECT c.CategoryName, ISNULL(SUM(sd.Quantity * sd.UnitPrice), 0)
                FROM SalesOrderDetails sd
                JOIN Books b ON sd.BookId = b.Id
                JOIN Categories c ON b.CategoryId = c.Id
                JOIN SalesOrders so ON sd.SalesOrderId = so.Id
                WHERE so.OrderDate >= @Start
                GROUP BY c.CategoryName", 
            p => AddParameter(p, "@Start", currentMonthStart));

            // Low Stock Items
            ExecuteQuery(cmd => {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    stats.LowStockItems.Add(new LowStockItem
                    {
                        Id = reader.GetInt32(0),
                        BookCode = reader.GetString(1),
                        Title = reader.GetString(2),
                        Author = reader.IsDBNull(3) ? "Unknown" : reader.GetString(3),
                        Category = reader.IsDBNull(4) ? "Unknown" : reader.GetString(4),
                        CurrentStock = reader.GetInt32(5),
                        Threshold = reader.GetInt32(6)
                    });
                }
                return true;
            }, @"
                SELECT b.Id, b.BookCode, b.Title, a.AuthorName, c.CategoryName, b.Quantity, b.MinStock
                FROM Books b
                LEFT JOIN Authors a ON b.AuthorId = a.Id
                LEFT JOIN Categories c ON b.CategoryId = c.Id
                WHERE b.Quantity <= b.MinStock AND b.IsActive = 1
                ORDER BY b.Quantity ASC");

            stats.LowStockCount = stats.LowStockItems.Count;

            return stats;
        }

        public async System.Threading.Tasks.Task<DashboardStats> GetStatsAsync()
        {
            var stats = new DashboardStats();

            DateTime now = DateTime.Now;
            DateTime currentMonthStart = new DateTime(now.Year, now.Month, 1);
            DateTime lastMonthStart = currentMonthStart.AddMonths(-1);
            
            // Helper to get sum
            async System.Threading.Tasks.Task<decimal> GetTotalRevenueAsync(DateTime start, DateTime end)
            {
                var result = await ExecuteScalarAsync<decimal?>("SELECT ISNULL(SUM(TotalAmount), 0) FROM SalesOrders WHERE OrderDate >= @Start AND OrderDate < @End", new { Start = start, End = end });
                return result ?? 0m;
            }

            async System.Threading.Tasks.Task<int> GetTotalOrdersAsync(DateTime start, DateTime end)
            {
                return await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM SalesOrders WHERE OrderDate >= @Start AND OrderDate < @End", new { Start = start, End = end });
            }

            decimal currentRevenue = await GetTotalRevenueAsync(currentMonthStart, now);
            decimal lastRevenue = await GetTotalRevenueAsync(lastMonthStart, currentMonthStart);
            
            int currentOrders = await GetTotalOrdersAsync(currentMonthStart, now);
            int lastOrders = await GetTotalOrdersAsync(lastMonthStart, currentMonthStart);

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
                stats.MonthlyRevenue.Add(mStart, await GetTotalRevenueAsync(mStart, mEnd));
            }

            // Pie Chart Data (Sales By Category)
            var categorySales = await QueryAsync<dynamic>(@"
                SELECT c.CategoryName, ISNULL(SUM(sd.Quantity * sd.UnitPrice), 0) AS TotalSales
                FROM SalesOrderDetails sd
                JOIN Books b ON sd.BookId = b.Id
                JOIN Categories c ON b.CategoryId = c.Id
                JOIN SalesOrders so ON sd.SalesOrderId = so.Id
                WHERE so.OrderDate >= @Start
                GROUP BY c.CategoryName", new { Start = currentMonthStart });

            foreach (var item in categorySales)
            {
                stats.SalesByCategory.Add((string)item.CategoryName, (decimal)item.TotalSales);
            }

            // Low Stock Items
            var lowStockItems = await QueryAsync<LowStockItem>(@"
                SELECT b.Id, b.BookCode, b.Title, ISNULL(a.AuthorName, 'Unknown') AS Author, ISNULL(c.CategoryName, 'Unknown') AS Category, b.Quantity AS CurrentStock, b.MinStock AS Threshold
                FROM Books b
                LEFT JOIN Authors a ON b.AuthorId = a.Id
                LEFT JOIN Categories c ON b.CategoryId = c.Id
                WHERE b.Quantity <= b.MinStock AND b.IsActive = 1
                ORDER BY b.Quantity ASC");

            stats.LowStockItems.AddRange(lowStockItems);
            stats.LowStockCount = stats.LowStockItems.Count;

            return stats;
        }
    }
}
