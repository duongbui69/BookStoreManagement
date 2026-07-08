using System;
using System.Collections.Generic;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class CategoryRepository
    {
        // Execute a SQL query and return the result
        private Category MapCategory(SqlDataReader reader)
        {
            return new Category
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        // Execute a SQL query and return the result
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
            var categories = new List<Category>();
            const string sql = @"SELECT Id, Name, Description, CreatedAt, UpdatedAt FROM Categories ORDER BY Id";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(MapCategory(reader));
                }
                return categories;
            }, sql);
        }

        public List<Category> GetActive()
        {
            var categories = new List<Category>();
            const string sql = @"SELECT Id, Name, Description, CreatedAt, UpdatedAt FROM Categories WHERE IsActive = 1 ORDER BY Id";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(MapCategory(reader));
                }
                return categories;
            }, sql);
        }

        public int Add(Category categories)
        {
            const string sql = @"
                INSERT INTO Categories (CategoryName, Description, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@CategoryName, @Description, @IsActive);
             ";
            return ExecuteQuery(command =>
            {
                return (int)command.ExecuteScalar();
            }, sql, parameters =>
            {
                parameters.AddWithValue("@CategoryName", categories.CategoryName);
                parameters.AddWithValue("@Description", (object?)categories.Description ?? DBNull.Value);
                parameters.AddWithValue("@IsActive", categories.IsActive);
            });
        }

        public bool Update(Category categories) 
        {
            const string sql = @"
                UPDATE Categories
                SET CategoryName = @CategoryName,
                    Description = @Description,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                return command.ExecuteNonQuery() > 0;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Id", categories.Id);
                parameters.AddWithValue("@CategoryName", categories.CategoryName);
                parameters.AddWithValue("@Description", (object?)categories.Description ?? DBNull.Value);
                parameters.AddWithValue("@IsActive", categories.IsActive);
            });
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Categories
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                return command.ExecuteNonQuery() > 0;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Id", id);
                parameters.AddWithValue("@IsActive", isActive);
            });
        }
    }
}