using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class CategoryItem
    {
        public int Id { get; set; }
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int BookCount { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CategoryRepository : RepositoryBase
    {
        private Category MapCategory(SqlDataReader reader)
        {
            return new Category
            {
                Id = GetInt(reader, "Id"),
                CategoryName = GetString(reader, "CategoryName"),
                Description = GetNullableString(reader, "Description"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public (List<CategoryItem> Items, int TotalCount) GetPagedCategories(int page, int pageSize, string searchTerm = "", string statusFilter = "")
        {
            string whereClause = "WHERE (c.CategoryName LIKE @Search OR c.Description LIKE @Search)";

            if (statusFilter == "locked") whereClause += " AND c.IsActive = 0";
            else if (statusFilter == "active") whereClause += " AND c.IsActive = 1";

            int totalCount = ExecuteScalarInt($"SELECT COUNT(*) FROM Categories c {whereClause}",
                p => AddParameter(p, "@Search", "%" + searchTerm + "%"));

            string query = $@"
                SELECT 
                    c.Id, 
                    'DM' + RIGHT('000' + CAST(c.Id AS VARCHAR(10)), 3) AS CategoryCode,
                    c.CategoryName, 
                    (SELECT COUNT(*) FROM Books b WHERE b.CategoryId = c.Id AND b.IsActive = 1) AS BookCount,
                    c.Description, 
                    c.IsActive
                FROM Categories c
                {whereClause}
                ORDER BY c.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var list = ExecuteQuery(cmd =>
            {
                var results = new List<CategoryItem>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bool isActive = reader.GetBoolean(5);
                    string status = isActive ? "Active" : "Locked";

                    results.Add(new CategoryItem
                    {
                        Id = reader.GetInt32(0),
                        CategoryCode = reader.GetString(1),
                        CategoryName = reader.GetString(2),
                        BookCount = reader.GetInt32(3),
                        Description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        Status = status,
                        IsActive = isActive
                    });
                }
                return results;
            }, query, p =>
            {
                AddParameter(p, "@Search", "%" + searchTerm + "%");
                AddParameter(p, "@Offset", (page - 1) * pageSize);
                AddParameter(p, "@PageSize", pageSize);
            });

            return (list, totalCount);
        }

        public async System.Threading.Tasks.Task<(List<CategoryItem> Items, int TotalCount)> GetPagedCategoriesAsync(int page, int pageSize, string searchTerm = "", string statusFilter = "")
        {
            string whereClause = "WHERE (c.CategoryName LIKE @Search OR c.Description LIKE @Search)";

            if (statusFilter == "locked") whereClause += " AND c.IsActive = 0";
            else if (statusFilter == "active") whereClause += " AND c.IsActive = 1";

            string countQuery = $"SELECT COUNT(*) FROM Categories c {whereClause}";
            int totalCount = await ExecuteScalarAsync<int>(countQuery, new { Search = "%" + searchTerm + "%" });

            string query = $@"
                SELECT 
                    c.Id, 
                    'DM' + RIGHT('000' + CAST(c.Id AS VARCHAR(10)), 3) AS CategoryCode,
                    c.CategoryName, 
                    (SELECT COUNT(*) FROM Books b WHERE b.CategoryId = c.Id AND b.IsActive = 1) AS BookCount,
                    c.Description, 
                    CASE WHEN c.IsActive = 1 THEN N'Hoạt động' ELSE N'Khóa' END AS Status,
                    c.IsActive
                FROM Categories c
                {whereClause}
                ORDER BY c.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var items = await QueryAsync<CategoryItem>(query, new { 
                Search = "%" + searchTerm + "%", 
                Offset = (page - 1) * pageSize, 
                PageSize = pageSize 
            });

            return (System.Linq.Enumerable.ToList(items), totalCount);
        }

        public List<Category> GetAll()
        {
            var categories = new List<Category>();
            const string sql = @"
                SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Categories
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) categories.Add(MapCategory(reader));
                return categories;
            }, sql);
        }

        public async System.Threading.Tasks.Task<List<Category>> GetAllAsync()
        {
            const string sql = @"
                SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Categories
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Category>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public List<Category> GetActive()
        {
            var categories = new List<Category>();
            const string sql = @"
                SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Categories
                WHERE IsActive = 1
                ORDER BY CategoryName;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) categories.Add(MapCategory(reader));
                return categories;
            }, sql);
        }

        public async System.Threading.Tasks.Task<List<Category>> GetActiveAsync()
        {
            const string sql = @"
                SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Categories
                WHERE IsActive = 1
                ORDER BY CategoryName;
            ";
            var result = await QueryAsync<Category>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public Category? GetById(int id)
        {
            const string sql = @"
                SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Categories
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapCategory(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public async System.Threading.Tasks.Task<Category?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Categories
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<Category>(sql, new { Id = id });
        }

        public List<Category> Search(string keyword)
        {
            var categories = new List<Category>();
            const string sql = @"
                SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Categories
                WHERE CategoryName LIKE N'%' + @Keyword + N'%'
                   OR Description LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) categories.Add(MapCategory(reader));
                return categories;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public async System.Threading.Tasks.Task<List<Category>> SearchAsync(string keyword)
        {
            const string sql = @"
                SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Categories
                WHERE CategoryName LIKE N'%' + @Keyword + N'%'
                   OR Description LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Category>(sql, new { Keyword = keyword });
            return System.Linq.Enumerable.ToList(result);
        }

        public int Add(Category category)
        {
            const string sql = @"
                INSERT INTO Categories (CategoryName, Description, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@CategoryName, @Description, @IsActive);
            ";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@CategoryName", category.CategoryName);
                AddParameter(parameters, "@Description", category.Description);
                AddParameter(parameters, "@IsActive", category.IsActive);
            });
        }

        public async System.Threading.Tasks.Task<int> AddAsync(Category category)
        {
            const string sql = @"
                INSERT INTO Categories (CategoryName, Description, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@CategoryName, @Description, @IsActive);
            ";
            return await ExecuteScalarAsync<int>(sql, category);
        }

        public bool Update(Category category)
        {
            const string sql = @"
                UPDATE Categories
                SET CategoryName = @CategoryName,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", category.Id);
                AddParameter(parameters, "@CategoryName", category.CategoryName);
                AddParameter(parameters, "@Description", category.Description);
                AddParameter(parameters, "@IsActive", category.IsActive);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(Category category)
        {
            const string sql = @"
                UPDATE Categories
                SET CategoryName = @CategoryName,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, category) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Categories
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
                UPDATE Categories
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, new { Id = id, IsActive = isActive }) > 0;
        }

        public bool IsNameExists(string categoryName, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Categories
                WHERE CategoryName = @CategoryName
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@CategoryName", categoryName);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> IsNameExistsAsync(string categoryName, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Categories
                WHERE CategoryName = @CategoryName
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return await ExecuteScalarAsync<int>(sql, new { CategoryName = categoryName, ExcludeId = excludeId }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
        {
            return await System.Threading.Tasks.Task.Run(() =>
            {
                const string sql = "UPDATE Categories SET IsActive = 0, UpdatedAt = SYSDATETIME() WHERE Id = @Id;";
                int rowsAffected = ExecuteNonQuery(sql, parameters => AddParameter(parameters, "@Id", id));
                return rowsAffected > 0;
            });
        }
    }
}
