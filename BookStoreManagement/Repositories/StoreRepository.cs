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
                ManagerName = GetNullableString(reader, "ManagerName"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Store> GetAll()
        {
            var stores = new List<Store>();
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, ManagerName, IsActive, CreatedAt, UpdatedAt
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

        public async System.Threading.Tasks.Task<List<Store>> GetAllAsync()
        {
            const string sql = "SELECT * FROM Stores ORDER BY Id DESC;";
            var result = await QueryAsync<Store>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public List<Store> GetActive()
        {
            var stores = new List<Store>();
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, ManagerName, IsActive, CreatedAt, UpdatedAt
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

        public async System.Threading.Tasks.Task<List<Store>> GetActiveAsync()
        {
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, ManagerName, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                WHERE IsActive = 1
                ORDER BY StoreName;
            ";
            var result = await QueryAsync<Store>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public Store? GetById(int id)
        {
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, ManagerName, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                WHERE Id = @Id;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapStore(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public async System.Threading.Tasks.Task<Store?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, ManagerName, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<Store>(sql, new { Id = id });
        }

        public List<Store> Search(string keyword)
        {
            var stores = new List<Store>();
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, ManagerName, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                WHERE StoreCode LIKE N'%' + @Keyword + N'%'
                   OR StoreName LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR ManagerName LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) stores.Add(MapStore(reader));
                return stores;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public async System.Threading.Tasks.Task<List<Store>> SearchAsync(string keyword)
        {
            const string sql = @"
                SELECT Id, StoreCode, StoreName, Address, Phone, ManagerName, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                WHERE StoreCode LIKE N'%' + @Keyword + N'%'
                   OR StoreName LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR ManagerName LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Store>(sql, new { Keyword = keyword });
            return System.Linq.Enumerable.ToList(result);
        }

        public int Add(Store store)
        {
            const string sql = @"
                INSERT INTO Stores (StoreCode, StoreName, Address, Phone, ManagerName, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@StoreCode, @StoreName, @Address, @Phone, @ManagerName, @IsActive);
            ";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@StoreCode", store.StoreCode);
                AddParameter(parameters, "@StoreName", store.StoreName);
                AddParameter(parameters, "@Address", store.Address);
                AddParameter(parameters, "@Phone", store.Phone ?? (object)DBNull.Value);
                AddParameter(parameters, "@ManagerName", store.ManagerName ?? (object)DBNull.Value);
                AddParameter(parameters, "@IsActive", store.IsActive);
            });
        }

        public async System.Threading.Tasks.Task<int> AddAsync(Store store)
        {
            const string sql = @"
                INSERT INTO Stores (StoreCode, StoreName, Address, Phone, ManagerName, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@StoreCode, @StoreName, @Address, @Phone, @ManagerName, @IsActive);
            ";
            return await ExecuteScalarAsync<int>(sql, store);
        }

        public bool Update(Store store)
        {
            const string sql = @"
                UPDATE Stores
                SET StoreCode = @StoreCode,
                    StoreName = @StoreName,
                    Address = @Address,
                    Phone = @Phone,
                    ManagerName = @ManagerName,
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
                AddParameter(parameters, "@Phone", store.Phone ?? (object)DBNull.Value);
                AddParameter(parameters, "@ManagerName", store.ManagerName ?? (object)DBNull.Value);
                AddParameter(parameters, "@IsActive", store.IsActive);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(Store store)
        {
            const string sql = @"
                UPDATE Stores
                SET StoreCode = @StoreCode,
                    StoreName = @StoreName,
                    Address = @Address,
                    Phone = @Phone,
                    ManagerName = @ManagerName,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, store) > 0;
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

        public async System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Stores
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, new { Id = id, IsActive = isActive }) > 0;
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

        public async System.Threading.Tasks.Task<bool> IsStoreCodeExistsAsync(string storeCode, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Stores
                WHERE StoreCode = @StoreCode
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";
            return await ExecuteScalarAsync<int>(sql, new { StoreCode = storeCode, ExcludeId = excludeId }) > 0;
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

        public async System.Threading.Tasks.Task<bool> IsStoreNameExistsAsync(string storeName, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Stores
                WHERE StoreName = @StoreName
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";
            return await ExecuteScalarAsync<int>(sql, new { StoreName = storeName, ExcludeId = excludeId }) > 0;
        }

        public (List<Store> Items, int TotalCount) GetPagedStores(int page, int pageSize, string searchTerm = "")
        {
            var stores = new List<Store>();
            int totalCount = 0;
            
            string whereClause = "";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause = @"
                    WHERE StoreCode LIKE N'%' + @SearchTerm + N'%'
                       OR StoreName LIKE N'%' + @SearchTerm + N'%'
                       OR ManagerName LIKE N'%' + @SearchTerm + N'%'
                ";
            }

            string countSql = $"SELECT COUNT(1) FROM Stores {whereClause}";
            
            totalCount = ExecuteScalarInt(countSql, parameters =>
            {
                if (!string.IsNullOrEmpty(searchTerm)) AddParameter(parameters, "@SearchTerm", searchTerm);
            });

            string sql = $@"
                SELECT Id, StoreCode, StoreName, Address, Phone, ManagerName, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                {whereClause}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            ";

            ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) stores.Add(MapStore(reader));
                return stores;
            }, sql, parameters =>
            {
                if (!string.IsNullOrEmpty(searchTerm)) AddParameter(parameters, "@SearchTerm", searchTerm);
                AddParameter(parameters, "@Offset", (page - 1) * pageSize);
                AddParameter(parameters, "@PageSize", pageSize);
            });

            return (stores, totalCount);
        }

        public async System.Threading.Tasks.Task<(List<Store> Items, int TotalCount)> GetPagedStoresAsync(int page, int pageSize, string searchTerm = "")
        {
            string whereClause = "";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause = @"
                    WHERE StoreCode LIKE N'%' + @SearchTerm + N'%'
                       OR StoreName LIKE N'%' + @SearchTerm + N'%'
                       OR ManagerName LIKE N'%' + @SearchTerm + N'%'
                ";
            }

            string countSql = $"SELECT COUNT(1) FROM Stores {whereClause}";
            int totalCount = await ExecuteScalarAsync<int>(countSql, new { SearchTerm = searchTerm });

            string sql = $@"
                SELECT Id, StoreCode, StoreName, Address, Phone, ManagerName, IsActive, CreatedAt, UpdatedAt
                FROM Stores
                {whereClause}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            ";

            var items = await QueryAsync<Store>(sql, new { 
                SearchTerm = searchTerm, 
                Offset = (page - 1) * pageSize, 
                PageSize = pageSize 
            });

            return (System.Linq.Enumerable.ToList(items), totalCount);
        }
    }
}
