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
                StoreId = GetInt(reader, "StoreId"),
                SupplierId = GetInt(reader, "SupplierId"),
                UserId = GetInt(reader, "UserId"),
                ImportDate = GetDateTime(reader, "ImportDate"),
                TotalAmount = GetDecimal(reader, "TotalAmount"),
                Note = GetNullableString(reader, "Note")
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
                SellingPrice = GetNullableDecimal(reader, "SellingPrice"),
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
                Note = GetNullableString(reader, "Note")
            };
        }

        public int CreateReceipt(PurchaseReceipt receipt, List<PurchaseReceiptDetail> details)
        {
            return ExecuteTransaction((connection, transaction) =>
            {
                const string insertReceiptSql = @"
                    INSERT INTO PurchaseReceipts (ReceiptCode, StoreId, SupplierId, UserId, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@ReceiptCode, @StoreId, @SupplierId, @UserId, @Note);
                ";

                using var receiptCommand = new SqlCommand(insertReceiptSql, connection, transaction);
                AddParameter(receiptCommand, "@ReceiptCode", receipt.ReceiptCode);
                AddParameter(receiptCommand, "@StoreId", receipt.StoreId);
                AddParameter(receiptCommand, "@SupplierId", receipt.SupplierId);
                AddParameter(receiptCommand, "@UserId", receipt.UserId);
                AddParameter(receiptCommand, "@Note", receipt.Note);

                int receiptId = Convert.ToInt32(receiptCommand.ExecuteScalar());

                const string insertDetailSql = @"
                    INSERT INTO PurchaseReceiptDetails (PurchaseReceiptId, BookId, Quantity, ImportPrice, SellingPrice)
                    VALUES (@PurchaseReceiptId, @BookId, @Quantity, @ImportPrice, @SellingPrice);
                ";

                foreach (var detail in details)
                {
                    using var detailCommand = new SqlCommand(insertDetailSql, connection, transaction);
                    AddParameter(detailCommand, "@PurchaseReceiptId", receiptId);
                    AddParameter(detailCommand, "@BookId", detail.BookId);
                    AddParameter(detailCommand, "@Quantity", detail.Quantity);
                    AddParameter(detailCommand, "@ImportPrice", detail.ImportPrice);
                    AddParameter(detailCommand, "@SellingPrice", detail.SellingPrice);
                    detailCommand.ExecuteNonQuery();
                }

                return receiptId;
            });
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

        public PurchaseReceipt? GetById(int id)
        {
            const string sql = @"
                SELECT Id, ReceiptCode, StoreId, SupplierId, UserId, ImportDate, TotalAmount, Note
                FROM PurchaseReceipts
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapPurchaseReceipt(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
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

        public List<PurchaseReceiptDetail> GetDetails(int receiptId)
        {
            var details = new List<PurchaseReceiptDetail>();
            const string sql = @"
                SELECT Id, PurchaseReceiptId, BookId, Quantity, ImportPrice, SellingPrice, LineTotal
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
