using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class UserRepository : RepositoryBase
    {

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
                AddParameter(parameters, "@Username", username);
                AddParameter(parameters, "@PasswordHash", password);
            });
        }

        public async System.Threading.Tasks.Task<User?> LoginAsync(string username, string password)
        {
            const string sql = @"SELECT u.Id, u.RoleId, u.Username, u.PasswordHash, u.FullName, u.Phone, u.Email, u.Address, u.IsActive, u.CreatedAt, u.UpdatedAt 
                                FROM Users u JOIN Roles r On u.RoleId = r.Id WHERE u.Username = @Username AND u.PasswordHash = @PasswordHash AND u.IsActive = 1";
            return await QueryFirstOrDefaultAsync<User>(sql, new { Username = username, PasswordHash = password });
        }

        public User? GetByUsername(string username)
        {
            const string sql = @"SELECT u.Id, u.RoleId, u.Username, u.PasswordHash, u.FullName, u.Phone, u.Email, u.Address, u.IsActive, u.CreatedAt, u.UpdatedAt 
                                FROM Users u WHERE u.Username = @Username";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUser(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Username", username));
        }

        public async System.Threading.Tasks.Task<User?> GetByUsernameAsync(string username)
        {
            const string sql = @"SELECT u.Id, u.RoleId, u.Username, u.PasswordHash, u.FullName, u.Phone, u.Email, u.Address, u.IsActive, u.CreatedAt, u.UpdatedAt 
                                FROM Users u WHERE u.Username = @Username";
            return await QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public User? GetById(int id)
        {
            const string sql = @"SELECT u.Id, u.RoleId, u.Username, u.PasswordHash, u.FullName, u.Phone, u.Email, u.Address, u.IsActive, u.CreatedAt, u.UpdatedAt 
                                FROM Users u WHERE u.Id = @Id";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUser(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public async System.Threading.Tasks.Task<User?> GetByIdAsync(int id)
        {
            const string sql = @"SELECT u.Id, u.RoleId, u.Username, u.PasswordHash, u.FullName, u.Phone, u.Email, u.Address, u.IsActive, u.CreatedAt, u.UpdatedAt 
                                FROM Users u WHERE u.Id = @Id";
            return await QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public int Add(User user)
        {
            const string sql = @"
                INSERT INTO Users (RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@RoleId, @Username, @PasswordHash, @FullName, @Phone, @Email, @Address, @IsActive, SYSDATETIME())";

            return (int?)ExecuteScalar(sql, parameters =>
            {
                AddParameter(parameters, "@RoleId", user.RoleId);
                AddParameter(parameters, "@Username", user.Username);
                AddParameter(parameters, "@PasswordHash", user.PasswordHash);
                AddParameter(parameters, "@FullName", user.FullName);
                AddParameter(parameters, "@Phone", (object)user.Phone ?? DBNull.Value);
                AddParameter(parameters, "@Email", (object)user.Email ?? DBNull.Value);
                AddParameter(parameters, "@Address", (object)user.Address ?? DBNull.Value);
                AddParameter(parameters, "@IsActive", user.IsActive);
            }) ?? 0;
        }

        public async System.Threading.Tasks.Task<int> AddAsync(User user)
        {
            const string sql = @"
                INSERT INTO Users (RoleId, Username, PasswordHash, FullName, Phone, Email, Address, IsActive, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES (@RoleId, @Username, @PasswordHash, @FullName, @Phone, @Email, @Address, @IsActive, SYSDATETIME())";
            return await ExecuteScalarAsync<int>(sql, new { user.RoleId, user.Username, user.PasswordHash, user.FullName, user.Phone, user.Email, user.Address, user.IsActive });
        }

        public bool Update(User user)
        {
            const string sql = @"
                UPDATE Users 
                SET RoleId = @RoleId, 
                    Username = @Username, FullName = @FullName, Phone = @Phone, Email = @Email, Address = @Address, 
                    IsActive = @IsActive, UpdatedAt = SYSDATETIME()
                WHERE Id = @Id";

            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", user.Id);
                AddParameter(parameters, "@RoleId", user.RoleId);
                AddParameter(parameters, "@Username", user.Username);
                AddParameter(parameters, "@FullName", user.FullName);
                AddParameter(parameters, "@Phone", (object)user.Phone ?? DBNull.Value);
                AddParameter(parameters, "@Email", (object)user.Email ?? DBNull.Value);
                AddParameter(parameters, "@Address", (object)user.Address ?? DBNull.Value);
                AddParameter(parameters, "@IsActive", user.IsActive);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> UpdateAsync(User user)
        {
            const string sql = @"
                UPDATE Users 
                SET RoleId = @RoleId, 
                    Username = @Username, FullName = @FullName, Phone = @Phone, Email = @Email, Address = @Address, 
                    IsActive = @IsActive, UpdatedAt = SYSDATETIME()
                WHERE Id = @Id";
            return await ExecuteAsync(sql, new { user.Id, user.RoleId, user.Username, user.FullName, user.Phone, user.Email, user.Address, user.IsActive }) > 0;
        }

        public bool ChangePassword(int userId, string newPasswordHash)
        {
            const string sql = @"UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = SYSDATETIME() WHERE Id = @Id";

            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", userId);
                AddParameter(parameters, "@PasswordHash", newPasswordHash);
            }) > 0;
        }

        public async System.Threading.Tasks.Task<bool> ChangePasswordAsync(int userId, string newPasswordHash)
        {
            const string sql = @"UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = SYSDATETIME() WHERE Id = @Id";
            return await ExecuteAsync(sql, new { Id = userId, PasswordHash = newPasswordHash }) > 0;
        }

        public bool Delete(int userId)
        {
            const string sql = @"UPDATE Users SET IsActive = 0, UpdatedAt = SYSDATETIME() WHERE Id = @Id";
            return ExecuteNonQuery(sql, parameters => AddParameter(parameters, "@Id", userId)) > 0;
        }

        public async System.Threading.Tasks.Task<bool> DeleteAsync(int userId)
        {
            const string sql = @"UPDATE Users SET IsActive = 0, UpdatedAt = SYSDATETIME() WHERE Id = @Id";
            return await ExecuteAsync(sql, new { Id = userId }) > 0;
        }

        private User MapUser(SqlDataReader reader)
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

        public (List<AccountItem> Items, int TotalCount) GetPagedAccounts(int page, int pageSize, string searchTerm)
        {
            string whereClause = "WHERE 1=1 ";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += "AND (u.Username LIKE @Search OR u.FullName LIKE @Search OR u.Email LIKE @Search OR u.Phone LIKE @Search) ";
            }

            Action<SqlParameterCollection> addParams = p =>
            {
                if (!string.IsNullOrEmpty(searchTerm)) AddParameter(p, "@Search", "%" + searchTerm + "%");
            };

            string countQuery = $@"
                SELECT COUNT(*) 
                FROM Users u 
                {whereClause}";
                
            int totalCount = ExecuteScalarInt(countQuery, addParams);

            string dataQuery = $@"
                SELECT 
                    u.Id, 
                    'NV' + CAST(u.Id AS VARCHAR) as UserCode, 
                    u.Username, 
                    u.FullName, 
                    ISNULL(u.Email, '') as Email, 
                    ISNULL(u.Phone, '') as Phone, 
                    ISNULL(r.RoleName, 'N/A') as RoleName, 
                    'Tất cả chi nhánh' as StoreName, 
                    u.IsActive, 
                    ISNULL(u.UpdatedAt, u.CreatedAt) as LastUpdate
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.Id
                {whereClause}
                ORDER BY u.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var list = ExecuteQuery(cmd =>
            {
                var results = new List<AccountItem>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new AccountItem
                    {
                        Id = reader.GetInt32(0),
                        UserCode = reader.GetString(1),
                        Username = reader.GetString(2),
                        FullName = reader.GetString(3),
                        Email = reader.GetString(4),
                        Phone = reader.GetString(5),
                        RoleName = reader.GetString(6),
                        StoreName = reader.GetString(7),
                        IsActive = reader.GetBoolean(8),
                        LastUpdate = reader.GetDateTime(9)
                    });
                }
                return results;
            }, dataQuery, p =>
            {
                addParams(p);
                AddParameter(p, "@Offset", (page - 1) * pageSize);
                AddParameter(p, "@PageSize", pageSize);
            });

            return (list, totalCount);
        }

        public async System.Threading.Tasks.Task<(List<AccountItem> Items, int TotalCount)> GetPagedAccountsAsync(int page, int pageSize, string searchTerm)
        {
            string whereClause = "WHERE 1=1 ";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += "AND (u.Username LIKE @Search OR u.FullName LIKE @Search OR u.Email LIKE @Search OR u.Phone LIKE @Search) ";
            }

            string countQuery = $@"
                SELECT COUNT(*) 
                FROM Users u 
                {whereClause}";
                
            int totalCount = await ExecuteScalarAsync<int>(countQuery, new { Search = "%" + searchTerm + "%" });

            string dataQuery = $@"
                SELECT 
                    u.Id, 
                    'NV' + CAST(u.Id AS VARCHAR) as UserCode, 
                    u.Username, 
                    u.FullName, 
                    ISNULL(u.Email, '') as Email, 
                    ISNULL(u.Phone, '') as Phone, 
                    ISNULL(r.RoleName, 'N/A') as RoleName, 
                    N'Tất cả chi nhánh' as StoreName, 
                    u.IsActive, 
                    ISNULL(u.UpdatedAt, u.CreatedAt) as LastUpdate
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.Id
                {whereClause}
                ORDER BY u.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var list = await QueryAsync<AccountItem>(dataQuery, new { 
                Search = "%" + searchTerm + "%", 
                Offset = (page - 1) * pageSize, 
                PageSize = pageSize 
            });

            return (System.Linq.Enumerable.ToList(list), totalCount);
        }
    }

    public class AccountItem
    {
        public int Id { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
