using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class UserRepository
    {
        // Execute a SQL query and return the result
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);

            addParameters?.Invoke(command.Parameters);

            connection.Open();
            return action(command);
        }

        // Login method to authenticate user
        public User? Login(string username, string password)
        {
            const string sql = @"SELECT u.Id, u.RoleId, u.Username, u.PasswordHash, u.FullName, u.Phone, u.Email, u.Address, u.IsActive, u.CreatedAt, u.UpdatedAt 
                                FROM Users u JOIN Roles r On u.RoleId = r.Id WHERE u.Username = @Username AND u.PasswordHash = @PasswordHash AND u.IsActive = 1";


            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapUser(reader);
                }
                return null;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Username", username);
                parameters.AddWithValue("@PasswordHash", password);
            });
        }

        // Get all users
        public List<User> getAll()
        {
            var users = new List<User>();
            const string sql = @"SELECT Id, RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt FROM Users ORDER BY Id";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(MapUser(reader));
                }
                return users;
            }, sql);
        }

        //Get user by ID
        public User? GetById(int id)
        {
            const string sql = @"SELECT Id, RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt FROM Users WHERE Id = @Id";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapUser(reader);
                }
                return null;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Id", id);
            });
        }

        // Add a new user and return the inserted user's ID
        public int Add(User user) {
            const string sql = @"INSERT INTO Users (RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive) OUTPUT INSERTED.Id VALUES (@RoleId, @Username, @PasswordHash, @Fullname, @Phone, @Email, @Address, @IsActive)";
            return ExecuteQuery(command =>
            {
                return (int)command.ExecuteScalar();
            }, sql, parameters =>
            {
                parameters.AddWithValue("@RoleId", user.RoleId);
                parameters.AddWithValue("@Username", user.Username);
                parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                parameters.AddWithValue("@FullName", user.FullName);
                parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
                parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
                parameters.AddWithValue("@Address", (object?)user.Address ?? DBNull.Value);
                parameters.AddWithValue("@IsActive", user.IsActive);
            });
        }

        // Update an existing user
        public bool Update(User user)
        {
            const string sql = @"UPDATE Users SET RoleId = @RoleId, Username = @Username, PasswordHash = @PasswordHash, FullName = @FullName, Phone = @Phone, Email = @Email, Address = @Address, IsActive = @IsActive, UpdatedAt = GETDATE() WHERE Id = @Id";
            return ExecuteQuery(command =>
            {
                return command.ExecuteNonQuery() > 0;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Id", user.Id);
                parameters.AddWithValue("@RoleId", user.RoleId);
                parameters.AddWithValue("@Username", user.Username);
                parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                parameters.AddWithValue("@FullName", user.FullName);
                parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
                parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
                parameters.AddWithValue("@Address", (object?)user.Address ?? DBNull.Value);
                parameters.AddWithValue("@IsActive", user.IsActive);
            });
        }

        // Change the password of a user
        public bool ChangePassword(int userId, string newPasswordHash)
        {
            const string sql = @"UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = GETDATE() WHERE Id = @Id";
            return ExecuteQuery(command =>
            {
                return command.ExecuteNonQuery() > 0;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Id", userId);
                parameters.AddWithValue("@PasswordHash", newPasswordHash);
            });
        }

        // Set the active status of a user
        public bool SetActive(int UserId, bool isActive)
        {
            const string sql = @"UPDATE Users SET IsActive = @IsActive, UpdatedAt = GETDATE() WHERE Id = @Id";
            return ExecuteQuery(command =>
            {
                return command.ExecuteNonQuery() > 0;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Id", UserId);
                parameters.AddWithValue("@IsActive", isActive);
            });
        }

        // Check if a username already exists in the database
        public bool IsUsernameExists(string username)
        {
            const string sql = @"SELECT COUNT(*) FROM Users WHERE Username = @Username";
            return ExecuteQuery(command =>
            {
                return (int)command.ExecuteScalar() > 0;
            }, sql, parameters =>
            {
                parameters.AddWithValue("@Username", username);
            });
        }

        // Map SqlDataReader to User object
        public User MapUser(SqlDataReader reader)
        {
            return new User
            {
                Id = DataReaderHelper.GetInt(reader, "Id"),
                RoleId = DataReaderHelper.GetInt(reader, "RoleId"),
                Username = DataReaderHelper.GetString(reader, "Username"),
                PasswordHash = DataReaderHelper.GetString(reader, "PasswordHash"),
                FullName = DataReaderHelper.GetString(reader, "FullName"),
                Phone = DataReaderHelper.GetNullableString(reader, "Phone"),
                Email = DataReaderHelper.GetNullableString(reader, "Email"),
                Address = DataReaderHelper.GetNullableString(reader, "Address"),
                IsActive = DataReaderHelper.GetBool(reader, "IsActive"),
                CreatedAt = DataReaderHelper.GetDateTime(reader, "CreatedAt"),
                UpdatedAt = DataReaderHelper.GetNullableDateTime(reader, "UpdatedAt")
            };
        }
    }
}