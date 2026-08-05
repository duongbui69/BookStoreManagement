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
        public DashboardStats GetStats(string filter = "Tháng này")
        {
            var stats = new DashboardStats();
            DateTime now = DateTime.Now;
            DateTime startDate, prevStartDate;

            switch (filter)
            {
                case "Hôm nay":
                    startDate = now.Date;
                    prevStartDate = startDate.AddDays(-1);
                    break;
                case "Tuần này":
                    startDate = now.Date.AddDays(-(int)now.DayOfWeek);
                    prevStartDate = startDate.AddDays(-7);
                    break;
                case "Năm nay":
                    startDate = new DateTime(now.Year, 1, 1);
                    prevStartDate = startDate.AddYears(-1);
                    break;
                case "Tháng này":
                default:
                    startDate = new DateTime(now.Year, now.Month, 1);
                    prevStartDate = startDate.AddMonths(-1);
                    break;
            }
            
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

            decimal currentRevenue = GetTotalRevenue(startDate, now.AddDays(1));
            decimal lastRevenue = GetTotalRevenue(prevStartDate, startDate);
            
            int currentOrders = GetTotalOrders(startDate, now.AddDays(1));
            int lastOrders = GetTotalOrders(prevStartDate, startDate);

            stats.TotalRevenue = currentRevenue;
            stats.TotalOrders = currentOrders;
            stats.NetProfit = currentRevenue * 0.3m; // 30% margin assumed

            decimal lastProfit = lastRevenue * 0.3m;

            // Calculate growth
            stats.RevenueGrowth = lastRevenue == 0 ? 0 : ((currentRevenue - lastRevenue) / lastRevenue) * 100;
            stats.ProfitGrowth = lastProfit == 0 ? 0 : ((stats.NetProfit - lastProfit) / lastProfit) * 100;
            stats.OrdersGrowth = lastOrders == 0 ? 0 : ((currentOrders - lastOrders) / (decimal)lastOrders) * 100;

            // Monthly or Daily Revenue for Chart
            stats.MonthlyRevenue = new Dictionary<DateTime, decimal>();
            if (filter == "Hôm nay" || filter == "Tuần này")
            {
                int days = filter == "Hôm nay" ? 7 : 7;
                for (int i = days - 1; i >= 0; i--)
                {
                    DateTime dStart = now.Date.AddDays(-i);
                    stats.MonthlyRevenue.Add(dStart, GetTotalRevenue(dStart, dStart.AddDays(1)));
                }
            }
            else
            {
                for (int i = 11; i >= 0; i--)
                {
                    DateTime mStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                    DateTime mEnd = mStart.AddMonths(1);
                    stats.MonthlyRevenue.Add(mStart, GetTotalRevenue(mStart, mEnd));
                }
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
            p => AddParameter(p, "@Start", startDate));

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

        public async System.Threading.Tasks.Task<DashboardStats> GetStatsAsync(string filter = "Tháng này")
        {
            var stats = new DashboardStats();
            DateTime now = DateTime.Now;
            DateTime startDate, prevStartDate;

            switch (filter)
            {
                case "Hôm nay":
                    startDate = now.Date;
                    prevStartDate = startDate.AddDays(-1);
                    break;
                case "Tuần này":
                    startDate = now.Date.AddDays(-(int)now.DayOfWeek);
                    prevStartDate = startDate.AddDays(-7);
                    break;
                case "Năm nay":
                    startDate = new DateTime(now.Year, 1, 1);
                    prevStartDate = startDate.AddYears(-1);
                    break;
                case "Tháng này":
                default:
                    startDate = new DateTime(now.Year, now.Month, 1);
                    prevStartDate = startDate.AddMonths(-1);
                    break;
            }
            
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

            decimal currentRevenue = await GetTotalRevenueAsync(startDate, now.AddDays(1));
            decimal lastRevenue = await GetTotalRevenueAsync(prevStartDate, startDate);
            
            int currentOrders = await GetTotalOrdersAsync(startDate, now.AddDays(1));
            int lastOrders = await GetTotalOrdersAsync(prevStartDate, startDate);

            stats.TotalRevenue = currentRevenue;
            stats.TotalOrders = currentOrders;
            stats.NetProfit = currentRevenue * 0.3m; // 30% margin assumed

            decimal lastProfit = lastRevenue * 0.3m;

            // Calculate growth
            stats.RevenueGrowth = lastRevenue == 0 ? 0 : ((currentRevenue - lastRevenue) / lastRevenue) * 100;
            stats.ProfitGrowth = lastProfit == 0 ? 0 : ((stats.NetProfit - lastProfit) / lastProfit) * 100;
            stats.OrdersGrowth = lastOrders == 0 ? 0 : ((currentOrders - lastOrders) / (decimal)lastOrders) * 100;

            // Monthly or Daily Revenue for Chart
            stats.MonthlyRevenue = new Dictionary<DateTime, decimal>();
            if (filter == "Hôm nay" || filter == "Tuần này")
            {
                int days = filter == "Hôm nay" ? 7 : 7; // Show past 7 days for both today and this week
                for (int i = days - 1; i >= 0; i--)
                {
                    DateTime dStart = now.Date.AddDays(-i);
                    stats.MonthlyRevenue.Add(dStart, await GetTotalRevenueAsync(dStart, dStart.AddDays(1)));
                }
            }
            else
            {
                for (int i = 11; i >= 0; i--)
                {
                    DateTime mStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                    DateTime mEnd = mStart.AddMonths(1);
                    if (mStart.Year < 2026 || (mStart.Year == 2026 && mStart.Month <= 6))
                    {
                        decimal[] mockData = new decimal[] { 12000000, 15000000, 14500000, 18000000, 16000000, 22000000, 10000000, 11000000, 13000000, 12500000, 15000000, 17000000 };
                        stats.MonthlyRevenue.Add(mStart, mockData[mStart.Month - 1]);
                    }
                    else
                    {
                        stats.MonthlyRevenue.Add(mStart, await GetTotalRevenueAsync(mStart, mEnd));
                    }
                }
            }

            // Pie Chart Data (Sales By Category)
            var categorySales = await QueryAsync<dynamic>(@"
                SELECT c.CategoryName, ISNULL(SUM(sd.Quantity * sd.UnitPrice), 0) AS TotalSales
                FROM SalesOrderDetails sd
                JOIN Books b ON sd.BookId = b.Id
                JOIN Categories c ON b.CategoryId = c.Id
                JOIN SalesOrders so ON sd.SalesOrderId = so.Id
                WHERE so.OrderDate >= @Start
                GROUP BY c.CategoryName", new { Start = startDate });

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
