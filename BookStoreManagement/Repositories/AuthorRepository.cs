using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class AuthorRepository : RepositoryBase
    {
        private Author MapAuthor(SqlDataReader reader)
        {
            return new Author
            {
                Id = GetInt(reader, "Id"),
                AuthorName = GetString(reader, "AuthorName"),
                Description = GetNullableString(reader, "Description"),
                Nationality = GetNullableString(reader, "Nationality"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Author> GetAll()
        {
            var items = new List<Author>();
            const string sql = @"
                SELECT Id, AuthorName, Description, Nationality, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapAuthor(reader));
                return items;
            }, sql);
        }

        public List<Author> GetActive()
        {
            var items = new List<Author>();
            const string sql = @"
                SELECT Id, AuthorName, Description, Nationality, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                WHERE IsActive = 1
                ORDER BY AuthorName;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapAuthor(reader));
                return items;
            }, sql);
        }

        public Author? GetById(int id)
        {
            const string sql = @"
                SELECT Id, AuthorName, Description, Nationality, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapAuthor(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public List<Author> Search(string keyword)
        {
            var items = new List<Author>();
            const string sql = @"
                SELECT Id, AuthorName, Description, Nationality, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                WHERE AuthorName LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) items.Add(MapAuthor(reader));
                return items;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public int Add(Author item)
        {
            const string sql = @"
                INSERT INTO Authors (AuthorName, Description, Nationality, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@AuthorName, @Description, @Nationality, @IsActive);
            ";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@AuthorName", item.AuthorName);
                AddParameter(parameters, "@Description", item.Description);
                AddParameter(parameters, "@Nationality", item.Nationality);
                AddParameter(parameters, "@IsActive", item.IsActive);
            });
        }

        public bool Update(Author item)
        {
            const string sql = @"
                UPDATE Authors
                SET AuthorName = @AuthorName,
                    Description = @Description,
                    Nationality = @Nationality,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", item.Id);
                AddParameter(parameters, "@AuthorName", item.AuthorName);
                AddParameter(parameters, "@Description", item.Description);
                AddParameter(parameters, "@Nationality", item.Nationality);
                AddParameter(parameters, "@IsActive", item.IsActive);
            }) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Authors
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
                FROM Authors
                WHERE AuthorName = @Name
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@Name", name);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }
        public async System.Threading.Tasks.Task<List<Author>> GetAllAsync()
        {
            const string sql = @"
                SELECT Id, AuthorName, Description, Nationality, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Author>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<List<Author>> GetActiveAsync()
        {
            const string sql = @"
                SELECT Id, AuthorName, Description, Nationality, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                WHERE IsActive = 1
                ORDER BY AuthorName;
            ";
            var result = await QueryAsync<Author>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<Author?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, AuthorName, Description, Nationality, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<Author>(sql, new { Id = id });
        }

        public async System.Threading.Tasks.Task<List<Author>> SearchAsync(string keyword)
        {
            const string sql = @"
                SELECT Id, AuthorName, Description, Nationality, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                WHERE AuthorName LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Author>(sql, new { Keyword = keyword });
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<int> AddAsync(Author item)
        {
            const string sql = @"
                INSERT INTO Authors (AuthorName, Description, Nationality, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@AuthorName, @Description, @Nationality, @IsActive);
            ";
            return await ExecuteScalarAsync<int>(sql, item);
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(Author item)
        {
            const string sql = @"
                UPDATE Authors
                SET AuthorName = @AuthorName,
                    Description = @Description,
                    Nationality = @Nationality,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, item) > 0;
        }

        public async System.Threading.Tasks.Task<bool> SetActiveAsync(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Authors
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
                FROM Authors
                WHERE AuthorName = @Name
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return await ExecuteScalarAsync<int>(sql, new { Name = name, ExcludeId = excludeId }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                const string sql = "UPDATE Authors SET IsActive = 0, UpdatedAt = SYSDATETIME() WHERE Id = @Id;";
                int rowsAffected = ExecuteNonQuery(sql, parameters => AddParameter(parameters, "@Id", id));
                return rowsAffected > 0;
            });
        }

        public async System.Threading.Tasks.Task<(IEnumerable<AuthorItem> Items, int TotalCount)> GetPagedAuthorsWithStatsAsync(string keyword, int pageNumber, int pageSize)
        {
            string whereClause = "";
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                whereClause = "WHERE a.AuthorName LIKE N'%' + @Keyword + '%' ";
            }

            string countSql = $@"
                SELECT COUNT(1) 
                FROM Authors a
                {whereClause}
            ";

            string querySql = $@"
                SELECT 
                    a.Id, a.AuthorName, a.Description, a.Nationality, a.IsActive, a.CreatedAt, a.UpdatedAt,
                    COUNT(b.Id) as BookCount
                FROM Authors a
                LEFT JOIN Books b ON a.Id = b.AuthorId
                {whereClause}
                GROUP BY a.Id, a.AuthorName, a.Description, a.Nationality, a.IsActive, a.CreatedAt, a.UpdatedAt
                ORDER BY a.Id DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
            ";

            int totalCount = await ExecuteScalarAsync<int>(countSql, new { Keyword = keyword });
            
            int offset = (pageNumber - 1) * pageSize;
            var items = await QueryAsync<AuthorItem>(querySql, new { Keyword = keyword, Offset = offset, PageSize = pageSize });

            return (items, totalCount);
        }
    }
}
