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
                CustomerCode = GetString(reader, "CustomerCode"),
                IdentityNumber = GetNullableString(reader, "IdentityNumber"),
                FullName = GetString(reader, "FullName"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Customer> GetAll()
        {
            var customers = new List<Customer>();
            const string sql = @"
                SELECT Id, CustomerCode, IdentityNumber, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
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

        public List<Customer> GetActive()
        {
            var customers = new List<Customer>();
            const string sql = @"
                SELECT Id, CustomerCode, IdentityNumber, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Customers
                WHERE IsActive = 1
                ORDER BY FullName;
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
                SELECT Id, CustomerCode, IdentityNumber, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Customers
                WHERE Id = @Id;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapCustomer(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public List<Customer> Search(string keyword)
        {
            var customers = new List<Customer>();
            const string sql = @"
                SELECT Id, CustomerCode, IdentityNumber, FullName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Customers
                WHERE CustomerCode LIKE N'%' + @Keyword + N'%'
                   OR IdentityNumber LIKE N'%' + @Keyword + N'%'
                   OR FullName LIKE N'%' + @Keyword + N'%'
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
            if (string.IsNullOrWhiteSpace(customer.CustomerCode))
            {
                customer.CustomerCode = GenerateCustomerCode();
            }

            const string sql = @"
                INSERT INTO Customers (CustomerCode, IdentityNumber, FullName, Phone, Email, Address, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@CustomerCode, @IdentityNumber, @FullName, @Phone, @Email, @Address, @IsActive);
            ";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@CustomerCode", customer.CustomerCode);
                AddParameter(parameters, "@IdentityNumber", customer.IdentityNumber);
                AddParameter(parameters, "@FullName", customer.FullName);
                AddParameter(parameters, "@Phone", customer.Phone);
                AddParameter(parameters, "@Email", customer.Email);
                AddParameter(parameters, "@Address", customer.Address);
                AddParameter(parameters, "@IsActive", customer.IsActive);
            });
        }

        public bool Update(Customer customer)
        {
            const string sql = @"
                UPDATE Customers
                SET CustomerCode = @CustomerCode,
                    IdentityNumber = @IdentityNumber,
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
                AddParameter(parameters, "@Id", customer.Id);
                AddParameter(parameters, "@CustomerCode", customer.CustomerCode);
                AddParameter(parameters, "@IdentityNumber", customer.IdentityNumber);
                AddParameter(parameters, "@FullName", customer.FullName);
                AddParameter(parameters, "@Phone", customer.Phone);
                AddParameter(parameters, "@Email", customer.Email);
                AddParameter(parameters, "@Address", customer.Address);
                AddParameter(parameters, "@IsActive", customer.IsActive);
            }) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Customers
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

        public bool IsCustomerCodeExists(string customerCode, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Customers
                WHERE CustomerCode = @CustomerCode
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@CustomerCode", customerCode);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public bool IsIdentityNumberExists(string identityNumber, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Customers
                WHERE IdentityNumber = @IdentityNumber
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@IdentityNumber", identityNumber);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }

        public string GenerateCustomerCode()
        {
            return "CUS" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
