using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class AuthorRepository : RepositoryBase
    {
        // Map a SqlDataReader to an Author object
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

        // Get all authors from the database
        public List<Author> GetAll()
        {
            var authors = new List<Author>();
            const string sql = @"
                SELECT Id, AuthorName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) authors.Add(MapAuthor(reader));
                return authors;
            }, sql);
        }

        // Get all active authors from the database
        public List<Author> GetActive()
        {
            var authors = new List<Author>();
            const string sql = @"
                SELECT Id, AuthorName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                WHERE IsActive = 1
                ORDER BY AuthorName;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) authors.Add(MapAuthor(reader));
                return authors;
            }, sql);
        }

        // Get an author by ID from the database
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

        // Search authors by keyword in the database
        public List<Author> Search(string keyword)
        {
            var authors = new List<Author>();
            const string sql = @"
                SELECT Id, AuthorName, Description, IsActive, CreatedAt, UpdatedAt
                FROM Authors
                WHERE AuthorName LIKE N'%' + @Keyword + N'%'
                   OR Description LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) authors.Add(MapAuthor(reader));
                return authors;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        // Add a new author to the database and return the new author's ID
        public int Add(Author author)
        {
            const string sql = @"
                INSERT INTO Authors (AuthorName, Description, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@AuthorName, @Description, @IsActive);
            ";
            return ExecuteQuery(command => Convert.ToInt32(command.ExecuteScalar()), sql, parameters =>
            {
                AddParameter(parameters, "@AuthorName", author.AuthorName);
                AddParameter(parameters, "@Description", author.Description);
                AddParameter(parameters, "@IsActive", author.IsActive);
            });
        }

        // Update an existing author in the database
        public bool Update(Author author)
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
                AddParameter(parameters, "@Id", author.Id);
                AddParameter(parameters, "@AuthorName", author.AuthorName);
                AddParameter(parameters, "@Description", author.Description);
                AddParameter(parameters, "@IsActive", author.IsActive);
            }) > 0;
        }

        // Set the active status of an author in the database
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

        // Check if an author name already exists in the database, optionally excluding a specific author ID
        public bool IsNameExists(string authorName, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Authors
                WHERE AuthorName = @AuthorName
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@AuthorName", authorName);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }
    }
}
