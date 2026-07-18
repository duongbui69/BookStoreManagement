using System;
using System.Collections.Generic;
using BookStoreManagement.Models;

namespace BookStoreManagement.Repositories
{
    public class VoucherTypeRepository : RepositoryBase
    {
        public List<VoucherType> GetAll()
        {
            const string sql = @"
                SELECT Id, Code, Name, Description, GroupType, Status 
                FROM VoucherTypes 
                ORDER BY Id ASC;
            ";
            var list = new List<VoucherType>();
            ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new VoucherType
                    {
                        Id = GetInt(reader, "Id"),
                        Code = GetString(reader, "Code"),
                        Name = GetString(reader, "Name"),
                        Description = GetNullableString(reader, "Description"),
                        GroupType = GetString(reader, "GroupType"),
                        Status = GetString(reader, "Status")
                    });
                }
                return true;
            }, sql);
            return list;
        }

        public VoucherType? GetById(int id)
        {
            const string sql = "SELECT * FROM VoucherTypes WHERE Id = @Id;";
            VoucherType? vt = null;
            ExecuteQuery(command =>
            {
                AddParameter(command.Parameters, "@Id", id);
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    vt = new VoucherType
                    {
                        Id = GetInt(reader, "Id"),
                        Code = GetString(reader, "Code"),
                        Name = GetString(reader, "Name"),
                        Description = GetNullableString(reader, "Description"),
                        GroupType = GetString(reader, "GroupType"),
                        Status = GetString(reader, "Status")
                    };
                }
                return true;
            }, sql);
            return vt;
        }

        public void Insert(VoucherType vt)
        {
            const string sql = @"
                INSERT INTO VoucherTypes (Code, Name, Description, GroupType, Status)
                VALUES (@Code, @Name, @Description, @GroupType, @Status);
            ";
            ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Code", vt.Code);
                AddParameter(parameters, "@Name", vt.Name);
                AddParameter(parameters, "@Description", vt.Description ?? (object)DBNull.Value);
                AddParameter(parameters, "@GroupType", vt.GroupType);
                AddParameter(parameters, "@Status", vt.Status);
            });
        }

        public void Update(VoucherType vt)
        {
            const string sql = @"
                UPDATE VoucherTypes 
                SET Code = @Code, Name = @Name, Description = @Description, 
                    GroupType = @GroupType, Status = @Status
                WHERE Id = @Id;
            ";
            ExecuteNonQuery(sql, parameters =>
            {
                AddParameter(parameters, "@Id", vt.Id);
                AddParameter(parameters, "@Code", vt.Code);
                AddParameter(parameters, "@Name", vt.Name);
                AddParameter(parameters, "@Description", vt.Description ?? (object)DBNull.Value);
                AddParameter(parameters, "@GroupType", vt.GroupType);
                AddParameter(parameters, "@Status", vt.Status);
            });
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM VoucherTypes WHERE Id = @Id;";
            ExecuteNonQuery(sql, parameters => AddParameter(parameters, "@Id", id));
        }

        public async System.Threading.Tasks.Task<List<VoucherType>> GetAllAsync()
        {
            const string sql = @"
                SELECT Id, Code, Name, Description, GroupType, Status 
                FROM VoucherTypes 
                ORDER BY Id ASC;
            ";
            var result = await QueryAsync<VoucherType>(sql);
            return System.Linq.Enumerable.ToList(result);
        }

        public async System.Threading.Tasks.Task<VoucherType?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM VoucherTypes WHERE Id = @Id;";
            return await QueryFirstOrDefaultAsync<VoucherType>(sql, new { Id = id });
        }

        public async System.Threading.Tasks.Task<int> InsertAsync(VoucherType vt)
        {
            const string sql = @"
                INSERT INTO VoucherTypes (Code, Name, Description, GroupType, Status)
                VALUES (@Code, @Name, @Description, @GroupType, @Status);
            ";
            return await ExecuteAsync(sql, new {
                Code = vt.Code,
                Name = vt.Name,
                Description = vt.Description ?? (object)DBNull.Value,
                GroupType = vt.GroupType,
                Status = vt.Status
            });
        }

        public async System.Threading.Tasks.Task<int> UpdateAsync(VoucherType vt)
        {
            const string sql = @"
                UPDATE VoucherTypes 
                SET Code = @Code, Name = @Name, Description = @Description, 
                    GroupType = @GroupType, Status = @Status
                WHERE Id = @Id;
            ";
            return await ExecuteAsync(sql, new {
                Id = vt.Id,
                Code = vt.Code,
                Name = vt.Name,
                Description = vt.Description ?? (object)DBNull.Value,
                GroupType = vt.GroupType,
                Status = vt.Status
            });
        }

        public async System.Threading.Tasks.Task<int> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM VoucherTypes WHERE Id = @Id;";
            return await ExecuteAsync(sql, new { Id = id });
        }
    }
}
