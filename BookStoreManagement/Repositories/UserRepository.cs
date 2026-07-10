using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class UserRepository : RepositoryBase
    {
        private User MapUser(SqlDataReader reader, bool includePassword = false)
        {
            var user = new User
            {
                Id = GetInt(reader, "Id"),
                UserCode = GetString(reader, "UserCode"),
                StoreId = GetNullableInt(reader, "StoreId"),
                StoreName = GetNullableString(reader, "StoreName"),
                RoleId = GetInt(reader, "RoleId"),
                RoleName = GetString(reader, "RoleName"),
                IdentityNumber = GetNullableString(reader, "IdentityNumber"),
                Username = GetString(reader, "Username"),
                FullName = GetString(reader, "FullName"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                HireDate = GetNullableDateTime(reader, "HireDate"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };

            if (includePassword)
            {
                user.PasswordHash = GetString(reader, "PasswordHash");
            }

            return user;
        }

        private UserListViewModel MapUserList(SqlDataReader reader)
        {
            return new UserListViewModel
            {
                Id = GetInt(reader, "Id"),
                UserCode = GetString(reader, "UserCode"),
                StoreId = GetNullableInt(reader, "StoreId"),
                StoreName = GetNullableString(reader, "StoreName"),
                RoleId = GetInt(reader, "RoleId"),
                RoleName = GetString(reader, "RoleName"),
                IdentityNumber = GetNullableString(reader, "IdentityNumber"),
                Username = GetString(reader, "Username"),
                FullName = GetString(reader, "FullName"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                HireDate = GetNullableDateTime(reader, "HireDate"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<UserListViewModel> GetAll()
        {
            var users = new List<UserListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_UserList
                ORDER BY Id DESC;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) users.Add(MapUserList(reader));
                return users;
            }, sql);
        }

        public List<UserListViewModel> GetByStoreId(int storeId)
        {
            var users = new List<UserListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_UserList
                WHERE StoreId = @StoreId
                ORDER BY Id DESC;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) users.Add(MapUserList(reader));
                return users;
            }, sql, parameters => AddParameter(parameters, "@StoreId", storeId));
        }

        public List<UserListViewModel> Search(string keyword)
        {
            var users = new List<UserListViewModel>();
            const string sql = @"
                SELECT *
                FROM vw_UserList
                WHERE UserCode LIKE N'%' + @Keyword + N'%'
                   OR Username LIKE N'%' + @Keyword + N'%'
                   OR FullName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                   OR IdentityNumber LIKE N'%' + @Keyword + N'%'
                   OR RoleName LIKE N'%' + @Keyword + N'%'
                   OR StoreName LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) users.Add(MapUserList(reader));
                return users;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public User? GetById(int id)
        {
            const string sql = @"
                SELECT
                    u.Id, u.UserCode, u.StoreId, s.StoreName, u.RoleId, r.RoleName,
                    u.IdentityNumber, u.Username, u.PasswordHash, u.FullName, u.Phone,
                    u.Email, u.Address, u.HireDate, u.IsActive, u.CreatedAt, u.UpdatedAt
                FROM Users u
                JOIN Roles r ON u.RoleId = r.Id
                LEFT JOIN Stores s ON u.StoreId = s.Id
                WHERE u.Id = @Id;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUser(reader, includePassword: true) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public User? GetByUsername(string username)
        {
            const string sql = @"
                SELECT
                    u.Id, u.UserCode, u.StoreId, s.StoreName, u.RoleId, r.RoleName,
                    u.IdentityNumber, u.Username, u.PasswordHash, u.FullName, u.Phone,
                    u.Email, u.Address, u.HireDate, u.IsActive, u.CreatedAt, u.UpdatedAt
                FROM Users u
                JOIN Roles r ON u.RoleId = r.Id
                LEFT JOIN Stores s ON u.StoreId = s.Id
                WHERE u.Username = @Username;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUser(reader, includePassword: true) : null;
            }, sql, parameters => AddParameter(parameters, "@Username", username));
        }

        public int Add(User user)
        {
            if (string.IsNullOrWhiteSpace(user.UserCode))
            {
                user.UserCode = GenerateUserCode();
            }

            const string sql = @"
                INSERT INTO Users (
                    UserCode, StoreId, RoleId, IdentityNumber, Username, PasswordHash,
                    FullName, Phone, Email, Address, HireDate, IsActive
                )
                OUTPUT INSERTED.Id
                VALUES (
                    @UserCode, @StoreId, @RoleId, @IdentityNumber, @Username, @PasswordHash,
                    @FullName, @Phone, @Email, @Address, @HireDate, @IsActive
                );
            ";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@UserCode", user.UserCode);
                AddParameter(parameters, "@StoreId", user.StoreId);
                AddParameter(parameters, "@RoleId", user.RoleId);
                AddParameter(parameters, "@IdentityNumber", user.IdentityNumber);
                AddParameter(parameters, "@Username", user.Username);
                AddParameter(parameters, "@PasswordHash", user.PasswordHash);
                AddParameter(parameters, "@FullName", user.FullName);
                AddParameter(parameters, "@Phone", user.Phone);
                AddParameter(parameters, "@Email", user.Email);
                AddParameter(parameters, "@Address", user.Address);
                AddParameter(parameters, "@HireDate", user.HireDate);
                AddParameter(parameters, "@IsActive", user.IsActive);
            });
        }

        public bool Update(User user)
        {
            const string sql = @"
                UPDATE Users
                SET UserCode = @UserCode,
                    StoreId = @StoreId,
                    RoleId = @RoleId,
                    IdentityNumber = @IdentityNumber,
                    FullName = @FullName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    HireDate = @HireDate,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";

            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", user.Id);
                AddParameter(parameters, "@UserCode", user.UserCode);
                AddParameter(parameters, "@StoreId", user.StoreId);
                AddParameter(parameters, "@RoleId", user.RoleId);
                AddParameter(parameters, "@IdentityNumber", user.IdentityNumber);
                AddParameter(parameters, "@FullName", user.FullName);
                AddParameter(parameters, "@Phone", user.Phone);
                AddParameter(parameters, "@Email", user.Email);
                AddParameter(parameters, "@Address", user.Address);
                AddParameter(parameters, "@HireDate", user.HireDate);
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

        public bool SetActive(int userId, bool isActive)
        {
            const string sql = @"
                UPDATE Users
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";

            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", userId);
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

        public bool IsUserCodeExists(string userCode, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Users
                WHERE UserCode = @UserCode
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@UserCode", userCode);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public bool IsIdentityNumberExists(string identityNumber, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Users
                WHERE IdentityNumber = @IdentityNumber
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@IdentityNumber", identityNumber);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public string GenerateUserCode()
        {
            return "EMP" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
