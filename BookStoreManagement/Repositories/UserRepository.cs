using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class UserRepository
    {
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);

            addParameters?.Invoke(command.Parameters);

            connection.Open();
            return action(command);
        }

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
    }
}