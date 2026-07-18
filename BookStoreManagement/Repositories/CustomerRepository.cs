using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class CustomerRepository : RepositoryBase
    {
        public async Task<List<Customer>> GetAllAsync()
        {
            const string sql = @"
                SELECT Id, CustomerCode, IdentityNumber, FullName, Phone, Email, Address, Points, IsActive, CreatedAt, UpdatedAt
                FROM Customers
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Customer>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public async Task<List<Customer>> GetActiveAsync()
        {
            const string sql = @"
                SELECT Id, CustomerCode, IdentityNumber, FullName, Phone, Email, Address, Points, IsActive, CreatedAt, UpdatedAt
                FROM Customers
                WHERE IsActive = 1
                ORDER BY FullName;
            ";
            var result = await QueryAsync<Customer>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, CustomerCode, IdentityNumber, FullName, Phone, Email, Address, Points, IsActive, CreatedAt, UpdatedAt
                FROM Customers
                WHERE Id = @Id;
            ";
            return await QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
        }

        public async Task<List<Customer>> SearchAsync(string keyword)
        {
            const string sql = @"
                SELECT Id, CustomerCode, IdentityNumber, FullName, Phone, Email, Address, Points, IsActive, CreatedAt, UpdatedAt
                FROM Customers
                WHERE CustomerCode LIKE N'%' + @Keyword + N'%'
                   OR IdentityNumber LIKE N'%' + @Keyword + N'%'
                   OR FullName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            var result = await QueryAsync<Customer>(sql, new { Keyword = keyword });
            return System.Linq.Enumerable.ToList(result);
        }

        public string GenerateCustomerCode()
        {
            return "CUS" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        public async Task<int> AddAsync(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.CustomerCode))
            {
                customer.CustomerCode = GenerateCustomerCode();
            }

            const string sql = @"
                INSERT INTO Customers (CustomerCode, IdentityNumber, FullName, Phone, Email, Address, Points, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@CustomerCode, @IdentityNumber, @FullName, @Phone, @Email, @Address, @Points, @IsActive);
            ";
            return await ExecuteScalarAsync<int>(sql, customer);
        }

        public async Task<bool> UpdateAsync(Customer customer)
        {
            const string sql = @"
                UPDATE Customers
                SET CustomerCode = @CustomerCode,
                    IdentityNumber = @IdentityNumber,
                    FullName = @FullName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    Points = @Points,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, customer) > 0;
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Customers
                SET IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, new { Id = id, IsActive = isActive }) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"
                UPDATE Customers SET IsActive = 0, UpdatedAt = SYSDATETIME() WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, new { Id = id }) > 0;
        }

        public async Task<bool> DeleteMultipleAsync(IEnumerable<int> ids)
        {
            const string sql = @"
                UPDATE Customers SET IsActive = 0, UpdatedAt = SYSDATETIME() WHERE Id IN @Ids;
            ";
            return await ExecuteAsync(sql, new { Ids = ids }) > 0;
        }

        public async Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Customers
                WHERE CustomerCode = @CustomerCode
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return await ExecuteScalarAsync<int>(sql, new { CustomerCode = customerCode, ExcludeId = excludeId }) > 0;
        }

        public async Task<bool> IsIdentityNumberExistsAsync(string identityNumber, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Customers
                WHERE IdentityNumber = @IdentityNumber
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";

            return await ExecuteScalarAsync<int>(sql, new { IdentityNumber = identityNumber, ExcludeId = excludeId }) > 0;
        }
    }
}
