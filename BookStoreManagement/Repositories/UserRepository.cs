using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class UserRepository : RepositoryBase
    {
        private User MapUser(SqlDataReader reader)
        {
            return new User
            {
                Id = GetInt(reader, "Id"),
                RoleId = GetInt(reader, "RoleId"),
                Username = GetString(reader, "Username"),
                PasswordHash = GetString(reader, "PasswordHash"),
                FullName = GetString(reader, "FullName"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<User> GetAll()
        {
            var users = new List<User>();

            const string sql = @"
                SELECT Id, RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Users
                ORDER BY Id DESC;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) users.Add(MapUser(reader));
                return users;
            }, sql);
        }

        public User? GetById(int id)
        {
            const string sql = @"
                SELECT Id, RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Users
                WHERE Id = @Id;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUser(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public User? GetByUsername(string username)
        {
            const string sql = @"
                SELECT Id, RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Users
                WHERE Username = @Username;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUser(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Username", username));
        }

        public List<User> Search(string keyword)
        {
            var users = new List<User>();

            const string sql = @"
                SELECT Id, RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Users
                WHERE Username LIKE N'%' + @Keyword + N'%'
                   OR FullName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) users.Add(MapUser(reader));
                return users;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public int Add(User user)
        {
            const string sql = @"
                INSERT INTO Users (RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@RoleId, @Username, @PasswordHash, @FullName, @Phone, @Email, @Address, @IsActive);
            ";

            return ExecuteQuery(command => Convert.ToInt32(command.ExecuteScalar()), sql, parameters =>
            {
                AddParameter(parameters, "@RoleId", user.RoleId);
                AddParameter(parameters, "@Username", user.Username);
                AddParameter(parameters, "@PasswordHash", user.PasswordHash);
                AddParameter(parameters, "@FullName", user.FullName);
                AddParameter(parameters, "@Phone", user.Phone);
                AddParameter(parameters, "@Email", user.Email);
                AddParameter(parameters, "@Address", user.Address);
                AddParameter(parameters, "@IsActive", user.IsActive);
            });
        }

        public bool Update(User user)
        {
            const string sql = @"
                UPDATE Users
                SET RoleId = @RoleId,
                    FullName = @FullName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";

            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", user.Id);
                AddParameter(parameters, "@RoleId", user.RoleId);
                AddParameter(parameters, "@FullName", user.FullName);
                AddParameter(parameters, "@Phone", user.Phone);
                AddParameter(parameters, "@Email", user.Email);
                AddParameter(parameters, "@Address", user.Address);
                AddParameter(parameters, "@IsActive", user.IsActive);
            }) > 0;
        }

        public bool ChangePassword(int userId, string newPasswordHash)
        {
            const string sql = @"
                UPDATE Users
                SET PasswordHash = @PasswordHash,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";

            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", userId);
                AddParameter(parameters, "@PasswordHash", newPasswordHash);
            }) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Users
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

        public bool IsUsernameExists(string username, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Users
                WHERE Username = @Username
            ";

            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@Username", username);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public bool IsEmailExists(string email, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Users
                WHERE Email = @Email
            ";

            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@Email", email);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }
    }
}
