using System;
using System.Collections.Generic;

namespace BookStoreManagement.Repositories
{
    public class RoleRepository
    {
        // Execute a SQL query and return the result
        private T ExecuteQuery<T>(Func<SqlCommand, T> action, string sql, Action<SqlParameterCollection>? addParameters = null)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(sql, connection);

            addParameters?.Invoke(command.Parameters);

            connection.Open();
            return action(command);
        }

        // Get all roles
        public List<Role> GetAll()
        {
            var roles = new List<Role>();
            const string sql = @"SELECT Id, RoleName, Description FROM Roles ORDER BY Id";
            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    roles.Add(MapRole(reader));
                }
                return roles;
            }, sql);
        }
    }
}