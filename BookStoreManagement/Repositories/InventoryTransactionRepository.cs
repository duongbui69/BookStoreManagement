using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string VoucherNo { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ImportQty { get; set; }
        public int ExportQty { get; set; }
        public int Balance { get; set; }
        public int BookId { get; set; }
    }

    public class LedgerSummary
    {
        public int TotalIn { get; set; }
        public int TotalOut { get; set; }
        public int EndBalance { get; set; }
    }

    public class InventoryTransactionRepository : RepositoryBase
    {
        public async Task<(List<InventoryTransactionDto> Items, int TotalCount)> GetPagedTransactionsAsync(
            int page, int pageSize, DateTime fromDate, DateTime toDate,
            int bookId = 0, int storeId = 0, string referenceType = "")
        {
            var filters = new List<string> { "t.CreatedAt >= @FromDate", "t.CreatedAt <= @ToDate" };
            if (bookId > 0) filters.Add("t.BookId = @BookId");
            if (storeId > 0) filters.Add("t.StoreId = @StoreId");
            if (!string.IsNullOrEmpty(referenceType)) filters.Add("t.ReferenceType = @ReferenceType");

            string whereClause = "WHERE " + string.Join(" AND ", filters);

            var parameters = new {
                FromDate = fromDate.Date,
                ToDate = toDate.Date.AddDays(1).AddSeconds(-1),
                BookId = bookId,
                StoreId = storeId,
                ReferenceType = referenceType,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };

            string countQuery = $"SELECT COUNT(*) FROM InventoryTransactions t {whereClause}";
            int totalCount = await ExecuteScalarAsync<int>(countQuery, parameters);

            string query = $@"
                WITH CTE_Transactions AS (
                    SELECT 
                        t.Id, t.CreatedAt, t.ReferenceType, t.ReferenceId, b.Title as ProductName, t.Note as Description, 
                        t.TransactionType, t.QuantityChange, t.BookId, t.StoreId,
                        SUM(t.QuantityChange) OVER (PARTITION BY t.BookId ORDER BY t.CreatedAt ROWS UNBOUNDED PRECEDING) as Balance
                    FROM InventoryTransactions t
                    JOIN Books b ON t.BookId = b.Id
                )
                SELECT * FROM CTE_Transactions t
                {whereClause}
                ORDER BY t.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var rawList = await QueryAsync<RawTransactionDto>(query, parameters);
            var list = new List<InventoryTransactionDto>();

            foreach(var r in rawList)
            {
                string refType = r.ReferenceType ?? "OTHER";
                int refId = r.ReferenceId ?? 0;
                string voucher = refType switch
                {
                    "PurchaseReceipt" => $"PNK-{r.CreatedAt:yyMM}-{refId:D2}",
                    "SalesOrder"      => $"PXK-{r.CreatedAt:yyMM}-{refId:D2}",
                    "ReturnReceipt"   => $"PNT-{r.CreatedAt:yyMM}-{refId:D2}",
                    _                 => $"{refType}-{refId}"
                };

                list.Add(new InventoryTransactionDto
                {
                    Id = r.Id,
                    CreatedAt = r.CreatedAt,
                    VoucherNo = voucher,
                    ReferenceType = refType,
                    ProductName = r.ProductName ?? "",
                    Description = r.Description ?? "",
                    ImportQty = r.QuantityChange > 0 ? r.QuantityChange : 0,
                    ExportQty = r.QuantityChange < 0 ? Math.Abs(r.QuantityChange) : 0,
                    BookId = r.BookId,
                    Balance = r.Balance
                });
            }

            return (list, totalCount);
        }

        public async Task<LedgerSummary> GetLedgerSummaryAsync(
            DateTime fromDate, DateTime toDate, int bookId = 0, int storeId = 0, string referenceType = "")
        {
            var filters = new List<string> { "t.CreatedAt >= @FromDate", "t.CreatedAt <= @ToDate" };
            if (bookId > 0) filters.Add("t.BookId = @BookId");
            if (storeId > 0) filters.Add("t.StoreId = @StoreId");
            if (!string.IsNullOrEmpty(referenceType)) filters.Add("t.ReferenceType = @ReferenceType");

            string whereClause = "WHERE " + string.Join(" AND ", filters);

            var parameters = new
            {
                FromDate = fromDate.Date,
                ToDate = toDate.Date.AddDays(1).AddSeconds(-1),
                BookId = bookId,
                StoreId = storeId,
                ReferenceType = referenceType
            };

            string sql = $@"
                SELECT 
                    ISNULL(SUM(CASE WHEN t.QuantityChange > 0 THEN t.QuantityChange ELSE 0 END), 0) AS TotalIn,
                    ISNULL(SUM(CASE WHEN t.QuantityChange < 0 THEN ABS(t.QuantityChange) ELSE 0 END), 0) AS TotalOut,
                    ISNULL(SUM(t.QuantityChange), 0) AS EndBalance
                FROM InventoryTransactions t
                {whereClause}";

            return await QueryFirstOrDefaultAsync<LedgerSummary>(sql, parameters)
                   ?? new LedgerSummary();
        }

        private class RawTransactionDto
        {
            public int Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? ReferenceType { get; set; }
            public int? ReferenceId { get; set; }
            public string? ProductName { get; set; }
            public string? Description { get; set; }
            public string? TransactionType { get; set; }
            public int QuantityChange { get; set; }
            public int BookId { get; set; }
            public int StoreId { get; set; }
            public int Balance { get; set; }
        }

        public InventoryTransaction? GetById(int id)
        {
            return ExecuteQuery(cmd =>
            {
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new InventoryTransaction
                    {
                        Id = reader.GetInt32(0),
                        StoreId = reader.GetInt32(1),
                        BookId = reader.GetInt32(2),
                        UserId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                        TransactionType = reader.GetString(4),
                        QuantityChange = reader.GetInt32(5),
                        ReferenceType = reader.IsDBNull(6) ? null : reader.GetString(6),
                        ReferenceId = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                        Note = reader.IsDBNull(8) ? null : reader.GetString(8),
                        CreatedAt = reader.GetDateTime(9)
                    };
                }
                return null;
            }, "SELECT Id, StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt FROM InventoryTransactions WHERE Id = @Id", p => AddParameter(p, "@Id", id));
        }

        public bool UpdateNote(int id, string note)
        {
            return ExecuteNonQuery("UPDATE InventoryTransactions SET Note = @Note WHERE Id = @Id", p =>
            {
                AddParameter(p, "@Note", note);
                AddParameter(p, "@Id", id);
            }) > 0;
        }

        public bool Delete(int id)
        {
            return ExecuteNonQuery("DELETE FROM InventoryTransactions WHERE Id = @Id", p => AddParameter(p, "@Id", id)) > 0;
        }

        public async System.Threading.Tasks.Task<InventoryTransaction?> GetByIdAsync(int id)
        {
            const string sql = "SELECT Id, StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note, CreatedAt FROM InventoryTransactions WHERE Id = @Id";
            return await QueryFirstOrDefaultAsync<InventoryTransaction>(sql, new { Id = id });
        }

        public async System.Threading.Tasks.Task<bool> UpdateNoteAsync(int id, string note)
        {
            const string sql = "UPDATE InventoryTransactions SET Note = @Note WHERE Id = @Id";
            return await ExecuteAsync(sql, new { Note = note, Id = id }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM InventoryTransactions WHERE Id = @Id";
            return await ExecuteAsync(sql, new { Id = id }) > 0;
        }
    }
}
