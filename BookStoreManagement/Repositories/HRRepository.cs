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
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
    }

    public class HRRepository
    {
        public HRStats GetStats()
        {
            var stats = new HRStats();
            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users", connection))
                stats.TotalEmployees = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE JoinDate >= DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)", connection))
                stats.NewThisMonth = (int)cmd.ExecuteScalar();

            using (var cmd = new SqlCommand("SELECT COUNT(DISTINCT Department) FROM Users WHERE Department IS NOT NULL", connection))
                stats.ActiveDepartments = (int)cmd.ExecuteScalar();

            // Mock load percentages based on employee counts
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Department = 'Logistics'", connection))
                stats.LogisticsLoad = Math.Min(100, (int)cmd.ExecuteScalar() * 15);

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Department LIKE '%Sales%'", connection))
                stats.SalesLoad = Math.Min(100, (int)cmd.ExecuteScalar() * 20);

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Department LIKE '%IT%'", connection))
                stats.ITLoad = Math.Min(100, (int)cmd.ExecuteScalar() * 25);

            return stats;
        }

        public (List<HREmployeeItem> Items, int TotalCount) GetPagedEmployees(int page, int pageSize, string departmentFilter, string statusFilter, string searchTerm)
        {
            var list = new List<HREmployeeItem>();
            int totalCount = 0;

            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();

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

            // Get total count
            string countQuery = $@"
                SELECT COUNT(*) 
                FROM Users u 
                LEFT JOIN Roles r ON u.RoleId = r.Id
                {whereClause}";
                
            using (var countCmd = new SqlCommand(countQuery, connection))
            {
                if (!string.IsNullOrEmpty(departmentFilter) && departmentFilter != "All Departments") countCmd.Parameters.AddWithValue("@Dept", departmentFilter);
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Status: All") countCmd.Parameters.AddWithValue("@Status", statusFilter);
                if (!string.IsNullOrEmpty(searchTerm)) countCmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                totalCount = (int)countCmd.ExecuteScalar();
            }

            // Get paged data
            string dataQuery = $@"
                SELECT 
                    u.FullName, ISNULL(u.Email, ''), ISNULL(r.RoleName, 'Employee'), ISNULL(u.EmployeeId, '#EMP-0000'), 
                    ISNULL(u.Department, 'Unassigned'), ISNULL(u.Status, 'ACTIVE'), ISNULL(u.JoinDate, GETDATE())
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.Id
                {whereClause}
                ORDER BY u.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using (var dataCmd = new SqlCommand(dataQuery, connection))
            {
                if (!string.IsNullOrEmpty(departmentFilter) && departmentFilter != "All Departments") dataCmd.Parameters.AddWithValue("@Dept", departmentFilter);
                if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Status: All") dataCmd.Parameters.AddWithValue("@Status", statusFilter);
                if (!string.IsNullOrEmpty(searchTerm)) dataCmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                dataCmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                dataCmd.Parameters.AddWithValue("@PageSize", pageSize);

                using var reader = dataCmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new HREmployeeItem
                    {
                        Name = reader.GetString(0),
                        Email = reader.GetString(1),
                        RoleName = reader.GetString(2),
                        EmployeeId = reader.GetString(3),
                        Department = reader.GetString(4),
                        Status = reader.GetString(5),
                        JoinDate = reader.GetDateTime(6)
                    });
                }
            }

            return (list, totalCount);
        }
    }
}
