using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class BookRepository
    {
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            addParameters?.Invoke(command.Parameters);
            connection.Open();
            return action(command);
        }

        public List<Book> GetAll()
        {
            const string sql = "SELECT * FROM Books WHERE IsActive = 1";
            return ExecuteQuery(command =>
            {
                var list = new List<Book>();
                using var reader = command.ExecuteReader();
                while (reader.Read()) list.Add(MapBook(reader));
                return list;
            }, sql);
        }

        public List<BookViewModel> GetBooksWithDetails()
        {
            const string sql = @"
                SELECT 
                    b.Id, b.BookCode, b.ISBN, b.Title, b.SellingPrice, b.Quantity, b.MinStock, b.ImagePath,
                    c.CategoryName,
                    a.AuthorName,
                    p.PublisherName
                FROM Books b
                LEFT JOIN Categories c ON b.CategoryId = c.Id
                LEFT JOIN Authors a ON b.AuthorId = a.Id
                LEFT JOIN Publishers p ON b.PublisherId = p.Id
                WHERE b.IsActive = 1";

            return ExecuteQuery(command =>
            {
                var list = new List<BookViewModel>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new BookViewModel
                    {
                        Id = DataReaderHelper.GetInt(reader, "Id"),
                        BookCode = DataReaderHelper.GetString(reader, "BookCode"),
                        Title = DataReaderHelper.GetString(reader, "Title"),
                        SellingPrice = DataReaderHelper.GetDecimal(reader, "SellingPrice"),
                        Quantity = DataReaderHelper.GetInt(reader, "Quantity"),
                        MinStock = DataReaderHelper.GetInt(reader, "MinStock"),
                        CategoryName = DataReaderHelper.GetNullableString(reader, "CategoryName") ?? "N/A",
                        AuthorName = DataReaderHelper.GetNullableString(reader, "AuthorName") ?? "N/A",
                        PublisherName = DataReaderHelper.GetNullableString(reader, "PublisherName") ?? "N/A",
                        ImagePath = DataReaderHelper.GetNullableString(reader, "ImagePath") ?? "",
                        ISBN = DataReaderHelper.GetNullableString(reader, "ISBN") ?? "N/A"
                    });
                }
                return list;
            }, sql);
        }

        public Book? GetById(int id)
        {
            const string sql = "SELECT * FROM Books WHERE Id = @Id AND IsActive = 1";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                if (reader.Read()) return MapBook(reader);
                return null;
            }, sql, parameters => parameters.AddWithValue("@Id", id));
        }

        public void Add(Book book)
        {
            const string sql = @"INSERT INTO Books (BookCode, ISBN, Title, CategoryId, AuthorId, PublisherId, 
                                 SellingPrice, Quantity, MinStock, Description, ImagePath, IsActive, CreatedAt) 
                                 VALUES (@BookCode, @ISBN, @Title, @CategoryId, @AuthorId, @PublisherId, 
                                 @SellingPrice, @Quantity, @MinStock, @Description, @ImagePath, 1, GETDATE())";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters => AddBookParameters(parameters, book));
        }

        public void Update(Book book)
        {
            const string sql = @"UPDATE Books SET BookCode = @BookCode, ISBN = @ISBN, Title = @Title, 
                                 CategoryId = @CategoryId, AuthorId = @AuthorId, PublisherId = @PublisherId, 
                                 SellingPrice = @SellingPrice, Quantity = @Quantity, MinStock = @MinStock, 
                                 Description = @Description, ImagePath = @ImagePath, UpdatedAt = GETDATE() 
                                 WHERE Id = @Id";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters =>
            {
                parameters.AddWithValue("@Id", book.Id);
                AddBookParameters(parameters, book);
            });
        }

        public void Delete(int id)
        {
            const string sql = "UPDATE Books SET IsActive = 0, UpdatedAt = GETDATE() WHERE Id = @Id";
            ExecuteQuery(command => command.ExecuteNonQuery(), sql, parameters => parameters.AddWithValue("@Id", id));
        }

        private void AddBookParameters(SqlParameterCollection parameters, Book book)
        {
            parameters.AddWithValue("@BookCode", book.BookCode);
            parameters.AddWithValue("@ISBN", book.ISBN ?? (object)DBNull.Value);
            parameters.AddWithValue("@Title", book.Title);
            parameters.AddWithValue("@CategoryId", book.CategoryId);
            parameters.AddWithValue("@AuthorId", book.AuthorId ?? (object)DBNull.Value);
            parameters.AddWithValue("@PublisherId", book.PublisherId ?? (object)DBNull.Value);
            parameters.AddWithValue("@SellingPrice", book.SellingPrice);
            parameters.AddWithValue("@Quantity", book.Quantity);
            parameters.AddWithValue("@MinStock", book.MinStock);
            parameters.AddWithValue("@Description", book.Description ?? (object)DBNull.Value);
            parameters.AddWithValue("@ImagePath", book.ImagePath ?? (object)DBNull.Value);
        }

        private Book MapBook(SqlDataReader reader)
        {
            return new Book
            {
                Id = DataReaderHelper.GetInt(reader, "Id"),
                BookCode = DataReaderHelper.GetString(reader, "BookCode"),
                ISBN = DataReaderHelper.GetNullableString(reader, "ISBN"),
                Title = DataReaderHelper.GetString(reader, "Title"),
                CategoryId = DataReaderHelper.GetInt(reader, "CategoryId"),
                AuthorId = DataReaderHelper.GetNullableInt(reader, "AuthorId"),
                PublisherId = DataReaderHelper.GetNullableInt(reader, "PublisherId"),
                SellingPrice = DataReaderHelper.GetDecimal(reader, "SellingPrice"),
                Quantity = DataReaderHelper.GetInt(reader, "Quantity"),
                MinStock = DataReaderHelper.GetInt(reader, "MinStock"),
                Description = DataReaderHelper.GetNullableString(reader, "Description"),
                ImagePath = DataReaderHelper.GetNullableString(reader, "ImagePath"),
                IsActive = DataReaderHelper.GetBool(reader, "IsActive"),
                CreatedAt = DataReaderHelper.GetDateTime(reader, "CreatedAt"),
                UpdatedAt = DataReaderHelper.GetNullableDateTime(reader, "UpdatedAt")
            };
        }
    }
}
