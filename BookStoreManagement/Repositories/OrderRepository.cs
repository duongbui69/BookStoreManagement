using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class OrderStats
    {
        public int PendingOrders { get; set; }
        public int CompletedToday { get; set; }
        public int RefundRequests { get; set; }
        public decimal Revenue24h { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class OrderRepository : RepositoryBase
    {
        public OrderStats GetStats()
        {
            var stats = new OrderStats();
            
            // Note: If you don't have these exact statuses, map them accordingly.
            stats.PendingOrders = ExecuteScalarInt("SELECT COUNT(*) FROM SalesOrders WHERE OrderStatus = 'Processing' OR OrderStatus = 'Awaiting'");
            stats.CompletedToday = ExecuteScalarInt("SELECT COUNT(*) FROM SalesOrders WHERE OrderStatus = 'Completed' AND CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)");
            stats.RefundRequests = ExecuteScalarInt("SELECT COUNT(*) FROM SalesOrders WHERE OrderStatus = 'Flagged' OR OrderStatus = 'Refunded'");
            stats.Revenue24h = ExecuteScalarDecimal("SELECT ISNULL(SUM(TotalAmount), 0) FROM SalesOrders WHERE OrderStatus = 'Completed' AND OrderDate >= DATEADD(day, -1, GETDATE())");

            return stats;
        }

        public (List<OrderItem> Items, int TotalCount) GetPagedOrders(int page, int pageSize, string statusFilter, string searchTerm)
        {
            string whereClause = "WHERE 1=1 ";
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                if (statusFilter == "Pending Orders")
                    whereClause += "AND (o.OrderStatus = 'Processing' OR o.OrderStatus = 'Awaiting') ";
                else if (statusFilter == "Completed")
                    whereClause += "AND o.OrderStatus = 'Completed' ";
                else if (statusFilter == "Refunded")
                    whereClause += "AND o.OrderStatus = 'Refunded' ";
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += "AND (o.OrderCode LIKE @Search OR c.FullName LIKE @Search) ";
            }

            Action<SqlParameterCollection> addParams = p =>
            {
                if (!string.IsNullOrEmpty(searchTerm)) AddParameter(p, "@Search", "%" + searchTerm + "%");
            };

            // Get total count
            string countQuery = $@"
                SELECT COUNT(*) 
                FROM SalesOrders o 
                LEFT JOIN Customers c ON o.CustomerId = c.Id
                {whereClause}";
                
            int totalCount = ExecuteScalarInt(countQuery, addParams);

            // Get paged data
            string dataQuery = $@"
                SELECT 
                    o.Id, o.OrderCode, o.OrderDate, ISNULL(c.FullName, 'Guest'), ISNULL(c.Email, 'N/A'), 
                    (SELECT ISNULL(SUM(Quantity), 0) FROM SalesOrderDetails d WHERE d.SalesOrderId = o.Id), 
                    o.TotalAmount, o.OrderStatus
                FROM SalesOrders o
                LEFT JOIN Customers c ON o.CustomerId = c.Id
                {whereClause}
                ORDER BY o.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var list = ExecuteQuery(cmd =>
            {
                var results = new List<OrderItem>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new OrderItem
                    {
                        Id = reader.GetInt32(0),
                        OrderId = reader.GetString(1),
                        Date = reader.GetDateTime(2),
                        CustomerName = reader.GetString(3),
                        CustomerEmail = reader.GetString(4),
                        ItemsCount = reader.GetInt32(5),
                        Total = reader.GetDecimal(6),
                        Status = reader.GetString(7)
                    });
                }
                return results;
            }, dataQuery, p =>
            {
                addParams(p);
                AddParameter(p, "@Offset", (page - 1) * pageSize);
                AddParameter(p, "@PageSize", pageSize);
            });

            return (list, totalCount);
        }
    }
}
