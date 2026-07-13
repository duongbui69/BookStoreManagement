using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
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
    }
}
