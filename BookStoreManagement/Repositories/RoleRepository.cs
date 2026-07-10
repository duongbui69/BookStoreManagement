using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class RoleRepository : RepositoryBase
    {
        private Role MapRole(SqlDataReader reader)
        {
            return new Role
            {
                Id = GetInt(reader, "Id"),
                RoleName = GetString(reader, "RoleName"),
                Description = GetNullableString(reader, "Description")
            };
        }

        public List<Role> GetAll()
        {
            var roles = new List<Role>();

            const string sql = @"
                SELECT Id, RoleName, Description
                FROM Roles
                ORDER BY Id;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) roles.Add(MapRole(reader));
                return roles;
            }, sql);
        }

        public Role? GetById(int id)
        {
            const string sql = @"
                SELECT Id, RoleName, Description
                FROM Roles
                WHERE Id = @Id;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapRole(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public Role? GetByName(string roleName)
        {
            const string sql = @"
                SELECT Id, RoleName, Description
                FROM Roles
                WHERE RoleName = @RoleName;
            ";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapRole(reader) : null;
            }, sql, parameters => AddParameter(parameters, "@RoleName", roleName));
        }
    }
}
