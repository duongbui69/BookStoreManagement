using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class ReportRepository : RepositoryBase
    {
        public decimal GetTodayRevenue(int? storeId = null)
        {
            string sql = @"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM SalesOrders
                WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)
                  AND OrderStatus = 'Completed'
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";

            return ExecuteScalarDecimal(sql, parameters =>
            {
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public decimal GetMonthRevenue(int? storeId = null)
        {
            string sql = @"
                SELECT ISNULL(SUM(TotalAmount), 0)
                FROM SalesOrders
                WHERE MONTH(OrderDate) = MONTH(GETDATE())
                  AND YEAR(OrderDate) = YEAR(GETDATE())
                  AND OrderStatus = 'Completed'
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";

            return ExecuteScalarDecimal(sql, parameters =>
            {
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public int GetTotalOrders(int? storeId = null)
        {
            string sql = "SELECT COUNT(*) FROM SalesOrders WHERE 1 = 1";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";

            return ExecuteScalarInt(sql, parameters =>
            {
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
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
            const string sql = "SELECT COUNT(*) FROM Customers WHERE IsActive = 1;";
            return ExecuteScalarInt(sql);
        }

        public int GetTotalStores()
        {
            const string sql = "SELECT COUNT(*) FROM Stores WHERE IsActive = 1;";
            return ExecuteScalarInt(sql);
        }

        public List<DailyRevenueViewModel> GetDailyRevenue()
        {
            var list = new List<DailyRevenueViewModel>();
            const string sql = "SELECT * FROM vw_DailyRevenue ORDER BY RevenueDate DESC;";
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

        public List<DailyRevenueByStoreViewModel> GetDailyRevenueByStore(int? storeId = null)
        {
            var list = new List<DailyRevenueByStoreViewModel>();
            string sql = "SELECT * FROM vw_DailyRevenueByStore";
            if (storeId.HasValue) sql += " WHERE StoreId = @StoreId";
            sql += " ORDER BY RevenueDate DESC, StoreName;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new DailyRevenueByStoreViewModel
                    {
                        StoreId = GetInt(reader, "StoreId"),
                        StoreName = GetString(reader, "StoreName"),
                        RevenueDate = GetDateTime(reader, "RevenueDate"),
                        TotalOrders = GetInt(reader, "TotalOrders"),
                        TotalRevenue = GetDecimal(reader, "TotalRevenue")
                    });
                }
                return list;
            }, sql, parameters =>
            {
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public List<TopSellingBookViewModel> GetTopSellingBooks(int top = 10)
        {
            var list = new List<TopSellingBookViewModel>();
            string sql = $@"
                SELECT TOP ({top}) *
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
            }, sql);
        }
    }
}
