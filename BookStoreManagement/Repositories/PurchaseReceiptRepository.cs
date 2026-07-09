using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;

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
                SupplierId = GetInt(reader, "SupplierId"),
                UserId = GetInt(reader, "UserId"),
                ImportDate = GetDateTime(reader, "ImportDate"),
                TotalAmount = GetDecimal(reader, "TotalAmount"),
                Note = GetNullableString(reader, "Note")
            };
        }

        private PurchaseReceiptListViewModel MapReceiptList(SqlDataReader reader)
        {
            return new PurchaseReceiptListViewModel
            {
                Id = GetInt(reader, "Id"),
                ReceiptCode = GetString(reader, "ReceiptCode"),
                ImportDate = GetDateTime(reader, "ImportDate"),
                SupplierId = GetInt(reader, "SupplierId"),
                SupplierName = GetString(reader, "SupplierName"),
                StaffId = GetInt(reader, "StaffId"),
                StaffName = GetString(reader, "StaffName"),
                TotalAmount = GetDecimal(reader, "TotalAmount"),
                Note = GetNullableString(reader, "Note")
            };
        }

        public int CreateReceipt(PurchaseReceipt receipt, List<PurchaseReceiptDetail> details)
        {
            return ExecuteTransaction((connection, transaction) =>
            {
                const string insertReceiptSql = @"
                    INSERT INTO PurchaseReceipts (ReceiptCode, SupplierId, UserId, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@ReceiptCode, @SupplierId, @UserId, @Note);
                ";

                using var receiptCommand = new SqlCommand(insertReceiptSql, connection, transaction);
                receiptCommand.Parameters.AddWithValue("@ReceiptCode", receipt.ReceiptCode);
                receiptCommand.Parameters.AddWithValue("@SupplierId", receipt.SupplierId);
                receiptCommand.Parameters.AddWithValue("@UserId", receipt.UserId);
                receiptCommand.Parameters.AddWithValue("@Note", (object?)receipt.Note ?? DBNull.Value);

                int receiptId = Convert.ToInt32(receiptCommand.ExecuteScalar());

                const string insertDetailSql = @"
                    INSERT INTO PurchaseReceiptDetails (PurchaseReceiptId, BookId, Quantity, ImportPrice)
                    VALUES (@PurchaseReceiptId, @BookId, @Quantity, @ImportPrice);
                ";

                foreach (var detail in details)
                {
                    using var detailCommand = new SqlCommand(insertDetailSql, connection, transaction);
                    detailCommand.Parameters.AddWithValue("@PurchaseReceiptId", receiptId);
                    detailCommand.Parameters.AddWithValue("@BookId", detail.BookId);
                    detailCommand.Parameters.AddWithValue("@Quantity", detail.Quantity);
                    detailCommand.Parameters.AddWithValue("@ImportPrice", detail.ImportPrice);
                    detailCommand.ExecuteNonQuery();
                }

                return receiptId;
            });
        }

        public List<PurchaseReceiptListViewModel> GetAll()
        {
            var receipts = new List<PurchaseReceiptListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_PurchaseReceiptList
                ORDER BY ImportDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReceiptList(reader));
                return receipts;
            }, sql);
        }

        public PurchaseReceipt? GetById(int id)
        {
            const string sql = @"
                SELECT Id, ReceiptCode, SupplierId, UserId, ImportDate, TotalAmount, Note
                FROM PurchaseReceipts
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapPurchaseReceipt(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public List<PurchaseReceiptListViewModel> Search(string keyword)
        {
            var receipts = new List<PurchaseReceiptListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_PurchaseReceiptList
                WHERE ReceiptCode LIKE N'%' + @Keyword + N'%'
                   OR SupplierName LIKE N'%' + @Keyword + N'%'
                   OR StaffName LIKE N'%' + @Keyword + N'%'
                ORDER BY ImportDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReceiptList(reader));
                return receipts;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public List<PurchaseReceiptListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            var receipts = new List<PurchaseReceiptListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_PurchaseReceiptList
                WHERE ImportDate >= @FromDate
                  AND ImportDate < DATEADD(DAY, 1, @ToDate)
                ORDER BY ImportDate DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) receipts.Add(MapReceiptList(reader));
                return receipts;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@FromDate", fromDate.Date);
                AddParameter(parameters, "@ToDate", toDate.Date);
            });
        }

        public bool IsReceiptCodeExists(string receiptCode)
        {
            const string sql = "SELECT COUNT(1) FROM PurchaseReceipts WHERE ReceiptCode = @ReceiptCode;";
            return ExecuteScalarInt(sql, parameters => AddParameter(parameters, "@ReceiptCode", receiptCode)) > 0;
        }

        public string GenerateReceiptCode()
        {
            return "PR" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
