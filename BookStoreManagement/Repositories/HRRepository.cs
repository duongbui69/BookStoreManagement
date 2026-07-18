using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class HRStats
    {
        public int TotalEmployees { get; set; }
        public int NewThisMonth { get; set; }
        public int ActiveDepartments { get; set; }
        public int LogisticsLoad { get; set; }
        public int SalesLoad { get; set; }
        public int ITLoad { get; set; }
    }

    public class HREmployeeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
    }

    public class HRRepository : RepositoryBase
    {
        public HRStats GetStats()
        {
            var stats = new HRStats();

            stats.TotalEmployees = ExecuteScalarInt("SELECT COUNT(*) FROM Users");
            stats.NewThisMonth = ExecuteScalarInt("SELECT COUNT(*) FROM Users WHERE JoinDate >= DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)");
            stats.ActiveDepartments = ExecuteScalarInt("SELECT COUNT(DISTINCT Department) FROM Users WHERE Department IS NOT NULL");

            // Mock load percentages based on employee counts
            stats.LogisticsLoad = Math.Min(100, ExecuteScalarInt("SELECT COUNT(*) FROM Users WHERE Department = 'Logistics'") * 15);
            stats.SalesLoad = Math.Min(100, ExecuteScalarInt("SELECT COUNT(*) FROM Users WHERE Department LIKE '%Sales%'") * 20);
            stats.ITLoad = Math.Min(100, ExecuteScalarInt("SELECT COUNT(*) FROM Users WHERE Department LIKE '%IT%'") * 25);

            return stats;
        }

        public (List<HREmployeeItem> Items, int TotalCount) GetPagedEmployees(int page, int pageSize, string departmentFilter, string statusFilter, string searchTerm)
        {
            string whereClause = "WHERE 1=1 ";
            if (!string.IsNullOrEmpty(departmentFilter) && departmentFilter != "All Departments")
            {
                whereClause += "AND u.Department = @Dept ";
            }
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Status: All")
            {
                whereClause += "AND u.Status = @Status ";
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += "AND (u.FullName LIKE @Search OR u.Email LIKE @Search OR u.EmployeeId LIKE @Search) ";
            }

            Action<SqlParameterCollection> addParams = p =>
            {
                if (!string.IsNullOrEmpty(departmentFilter) && departmentFilter != "All Departments") AddParameter(p, "@Dept", departmentFilter);
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Status: All") AddParameter(p, "@Status", statusFilter);
                if (!string.IsNullOrEmpty(searchTerm)) AddParameter(p, "@Search", "%" + searchTerm + "%");
            };

            // Get total count
            string countQuery = $@"
                SELECT COUNT(*) 
                FROM Users u 
                LEFT JOIN Roles r ON u.RoleId = r.Id
                {whereClause}";
                
            int totalCount = ExecuteScalarInt(countQuery, addParams);

            // Get paged data
            string dataQuery = $@"
                SELECT 
                    u.Id, u.FullName, ISNULL(u.Email, ''), ISNULL(r.RoleName, 'Employee'), ISNULL(u.EmployeeId, '#EMP-0000'), 
                    ISNULL(u.Department, 'Unassigned'), ISNULL(u.Status, 'ACTIVE'), ISNULL(u.JoinDate, GETDATE())
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.Id
                {whereClause}
                ORDER BY u.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var list = ExecuteQuery(cmd =>
            {
                var results = new List<HREmployeeItem>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new HREmployeeItem
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Email = reader.GetString(2),
                        RoleName = reader.GetString(3),
                        EmployeeId = reader.GetString(4),
                        Department = reader.GetString(5),
                        Status = reader.GetString(6),
                        JoinDate = reader.GetDateTime(7)
                    });
                }
                return results;
            }, dataQuery, p =>
            {
                addParams(p);
                AddParameter(p, "@Offset", (page - 1) * pageSize);
                AddParameter(p, "@PageSize", pageSize);
            });

            return (list, totalCount);
        }

        public async System.Threading.Tasks.Task<HRStats> GetStatsAsync()
        {
            var stats = new HRStats();

            stats.TotalEmployees = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");
            stats.NewThisMonth = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE JoinDate >= DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)");
            stats.ActiveDepartments = await ExecuteScalarAsync<int>("SELECT COUNT(DISTINCT Department) FROM Users WHERE Department IS NOT NULL");

            stats.LogisticsLoad = Math.Min(100, await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE Department = 'Logistics'") * 15);
            stats.SalesLoad = Math.Min(100, await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE Department LIKE '%Sales%'") * 20);
            stats.ITLoad = Math.Min(100, await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE Department LIKE '%IT%'") * 25);

            return stats;
        }

        public async System.Threading.Tasks.Task<(List<HREmployeeItem> Items, int TotalCount)> GetPagedEmployeesAsync(int page, int pageSize, string departmentFilter, string statusFilter, string searchTerm)
        {
            string whereClause = "WHERE 1=1 ";
            if (!string.IsNullOrEmpty(departmentFilter) && departmentFilter != "All Departments")
            {
                whereClause += "AND u.Department = @Dept ";
            }
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Status: All")
            {
                whereClause += "AND u.Status = @Status ";
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                whereClause += "AND (u.FullName LIKE @Search OR u.Email LIKE @Search OR u.EmployeeId LIKE @Search) ";
            }

            var parameters = new
            {
                Dept = departmentFilter,
                Status = statusFilter,
                Search = "%" + searchTerm + "%",
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };

            string countQuery = $@"
                SELECT COUNT(*) 
                FROM Users u 
                LEFT JOIN Roles r ON u.RoleId = r.Id
                {whereClause}";
                
            int totalCount = await ExecuteScalarAsync<int>(countQuery, parameters);

            string dataQuery = $@"
                SELECT 
                    u.Id, u.FullName AS Name, ISNULL(u.Email, '') AS Email, ISNULL(r.RoleName, 'Employee') AS RoleName, ISNULL(u.EmployeeId, '#EMP-0000') AS EmployeeId, 
                    ISNULL(u.Department, 'Unassigned') AS Department, ISNULL(u.Status, 'ACTIVE') AS Status, ISNULL(u.JoinDate, GETDATE()) AS JoinDate
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.Id
                {whereClause}
                ORDER BY u.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var items = await QueryAsync<HREmployeeItem>(dataQuery, parameters);

            return (System.Linq.Enumerable.ToList(items), totalCount);
        }
    }
}
