using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;
using Dapper;

namespace BookStoreManagement.Repositories
{
    public class ExportReceiptRepository : RepositoryBase
    {
        public ExportReceiptRepository()
        {
        }

        public int CreateReceipt(ExportReceipt receipt, List<ExportReceiptDetail> details)
        {
            return ExecuteTransaction((connection, transaction) =>
            {
                const string insertReceiptSql = @"
                    INSERT INTO ExportReceipts (ReceiptCode, StoreId, UserId, Reason, CustomerName, ExportDate, TotalAmount, Status, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@ReceiptCode, @StoreId, @UserId, @Reason, @CustomerName, @ExportDate, @TotalAmount, @Status, @Note);
                ";

                int receiptId = connection.ExecuteScalar<int>(insertReceiptSql, receipt, transaction);

                const string insertDetailSql = @"
                    INSERT INTO ExportReceiptDetails (ReceiptId, BookId, Quantity, Price)
                    VALUES (@ReceiptId, @BookId, @Quantity, @Price);
                ";

                const string updateBookQtySql = @"
                    -- Kiểm tra tồn kho trước khi trừ
                    IF (SELECT ISNULL(Quantity, 0) FROM Books WHERE Id = @BookId) < @Quantity
                        RAISERROR(N'Sách không đủ số lượng trong kho chính để xuất.', 16, 1);

                    IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                    BEGIN
                        IF (SELECT ISNULL(Quantity, 0) FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId) < @Quantity
                            RAISERROR(N'Sách không đủ số lượng tại kho cửa hàng để xuất.', 16, 1);
                        UPDATE StoreBookInventories 
                        SET Quantity = Quantity - @Quantity 
                        WHERE StoreId = @StoreId AND BookId = @BookId;
                    END
                    ELSE
                        RAISERROR(N'Sách không tồn tại trong kho cửa hàng để xuất.', 16, 1);

                    UPDATE Books 
                    SET Quantity = Quantity - @Quantity 
                    WHERE Id = @BookId;
                ";

                const string insertInvTransSql = @"
                    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                    VALUES (@StoreId, @BookId, @UserId, 'EXPORT', -@Quantity, 'ExportReceipt', @ReferenceId, @Note, SYSDATETIME());
                ";

                foreach (var detail in details)
                {
                    connection.Execute(insertDetailSql, new 
                    {
                        ReceiptId = receiptId,
                        detail.BookId,
                        detail.Quantity,
                        detail.Price
                    }, transaction);

                    connection.Execute(updateBookQtySql, new 
                    {
                        receipt.StoreId,
                        detail.BookId,
                        detail.Quantity
                    }, transaction);

                    connection.Execute(insertInvTransSql, new 
                    {
                        receipt.StoreId,
                        detail.BookId,
                        receipt.UserId,
                        detail.Quantity,
                        ReferenceId = receiptId,
                        Note = $"Xuất kho: {receipt.Reason}"
                    }, transaction);
                }

                return receiptId;
            });
        }

        public async System.Threading.Tasks.Task<int> CreateReceiptAsync(ExportReceipt receipt, List<ExportReceiptDetail> details)
        {
            int receiptId = 0;
            await ExecuteTransactionAsync(async (connection, transaction) =>
            {
                const string insertReceiptSql = @"
                    INSERT INTO ExportReceipts (ReceiptCode, StoreId, UserId, Reason, CustomerName, ExportDate, TotalAmount, Status, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@ReceiptCode, @StoreId, @UserId, @Reason, @CustomerName, @ExportDate, @TotalAmount, @Status, @Note);
                ";

                receiptId = await connection.ExecuteScalarAsync<int>(insertReceiptSql, receipt, transaction);

                const string insertDetailSql = @"
                    INSERT INTO ExportReceiptDetails (ReceiptId, BookId, Quantity, Price)
                    VALUES (@ReceiptId, @BookId, @Quantity, @Price);
                ";

                const string updateBookQtySql = @"
                    -- Kiểm tra tồn kho trước khi trừ
                    IF (SELECT ISNULL(Quantity, 0) FROM Books WHERE Id = @BookId) < @Quantity
                        RAISERROR(N'Sách không đủ số lượng trong kho chính để xuất.', 16, 1);

                    IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                    BEGIN
                        IF (SELECT ISNULL(Quantity, 0) FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId) < @Quantity
                            RAISERROR(N'Sách không đủ số lượng tại kho cửa hàng để xuất.', 16, 1);
                        UPDATE StoreBookInventories 
                        SET Quantity = Quantity - @Quantity 
                        WHERE StoreId = @StoreId AND BookId = @BookId;
                    END
                    ELSE
                        RAISERROR(N'Sách không tồn tại trong kho cửa hàng để xuất.', 16, 1);

                    UPDATE Books 
                    SET Quantity = Quantity - @Quantity 
                    WHERE Id = @BookId;
                ";

                const string insertInvTransSql = @"
                    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                    VALUES (@StoreId, @BookId, @UserId, 'EXPORT', -@Quantity, 'ExportReceipt', @ReferenceId, @Note, SYSDATETIME());
                ";

                foreach (var detail in details)
                {
                    await connection.ExecuteAsync(insertDetailSql, new 
                    {
                        ReceiptId = receiptId,
                        detail.BookId,
                        detail.Quantity,
                        detail.Price
                    }, transaction);

                    await connection.ExecuteAsync(updateBookQtySql, new 
                    {
                        receipt.StoreId,
                        detail.BookId,
                        detail.Quantity
                    }, transaction);

                    await connection.ExecuteAsync(insertInvTransSql, new 
                    {
                        receipt.StoreId,
                        detail.BookId,
                        receipt.UserId,
                        detail.Quantity,
                        ReferenceId = receiptId,
                        Note = $"Xuất kho: {receipt.Reason}"
                    }, transaction);
                }
            });
            return receiptId;
        }

        public List<ExportReceiptListViewModel> GetAll()
        {
            const string sql = @"
                SELECT 
                    e.Id, e.ReceiptCode, e.Reason, e.CustomerName, 
                    e.ExportDate, e.UserId, u.FullName AS UserName, 
                    e.TotalAmount, e.Status
                FROM ExportReceipts e
                LEFT JOIN Users u ON e.UserId = u.Id
                ORDER BY e.ExportDate DESC;
            ";

            var list = new List<ExportReceiptListViewModel>();
            ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new ExportReceiptListViewModel
                    {
                        Id = GetInt(reader, "Id"),
                        ReceiptCode = GetString(reader, "ReceiptCode"),
                        Reason = GetString(reader, "Reason"),
                        CustomerName = GetString(reader, "CustomerName"),
                        ExportDate = GetDateTime(reader, "ExportDate"),
                        UserId = GetInt(reader, "UserId"),
                        UserName = GetString(reader, "UserName"),
                        TotalAmount = GetDecimal(reader, "TotalAmount"),
                        Status = GetString(reader, "Status")
                    });
                }
                return true;
            }, sql);
            return list;
        }

        public async System.Threading.Tasks.Task<List<ExportReceiptListViewModel>> GetAllAsync()
        {
            const string sql = @"
                SELECT 
                    e.Id, e.ReceiptCode, e.Reason, e.CustomerName, 
                    e.ExportDate, e.UserId, u.FullName AS UserName, 
                    e.TotalAmount, e.Status
                FROM ExportReceipts e
                LEFT JOIN Users u ON e.UserId = u.Id
                ORDER BY e.ExportDate DESC;
            ";
            var result = await QueryAsync<ExportReceiptListViewModel>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public ExportReceipt? GetById(int id)
        {
            const string sql = @"
                SELECT Id, ReceiptCode, Reason, CustomerName, UserId, ExportDate, TotalAmount, Status, Note
                FROM ExportReceipts
                WHERE Id = @Id;
            ";
            ExportReceipt? receipt = null;
            ExecuteQuery(command =>
            {
                AddParameter(command.Parameters, "@Id", id);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    receipt = new ExportReceipt
                    {
                        Id = GetInt(reader, "Id"),
                        ReceiptCode = GetString(reader, "ReceiptCode"),
                        Reason = GetString(reader, "Reason"),
                        CustomerName = GetString(reader, "CustomerName"),
                        UserId = GetInt(reader, "UserId"),
                        ExportDate = GetDateTime(reader, "ExportDate"),
                        TotalAmount = GetDecimal(reader, "TotalAmount"),
                        Status = GetString(reader, "Status"),
                        Note = GetNullableString(reader, "Note")
                    };
                }
                return true;
            }, sql);
            return receipt;
        }

        public async System.Threading.Tasks.Task<ExportReceipt?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, ReceiptCode, Reason, CustomerName, UserId, ExportDate, TotalAmount, Status, Note
                FROM ExportReceipts
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<ExportReceipt>(sql, new { Id = id });
        }

        public List<ExportReceiptDetail> GetDetails(int receiptId)
        {
            const string sql = @"
                SELECT Id, ReceiptId, BookId, Quantity, Price, LineTotal
                FROM ExportReceiptDetails
                WHERE ReceiptId = @ReceiptId;
            ";
            var list = new List<ExportReceiptDetail>();
            ExecuteQuery(command =>
            {
                AddParameter(command.Parameters, "@ReceiptId", receiptId);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new ExportReceiptDetail
                    {
                        Id = GetInt(reader, "Id"),
                        ReceiptId = GetInt(reader, "ReceiptId"),
                        BookId = GetInt(reader, "BookId"),
                        Quantity = GetInt(reader, "Quantity"),
                        Price = GetDecimal(reader, "Price"),
                        LineTotal = GetDecimal(reader, "LineTotal")
                    });
                }
                return true;
            }, sql);
            return list;
        }

        public async System.Threading.Tasks.Task<List<ExportReceiptDetail>> GetDetailsAsync(int receiptId)
        {
            const string sql = @"
                SELECT Id, ReceiptId, BookId, Quantity, Price, LineTotal
                FROM ExportReceiptDetails
                WHERE ReceiptId = @ReceiptId;
            ";
            var result = await QueryAsync<ExportReceiptDetail>(sql, new { ReceiptId = receiptId });
            return System.Linq.Enumerable.ToList(result);
        }

        public (int TotalReceipts, decimal TotalValue, int PendingCount) GetStats()
        {
            const string sql = @"
                SELECT 
                    (SELECT COUNT(1) FROM ExportReceipts WHERE MONTH(ExportDate) = MONTH(GETDATE()) AND YEAR(ExportDate) = YEAR(GETDATE())) AS TotalReceipts,
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM ExportReceipts WHERE Status = N'Hoàn thành') AS TotalValue,
                    (SELECT COUNT(1) FROM ExportReceipts WHERE Status = N'Chờ duyệt') AS PendingCount;
            ";
            int totalReceipts = 0, pendingCount = 0;
            decimal totalValue = 0;
            ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    totalReceipts = GetInt(reader, "TotalReceipts");
                    totalValue = GetDecimal(reader, "TotalValue");
                    pendingCount = GetInt(reader, "PendingCount");
                }
                return true;
            }, sql);
            return (totalReceipts, totalValue, pendingCount);
        }

        public async System.Threading.Tasks.Task<(int TotalReceipts, decimal TotalValue, int PendingCount)> GetStatsAsync()
        {
            const string sql = @"
                SELECT 
                    (SELECT COUNT(1) FROM ExportReceipts WHERE MONTH(ExportDate) = MONTH(GETDATE()) AND YEAR(ExportDate) = YEAR(GETDATE())) AS TotalReceipts,
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM ExportReceipts WHERE Status = N'Hoàn thành') AS TotalValue,
                    (SELECT COUNT(1) FROM ExportReceipts WHERE Status = N'Chờ duyệt') AS PendingCount;
            ";
            var result = await QueryFirstOrDefaultAsync<(int TotalReceipts, decimal TotalValue, int PendingCount)>(sql);
            return result;
        }

        public void UpdateStatus(int id, string status)
        {
            const string sql = "UPDATE ExportReceipts SET Status = @Status WHERE Id = @Id;";
            ExecuteNonQuery(sql, parameters => 
            {
                AddParameter(parameters, "@Id", id);
                AddParameter(parameters, "@Status", status);
            });
        }

        public async System.Threading.Tasks.Task UpdateStatusAsync(int id, string status)
        {
            const string sql = "UPDATE ExportReceipts SET Status = @Status WHERE Id = @Id;";
            await ExecuteAsync(sql, new { Id = id, Status = status });
        }
    }
}
