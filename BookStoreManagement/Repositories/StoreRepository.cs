using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class StoreRepository : RepositoryBase
    {
        private Store MapStore(SqlDataReader reader)
        {
            return new Store
            {
                Id = GetInt(reader, "Id"),
                StoreCode = GetString(reader, "StoreCode"),
                StoreName = GetString(reader, "StoreName"),
                Address = GetString(reader, "Address"),
                Phone = GetNullableString(reader, "Phone"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Store> GetAll()
        {
            var stores = new List<Store>();
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                ORDER BY Id DESC;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) stores.Add(MapStore(reader));
                return stores;
            }, sql);
        }

        public List<Store> GetActive()
        {
            var stores = new List<Store>();
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                WHERE IsActive = 1
                ORDER BY StoreName;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) stores.Add(MapStore(reader));
                return stores;
            }, sql);
        }

        public Store? GetById(int id)
        {
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                WHERE Id = @Id;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapStore(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public List<Store> Search(string keyword)
        {
            var stores = new List<Store>();
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                WHERE StoreCode LIKE N'%' + @Keyword + N'%'
                   OR StoreName LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) stores.Add(MapStore(reader));
                return stores;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public int Add(Store store)
        {
            const string sql = @"
                INSERT INTO Stores (StoreCode, StoreName, Address, Phone, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@StoreCode, @StoreName, @Address, @Phone, @IsActive);
            ";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@StoreCode", store.StoreCode);
                AddParameter(parameters, "@StoreName", store.StoreName);
                AddParameter(parameters, "@Address", store.Address);
                AddParameter(parameters, "@Phone", store.Phone);
                AddParameter(parameters, "@IsActive", store.IsActive);
            });
        }

        public bool Update(Store store)
        {
            const string sql = @"
                UPDATE Stores
                SET StoreCode = @StoreCode,
                    StoreName = @StoreName,
                    Address = @Address,
                    Phone = @Phone,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";

            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", store.Id);
                AddParameter(parameters, "@StoreCode", store.StoreCode);
                AddParameter(parameters, "@StoreName", store.StoreName);
                AddParameter(parameters, "@Address", store.Address);
                AddParameter(parameters, "@Phone", store.Phone);
                AddParameter(parameters, "@IsActive", store.IsActive);
            }) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Stores
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

        public bool IsStoreCodeExists(string storeCode, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Stores
                WHERE StoreCode = @StoreCode
            ";

            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@StoreCode", storeCode);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public bool IsStoreNameExists(string storeName, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Stores
                WHERE StoreName = @StoreName
            ";

            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@StoreName", storeName);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }
    }
}
