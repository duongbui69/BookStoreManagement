using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public abstract class RepositoryBase
    {
        // --- NEW DAPPER ASYNC METHODS ---
        protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<T>(sql, param);
        }

        protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        protected async Task<int> ExecuteAsync(string sql, object? param = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            return await connection.ExecuteAsync(sql, param);
        }

        protected async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<T>(sql, param);
        }

        protected async Task ExecuteTransactionAsync(Func<SqlConnection, SqlTransaction, Task> action)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                await action(connection, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // --- OLD ADO.NET METHODS (To keep compilation working while refactoring) ---
        protected T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);
            addParameters?.Invoke(command.Parameters);
            connection.Open();
            return action(command);
        }

        protected int ExecuteNonQuery(string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            return ExecuteQuery(command => command.ExecuteNonQuery(), sql, addParameters);
        }

        protected object? ExecuteScalar(string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            return ExecuteQuery(command => command.ExecuteScalar(), sql, addParameters);
        }

        protected int ExecuteScalarInt(string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            object? result = ExecuteScalar(sql, addParameters);
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        protected decimal ExecuteScalarDecimal(string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            object? result = ExecuteScalar(sql, addParameters);
            return result == null || result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }

        protected T ExecuteTransaction<T>(Func<SqlConnection, SqlTransaction, T> action)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                T result = action(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        protected void ExecuteTransaction(Action<SqlConnection, SqlTransaction> action)
        {
            ExecuteTransaction((connection, transaction) => { action(connection, transaction); return true; });
        }

        protected void AddParameter(SqlParameterCollection parameters, string name, object? value)
        {
            parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        protected void AddParameter(SqlCommand command, string name, object? value)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        protected string GetString(SqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);
            return reader.IsDBNull(index) ? string.Empty : reader.GetString(index);
        }

        protected string? GetNullableString(SqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);
            return reader.IsDBNull(index) ? null : reader.GetString(index);
        }

        protected int GetInt(SqlDataReader reader, string columnName)
        {
            return reader.GetInt32(reader.GetOrdinal(columnName));
        }

        protected int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);
            return reader.IsDBNull(index) ? null : reader.GetInt32(index);
        }

        protected bool GetBool(SqlDataReader reader, string columnName)
        {
            return reader.GetBoolean(reader.GetOrdinal(columnName));
        }

        protected decimal GetDecimal(SqlDataReader reader, string columnName)
        {
            return reader.GetDecimal(reader.GetOrdinal(columnName));
        }

        protected decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);
            return reader.IsDBNull(index) ? null : reader.GetDecimal(index);
        }

        protected DateTime GetDateTime(SqlDataReader reader, string columnName)
        {
            return reader.GetDateTime(reader.GetOrdinal(columnName));
        }

        protected DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);
            return reader.IsDBNull(index) ? null : reader.GetDateTime(index);
        }
    }
}
