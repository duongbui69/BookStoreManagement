using System;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public abstract class RepositoryBase
    {
        protected T ExecuteQuery<T>(
            Func<SqlCommand, T> action,
            string sql,
            Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);

            addParameters?.Invoke(command.Parameters);

            connection.Open();
            return action(command);
        }

        protected int ExecuteNonQuery(
            string sql,
            Action<SqlParameterCollection>? addParameters = null)
        {
            return ExecuteQuery(command => command.ExecuteNonQuery(), sql, addParameters);
        }

        protected object? ExecuteScalar(
            string sql,
            Action<SqlParameterCollection>? addParameters = null)
        {
            return ExecuteQuery(command => command.ExecuteScalar(), sql, addParameters);
        }

        protected int ExecuteScalarInt(
            string sql,
            Action<SqlParameterCollection>? addParameters = null)
        {
            object? result = ExecuteScalar(sql, addParameters);
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        protected decimal ExecuteScalarDecimal(
            string sql,
            Action<SqlParameterCollection>? addParameters = null)
        {
            object? result = ExecuteScalar(sql, addParameters);
            return result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
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

        protected void AddParameter(SqlParameterCollection parameters, string parameterName, object? value)
        {
            parameters.AddWithValue(parameterName, value ?? DBNull.Value);
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

        protected bool GetBool(SqlDataReader reader, string columnName)
        {
            return reader.GetBoolean(reader.GetOrdinal(columnName));
        }

        protected decimal GetDecimal(SqlDataReader reader, string columnName)
        {
            return reader.GetDecimal(reader.GetOrdinal(columnName));
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
