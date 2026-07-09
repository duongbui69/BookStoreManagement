using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class InventoryRepository : RepositoryBase
    {
        private InventoryHistoryViewModel MapInventoryHistory(SqlDataReader reader)
        {
            return new InventoryHistoryViewModel
            {
                Id = GetInt(reader, "Id"),
                BookId = GetInt(reader, "BookId"),
                BookCode = GetString(reader, "BookCode"),
                Title = GetString(reader, "Title"),
                StaffId = GetNullableInt(reader, "StaffId"),
                StaffName = GetNullableString(reader, "StaffName"),
                TransactionType = GetString(reader, "TransactionType"),
                QuantityChange = GetInt(reader, "QuantityChange"),
                ReferenceType = GetNullableString(reader, "ReferenceType"),
                ReferenceId = GetNullableInt(reader, "ReferenceId"),
                Note = GetNullableString(reader, "Note"),
                CreatedAt = GetDateTime(reader, "CreatedAt")
            };
        }

        public List<InventoryHistoryViewModel> GetHistory()
        {
            var history = new List<InventoryHistoryViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_InventoryHistory
                ORDER BY CreatedAt DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) history.Add(MapInventoryHistory(reader));
                return history;
            }, sql);
        }

        public List<InventoryHistoryViewModel> GetHistoryByBookId(int bookId)
        {
            var history = new List<InventoryHistoryViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_InventoryHistory
                WHERE BookId = @BookId
                ORDER BY CreatedAt DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) history.Add(MapInventoryHistory(reader));
                return history;
            }, sql, parameters => AddParameter(parameters, "@BookId", bookId));
        }

        public List<LowStockBookViewModel> GetLowStockBooks()
        {
            var books = new List<LowStockBookViewModel>();
            const string sql = @"
                SELECT Id, BookCode, Title, Quantity, MinStock
                FROM vw_LowStockBooks
                ORDER BY Quantity ASC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    books.Add(new LowStockBookViewModel
                    {
                        Id = GetInt(reader, "Id"),
                        BookCode = GetString(reader, "BookCode"),
                        Title = GetString(reader, "Title"),
                        Quantity = GetInt(reader, "Quantity"),
                        MinStock = GetInt(reader, "MinStock")
                    });
                }
                return books;
            }, sql);
        }

        public bool AdjustStock(int bookId, int userId, int quantityChange, string? note)
        {
            const string sql = @"
                EXEC sp_AdjustBookStock
                    @BookId = @BookId,
                    @UserId = @UserId,
                    @QuantityChange = @QuantityChange,
                    @Note = @Note;
            ";
            ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@BookId", bookId);
                AddParameter(parameters, "@UserId", userId);
                AddParameter(parameters, "@QuantityChange", quantityChange);
                AddParameter(parameters, "@Note", note);
            });
            return true;
        }
    }
}
