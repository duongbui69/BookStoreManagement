using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
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
                StoreId = GetInt(reader, "StoreId"),
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
                StoreId = GetInt(reader, "StoreId"),
                StoreName = GetString(reader, "StoreName"),
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
                StoreId = GetInt(reader, "StoreId"),
                StoreName = GetString(reader, "StoreName"),
                OrderDate = GetDateTime(reader, "OrderDate"),
                StaffId = GetInt(reader, "StaffId"),
                StaffName = GetString(reader, "StaffName"),
                CustomerId = GetNullableInt(reader, "CustomerId"),
                CustomerName = GetString(reader, "CustomerName"),
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
                    INSERT INTO SalesOrders (OrderCode, UserId, CustomerId, StoreId, TotalAmount, PaymentMethod, OrderStatus, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderCode, @UserId, @CustomerId, @StoreId, @TotalAmount, @PaymentMethod, @OrderStatus, @Note);
                ";

                using var orderCommand = new SqlCommand(insertOrderSql, connection, transaction);
                AddParameter(orderCommand, "@OrderCode", order.OrderCode);
                AddParameter(orderCommand, "@UserId", order.UserId);
                AddParameter(orderCommand, "@CustomerId", order.CustomerId);
                AddParameter(orderCommand, "@StoreId", order.StoreId);
                AddParameter(orderCommand, "@TotalAmount", order.TotalAmount);
                AddParameter(orderCommand, "@PaymentMethod", order.PaymentMethod);
                AddParameter(orderCommand, "@OrderStatus", order.OrderStatus);
                AddParameter(orderCommand, "@Note", order.Note);

                int orderId = Convert.ToInt32(orderCommand.ExecuteScalar());

                const string insertDetailSql = @"
                    INSERT INTO SalesOrderDetails (SalesOrderId, BookId, Quantity, UnitPrice, DiscountAmount)
                    VALUES (@SalesOrderId, @BookId, @Quantity, @UnitPrice, @DiscountAmount);
                ";

                foreach (var detail in details)
                {
                    using var detailCommand = new SqlCommand(insertDetailSql, connection, transaction);
                    AddParameter(detailCommand, "@SalesOrderId", orderId);
                    AddParameter(detailCommand, "@BookId", detail.BookId);
                    AddParameter(detailCommand, "@Quantity", detail.Quantity);
                    AddParameter(detailCommand, "@UnitPrice", detail.UnitPrice);
                    AddParameter(detailCommand, "@DiscountAmount", detail.DiscountAmount);
                    detailCommand.ExecuteNonQuery();
                }

                return orderId;
            });
        }

        public List<SalesOrderListViewModel> GetAll()
        {
            var orders = new List<SalesOrderListViewModel>();
            const string sql = "SELECT * FROM vw_SalesOrderList ORDER BY OrderDate DESC;";
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
                SELECT * FROM vw_SalesOrderList
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

        public List<SalesOrderListViewModel> GetByStoreId(int storeId)
        {
            var orders = new List<SalesOrderListViewModel>();
            const string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE StoreId = @StoreId
                ORDER BY OrderDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql, parameters => AddParameter(parameters, "@StoreId", storeId));
        }

        public List<SalesOrderListViewModel> Search(string keyword, int? storeId = null)
        {
            var orders = new List<SalesOrderListViewModel>();
            string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE (
                    OrderCode LIKE N'%' + @Keyword + N'%'
                    OR StoreName LIKE N'%' + @Keyword + N'%'
                    OR StaffName LIKE N'%' + @Keyword + N'%'
                    OR CustomerName LIKE N'%' + @Keyword + N'%'
                    OR PaymentMethod LIKE N'%' + @Keyword + N'%'
                    OR OrderStatus LIKE N'%' + @Keyword + N'%'
                )
            ";
            // if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY OrderDate DESC;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@Keyword", keyword);
            });
        }

        public List<SalesOrderListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            var orders = new List<SalesOrderListViewModel>();
            string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE OrderDate >= @FromDate
                  AND OrderDate <= @ToDate
            ";
            // if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY OrderDate DESC;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@FromDate", fromDate);
                AddParameter(parameters, "@ToDate", toDate);
                // if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public List<SalesOrderDetailFullViewModel> GetDetails(int salesOrderId)
        {
            var details = new List<SalesOrderDetailFullViewModel>();
            const string sql = @"
                SELECT * FROM vw_SalesOrderDetailFull
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

        public async Task<int> CreateOrderAsync(SalesOrder order, List<SalesOrderDetail> details)
        {
            int orderId = 0;
            await ExecuteTransactionAsync(async (connection, transaction) =>
            {
                const string insertOrderSql = @"
                    INSERT INTO SalesOrders (OrderCode, UserId, CustomerId, StoreId, TotalAmount, PaymentMethod, OrderStatus, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderCode, @UserId, @CustomerId, @StoreId, @TotalAmount, @PaymentMethod, @OrderStatus, @Note);
                ";

                orderId = await connection.ExecuteScalarAsync<int>(insertOrderSql, new
                {
                    OrderCode = order.OrderCode,
                    UserId = order.UserId,
                    CustomerId = order.CustomerId,
                    StoreId = order.StoreId,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod,
                    OrderStatus = order.OrderStatus,
                    Note = order.Note
                }, transaction);

                const string insertDetailSql = @"
                    INSERT INTO SalesOrderDetails (SalesOrderId, BookId, Quantity, UnitPrice, DiscountAmount)
                    VALUES (@SalesOrderId, @BookId, @Quantity, @UnitPrice, @DiscountAmount);
                ";

                foreach (var detail in details)
                {
                    await connection.ExecuteAsync(insertDetailSql, new
                    {
                        SalesOrderId = orderId,
                        BookId = detail.BookId,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        DiscountAmount = detail.DiscountAmount
                    }, transaction);
                }
            });
            return orderId;
        }

        public async Task<List<SalesOrderListViewModel>> GetAllAsync()
        {
            const string sql = "SELECT * FROM vw_SalesOrderList ORDER BY OrderDate DESC;";
            var items = await QueryAsync<SalesOrderListViewModel>(sql);
            return System.Linq.Enumerable.ToList(items);
        }

        public async Task<SalesOrder?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, OrderCode, UserId, CustomerId, OrderDate, TotalAmount, PaymentMethod, OrderStatus, Note
                FROM SalesOrders
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<SalesOrder>(sql, new { Id = id });
        }

        public async Task<SalesOrder?> GetByOrderCodeAsync(string orderCode)
        {
            const string sql = @"
                SELECT Id, OrderCode, UserId, CustomerId, OrderDate, TotalAmount, PaymentMethod, OrderStatus, Note
                FROM SalesOrders
                WHERE OrderCode = @OrderCode;
            ";
            return await QueryFirstOrDefaultAsync<SalesOrder>(sql, new { OrderCode = orderCode });
        }

        public async Task<List<SalesOrderListViewModel>> GetByStaffIdAsync(int staffId)
        {
            const string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE StaffId = @StaffId
                ORDER BY OrderDate DESC;
            ";
            var items = await QueryAsync<SalesOrderListViewModel>(sql, new { StaffId = staffId });
            return System.Linq.Enumerable.ToList(items);
        }

        public async Task<List<SalesOrderListViewModel>> GetByStoreIdAsync(int storeId)
        {
            const string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE StoreId = @StoreId
                ORDER BY OrderDate DESC;
            ";
            var items = await QueryAsync<SalesOrderListViewModel>(sql, new { StoreId = storeId });
            return System.Linq.Enumerable.ToList(items);
        }

        public async Task<List<SalesOrderListViewModel>> SearchAsync(string keyword, int? storeId = null)
        {
            string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE (
                    OrderCode LIKE N'%' + @Keyword + N'%'
                    OR StoreName LIKE N'%' + @Keyword + N'%'
                    OR StaffName LIKE N'%' + @Keyword + N'%'
                    OR CustomerName LIKE N'%' + @Keyword + N'%'
                    OR PaymentMethod LIKE N'%' + @Keyword + N'%'
                    OR OrderStatus LIKE N'%' + @Keyword + N'%'
                )
            ";
            sql += " ORDER BY OrderDate DESC;";
            var items = await QueryAsync<SalesOrderListViewModel>(sql, new { Keyword = keyword });
            return System.Linq.Enumerable.ToList(items);
        }

        public async Task<List<SalesOrderListViewModel>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE OrderDate >= @FromDate
                  AND OrderDate <= @ToDate
            ";
            sql += " ORDER BY OrderDate DESC;";
            var items = await QueryAsync<SalesOrderListViewModel>(sql, new { FromDate = fromDate, ToDate = toDate });
            return System.Linq.Enumerable.ToList(items);
        }

        public async Task<List<SalesOrderDetailFullViewModel>> GetDetailsAsync(int salesOrderId)
        {
            const string sql = @"
                SELECT * FROM vw_SalesOrderDetailFull
                WHERE SalesOrderId = @SalesOrderId;
            ";
            var items = await QueryAsync<SalesOrderDetailFullViewModel>(sql, new { SalesOrderId = salesOrderId });
            return System.Linq.Enumerable.ToList(items);
        }

        public async Task<bool> CancelOrderAsync(int salesOrderId)
        {
            const string sql = @"
                UPDATE SalesOrders
                SET OrderStatus = @OrderStatus
                WHERE Id = @Id
                  AND OrderStatus = @CompletedStatus;
            ";
            return await ExecuteAsync(sql, new { Id = salesOrderId, OrderStatus = AppConstants.OrderStatuses.Cancelled, CompletedStatus = AppConstants.OrderStatuses.Completed }) > 0;
        }

        public async Task<bool> IsOrderCodeExistsAsync(string orderCode)
        {
            const string sql = "SELECT COUNT(1) FROM SalesOrders WHERE OrderCode = @OrderCode;";
            return await ExecuteScalarAsync<int>(sql, new { OrderCode = orderCode }) > 0;
        }

        public async Task<string> GenerateOrderCodeAsync()
        {
            return await Task.FromResult("SO" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        }
    }
}
