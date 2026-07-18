using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class ReturnReceiptRepository : RepositoryBase
    {
        private ReturnReceipt MapReturnReceipt(SqlDataReader reader)
        {
            return new ReturnReceipt
            {
                Id = GetInt(reader, "Id"),
                ReturnCode = GetString(reader, "ReturnCode"),
                SalesOrderId = GetNullableInt(reader, "SalesOrderId"),
                StoreId = GetInt(reader, "StoreId"),
                CustomerId = GetNullableInt(reader, "CustomerId"),
                UserId = GetInt(reader, "UserId"),
                ReturnDate = GetDateTime(reader, "ReturnDate"),
                TotalRefundAmount = GetDecimal(reader, "TotalRefundAmount"),
                Note = GetNullableString(reader, "Note"),
                ReturnStatus = GetString(reader, "ReturnStatus")
            };
        }

        private ReturnReceiptListViewModel MapReturnList(SqlDataReader reader)
        {
            return new ReturnReceiptListViewModel
            {
                Id = GetInt(reader, "Id"),
                ReturnCode = GetString(reader, "ReturnCode"),
                SalesOrderId = GetNullableInt(reader, "SalesOrderId"),
                OrderCode = GetNullableString(reader, "OrderCode"),
                StoreId = GetInt(reader, "StoreId"),
                StoreName = GetString(reader, "StoreName"),
                CustomerId = GetNullableInt(reader, "CustomerId"),
                CustomerName = GetString(reader, "CustomerName"),
                StaffId = GetInt(reader, "StaffId"),
                StaffName = GetString(reader, "StaffName"),
                ReturnDate = GetDateTime(reader, "ReturnDate"),
                TotalRefundAmount = GetDecimal(reader, "TotalRefundAmount"),
                Note = GetNullableString(reader, "Note"),
                ReturnStatus = GetString(reader, "ReturnStatus")
            };
        }

        private ReturnReceiptDetailFullViewModel MapReturnDetail(SqlDataReader reader)
        {
            return new ReturnReceiptDetailFullViewModel
            {
                ReturnReceiptId = GetInt(reader, "ReturnReceiptId"),
                ReturnCode = GetString(reader, "ReturnCode"),
                ReturnDate = GetDateTime(reader, "ReturnDate"),
                StoreId = GetInt(reader, "StoreId"),
                StoreName = GetString(reader, "StoreName"),
                CustomerId = GetNullableInt(reader, "CustomerId"),
                CustomerName = GetString(reader, "CustomerName"),
                StaffId = GetInt(reader, "StaffId"),
                StaffName = GetString(reader, "StaffName"),
                BookId = GetInt(reader, "BookId"),
                BookCode = GetString(reader, "BookCode"),
                Title = GetString(reader, "Title"),
                Quantity = GetInt(reader, "Quantity"),
                UnitPrice = GetDecimal(reader, "UnitPrice"),
                RefundAmount = GetDecimal(reader, "RefundAmount"),
                ReturnReason = GetNullableString(reader, "ReturnReason"),
                IsRestock = GetBool(reader, "IsRestock"),
                TotalRefundAmount = GetDecimal(reader, "TotalRefundAmount"),
                Note = GetNullableString(reader, "Note"),
                ReturnStatus = GetString(reader, "ReturnStatus")
            };
        }

        public int CreateReturn(ReturnReceipt receipt, List<ReturnReceiptDetail> details)
        {
            return ExecuteTransaction((connection, transaction) =>
            {
                const string insertReceiptSql = @"
                    INSERT INTO ReturnReceipts (ReturnCode, SalesOrderId, StoreId, CustomerId, UserId, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@ReturnCode, @SalesOrderId, @StoreId, @CustomerId, @UserId, @Note);
                ";

                using var receiptCommand = new SqlCommand(insertReceiptSql, connection, transaction);
                AddParameter(receiptCommand, "@ReturnCode", receipt.ReturnCode);
                AddParameter(receiptCommand, "@SalesOrderId", receipt.SalesOrderId);
                AddParameter(receiptCommand, "@StoreId", receipt.StoreId);
                AddParameter(receiptCommand, "@CustomerId", receipt.CustomerId);
                AddParameter(receiptCommand, "@UserId", receipt.UserId);
                AddParameter(receiptCommand, "@Note", receipt.Note);

                int returnReceiptId = Convert.ToInt32(receiptCommand.ExecuteScalar());

                const string insertDetailSql = @"
                    INSERT INTO ReturnReceiptDetails (
                        ReturnReceiptId, SalesOrderDetailId, BookId, Quantity, UnitPrice, ReturnReason, IsRestock
                    )
                    VALUES (
                        @ReturnReceiptId, @SalesOrderDetailId, @BookId, @Quantity, @UnitPrice, @ReturnReason, @IsRestock
                    );
                ";

                const string restockSql = @"
                    UPDATE Books 
                    SET Quantity = Quantity + @Quantity 
                    WHERE Id = @BookId;

                    IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                        UPDATE StoreBookInventories SET Quantity = Quantity + @Quantity WHERE StoreId = @StoreId AND BookId = @BookId;
                    ELSE
                        INSERT INTO StoreBookInventories (StoreId, BookId, Quantity, MinStock, SellingPrice, IsActive, CreatedAt)
                        VALUES (@StoreId, @BookId, @Quantity, 0, @UnitPrice, 1, SYSDATETIME());
                ";

                const string insertInvTransSql = @"
                    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                    VALUES (@StoreId, @BookId, @UserId, 'RETURN_RECEIPT', @Quantity, 'ReturnReceipt', @ReferenceId, @Note, SYSDATETIME());
                ";

                foreach (var detail in details)
                {
                    using var detailCommand = new SqlCommand(insertDetailSql, connection, transaction);
                    AddParameter(detailCommand, "@ReturnReceiptId", returnReceiptId);
                    AddParameter(detailCommand, "@SalesOrderDetailId", detail.SalesOrderDetailId);
                    AddParameter(detailCommand, "@BookId", detail.BookId);
                    AddParameter(detailCommand, "@Quantity", detail.Quantity);
                    AddParameter(detailCommand, "@UnitPrice", detail.UnitPrice);
                    AddParameter(detailCommand, "@ReturnReason", detail.ReturnReason);
                    AddParameter(detailCommand, "@IsRestock", detail.IsRestock);
                    detailCommand.ExecuteNonQuery();

                    if (detail.IsRestock)
                    {
                        using var restockCmd = new SqlCommand(restockSql, connection, transaction);
                        AddParameter(restockCmd, "@StoreId", receipt.StoreId);
                        AddParameter(restockCmd, "@BookId", detail.BookId);
                        AddParameter(restockCmd, "@Quantity", detail.Quantity);
                        AddParameter(restockCmd, "@UnitPrice", detail.UnitPrice);
                        restockCmd.ExecuteNonQuery();

                        using var transCmd = new SqlCommand(insertInvTransSql, connection, transaction);
                        AddParameter(transCmd, "@StoreId", receipt.StoreId);
                        AddParameter(transCmd, "@BookId", detail.BookId);
                        AddParameter(transCmd, "@UserId", receipt.UserId);
                        AddParameter(transCmd, "@Quantity", detail.Quantity);
                        AddParameter(transCmd, "@ReferenceId", returnReceiptId);
                        AddParameter(transCmd, "@Note", $"Hoàn trả từ phiếu {receipt.ReturnCode}");
                        transCmd.ExecuteNonQuery();
                    }
                }

                return returnReceiptId;
            });
        }

        public List<ReturnReceiptListViewModel> GetAll()
        {
            var receipts = new List<ReturnReceiptListViewModel>();
            const string sql = "SELECT * FROM vw_ReturnReceiptList ORDER BY ReturnDate DESC;";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReturnList(reader));
                return receipts;
            }, sql);
        }

        public ReturnReceipt? GetById(int id)
        {
            const string sql = @"
                SELECT Id, ReturnCode, SalesOrderId, StoreId, CustomerId, UserId, ReturnDate, TotalRefundAmount, Note, ReturnStatus
                FROM ReturnReceipts
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapReturnReceipt(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public List<ReturnReceiptListViewModel> GetByStoreId(int storeId)
        {
            var receipts = new List<ReturnReceiptListViewModel>();
            const string sql = @"
                SELECT * FROM vw_ReturnReceiptList
                WHERE StoreId = @StoreId
                ORDER BY ReturnDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReturnList(reader));
                return receipts;
            }, sql, parameters => AddParameter(parameters, "@StoreId", storeId));
        }

        public List<ReturnReceiptDetailFullViewModel> GetDetails(int returnReceiptId)
        {
            var details = new List<ReturnReceiptDetailFullViewModel>();
            const string sql = @"
                SELECT * FROM vw_ReturnReceiptDetailFull
                WHERE ReturnReceiptId = @ReturnReceiptId;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) details.Add(MapReturnDetail(reader));
                return details;
            }, sql, parameters => AddParameter(parameters, "@ReturnReceiptId", returnReceiptId));
        }

        public List<ReturnReceiptListViewModel> Search(string keyword, int? storeId = null)
        {
            var receipts = new List<ReturnReceiptListViewModel>();
            string sql = @"
                SELECT * FROM vw_ReturnReceiptList
                WHERE (
                    ReturnCode LIKE N'%' + @Keyword + N'%'
                    OR OrderCode LIKE N'%' + @Keyword + N'%'
                    OR StoreName LIKE N'%' + @Keyword + N'%'
                    OR CustomerName LIKE N'%' + @Keyword + N'%'
                    OR StaffName LIKE N'%' + @Keyword + N'%'
                )
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY ReturnDate DESC;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReturnList(reader));
                return receipts;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@Keyword", keyword);
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public List<ReturnReceiptListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            var receipts = new List<ReturnReceiptListViewModel>();
            string sql = @"
                SELECT * FROM vw_ReturnReceiptList
                WHERE ReturnDate >= @FromDate
                  AND ReturnDate < DATEADD(DAY, 1, @ToDate)
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY ReturnDate DESC;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReturnList(reader));
                return receipts;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@FromDate", fromDate.Date);
                AddParameter(parameters, "@ToDate", toDate.Date);
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public bool IsReturnCodeExists(string returnCode)
        {
            const string sql = "SELECT COUNT(1) FROM ReturnReceipts WHERE ReturnCode = @ReturnCode;";
            return ExecuteScalarInt(sql, parameters => AddParameter(parameters, "@ReturnCode", returnCode)) > 0;
        }

        public string GenerateReturnCode()
        {
            return "RT" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        public async System.Threading.Tasks.Task<int> CreateReturnAsync(ReturnReceipt receipt, List<ReturnReceiptDetail> details)
        {
            int returnReceiptId = 0;
            await ExecuteTransactionAsync(async (connection, transaction) =>
            {
                const string insertReceiptSql = @"
                    INSERT INTO ReturnReceipts (ReturnCode, SalesOrderId, StoreId, CustomerId, UserId, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@ReturnCode, @SalesOrderId, @StoreId, @CustomerId, @UserId, @Note);
                ";
                returnReceiptId = await Dapper.SqlMapper.ExecuteScalarAsync<int>(connection, insertReceiptSql, new {
                    receipt.ReturnCode,
                    receipt.SalesOrderId,
                    receipt.StoreId,
                    receipt.CustomerId,
                    receipt.UserId,
                    receipt.Note
                }, transaction);

                const string insertDetailSql = @"
                    INSERT INTO ReturnReceiptDetails (
                        ReturnReceiptId, SalesOrderDetailId, BookId, Quantity, UnitPrice, ReturnReason, IsRestock
                    )
                    VALUES (
                        @ReturnReceiptId, @SalesOrderDetailId, @BookId, @Quantity, @UnitPrice, @ReturnReason, @IsRestock
                    );
                ";

                const string restockSql = @"
                    UPDATE Books 
                    SET Quantity = Quantity + @Quantity 
                    WHERE Id = @BookId;

                    IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                        UPDATE StoreBookInventories SET Quantity = Quantity + @Quantity WHERE StoreId = @StoreId AND BookId = @BookId;
                    ELSE
                        INSERT INTO StoreBookInventories (StoreId, BookId, Quantity, MinStock, SellingPrice, IsActive, CreatedAt)
                        VALUES (@StoreId, @BookId, @Quantity, 0, @UnitPrice, 1, SYSDATETIME());
                ";

                const string insertInvTransSql = @"
                    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                    VALUES (@StoreId, @BookId, @UserId, 'RETURN_RECEIPT', @Quantity, 'ReturnReceipt', @ReferenceId, @Note, SYSDATETIME());
                ";
                foreach (var detail in details)
                {
                    await Dapper.SqlMapper.ExecuteAsync(connection, insertDetailSql, new {
                        ReturnReceiptId = returnReceiptId,
                        detail.SalesOrderDetailId,
                        detail.BookId,
                        detail.Quantity,
                        detail.UnitPrice,
                        detail.ReturnReason,
                        detail.IsRestock
                    }, transaction);

                    if (detail.IsRestock)
                    {
                        await Dapper.SqlMapper.ExecuteAsync(connection, restockSql, new {
                            StoreId = receipt.StoreId,
                            BookId = detail.BookId,
                            Quantity = detail.Quantity,
                            UnitPrice = detail.UnitPrice
                        }, transaction);

                        await Dapper.SqlMapper.ExecuteAsync(connection, insertInvTransSql, new {
                            StoreId = receipt.StoreId,
                            BookId = detail.BookId,
                            UserId = receipt.UserId,
                            Quantity = detail.Quantity,
                            ReferenceId = returnReceiptId,
                            Note = $"Hoàn trả từ phiếu {receipt.ReturnCode}"
                        }, transaction);
                    }
                }
            });
            return returnReceiptId;
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptListViewModel>> GetAllAsync()
        {
            const string sql = "SELECT * FROM vw_ReturnReceiptList ORDER BY ReturnDate DESC;";
            var result = await QueryAsync<ReturnReceiptListViewModel>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<ReturnReceipt?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, ReturnCode, SalesOrderId, StoreId, CustomerId, UserId, ReturnDate, TotalRefundAmount, Note
                FROM ReturnReceipts
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<ReturnReceipt>(sql, new { Id = id });
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptListViewModel>> GetByStoreIdAsync(int storeId)
        {
            const string sql = @"
                SELECT * FROM vw_ReturnReceiptList
                WHERE StoreId = @StoreId
                ORDER BY ReturnDate DESC;
            ";
            var result = await QueryAsync<ReturnReceiptListViewModel>(sql, new { StoreId = storeId });
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptDetailFullViewModel>> GetDetailsAsync(int returnReceiptId)
        {
            const string sql = @"
                SELECT * FROM vw_ReturnReceiptDetailFull
                WHERE ReturnReceiptId = @ReturnReceiptId;
            ";
            var result = await QueryAsync<ReturnReceiptDetailFullViewModel>(sql, new { ReturnReceiptId = returnReceiptId });
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptListViewModel>> SearchAsync(string keyword, int? storeId = null)
        {
            string sql = @"
                SELECT * FROM vw_ReturnReceiptList
                WHERE (
                    ReturnCode LIKE N'%' + @Keyword + N'%'
                    OR OrderCode LIKE N'%' + @Keyword + N'%'
                    OR StoreName LIKE N'%' + @Keyword + N'%'
                    OR CustomerName LIKE N'%' + @Keyword + N'%'
                    OR StaffName LIKE N'%' + @Keyword + N'%'
                )
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY ReturnDate DESC;";

            var result = await QueryAsync<ReturnReceiptListViewModel>(sql, new { Keyword = keyword, StoreId = storeId });
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<List<ReturnReceiptListViewModel>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            string sql = @"
                SELECT * FROM vw_ReturnReceiptList
                WHERE ReturnDate >= @FromDate
                  AND ReturnDate < DATEADD(DAY, 1, @ToDate)
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY ReturnDate DESC;";

            var result = await QueryAsync<ReturnReceiptListViewModel>(sql, new { FromDate = fromDate.Date, ToDate = toDate.Date, StoreId = storeId });
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<bool> IsReturnCodeExistsAsync(string returnCode)
        {
            const string sql = "SELECT COUNT(1) FROM ReturnReceipts WHERE ReturnCode = @ReturnCode;";
            var count = await ExecuteScalarAsync<int>(sql, new { ReturnCode = returnCode });
            return count > 0;
        }

        public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                const string sql = "DELETE FROM ReturnReceipts WHERE Id = @Id;";
                int rowsAffected = ExecuteNonQuery(sql, parameters => AddParameter(parameters, "@Id", id));
                return rowsAffected > 0;
            });
        }
    }
}
