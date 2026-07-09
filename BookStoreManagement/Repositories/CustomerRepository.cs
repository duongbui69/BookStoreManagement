using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class CustomerRepository : RepositoryBase
    {
        private Customer MapCustomer(SqlDataReader reader)
        {
            return new Customer
            {
                Id = GetInt(reader, "Id"),
                FullName = GetString(reader, "FullName"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Customer> GetAll()
        {
            var customers = new List<Customer>();
            const string sql = @"
                SELECT Id, FullName, Phone, Email, Address, CreatedAt, UpdatedAt
                FROM Customers
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) customers.Add(MapCustomer(reader));
                return customers;
            }, sql);
        }

        public Customer? GetById(int id)
        {
            const string sql = @"
                SELECT Id, FullName, Phone, Email, Address, CreatedAt, UpdatedAt
                FROM Customers
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapCustomer(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public Customer? GetRetailCustomer()
        {
            const string sql = @"
                SELECT TOP 1 Id, FullName, Phone, Email, Address, CreatedAt, UpdatedAt
                FROM Customers
                WHERE FullName = N'Khach le' OR FullName = N'Khách lẻ'
                ORDER BY Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapCustomer(reader) : null;
            }, sql);
        }

        public List<Customer> Search(string keyword)
        {
            var customers = new List<Customer>();
            const string sql = @"
                SELECT Id, FullName, Phone, Email, Address, CreatedAt, UpdatedAt
                FROM Customers
                WHERE FullName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) customers.Add(MapCustomer(reader));
                return customers;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public int Add(Customer customer)
        {
            const string sql = @"
                INSERT INTO Customers (FullName, Phone, Email, Address)
                OUTPUT INSERTED.Id
                VALUES (@FullName, @Phone, @Email, @Address);
            ";
            return ExecuteQuery(command => Convert.ToInt32(command.ExecuteScalar()), sql, parameters =>
            {
                AddParameter(parameters, "@FullName", customer.FullName);
                AddParameter(parameters, "@Phone", customer.Phone);
                AddParameter(parameters, "@Email", customer.Email);
                AddParameter(parameters, "@Address", customer.Address);
            });
        }

        public bool Update(Customer customer)
        {
            const string sql = @"
                UPDATE Customers
                SET FullName = @FullName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", customer.Id);
                AddParameter(parameters, "@FullName", customer.FullName);
                AddParameter(parameters, "@Phone", customer.Phone);
                AddParameter(parameters, "@Email", customer.Email);
                AddParameter(parameters, "@Address", customer.Address);
            }) > 0;
        }

        public bool Delete(int id)
        {
            const string sql = @"
                DELETE FROM Customers
                WHERE Id = @Id
                  AND Id NOT IN (SELECT CustomerId FROM SalesOrders WHERE CustomerId IS NOT NULL);
            ";
            return ExecuteNonQuery(sql, parameters => AddParameter(parameters, "@Id", id)) > 0;
        }
    }
}
