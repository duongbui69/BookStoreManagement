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
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Author> GetAll()
        {
            var items = new List<Author>();
            const string sql = @"
                SELECT Id, AuthorName, Description, IsActive, CreatedAt, UpdatedAt
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
                SELECT Id, AuthorName, Description, IsActive, CreatedAt, UpdatedAt
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
                SELECT Id, AuthorName, Description, IsActive, CreatedAt, UpdatedAt
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
                SELECT Id, AuthorName, Description, IsActive, CreatedAt, UpdatedAt
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
                INSERT INTO Authors (AuthorName, Description, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@AuthorName, @Description, @IsActive);
            ";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@AuthorName", item.AuthorName);
                AddParameter(parameters, "@Description", item.Description);
                AddParameter(parameters, "@IsActive", item.IsActive);
            });
        }

        public bool Update(Author item)
        {
            const string sql = @"
                UPDATE Authors
                SET AuthorName = @AuthorName,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", item.Id);
                AddParameter(parameters, "@AuthorName", item.AuthorName);
                AddParameter(parameters, "@Description", item.Description);
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
    }
}
