using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class CustomerRepository
    {
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            addParameters?.Invoke(command.Parameters);
            connection.Open();
            return action(command);
        }

        public List<Customer> GetAll()
        {
            const string sql = "SELECT * FROM Customers ORDER BY FullName";
            return ExecuteQuery(command =>
            {
                var list = new List<Customer>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Customer
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        FullName = reader.GetString(reader.GetOrdinal("FullName")),
                        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                        Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                        Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                    });
                }
                return list;
            }, sql);
        }

        public void Add(Customer customer)
        {
            const string sql = @"
                INSERT INTO Customers (FullName, Phone, Email, Address, CreatedAt)
                VALUES (@FullName, @Phone, @Email, @Address, GETDATE());
            ";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters =>
            {
                parameters.AddWithValue("@FullName", customer.FullName);
                parameters.AddWithValue("@Phone", (object?)customer.Phone ?? DBNull.Value);
                parameters.AddWithValue("@Email", (object?)customer.Email ?? DBNull.Value);
                parameters.AddWithValue("@Address", (object?)customer.Address ?? DBNull.Value);
            });
        }
    }
}
