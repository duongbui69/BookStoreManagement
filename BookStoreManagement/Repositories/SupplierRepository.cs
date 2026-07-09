using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class SupplierRepository : RepositoryBase
    {
        private Supplier MapSupplier(SqlDataReader reader)
        {
            return new Supplier
            {
                Id = GetInt(reader, "Id"),
                SupplierName = GetString(reader, "SupplierName"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Supplier> GetAll()
        {
            var suppliers = new List<Supplier>();
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) suppliers.Add(MapSupplier(reader));
                return suppliers;
            }, sql);
        }

        public List<Supplier> GetActive()
        {
            var suppliers = new List<Supplier>();
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                WHERE IsActive = 1
                ORDER BY SupplierName;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) suppliers.Add(MapSupplier(reader));
                return suppliers;
            }, sql);
        }

        public Supplier? GetById(int id)
        {
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapSupplier(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public List<Supplier> Search(string keyword)
        {
            var suppliers = new List<Supplier>();
            const string sql = @"
                SELECT Id, SupplierName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Suppliers
                WHERE SupplierName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) suppliers.Add(MapSupplier(reader));
                return suppliers;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public int Add(Supplier supplier)
        {
            const string sql = @"
                INSERT INTO Suppliers (SupplierName, Phone, Email, Address, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@SupplierName, @Phone, @Email, @Address, @IsActive);
            ";
            return ExecuteQuery(command => Convert.ToInt32(command.ExecuteScalar()), sql, parameters =>
            {
                AddParameter(parameters, "@SupplierName", supplier.SupplierName);
                AddParameter(parameters, "@Phone", supplier.Phone);
                AddParameter(parameters, "@Email", supplier.Email);
                AddParameter(parameters, "@Address", supplier.Address);
                AddParameter(parameters, "@IsActive", supplier.IsActive);
            });
        }

        public bool Update(Supplier supplier)
        {
            const string sql = @"
                UPDATE Suppliers
                SET SupplierName = @SupplierName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", supplier.Id);
                AddParameter(parameters, "@SupplierName", supplier.SupplierName);
                AddParameter(parameters, "@Phone", supplier.Phone);
                AddParameter(parameters, "@Email", supplier.Email);
                AddParameter(parameters, "@Address", supplier.Address);
                AddParameter(parameters, "@IsActive", supplier.IsActive);
            }) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Suppliers
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

        public bool IsNameExists(string supplierName, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Suppliers
                WHERE SupplierName = @SupplierName
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@SupplierName", supplierName);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }
    }
}
