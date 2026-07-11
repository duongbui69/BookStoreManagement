using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class PublisherRepository
    {
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            addParameters?.Invoke(command.Parameters);
            connection.Open();
            return action(command);
        }

        public List<Publisher> GetAll()
        {
            const string sql = "SELECT * FROM Publishers WHERE IsActive = 1";
            return ExecuteQuery(command =>
            {
                var list = new List<Publisher>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapPublisher(reader));
                }
                return list;
            }, sql);
        }

        public Publisher? GetById(int id)
        {
            const string sql = "SELECT * FROM Publishers WHERE Id = @Id AND IsActive = 1";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                if (reader.Read()) return MapPublisher(reader);
                return null;
            }, sql, parameters => parameters.AddWithValue("@Id", id));
        }

        public void Add(Publisher publisher)
        {
            const string sql = @"INSERT INTO Publishers (PublisherName, Phone, Email, Address, IsActive, CreatedAt) 
                                 VALUES (@PublisherName, @Phone, @Email, @Address, 1, GETDATE())";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters =>
            {
                parameters.AddWithValue("@PublisherName", publisher.PublisherName);
                parameters.AddWithValue("@Phone", publisher.Phone ?? (object)DBNull.Value);
                parameters.AddWithValue("@Email", publisher.Email ?? (object)DBNull.Value);
                parameters.AddWithValue("@Address", publisher.Address ?? (object)DBNull.Value);
            });
        }

        public void Update(Publisher publisher)
        {
            const string sql = @"UPDATE Publishers SET PublisherName = @PublisherName, Phone = @Phone, 
                                 Email = @Email, Address = @Address, UpdatedAt = GETDATE() WHERE Id = @Id";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters =>
            {
                parameters.AddWithValue("@Id", publisher.Id);
                parameters.AddWithValue("@PublisherName", publisher.PublisherName);
                parameters.AddWithValue("@Phone", publisher.Phone ?? (object)DBNull.Value);
                parameters.AddWithValue("@Email", publisher.Email ?? (object)DBNull.Value);
                parameters.AddWithValue("@Address", publisher.Address ?? (object)DBNull.Value);
            });
        }

        public void Delete(int id)
        {
            const string sql = "UPDATE Publishers SET IsActive = 0, UpdatedAt = GETDATE() WHERE Id = @Id";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters => parameters.AddWithValue("@Id", id));
        }

        private Publisher MapPublisher(SqlDataReader reader)
        {
            return new Publisher
            {
                Id = DataReaderHelper.GetInt(reader, "Id"),
                PublisherName = DataReaderHelper.GetString(reader, "PublisherName"),
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
