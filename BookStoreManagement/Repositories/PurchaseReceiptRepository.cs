using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;
using Dapper;

namespace BookStoreManagement.Repositories
{
    public class PurchaseReceiptRepository : RepositoryBase
    {
        private PurchaseReceipt MapPurchaseReceipt(SqlDataReader reader)
        {
            return new PurchaseReceipt
            {
                Id = GetInt(reader, "Id"),
                ReceiptCode = GetString(reader, "ReceiptCode"),
                StoreId = GetInt(reader, "StoreId"),
                SupplierId = GetInt(reader, "SupplierId"),
                UserId = GetInt(reader, "UserId"),
                ImportDate = GetDateTime(reader, "ImportDate"),
                TotalAmount = GetDecimal(reader, "TotalAmount"),
                Note = GetNullableString(reader, "Note"),
                Status = GetString(reader, "Status")
            };
        }

        private PurchaseReceiptDetail MapPurchaseReceiptDetail(SqlDataReader reader)
        {
            return new PurchaseReceiptDetail
            {
                Id = GetInt(reader, "Id"),
                PurchaseReceiptId = GetInt(reader, "PurchaseReceiptId"),
                BookId = GetInt(reader, "BookId"),
                Quantity = GetInt(reader, "Quantity"),
                ImportPrice = GetDecimal(reader, "ImportPrice"),
                
                LineTotal = GetDecimal(reader, "LineTotal")
            };
        }

        private PurchaseReceiptListViewModel MapReceiptList(SqlDataReader reader)
        {
            return new PurchaseReceiptListViewModel
            {
                Id = GetInt(reader, "Id"),
                ReceiptCode = GetString(reader, "ReceiptCode"),
                StoreId = GetInt(reader, "StoreId"),
                StoreName = GetString(reader, "StoreName"),
                ImportDate = GetDateTime(reader, "ImportDate"),
                SupplierId = GetInt(reader, "SupplierId"),
                SupplierName = GetString(reader, "SupplierName"),
                StaffId = GetInt(reader, "StaffId"),
                StaffName = GetString(reader, "StaffName"),
                TotalAmount = GetDecimal(reader, "TotalAmount"),
                Note = GetNullableString(reader, "Note"),
                Status = GetString(reader, "Status")
            };
        }

        public int CreateReceipt(PurchaseReceipt receipt, List<PurchaseReceiptDetail> details)
        {
            return ExecuteTransaction((connection, transaction) =>
            {
                const string insertReceiptSql = @"
                    INSERT INTO PurchaseReceipts (ReceiptCode, StoreId, SupplierId, UserId, Note, TotalAmount)
                    OUTPUT INSERTED.Id
                    VALUES (@ReceiptCode, @StoreId, @SupplierId, @UserId, @Note, @TotalAmount);
                ";

                using var receiptCommand = new SqlCommand(insertReceiptSql, connection, transaction);
                AddParameter(receiptCommand, "@ReceiptCode", receipt.ReceiptCode);
                AddParameter(receiptCommand, "@StoreId", receipt.StoreId);
                AddParameter(receiptCommand, "@SupplierId", receipt.SupplierId);
                AddParameter(receiptCommand, "@UserId", receipt.UserId);
                AddParameter(receiptCommand, "@Note", receipt.Note);
                AddParameter(receiptCommand, "@TotalAmount", receipt.TotalAmount);

                int receiptId = Convert.ToInt32(receiptCommand.ExecuteScalar());

                const string insertDetailSql = @"
                    INSERT INTO PurchaseReceiptDetails (PurchaseReceiptId, BookId, Quantity, ImportPrice)
                    VALUES (@PurchaseReceiptId, @BookId, @Quantity, @ImportPrice);
                ";
                
                const string updateBookQtySql = @"
                    UPDATE Books 
                    SET Quantity = Quantity + @Quantity 
                    WHERE Id = @BookId;

                    IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                        UPDATE StoreBookInventories SET Quantity = Quantity + @Quantity WHERE StoreId = @StoreId AND BookId = @BookId;
                    ELSE
                        INSERT INTO StoreBookInventories (StoreId, BookId, Quantity, MinStock, SellingPrice, IsActive, CreatedAt)
                        VALUES (@StoreId, @BookId, @Quantity, 0, @SellingPrice, 1, SYSDATETIME());
                ";

                const string insertInvTransSql = @"
                    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                    VALUES (@StoreId, @BookId, @UserId, 'IMPORT', @Quantity, 'PurchaseReceipt', @ReferenceId, @Note, SYSDATETIME());
                ";

                foreach (var detail in details)
                {
                    if ((detail.SellingPrice ?? 0) <= 0)
                        throw new Exception($"Giá bán của sản phẩm {detail.BookId} phải lớn hơn 0.");

                    using var detailCommand = new SqlCommand(insertDetailSql, connection, transaction);
                    AddParameter(detailCommand, "@PurchaseReceiptId", receiptId);
                    AddParameter(detailCommand, "@BookId", detail.BookId);
                    AddParameter(detailCommand, "@Quantity", detail.Quantity);
                    AddParameter(detailCommand, "@ImportPrice", detail.ImportPrice);
                    AddParameter(detailCommand, "@SellingPrice", detail.SellingPrice);
                    detailCommand.ExecuteNonQuery();

                    using var bookCommand = new SqlCommand(updateBookQtySql, connection, transaction);
                    AddParameter(bookCommand, "@StoreId", receipt.StoreId);
                    AddParameter(bookCommand, "@Quantity", detail.Quantity);
                    AddParameter(bookCommand, "@BookId", detail.BookId);
                    AddParameter(bookCommand, "@SellingPrice", detail.SellingPrice ?? 0);
                    bookCommand.ExecuteNonQuery();

                    using var transCommand = new SqlCommand(insertInvTransSql, connection, transaction);
                    AddParameter(transCommand, "@StoreId", receipt.StoreId);
                    AddParameter(transCommand, "@BookId", detail.BookId);
                    AddParameter(transCommand, "@UserId", receipt.UserId);
                    AddParameter(transCommand, "@Quantity", detail.Quantity);
                    AddParameter(transCommand, "@ReferenceId", receiptId);
                    AddParameter(transCommand, "@Note", $"Nhập kho từ phiếu {receipt.ReceiptCode}");
                    transCommand.ExecuteNonQuery();
                }

                return receiptId;
            });
        }

        public async Task<int> CreateReceiptAsync(PurchaseReceipt receipt, List<PurchaseReceiptDetail> details)
        {
            int receiptId = 0;
            await ExecuteTransactionAsync(async (connection, transaction) =>
            {
                const string insertReceiptSql = @"
                    INSERT INTO PurchaseReceipts (ReceiptCode, StoreId, SupplierId, UserId, Note, TotalAmount)
                    OUTPUT INSERTED.Id
                    VALUES (@ReceiptCode, @StoreId, @SupplierId, @UserId, @Note, @TotalAmount);
                ";
                receiptId = await connection.ExecuteScalarAsync<int>(insertReceiptSql, receipt, transaction);

                const string insertDetailSql = @"
                    INSERT INTO PurchaseReceiptDetails (PurchaseReceiptId, BookId, Quantity, ImportPrice)
                    VALUES (@PurchaseReceiptId, @BookId, @Quantity, @ImportPrice);
                ";

                const string updateBookQtySql = @"
                    UPDATE Books 
                    SET Quantity = Quantity + @Quantity 
                    WHERE Id = @BookId;

                    IF EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
                        UPDATE StoreBookInventories SET Quantity = Quantity + @Quantity WHERE StoreId = @StoreId AND BookId = @BookId;
                    ELSE
                        INSERT INTO StoreBookInventories (StoreId, BookId, Quantity, MinStock, SellingPrice, IsActive, CreatedAt)
                        VALUES (@StoreId, @BookId, @Quantity, 0, @SellingPrice, 1, SYSDATETIME());
                ";

                const string insertInvTransSql = @"
                    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt)
                    VALUES (@StoreId, @BookId, @UserId, 'IMPORT', @Quantity, 'PurchaseReceipt', @ReferenceId, @Note, SYSDATETIME());
                ";

                foreach (var detail in details)
                {
                    if ((detail.SellingPrice ?? 0) <= 0)
                        throw new Exception($"Giá bán của sản phẩm {detail.BookId} phải lớn hơn 0.");

                    await connection.ExecuteAsync(insertDetailSql, new 
                    {
                        PurchaseReceiptId = receiptId,
                        detail.BookId,
                        detail.Quantity,
                        detail.ImportPrice,
                        detail.SellingPrice
                    }, transaction);

                    await connection.ExecuteAsync(updateBookQtySql, new 
                    {
                        StoreId = receipt.StoreId,
                        Quantity = detail.Quantity,
                        BookId = detail.BookId,
                        SellingPrice = detail.SellingPrice ?? 0
                    }, transaction);

                    await connection.ExecuteAsync(insertInvTransSql, new
                    {
                        StoreId = receipt.StoreId,
                        BookId = detail.BookId,
                        UserId = receipt.UserId,
                        Quantity = detail.Quantity,
                        ReferenceId = receiptId,
                        Note = $"Nhập kho từ phiếu {receipt.ReceiptCode}"
                    }, transaction);
                }
            });
            return receiptId;
        }

        public List<PurchaseReceiptListViewModel> GetAll()
        {
            var receipts = new List<PurchaseReceiptListViewModel>();
            const string sql = "SELECT * FROM vw_PurchaseReceiptList ORDER BY ImportDate DESC;";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReceiptList(reader));
                return receipts;
            }, sql);
        }

        public async Task<List<PurchaseReceiptListViewModel>> GetAllAsync()
        {
            const string sql = "SELECT * FROM vw_PurchaseReceiptList ORDER BY ImportDate DESC;";
            var result = await QueryAsync<PurchaseReceiptListViewModel>(sql);
            return result.ToList();
        }

        public PurchaseReceipt? GetById(int id)
        {
            const string sql = @"
                SELECT Id, ReceiptCode, StoreId, SupplierId, UserId, ImportDate, TotalAmount, Note, Status
                FROM PurchaseReceipts
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapPurchaseReceipt(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public async Task<PurchaseReceipt?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, ReceiptCode, StoreId, SupplierId, UserId, ImportDate, TotalAmount, Note, Status
                FROM PurchaseReceipts
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<PurchaseReceipt>(sql, new { Id = id });
        }

        public List<PurchaseReceiptListViewModel> GetByStoreId(int storeId)
        {
            var receipts = new List<PurchaseReceiptListViewModel>();
            const string sql = @"
                SELECT * FROM vw_PurchaseReceiptList
                WHERE StoreId = @StoreId
                ORDER BY ImportDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReceiptList(reader));
                return receipts;
            }, sql, parameters => AddParameter(parameters, "@StoreId", storeId));
        }

        public async Task<List<PurchaseReceiptListViewModel>> GetByStoreIdAsync(int storeId)
        {
            const string sql = @"
                SELECT * FROM vw_PurchaseReceiptList
                WHERE StoreId = @StoreId
                ORDER BY ImportDate DESC;
            ";
            var result = await QueryAsync<PurchaseReceiptListViewModel>(sql, new { StoreId = storeId });
            return result.ToList();
        }

        public List<PurchaseReceiptDetail> GetDetails(int receiptId)
        {
            var details = new List<PurchaseReceiptDetail>();
            const string sql = @"
                SELECT Id, PurchaseReceiptId, BookId, Quantity, ImportPrice, LineTotal
                FROM PurchaseReceiptDetails
                WHERE PurchaseReceiptId = @ReceiptId;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) details.Add(MapPurchaseReceiptDetail(reader));
                return details;
            }, sql, parameters => AddParameter(parameters, "@ReceiptId", receiptId));
        }

        public async Task<List<PurchaseReceiptDetail>> GetDetailsAsync(int receiptId)
        {
            const string sql = @"
                SELECT Id, PurchaseReceiptId, BookId, Quantity, ImportPrice, LineTotal
                FROM PurchaseReceiptDetails
                WHERE PurchaseReceiptId = @ReceiptId;
            ";
            var result = await QueryAsync<PurchaseReceiptDetail>(sql, new { ReceiptId = receiptId });
            return result.ToList();
        }

        public List<PurchaseReceiptListViewModel> Search(string keyword, int? storeId = null)
        {
            var receipts = new List<PurchaseReceiptListViewModel>();
            string sql = @"
                SELECT * FROM vw_PurchaseReceiptList
                WHERE (
                    ReceiptCode LIKE N'%' + @Keyword + N'%'
                    OR StoreName LIKE N'%' + @Keyword + N'%'
                    OR SupplierName LIKE N'%' + @Keyword + N'%'
                    OR StaffName LIKE N'%' + @Keyword + N'%'
                )
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY ImportDate DESC;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReceiptList(reader));
                return receipts;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@Keyword", keyword);
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public async Task<List<PurchaseReceiptListViewModel>> SearchAsync(string keyword, int? storeId = null)
        {
            string sql = @"
                SELECT * FROM vw_PurchaseReceiptList
                WHERE (
                    ReceiptCode LIKE N'%' + @Keyword + N'%'
                    OR StoreName LIKE N'%' + @Keyword + N'%'
                    OR SupplierName LIKE N'%' + @Keyword + N'%'
                    OR StaffName LIKE N'%' + @Keyword + N'%'
                )
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY ImportDate DESC;";

            var result = await QueryAsync<PurchaseReceiptListViewModel>(sql, new { Keyword = keyword, StoreId = storeId });
            return result.ToList();
        }

        public List<PurchaseReceiptListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            var receipts = new List<PurchaseReceiptListViewModel>();
            string sql = @"
                SELECT * FROM vw_PurchaseReceiptList
                WHERE ImportDate >= @FromDate
                  AND ImportDate < DATEADD(DAY, 1, @ToDate)
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY ImportDate DESC;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReceiptList(reader));
                return receipts;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@FromDate", fromDate.Date);
                AddParameter(parameters, "@ToDate", toDate.Date);
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public async Task<List<PurchaseReceiptListViewModel>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int? storeId = null)
        {
            string sql = @"
                SELECT * FROM vw_PurchaseReceiptList
                WHERE ImportDate >= @FromDate
                  AND ImportDate < DATEADD(DAY, 1, @ToDate)
            ";
            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY ImportDate DESC;";

            var result = await QueryAsync<PurchaseReceiptListViewModel>(sql, new { FromDate = fromDate.Date, ToDate = toDate.Date, StoreId = storeId });
            return result.ToList();
        }

        public bool IsReceiptCodeExists(string receiptCode)
        {
            const string sql = "SELECT COUNT(1) FROM PurchaseReceipts WHERE ReceiptCode = @ReceiptCode;";
            return ExecuteScalarInt(sql, parameters => AddParameter(parameters, "@ReceiptCode", receiptCode)) > 0;
        }

        public async Task<bool> IsReceiptCodeExistsAsync(string receiptCode)
        {
            const string sql = "SELECT COUNT(1) FROM PurchaseReceipts WHERE ReceiptCode = @ReceiptCode;";
            return await ExecuteScalarAsync<int>(sql, new { ReceiptCode = receiptCode }) > 0;
        }

        public string GenerateReceiptCode()
        {
            return "PR" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        public (int TotalReceipts, decimal TotalValue, int PendingCount) GetStats()
        {
            const string sql = @"
                SELECT 
                    (SELECT COUNT(1) FROM PurchaseReceipts WHERE MONTH(ImportDate) = MONTH(GETDATE()) AND YEAR(ImportDate) = YEAR(GETDATE())) AS TotalReceipts,
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM PurchaseReceipts WHERE Status = N'Đã nhập') AS TotalValue,
                    (SELECT COUNT(1) FROM PurchaseReceipts WHERE Status = N'Chờ duyệt') AS PendingCount;
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

        public async Task<(int TotalReceipts, decimal TotalValue, int PendingCount)> GetStatsAsync()
        {
            const string sql = @"
                SELECT 
                    (SELECT COUNT(1) FROM PurchaseReceipts WHERE MONTH(ImportDate) = MONTH(GETDATE()) AND YEAR(ImportDate) = YEAR(GETDATE())) AS TotalReceipts,
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM PurchaseReceipts WHERE Status = N'Đã nhập') AS TotalValue,
                    (SELECT COUNT(1) FROM PurchaseReceipts WHERE Status = N'Chờ duyệt') AS PendingCount;
            ";
            var result = await QueryFirstOrDefaultAsync<dynamic>(sql);
            if (result != null)
            {
                return (
                    (int)result.TotalReceipts,
                    (decimal)result.TotalValue,
                    (int)result.PendingCount
                );
            }
            return (0, 0m, 0);
        }

        public void UpdateStatus(int id, string status)
        {
            const string sql = "UPDATE PurchaseReceipts SET Status = @Status WHERE Id = @Id;";
            ExecuteNonQuery(sql, parameters => 
            {
                AddParameter(parameters, "@Id", id);
                AddParameter(parameters, "@Status", status);
            });
        }

        public async Task UpdateStatusAsync(int id, string status)
        {
            const string sql = "UPDATE PurchaseReceipts SET Status = @Status WHERE Id = @Id;";
            await ExecuteAsync(sql, new { Id = id, Status = status });
        }
    }
}
