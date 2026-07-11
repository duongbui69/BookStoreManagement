using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class AuthorRepository
    {
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            addParameters?.Invoke(command.Parameters);
            connection.Open();
            return action(command);
        }

        public List<Author> GetAll()
        {
            const string sql = "SELECT * FROM Authors WHERE IsActive = 1";
            return ExecuteQuery(command =>
            {
                var list = new List<Author>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapAuthor(reader));
                }
                return list;
            }, sql);
        }

        public Author? GetById(int id)
        {
            const string sql = "SELECT * FROM Authors WHERE Id = @Id AND IsActive = 1";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                if (reader.Read()) return MapAuthor(reader);
                return null;
            }, sql, parameters => parameters.AddWithValue("@Id", id));
        }

        public void Add(Author author)
        {
            const string sql = @"INSERT INTO Authors (AuthorName, Description, IsActive, CreatedAt) 
                                 VALUES (@AuthorName, @Description, 1, GETDATE())";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters =>
            {
                parameters.AddWithValue("@AuthorName", author.AuthorName);
                parameters.AddWithValue("@Description", author.Description ?? (object)DBNull.Value);
            });
        }

        public void Update(Author author)
        {
            const string sql = @"UPDATE Authors SET AuthorName = @AuthorName, Description = @Description, 
                                 UpdatedAt = GETDATE() WHERE Id = @Id";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters =>
            {
                parameters.AddWithValue("@Id", author.Id);
                parameters.AddWithValue("@AuthorName", author.AuthorName);
                parameters.AddWithValue("@Description", author.Description ?? (object)DBNull.Value);
            });
        }

        public void Delete(int id)
        {
            const string sql = "UPDATE Authors SET IsActive = 0, UpdatedAt = GETDATE() WHERE Id = @Id";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters => parameters.AddWithValue("@Id", id));
        }

        private Author MapAuthor(SqlDataReader reader)
        {
            return new Author
            {
                Id = DataReaderHelper.GetInt(reader, "Id"),
                AuthorName = DataReaderHelper.GetString(reader, "AuthorName"),
                Description = DataReaderHelper.GetNullableString(reader, "Description"),
                IsActive = DataReaderHelper.GetBool(reader, "IsActive"),
                CreatedAt = DataReaderHelper.GetDateTime(reader, "CreatedAt"),
                UpdatedAt = DataReaderHelper.GetNullableDateTime(reader, "UpdatedAt")
            };
        }
    }
}
