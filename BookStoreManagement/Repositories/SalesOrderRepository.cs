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
                RefundAmount = GetNullableDecimal(reader, "RefundAmount") ?? 0m,
                ActualAmount = GetNullableDecimal(reader, "ActualAmount") ?? GetDecimal(reader, "TotalAmount"),
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
                RefundAmount = GetNullableDecimal(reader, "RefundAmount") ?? 0m,
                ActualAmount = GetNullableDecimal(reader, "ActualAmount") ?? GetDecimal(reader, "TotalAmount"),
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
                    INSERT INTO SalesOrders (OrderCode, UserId, CustomerId, StoreId, TotalAmount, RefundAmount, ActualAmount, PaymentMethod, OrderStatus, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderCode, @UserId, @CustomerId, @StoreId, @TotalAmount, 0, @TotalAmount, @PaymentMethod, @OrderStatus, @Note);
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

                    // Trừ tồn kho sau khi bán
                    const string deductBookSql = @"
                        UPDATE Books SET Quantity = Quantity - @Qty
                        WHERE Id = @BookId AND Quantity >= @Qty;
                    ";
                    using var deductBookCmd = new SqlCommand(deductBookSql, connection, transaction);
                    AddParameter(deductBookCmd, "@Qty", detail.Quantity);
                    AddParameter(deductBookCmd, "@BookId", detail.BookId);
                    int rowsAffected = deductBookCmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                        throw new Exception($"Không đủ hàng trong kho (BookId={detail.BookId}).");

                    const string deductStoreSql = @"
                        IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                            UPDATE StoreBookInventories
                            SET Quantity = Quantity - @Qty, UpdatedAt = SYSDATETIME()
                            WHERE StoreId = @StoreId AND BookId = @BookId AND Quantity >= @Qty;
                    ";
                    using var deductStoreCmd = new SqlCommand(deductStoreSql, connection, transaction);
                    AddParameter(deductStoreCmd, "@StoreId", order.StoreId);
                    AddParameter(deductStoreCmd, "@BookId", detail.BookId);
                    AddParameter(deductStoreCmd, "@Qty", detail.Quantity);
                    int storeRowsAffected = deductStoreCmd.ExecuteNonQuery();
                    if (storeRowsAffected == 0)
                        throw new Exception($"Không đủ hàng trong kho cửa hàng (BookId={detail.BookId}).");

                    // Ghi lịch sử thiếu hàng
                    const string logSql = @"
                        INSERT INTO InventoryTransactions
                            (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                        VALUES
                            (@StoreId, @BookId, @UserId, 'SALE', -@Qty, 'SalesOrder', @RefId, N'Bán hàng tại quầy', SYSDATETIME());
                    ";
                    using var logCmd = new SqlCommand(logSql, connection, transaction);
                    AddParameter(logCmd, "@StoreId", order.StoreId);
                    AddParameter(logCmd, "@BookId", detail.BookId);
                    AddParameter(logCmd, "@UserId", order.UserId);
                    AddParameter(logCmd, "@Qty", detail.Quantity);
                    AddParameter(logCmd, "@RefId", orderId);
                    logCmd.ExecuteNonQuery();
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
                SELECT Id, OrderCode, StoreId, UserId, CustomerId, OrderDate,
                       TotalAmount, RefundAmount, ActualAmount, PaymentMethod, OrderStatus, Note
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
                SELECT Id, OrderCode, StoreId, UserId, CustomerId, OrderDate,
                       TotalAmount, RefundAmount, ActualAmount, PaymentMethod, OrderStatus, Note
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
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY OrderDate DESC;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@Keyword", keyword);
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public List<SalesOrderListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            var orders = new List<SalesOrderListViewModel>();
            string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE OrderDate >= @FromDate
                  AND OrderDate <= @ToDate
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
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
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
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
            return ExecuteTransaction((connection, transaction) =>
            {
                // 1. Lấy thông tin đơn hàng để lấy StoreId và UserId
                const string getOrderSql = @"
                    SELECT Id, StoreId, UserId, OrderStatus FROM SalesOrders WHERE Id = @Id;";
                using var getCmd = new SqlCommand(getOrderSql, connection, transaction);
                AddParameter(getCmd, "@Id", salesOrderId);
                int storeId = 0; int userId = 0; string currentStatus = "";
                using (var rdr = getCmd.ExecuteReader())
                {
                    if (!rdr.Read()) return 0;
                    storeId = (int)rdr["StoreId"];
                    userId = (int)rdr["UserId"];
                    currentStatus = rdr["OrderStatus"].ToString() ?? "";
                }
                if (currentStatus != AppConstants.OrderStatuses.Completed) return 0;

                // 2. Cập nhật trạng thái đơn
                const string cancelSql = @"
                    UPDATE SalesOrders SET OrderStatus = @OrderStatus
                    WHERE Id = @Id AND OrderStatus = @CompletedStatus;";
                using var cancelCmd = new SqlCommand(cancelSql, connection, transaction);
                AddParameter(cancelCmd, "@Id", salesOrderId);
                AddParameter(cancelCmd, "@OrderStatus", AppConstants.OrderStatuses.Cancelled);
                AddParameter(cancelCmd, "@CompletedStatus", AppConstants.OrderStatuses.Completed);
                int affected = cancelCmd.ExecuteNonQuery();
                if (affected == 0) return 0;

                // 3. Lấy danh sách sản phẩm của đơn
                const string getDetailsSql = @"
                    SELECT BookId, Quantity FROM SalesOrderDetails WHERE SalesOrderId = @SalesOrderId;";
                using var detailCmd = new SqlCommand(getDetailsSql, connection, transaction);
                AddParameter(detailCmd, "@SalesOrderId", salesOrderId);
                var books = new List<(int BookId, int Qty)>();
                using (var rdr = detailCmd.ExecuteReader())
                    while (rdr.Read())
                        books.Add(((int)rdr["BookId"], (int)rdr["Quantity"]));

                // 4. Hoàn lại tồn kho và ghi lịch sử
                foreach (var (bookId, qty) in books)
                {
                    const string restockSql = @"
                        UPDATE Books SET Quantity = Quantity + @Qty WHERE Id = @BookId;
                        IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                            UPDATE StoreBookInventories SET Quantity = Quantity + @Qty, UpdatedAt = SYSDATETIME()
                            WHERE StoreId = @StoreId AND BookId = @BookId;";
                    using var restockCmd = new SqlCommand(restockSql, connection, transaction);
                    AddParameter(restockCmd, "@BookId", bookId);
                    AddParameter(restockCmd, "@Qty", qty);
                    AddParameter(restockCmd, "@StoreId", storeId);
                    restockCmd.ExecuteNonQuery();

                    const string logSql = @"
                        INSERT INTO InventoryTransactions
                            (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                        VALUES
                            (@StoreId, @BookId, @UserId, 'CANCEL', @Qty, 'SalesOrder', @RefId, N'Hoàn kho do hủy đơn', SYSDATETIME());";
                    using var logCmd = new SqlCommand(logSql, connection, transaction);
                    AddParameter(logCmd, "@StoreId", storeId);
                    AddParameter(logCmd, "@BookId", bookId);
                    AddParameter(logCmd, "@UserId", userId);
                    AddParameter(logCmd, "@Qty", qty);
                    AddParameter(logCmd, "@RefId", salesOrderId);
                    logCmd.ExecuteNonQuery();
                }
                return affected;
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
                    INSERT INTO SalesOrders (OrderCode, UserId, CustomerId, StoreId, TotalAmount, RefundAmount, ActualAmount, PaymentMethod, OrderStatus, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderCode, @UserId, @CustomerId, @StoreId, @TotalAmount, 0, @TotalAmount, @PaymentMethod, @OrderStatus, @Note);
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

                    // Trừ tồn kho sau khi bán
                    const string deductBookSql = @"
                        UPDATE Books SET Quantity = Quantity - @Qty
                        WHERE Id = @BookId AND Quantity >= @Qty;
                    ";
                    int rowsAffected = await connection.ExecuteAsync(deductBookSql,
                        new { Qty = detail.Quantity, BookId = detail.BookId }, transaction);
                    if (rowsAffected == 0)
                        throw new Exception($"Không đủ hàng trong kho (BookId={detail.BookId}).");

                    const string deductStoreSql = @"
                        IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                            UPDATE StoreBookInventories
                            SET Quantity = Quantity - @Qty, UpdatedAt = SYSDATETIME()
                            WHERE StoreId = @StoreId AND BookId = @BookId AND Quantity >= @Qty;
                    ";
                    await connection.ExecuteAsync(deductStoreSql,
                        new { StoreId = order.StoreId, BookId = detail.BookId, Qty = detail.Quantity }, transaction);

                    // Ghi lịch sử kho
                    const string logSql = @"
                        INSERT INTO InventoryTransactions
                            (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                        VALUES
                            (@StoreId, @BookId, @UserId, 'SALE', -@Qty, 'SalesOrder', @RefId, N'Bán hàng tại quầy', SYSDATETIME());
                    ";
                    await connection.ExecuteAsync(logSql, new
                    {
                        StoreId = order.StoreId,
                        BookId = detail.BookId,
                        UserId = order.UserId,
                        Qty = detail.Quantity,
                        RefId = orderId
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
                SELECT Id, OrderCode, StoreId, UserId, CustomerId, OrderDate,
                       TotalAmount, RefundAmount, ActualAmount, PaymentMethod, OrderStatus, Note
                FROM SalesOrders
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<SalesOrder>(sql, new { Id = id });
        }

        public async Task<SalesOrder?> GetByOrderCodeAsync(string orderCode)
        {
            const string sql = @"
                SELECT Id, OrderCode, StoreId, UserId, CustomerId, OrderDate,
                       TotalAmount, RefundAmount, ActualAmount, PaymentMethod, OrderStatus, Note
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
            try
            {
                await ExecuteTransactionAsync(async (connection, transaction) =>
                {
                    // 1. Lấy thông tin đơn hàng
                    const string getOrderSql = "SELECT Id, StoreId, UserId, OrderStatus FROM SalesOrders WHERE Id = @Id;";
                    var order = await connection.QueryFirstOrDefaultAsync<dynamic>(getOrderSql, new { Id = salesOrderId }, transaction);
                    if (order == null || (string)order.OrderStatus != AppConstants.OrderStatuses.Completed)
                        throw new InvalidOperationException("Không thể hủy: Đơn hàng không tồn tại hoặc chưa hoàn thành.");

                    int storeId = (int)order.StoreId;
                    int userId = (int)order.UserId;

                    // 2. Cập nhật trạng thái đơn
                    const string cancelSql = @"
                        UPDATE SalesOrders SET OrderStatus = @OrderStatus
                        WHERE Id = @Id AND OrderStatus = @CompletedStatus;";
                    int affected = await connection.ExecuteAsync(cancelSql,
                        new { Id = salesOrderId, OrderStatus = AppConstants.OrderStatuses.Cancelled, CompletedStatus = AppConstants.OrderStatuses.Completed },
                        transaction);
                    if (affected == 0) throw new InvalidOperationException("Không thể hủy đơn hàng.");

                    // 3. Lấy danh sách chi tiết đơn hàng
                    const string getDetailsSql = "SELECT BookId, Quantity FROM SalesOrderDetails WHERE SalesOrderId = @SalesOrderId;";
                    var details = await connection.QueryAsync<dynamic>(getDetailsSql, new { SalesOrderId = salesOrderId }, transaction);

                    // 4. Hoàn lại tồn kho và ghi lịch sử
                    foreach (var detail in details)
                    {
                        int bookId = (int)detail.BookId;
                        int qty = (int)detail.Quantity;

                        const string restockSql = @"
                            UPDATE Books SET Quantity = Quantity + @Qty WHERE Id = @BookId;
                            IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                                UPDATE StoreBookInventories SET Quantity = Quantity + @Qty, UpdatedAt = SYSDATETIME()
                                WHERE StoreId = @StoreId AND BookId = @BookId;";
                        await connection.ExecuteAsync(restockSql,
                            new { BookId = bookId, Qty = qty, StoreId = storeId }, transaction);

                        const string logSql = @"
                            INSERT INTO InventoryTransactions
                                (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                            VALUES
                                (@StoreId, @BookId, @UserId, 'CANCEL', @Qty, 'SalesOrder', @RefId, N'Hoàn kho do hủy đơn', SYSDATETIME());";
                        await connection.ExecuteAsync(logSql,
                            new { StoreId = storeId, BookId = bookId, UserId = userId, Qty = qty, RefId = salesOrderId },
                            transaction);
                    }
                });
                return true;
            }
            catch
            {
                throw; // re-throw để caller xử lý thông báo lỗi
            }
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
