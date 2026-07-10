using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class StoreBookInventoryRepository : RepositoryBase
    {
        private StoreBookInventory MapInventory(SqlDataReader reader)
        {
            return new StoreBookInventory
            {
                Id = GetInt(reader, "Id"),
                StoreId = GetInt(reader, "StoreId"),
                BookId = GetInt(reader, "BookId"),
                Quantity = GetInt(reader, "Quantity"),
                MinStock = GetInt(reader, "MinStock"),
                ImportPrice = GetNullableDecimal(reader, "ImportPrice"),
                SellingPrice = GetDecimal(reader, "SellingPrice"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        private StoreBookInventoryViewModel MapInventoryView(SqlDataReader reader)
        {
            return new StoreBookInventoryViewModel
            {
                Id = GetInt(reader, "Id"),
                StoreId = GetInt(reader, "StoreId"),
                StoreCode = GetString(reader, "StoreCode"),
                StoreName = GetString(reader, "StoreName"),
                BookId = GetInt(reader, "BookId"),
                BookCode = GetString(reader, "BookCode"),
                ISBN = GetNullableString(reader, "ISBN"),
                Title = GetString(reader, "Title"),
                CategoryName = GetString(reader, "CategoryName"),
                AuthorName = GetNullableString(reader, "AuthorName"),
                PublisherName = GetNullableString(reader, "PublisherName"),
                Quantity = GetInt(reader, "Quantity"),
                MinStock = GetInt(reader, "MinStock"),
                ImportPrice = GetNullableDecimal(reader, "ImportPrice"),
                SellingPrice = GetDecimal(reader, "SellingPrice"),
                StockStatus = GetString(reader, "StockStatus"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<StoreBookInventoryViewModel> GetAll()
        {
            var items = new List<StoreBookInventoryViewModel>();
            const string sql = "SELECT * FROM vw_StoreBookInventory ORDER BY StoreId, Title;";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapInventoryView(reader));
                return items;
            }, sql);
        }

        public List<StoreBookInventoryViewModel> GetByStoreId(int storeId)
        {
            var items = new List<StoreBookInventoryViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_StoreBookInventory
                WHERE StoreId = @StoreId
                ORDER BY Title;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapInventoryView(reader));
                return items;
            }, sql, parameters => AddParameter(parameters, "@StoreId", storeId));
        }

        public StoreBookInventory? GetByStoreAndBook(int storeId, int bookId)
        {
            const string sql = @"
                SELECT Id, StoreId, BookId, Quantity, MinStock, ImportPrice, SellingPrice, IsActive, CreatedAt, UpdatedAt
                FROM StoreBookInventories
                WHERE StoreId = @StoreId AND BookId = @BookId;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapInventory(reader) : null;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@StoreId", storeId);
                AddParameter(parameters, "@BookId", bookId);
            });
        }

        public List<StoreBookInventoryViewModel> Search(string keyword, int? storeId = null)
        {
            var items = new List<StoreBookInventoryViewModel>();
            string sql = @"
                SELECT *
                FROM vw_StoreBookInventory
                WHERE (
                    StoreCode LIKE N'%' + @Keyword + N'%'
                    OR StoreName LIKE N'%' + @Keyword + N'%'
                    OR BookCode LIKE N'%' + @Keyword + N'%'
                    OR ISBN LIKE N'%' + @Keyword + N'%'
                    OR Title LIKE N'%' + @Keyword + N'%'
                    OR CategoryName LIKE N'%' + @Keyword + N'%'
                    OR AuthorName LIKE N'%' + @Keyword + N'%'
                )
            ";

            if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY StoreId, Title;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapInventoryView(reader));
                return items;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@Keyword", keyword);
                if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }

        public int Add(StoreBookInventory inventory)
        {
            const string sql = @"
                INSERT INTO StoreBookInventories (StoreId, BookId, Quantity, MinStock, ImportPrice, SellingPrice, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@StoreId, @BookId, @Quantity, @MinStock, @ImportPrice, @SellingPrice, @IsActive);
            ";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@StoreId", inventory.StoreId);
                AddParameter(parameters, "@BookId", inventory.BookId);
                AddParameter(parameters, "@Quantity", inventory.Quantity);
                AddParameter(parameters, "@MinStock", inventory.MinStock);
                AddParameter(parameters, "@ImportPrice", inventory.ImportPrice);
                AddParameter(parameters, "@SellingPrice", inventory.SellingPrice);
                AddParameter(parameters, "@IsActive", inventory.IsActive);
            });
        }

        public bool Update(StoreBookInventory inventory)
        {
            const string sql = @"
                UPDATE StoreBookInventories
                SET Quantity = @Quantity,
                    MinStock = @MinStock,
                    ImportPrice = @ImportPrice,
                    SellingPrice = @SellingPrice,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", inventory.Id);
                AddParameter(parameters, "@Quantity", inventory.Quantity);
                AddParameter(parameters, "@MinStock", inventory.MinStock);
                AddParameter(parameters, "@ImportPrice", inventory.ImportPrice);
                AddParameter(parameters, "@SellingPrice", inventory.SellingPrice);
                AddParameter(parameters, "@IsActive", inventory.IsActive);
            }) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE StoreBookInventories
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", id);
                AddParameter(parameters, "@IsActive", isActive);
            }) > 0;
        }
    }
}
