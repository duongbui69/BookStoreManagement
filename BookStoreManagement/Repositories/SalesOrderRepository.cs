using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class SalesOrderRepository : RepositoryBase
    {
        private SalesOrder MapSalesOrder(SqlDataReader reader)
        {
            return new SalesOrder
            {
                Id = GetInt(reader, "Id"),
                OrderCode = GetString(reader, "OrderCode"),
                UserId = GetInt(reader, "UserId"),
                CustomerId = GetNullableInt(reader, "CustomerId"),
                OrderDate = GetDateTime(reader, "OrderDate"),
                TotalAmount = GetDecimal(reader, "TotalAmount"),
                PaymentMethod = GetString(reader, "PaymentMethod"),
                OrderStatus = GetString(reader, "OrderStatus"),
                Note = GetNullableString(reader, "Note")
            };
        }

        private SalesOrderListViewModel MapOrderList(SqlDataReader reader)
        {
            return new SalesOrderListViewModel
            {
                Id = GetInt(reader, "Id"),
                OrderCode = GetString(reader, "OrderCode"),
                OrderDate = GetDateTime(reader, "OrderDate"),
                StaffId = GetInt(reader, "StaffId"),
                StaffName = GetString(reader, "StaffName"),
                CustomerId = GetNullableInt(reader, "CustomerId"),
                CustomerName = GetString(reader, "CustomerName"),
                TotalAmount = GetDecimal(reader, "TotalAmount"),
                PaymentMethod = GetString(reader, "PaymentMethod"),
                OrderStatus = GetString(reader, "OrderStatus"),
                Note = GetNullableString(reader, "Note")
            };
        }

        private SalesOrderDetailFullViewModel MapOrderDetail(SqlDataReader reader)
        {
            return new SalesOrderDetailFullViewModel
            {
                SalesOrderId = GetInt(reader, "SalesOrderId"),
                OrderCode = GetString(reader, "OrderCode"),
                OrderDate = GetDateTime(reader, "OrderDate"),
                StaffId = GetInt(reader, "StaffId"),
                StaffName = GetString(reader, "StaffName"),
                CustomerId = GetNullableInt(reader, "CustomerId"),
                CustomerName = GetNullableString(reader, "CustomerName"),
                BookId = GetInt(reader, "BookId"),
                BookCode = GetString(reader, "BookCode"),
                Title = GetString(reader, "Title"),
                Quantity = GetInt(reader, "Quantity"),
                UnitPrice = GetDecimal(reader, "UnitPrice"),
                DiscountAmount = GetDecimal(reader, "DiscountAmount"),
                LineTotal = GetDecimal(reader, "LineTotal"),
                TotalAmount = GetDecimal(reader, "TotalAmount"),
                PaymentMethod = GetString(reader, "PaymentMethod"),
                OrderStatus = GetString(reader, "OrderStatus"),
                Note = GetNullableString(reader, "Note")
            };
        }

        public int CreateOrder(SalesOrder order, List<SalesOrderDetail> details)
        {
            return ExecuteTransaction((connection, transaction) =>
            {
                const string insertOrderSql = @"
                    INSERT INTO SalesOrders (OrderCode, UserId, CustomerId, PaymentMethod, OrderStatus, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderCode, @UserId, @CustomerId, @PaymentMethod, @OrderStatus, @Note);
                ";

                using var orderCommand = new SqlCommand(insertOrderSql, connection, transaction);
                orderCommand.Parameters.AddWithValue("@OrderCode", order.OrderCode);
                orderCommand.Parameters.AddWithValue("@UserId", order.UserId);
                orderCommand.Parameters.AddWithValue("@CustomerId", (object?)order.CustomerId ?? DBNull.Value);
                orderCommand.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod);
                orderCommand.Parameters.AddWithValue("@OrderStatus", order.OrderStatus);
                orderCommand.Parameters.AddWithValue("@Note", (object?)order.Note ?? DBNull.Value);

                int orderId = Convert.ToInt32(orderCommand.ExecuteScalar());

                const string insertDetailSql = @"
                    INSERT INTO SalesOrderDetails (SalesOrderId, BookId, Quantity, UnitPrice, DiscountAmount)
                    VALUES (@SalesOrderId, @BookId, @Quantity, @UnitPrice, @DiscountAmount);
                ";

                foreach (var detail in details)
                {
                    using var detailCommand = new SqlCommand(insertDetailSql, connection, transaction);
                    detailCommand.Parameters.AddWithValue("@SalesOrderId", orderId);
                    detailCommand.Parameters.AddWithValue("@BookId", detail.BookId);
                    detailCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                    detailCommand.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
                    detailCommand.Parameters.AddWithValue("@DiscountAmount", detail.DiscountAmount);
                    detailCommand.ExecuteNonQuery();
                }

                return orderId;
            });
        }

        public List<SalesOrderListViewModel> GetAll()
        {
            var orders = new List<SalesOrderListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_SalesOrderList
                ORDER BY OrderDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql);
        }

        public SalesOrder? GetById(int id)
        {
            const string sql = @"
                SELECT Id, OrderCode, UserId, CustomerId, OrderDate, TotalAmount, PaymentMethod, OrderStatus, Note
                FROM SalesOrders
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapSalesOrder(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public SalesOrder? GetByOrderCode(string orderCode)
        {
            const string sql = @"
                SELECT Id, OrderCode, UserId, CustomerId, OrderDate, TotalAmount, PaymentMethod, OrderStatus, Note
                FROM SalesOrders
                WHERE OrderCode = @OrderCode;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapSalesOrder(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@OrderCode", orderCode));
        }

        public List<SalesOrderListViewModel> GetByStaffId(int staffId)
        {
            var orders = new List<SalesOrderListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_SalesOrderList
                WHERE StaffId = @StaffId
                ORDER BY OrderDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql, parameters => AddParameter(parameters, "@StaffId", staffId));
        }

        public List<SalesOrderListViewModel> Search(string keyword)
        {
            var orders = new List<SalesOrderListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_SalesOrderList
                WHERE OrderCode LIKE N'%' + @Keyword + N'%'
                   OR StaffName LIKE N'%' + @Keyword + N'%'
                   OR CustomerName LIKE N'%' + @Keyword + N'%'
                   OR PaymentMethod LIKE N'%' + @Keyword + N'%'
                   OR OrderStatus LIKE N'%' + @Keyword + N'%'
                ORDER BY OrderDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public List<SalesOrderListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            var orders = new List<SalesOrderListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_SalesOrderList
                WHERE OrderDate >= @FromDate
                  AND OrderDate < DATEADD(DAY, 1, @ToDate)
                ORDER BY OrderDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@FromDate", fromDate.Date);
                AddParameter(parameters, "@ToDate", toDate.Date);
            });
        }

        public List<SalesOrderDetailFullViewModel> GetDetails(int salesOrderId)
        {
            var details = new List<SalesOrderDetailFullViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_SalesOrderDetailFull
                WHERE SalesOrderId = @SalesOrderId;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) details.Add(MapOrderDetail(reader));
                return details;
            }, sql, parameters => AddParameter(parameters, "@SalesOrderId", salesOrderId));
        }

        public bool CancelOrder(int salesOrderId)
        {
            const string sql = @"
                UPDATE SalesOrders
                SET OrderStatus = @OrderStatus
                WHERE Id = @Id
                  AND OrderStatus = @CompletedStatus;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", salesOrderId);
                AddParameter(parameters, "@OrderStatus", AppConstants.OrderStatuses.Cancelled);
                AddParameter(parameters, "@CompletedStatus", AppConstants.OrderStatuses.Completed);
            }) > 0;
        }

        public bool IsOrderCodeExists(string orderCode)
        {
            const string sql = "SELECT COUNT(1) FROM SalesOrders WHERE OrderCode = @OrderCode;";
            return ExecuteScalarInt(sql, parameters => AddParameter(parameters, "@OrderCode", orderCode)) > 0;
        }

        public string GenerateOrderCode()
        {
            return "SO" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
