using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using Dapper;

namespace BookStoreManagement.Repositories
{
    public class SupplierRepository : RepositoryBase
    {
        private Supplier MapSupplier(SqlDataReader reader)
        {
            return new Supplier
            {
                Id = GetInt(reader, "Id"),
                SupplierName = GetString(reader, "SupplierName"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Supplier> GetAll()
        {
            var items = new List<Supplier>();
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapSupplier(reader));
                return items;
            }, sql);
        }

        public async System.Threading.Tasks.Task<List<Supplier>> GetAllAsync()
        {
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Supplier>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public List<Supplier> GetActive()
        {
            var items = new List<Supplier>();
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                WHERE IsActive = 1
                ORDER BY SupplierName;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapSupplier(reader));
                return items;
            }, sql);
        }

        public async System.Threading.Tasks.Task<List<Supplier>> GetActiveAsync()
        {
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                WHERE IsActive = 1
                ORDER BY SupplierName;
            ";
            var result = await QueryAsync<Supplier>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public Supplier? GetById(int id)
        {
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapSupplier(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public async System.Threading.Tasks.Task<Supplier?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<Supplier>(sql, new { Id = id });
        }

        public List<Supplier> Search(string keyword)
        {
            var items = new List<Supplier>();
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                WHERE SupplierName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%' 
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapSupplier(reader));
                return items;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public async System.Threading.Tasks.Task<List<Supplier>> SearchAsync(string keyword)
        {
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                WHERE SupplierName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%' 
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Supplier>(sql, new { Keyword = keyword });
            return System.Linq.Enumerable.ToList(result);
        }

        public int Add(Supplier item)
        {
            const string sql = @"
                INSERT INTO Suppliers (SupplierName, Phone, Email, Address, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@SupplierName, @Phone, @Email, @Address, @IsActive);
            ";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@SupplierName", item.SupplierName);
                AddParameter(parameters, "@Phone", item.Phone);
                AddParameter(parameters, "@Email", item.Email);
                AddParameter(parameters, "@Address", item.Address);
                AddParameter(parameters, "@IsActive", item.IsActive);
            });
        }

        public async System.Threading.Tasks.Task<int> AddAsync(Supplier item)
        {
            const string sql = @"
                INSERT INTO Suppliers (SupplierName, Phone, Email, Address, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@SupplierName, @Phone, @Email, @Address, @IsActive);
            ";
            return await ExecuteScalarAsync<int>(sql, new { 
                SupplierName = item.SupplierName, 
                Phone = item.Phone, 
                Email = item.Email, 
                Address = item.Address, 
                IsActive = item.IsActive 
            });
        }

        public bool Update(Supplier item)
        {
            const string sql = @"
                UPDATE Suppliers
                SET SupplierName = @SupplierName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", item.Id);
                AddParameter(parameters, "@SupplierName", item.SupplierName);
                AddParameter(parameters, "@Phone", item.Phone);
                AddParameter(parameters, "@Email", item.Email);
                AddParameter(parameters, "@Address", item.Address);
                AddParameter(parameters, "@IsActive", item.IsActive);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(Supplier item)
        {
            const string sql = @"
                UPDATE Suppliers
                SET SupplierName = @SupplierName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            int rows = await ExecuteAsync(sql, new { 
                Id = item.Id, 
                SupplierName = item.SupplierName, 
                Phone = item.Phone, 
                Email = item.Email, 
                Address = item.Address, 
                IsActive = item.IsActive 
            });
            return rows > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Suppliers
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
                UPDATE Suppliers
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            int rows = await ExecuteAsync(sql, new { Id = id, IsActive = isActive });
            return rows > 0;
        }

        public bool IsNameExists(string name, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Suppliers
                WHERE SupplierName = @Name
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@Name", name);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Suppliers
                WHERE SupplierName = @Name
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            var param = new DynamicParameters();
            param.Add("Name", name);
            if (excludeId.HasValue) param.Add("ExcludeId", excludeId.Value);

            int count = await ExecuteScalarAsync<int>(sql, param);
            return count > 0;
        }

        public async System.Threading.Tasks.Task<(IEnumerable<Supplier> Items, int TotalCount)> GetPagedSuppliersAsync(string keyword, int pageNumber, int pageSize)
        {
            string whereClause = "";
            var param = new DynamicParameters();
            
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                whereClause = "WHERE SupplierName LIKE N'%' + @Keyword + '%' OR Phone LIKE '%' + @Keyword + '%' OR Address LIKE N'%' + @Keyword + '%'";
                param.Add("Keyword", keyword);
            }

            string countSql = $@"
                SELECT COUNT(1) 
                FROM Suppliers
                {whereClause}
            ";

            string querySql = $@"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                {whereClause}
                ORDER BY Id DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            param.Add("Offset", (pageNumber - 1) * pageSize);
            param.Add("PageSize", pageSize);

            using var connection = BookStoreManagement.Database.DbConnectionFactory.CreateConnection();
            int totalCount = await connection.ExecuteScalarAsync<int>(countSql, param);
            var items = await connection.QueryAsync<Supplier>(querySql, param);

            return (items, totalCount);
        }
    }
}
