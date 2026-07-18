using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class PublisherRepository : RepositoryBase
    {
        private Publisher MapPublisher(SqlDataReader reader)
        {
            return new Publisher
            {
                Id = GetInt(reader, "Id"),
                PublisherName = GetString(reader, "PublisherName"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Publisher> GetAll()
        {
            var items = new List<Publisher>();
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapPublisher(reader));
                return items;
            }, sql);
        }

        public List<Publisher> GetActive()
        {
            var items = new List<Publisher>();
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                WHERE IsActive = 1
                ORDER BY PublisherName;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapPublisher(reader));
                return items;
            }, sql);
        }

        public Publisher? GetById(int id)
        {
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapPublisher(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public List<Publisher> Search(string keyword)
        {
            var items = new List<Publisher>();
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                WHERE PublisherName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%' 
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapPublisher(reader));
                return items;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public int Add(Publisher item)
        {
            const string sql = @"
                INSERT INTO Publishers (PublisherName, Phone, Email, Address, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@PublisherName, @Phone, @Email, @Address, @IsActive);
            ";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@PublisherName", item.PublisherName);
                AddParameter(parameters, "@Phone", item.Phone);
                AddParameter(parameters, "@Email", item.Email);
                AddParameter(parameters, "@Address", item.Address);
                AddParameter(parameters, "@IsActive", item.IsActive);
            });
        }

        public bool Update(Publisher item)
        {
            const string sql = @"
                UPDATE Publishers
                SET PublisherName = @PublisherName,
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
                AddParameter(parameters, "@PublisherName", item.PublisherName);
                AddParameter(parameters, "@Phone", item.Phone);
                AddParameter(parameters, "@Email", item.Email);
                AddParameter(parameters, "@Address", item.Address);
                AddParameter(parameters, "@IsActive", item.IsActive);
            }) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Publishers
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

        public bool IsNameExists(string name, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Publishers
                WHERE PublisherName = @Name
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@Name", name);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<List<Publisher>> GetAllAsync()
        {
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Publisher>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<List<Publisher>> GetActiveAsync()
        {
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                WHERE IsActive = 1
                ORDER BY PublisherName;
            ";
            var result = await QueryAsync<Publisher>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<Publisher?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<Publisher>(sql, new { Id = id });
        }

        public async System.Threading.Tasks.Task<List<Publisher>> SearchAsync(string keyword)
        {
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                WHERE PublisherName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%' 
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Publisher>(sql, new { Keyword = keyword });
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<int> AddAsync(Publisher item)
        {
            const string sql = @"
                INSERT INTO Publishers (PublisherName, Phone, Email, Address, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@PublisherName, @Phone, @Email, @Address, @IsActive);
            ";
            return await ExecuteScalarAsync<int>(sql, item);
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(Publisher item)
        {
            const string sql = @"
                UPDATE Publishers
                SET PublisherName = @PublisherName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, item) > 0;
        }

        public async System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Publishers
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, new { Id = id, IsActive = isActive }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Publishers
                WHERE PublisherName = @Name
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return await ExecuteScalarAsync<int>(sql, new { Name = name, ExcludeId = excludeId }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                const string sql = "UPDATE Publishers SET IsActive = 0, UpdatedAt = SYSDATETIME() WHERE Id = @Id;";
                int rowsAffected = ExecuteNonQuery(sql, parameters => AddParameter(parameters, "@Id", id));
                return rowsAffected > 0;
            });
        }

        public async System.Threading.Tasks.Task<(IEnumerable<Publisher> Items, int TotalCount)> GetPagedPublishersAsync(string keyword, int pageNumber, int pageSize)
        {
            string whereClause = "";
            var param = new Dapper.DynamicParameters();
            
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                whereClause = "WHERE PublisherName LIKE N'%' + @Keyword + '%' OR Phone LIKE '%' + @Keyword + '%' OR Address LIKE N'%' + @Keyword + '%' OR Email LIKE '%' + @Keyword + '%'";
                param.Add("Keyword", keyword);
            }

            string countSql = $@"
                SELECT COUNT(1) 
                FROM Publishers
                {whereClause}
            ";

            string querySql = $@"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                {whereClause}
                ORDER BY Id DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            param.Add("Offset", (pageNumber - 1) * pageSize);
            param.Add("PageSize", pageSize);

            using var connection = BookStoreManagement.Database.DbConnectionFactory.CreateConnection();
            int totalCount = await Dapper.SqlMapper.ExecuteScalarAsync<int>(connection, countSql, param);
            var items = await Dapper.SqlMapper.QueryAsync<Publisher>(connection, querySql, param);

            return (items, totalCount);
        }
    }
}
