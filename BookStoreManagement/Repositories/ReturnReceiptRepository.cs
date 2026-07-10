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
                Note = GetNullableString(reader, "Note")
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
                Note = GetNullableString(reader, "Note")
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
                Note = GetNullableString(reader, "Note")
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
                SELECT Id, ReturnCode, SalesOrderId, StoreId, CustomerId, UserId, ReturnDate, TotalRefundAmount, Note
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
    }
}
