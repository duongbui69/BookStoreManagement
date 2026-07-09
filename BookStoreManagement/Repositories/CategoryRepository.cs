using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class CategoryRepository
    {
        // Map a SqlDataReader to a Category object
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

        // Execute a SQL query and return the result
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);

            addParameters?.Invoke(command.Parameters);

            connection.Open();
            return action(command);
        }

        // Get all categories from the database
        public List<Category> GetAll()
        {
            var categories = new List<Category>();
            const string sql = @"SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt FROM Categories ORDER BY Id";
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

        // Get all active categories from the database
        public List<Category> GetActive()
        {
            var categories = new List<Category>();
            const string sql = @"SELECT Id, CategoryName, Description, IsActive, CreatedAt, UpdatedAt FROM Categories WHERE IsActive = 1 ORDER BY Id";
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

        // Get a category by its ID
        public int Add(Category categories)
        {
            const string sql = @"
                INSERT INTO Categories (CategoryName, Description, IsActive, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@CategoryName, @Description, @IsActive, SYSDATETIME());
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

        // Update an existing category in the database
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

        // Set the active status of a category by its ID
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