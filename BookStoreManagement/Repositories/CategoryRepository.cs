using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class CategoryRepository
    {
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            addParameters?.Invoke(command.Parameters);
            connection.Open();
            return action(command);
        }

        public List<Category> GetAll()
        {
            const string sql = "SELECT * FROM Categories WHERE IsActive = 1";
            return ExecuteQuery(command =>
            {
                var list = new List<Category>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapCategory(reader));
                }
                return list;
            }, sql);
        }

        public Category? GetById(int id)
        {
            const string sql = "SELECT * FROM Categories WHERE Id = @Id AND IsActive = 1";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                if (reader.Read()) return MapCategory(reader);
                return null;
            }, sql, parameters => parameters.AddWithValue("@Id", id));
        }

        public void Add(Category category)
        {
            const string sql = @"INSERT INTO Categories (CategoryName, Description, IsActive, CreatedAt) 
                                 VALUES (@CategoryName, @Description, 1, GETDATE())";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters =>
            {
                parameters.AddWithValue("@CategoryName", category.CategoryName);
                parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);
            });
        }

        public void Update(Category category)
        {
            const string sql = @"UPDATE Categories SET CategoryName = @CategoryName, Description = @Description, 
                                 UpdatedAt = GETDATE() WHERE Id = @Id";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters =>
            {
                parameters.AddWithValue("@Id", category.Id);
                parameters.AddWithValue("@CategoryName", category.CategoryName);
                parameters.AddWithValue("@Description", category.Description ?? (object)DBNull.Value);
            });
        }

        public void Delete(int id)
        {
            const string sql = "UPDATE Categories SET IsActive = 0, UpdatedAt = GETDATE() WHERE Id = @Id";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters => parameters.AddWithValue("@Id", id));
        }

        private Category MapCategory(SqlDataReader reader)
        {
            return new Category
            {
                Id = DataReaderHelper.GetInt(reader, "Id"),
                CategoryName = DataReaderHelper.GetString(reader, "CategoryName"),
                Description = DataReaderHelper.GetNullableString(reader, "Description"),
                IsActive = DataReaderHelper.GetBool(reader, "IsActive"),
                CreatedAt = DataReaderHelper.GetDateTime(reader, "CreatedAt"),
                UpdatedAt = DataReaderHelper.GetNullableDateTime(reader, "UpdatedAt")
            };
        }
    }
}
