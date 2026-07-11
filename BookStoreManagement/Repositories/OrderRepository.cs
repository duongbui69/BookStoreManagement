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
        public string OrderId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class OrderRepository
    {
        public OrderStats GetStats()
        {
            var stats = new OrderStats();
            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            // Note: If you don't have these exact statuses, map them accordingly.
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM SalesOrders WHERE OrderStatus = 'Processing' OR OrderStatus = 'Awaiting'", connection))
                stats.PendingOrders = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM SalesOrders WHERE OrderStatus = 'Completed' AND CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)", connection))
                stats.CompletedToday = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM SalesOrders WHERE OrderStatus = 'Flagged' OR OrderStatus = 'Refunded'", connection))
                stats.RefundRequests = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT ISNULL(SUM(TotalAmount), 0) FROM SalesOrders WHERE OrderStatus = 'Completed' AND OrderDate >= DATEADD(day, -1, GETDATE())", connection))
                stats.Revenue24h = Convert.ToDecimal(cmd.ExecuteScalar());

            return stats;
        }

        public (List<OrderItem> Items, int TotalCount) GetPagedOrders(int page, int pageSize, string statusFilter, string searchTerm)
        {
            var list = new List<OrderItem>();
            int totalCount = 0;

            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();

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

            // Get total count
            string countQuery = $@"
                SELECT COUNT(*) 
                FROM SalesOrders o 
                LEFT JOIN Customers c ON o.CustomerId = c.Id
                {whereClause}";
                
            using (var countCmd = new SqlCommand(countQuery, connection))
            {
                if (!string.IsNullOrEmpty(searchTerm)) countCmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                totalCount = (int)countCmd.ExecuteScalar();
            }

            // Get paged data
            string dataQuery = $@"
                SELECT 
                    o.OrderCode, o.OrderDate, ISNULL(c.FullName, 'Guest'), ISNULL(c.Email, 'N/A'), 
                    (SELECT ISNULL(SUM(Quantity), 0) FROM SalesOrderDetails d WHERE d.SalesOrderId = o.Id), 
                    o.TotalAmount, o.OrderStatus
                FROM SalesOrders o
                LEFT JOIN Customers c ON o.CustomerId = c.Id
                {whereClause}
                ORDER BY o.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using (var dataCmd = new SqlCommand(dataQuery, connection))
            {
                if (!string.IsNullOrEmpty(searchTerm)) dataCmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                dataCmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                dataCmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = dataCmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new OrderItem
                    {
                        OrderId = reader.GetString(0),
                        Date = reader.GetDateTime(1),
                        CustomerName = reader.GetString(2),
                        CustomerEmail = reader.GetString(3),
                        ItemsCount = reader.GetInt32(4),
                        Total = reader.GetDecimal(5),
                        Status = reader.GetString(6)
                    });
                }
            }

            return (list, totalCount);
        }
    }
}
