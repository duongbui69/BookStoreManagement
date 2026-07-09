using System;
using System.Collections.Generic;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class ReportRepository : RepositoryBase
    {
        public decimal GetTodayRevenue()
        {
            const string sql = @"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM SalesOrders
                WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)
                  AND OrderStatus = 'Completed';
            ";
            return ExecuteScalarDecimal(sql);
        }

        public decimal GetMonthRevenue()
        {
            const string sql = @"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM SalesOrders
                WHERE MONTH(OrderDate) = MONTH(GETDATE())
                  AND YEAR(OrderDate) = YEAR(GETDATE())
                  AND OrderStatus = 'Completed';
            ";
            return ExecuteScalarDecimal(sql);
        }

        public decimal GetTotalRevenue()
        {
            const string sql = @"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM SalesOrders
                WHERE OrderStatus = 'Completed';
            ";
            return ExecuteScalarDecimal(sql);
        }

        public decimal GetRevenueByDateRange(DateTime fromDate, DateTime toDate)
        {
            const string sql = @"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM SalesOrders
                WHERE OrderStatus = 'Completed'
                  AND OrderDate >= @FromDate
                  AND OrderDate < DATEADD(DAY, 1, @ToDate);
            ";
            return ExecuteScalarDecimal(sql, parameters =>
            {
                AddParameter(parameters, "@FromDate", fromDate.Date);
                AddParameter(parameters, "@ToDate", toDate.Date);
            });
        }

        public int GetTotalOrders()
        {
            const string sql = "SELECT COUNT(*) FROM SalesOrders;";
            return ExecuteScalarInt(sql);
        }

        public int GetCompletedOrders()
        {
            const string sql = "SELECT COUNT(*) FROM SalesOrders WHERE OrderStatus = 'Completed';";
            return ExecuteScalarInt(sql);
        }

        public int GetCancelledOrders()
        {
            const string sql = "SELECT COUNT(*) FROM SalesOrders WHERE OrderStatus = 'Cancelled';";
            return ExecuteScalarInt(sql);
        }

        public int GetTotalBooks()
        {
            const string sql = "SELECT COUNT(*) FROM Books WHERE IsActive = 1;";
            return ExecuteScalarInt(sql);
        }

        public int GetTotalUsers()
        {
            const string sql = "SELECT COUNT(*) FROM Users WHERE IsActive = 1;";
            return ExecuteScalarInt(sql);
        }

        public int GetTotalCustomers()
        {
            const string sql = "SELECT COUNT(*) FROM Customers;";
            return ExecuteScalarInt(sql);
        }

        public List<DailyRevenueViewModel> GetDailyRevenue()
        {
            var list = new List<DailyRevenueViewModel>();
            const string sql = @"
                SELECT RevenueDate, TotalOrders, TotalRevenue
                FROM vw_DailyRevenue
                ORDER BY RevenueDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new DailyRevenueViewModel
                    {
                        RevenueDate = GetDateTime(reader, "RevenueDate"),
                        TotalOrders = GetInt(reader, "TotalOrders"),
                        TotalRevenue = GetDecimal(reader, "TotalRevenue")
                    });
                }
                return list;
            }, sql);
        }

        public List<DailyRevenueViewModel> GetDailyRevenueByDateRange(DateTime fromDate, DateTime toDate)
        {
            var list = new List<DailyRevenueViewModel>();
            const string sql = @"
                SELECT RevenueDate, TotalOrders, TotalRevenue
                FROM vw_DailyRevenue
                WHERE RevenueDate >= @FromDate
                  AND RevenueDate <= @ToDate
                ORDER BY RevenueDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new DailyRevenueViewModel
                    {
                        RevenueDate = GetDateTime(reader, "RevenueDate"),
                        TotalOrders = GetInt(reader, "TotalOrders"),
                        TotalRevenue = GetDecimal(reader, "TotalRevenue")
                    });
                }
                return list;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@FromDate", fromDate.Date);
                AddParameter(parameters, "@ToDate", toDate.Date);
            });
        }

        public List<TopSellingBookViewModel> GetTopSellingBooks(int top = 10)
        {
            var list = new List<TopSellingBookViewModel>();
            const string sql = @"
                SELECT TOP (@Top) BookId, BookCode, Title, TotalSold, TotalRevenue
                FROM vw_TopSellingBooks
                ORDER BY TotalSold DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new TopSellingBookViewModel
                    {
                        BookId = GetInt(reader, "BookId"),
                        BookCode = GetString(reader, "BookCode"),
                        Title = GetString(reader, "Title"),
                        TotalSold = GetInt(reader, "TotalSold"),
                        TotalRevenue = GetDecimal(reader, "TotalRevenue")
                    });
                }
                return list;
            }, sql, parameters => AddParameter(parameters, "@Top", top));
        }
    }
}
