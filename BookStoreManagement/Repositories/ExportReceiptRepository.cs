using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class ExportReceiptRepository : RepositoryBase
    {
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
