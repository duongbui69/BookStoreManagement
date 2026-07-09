using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class PublisherRepository : RepositoryBase
    {
        private Publisher MapPublisher(SqlDataReader reader)
        {
            return new Publisher
            {
                Id = GetInt(reader, "Id"),
                PublisherName = GetString(reader, "PublisherName"),
                Phone = GetNullableString(reader, "Phone"),
                Email = GetNullableString(reader, "Email"),
                Address = GetNullableString(reader, "Address"),
                IsActive = GetBool(reader, "IsActive"),
                CreatedAt = GetDateTime(reader, "CreatedAt"),
                UpdatedAt = GetNullableDateTime(reader, "UpdatedAt")
            };
        }

        public List<Publisher> GetAll()
        {
            var publishers = new List<Publisher>();
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) publishers.Add(MapPublisher(reader));
                return publishers;
            }, sql);
        }

        public List<Publisher> GetActive()
        {
            var publishers = new List<Publisher>();
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                WHERE IsActive = 1
                ORDER BY PublisherName;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) publishers.Add(MapPublisher(reader));
                return publishers;
            }, sql);
        }

        public Publisher? GetById(int id)
        {
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                WHERE Id = @Id;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapPublisher(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public List<Publisher> Search(string keyword)
        {
            var publishers = new List<Publisher>();
            const string sql = @"
                SELECT Id, PublisherName, Phone, Email, Address, IsActive, CreatedAt, UpdatedAt
                FROM Publishers
                WHERE PublisherName LIKE N'%' + @Keyword + N'%'
                   OR Phone LIKE N'%' + @Keyword + N'%'
                   OR Email LIKE N'%' + @Keyword + N'%'
                   OR Address LIKE N'%' + @Keyword + N'%'
                ORDER BY Id DESC;
            ";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) publishers.Add(MapPublisher(reader));
                return publishers;
            }, sql, parameters => AddParameter(parameters, "@Keyword", keyword));
        }

        public int Add(Publisher publisher)
        {
            const string sql = @"
                INSERT INTO Publishers (PublisherName, Phone, Email, Address, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@PublisherName, @Phone, @Email, @Address, @IsActive);
            ";
            return ExecuteQuery(command => Convert.ToInt32(command.ExecuteScalar()), sql, parameters =>
            {
                AddParameter(parameters, "@PublisherName", publisher.PublisherName);
                AddParameter(parameters, "@Phone", publisher.Phone);
                AddParameter(parameters, "@Email", publisher.Email);
                AddParameter(parameters, "@Address", publisher.Address);
                AddParameter(parameters, "@IsActive", publisher.IsActive);
            });
        }

        public bool Update(Publisher publisher)
        {
            const string sql = @"
                UPDATE Publishers
                SET PublisherName = @PublisherName,
                    Phone = @Phone,
                    Email = @Email,
                    Address = @Address,
                    IsActive = @IsActive,
                    UpdatedAt = SYSDATETIME()
                WHERE Id = @Id;
            ";
            return ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", publisher.Id);
                AddParameter(parameters, "@PublisherName", publisher.PublisherName);
                AddParameter(parameters, "@Phone", publisher.Phone);
                AddParameter(parameters, "@Email", publisher.Email);
                AddParameter(parameters, "@Address", publisher.Address);
                AddParameter(parameters, "@IsActive", publisher.IsActive);
            }) > 0;
        }

        public bool SetActive(int id, bool isActive)
        {
            const string sql = @"
                UPDATE Publishers
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

        public bool IsNameExists(string publisherName, int? excludeId = null)
        {
            string sql = @"
                SELECT COUNT(1)
                FROM Publishers
                WHERE PublisherName = @PublisherName
            ";
            if (excludeId.HasValue) sql += " AND Id <> @ExcludeId";
            return ExecuteScalarInt(sql, parameters =>
            {
                AddParameter(parameters, "@PublisherName", publisherName);
                if (excludeId.HasValue) AddParameter(parameters, "@ExcludeId", excludeId.Value);
            }) > 0;
        }
    }
}
