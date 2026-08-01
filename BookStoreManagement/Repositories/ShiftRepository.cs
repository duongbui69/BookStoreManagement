using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Database;
using BookStoreManagement.Models;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Repositories
{
    public class ShiftRepository
    {
        public Shift? GetActiveShift(int staffId, int storeId)
        {
            Shift? shift = null;
            string query = @"SELECT Id, StaffId, StoreId, ShiftName, StartTime, EndTime, InitialCash, Revenue, TotalOrders, Status 
                             FROM Shifts 
                             WHERE StaffId = @StaffId AND StoreId = @StoreId AND Status = 'Open'";

            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StaffId", staffId);
                cmd.Parameters.AddWithValue("@StoreId", storeId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        shift = new Shift
                        {
                            Id = reader.GetInt32(0),
                            StaffId = reader.GetInt32(1),
                            StoreId = reader.GetInt32(2),
                            ShiftName = reader.GetString(3),
                            StartTime = reader.GetDateTime(4),
                            EndTime = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                            InitialCash = reader.GetDecimal(6),
                            Revenue = reader.GetDecimal(7),
                            TotalOrders = reader.GetInt32(8),
                            Status = reader.GetString(9)
                        };
                    }
                }
            }
            return shift;
        }

        public void OpenShift(Shift shift)
        {
            string query = @"INSERT INTO Shifts (StaffId, StoreId, ShiftName, StartTime, InitialCash, Revenue, TotalOrders, Status) 
                             VALUES (@StaffId, @StoreId, @ShiftName, @StartTime, @InitialCash, 0, 0, 'Open')";

            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StaffId", shift.StaffId);
                cmd.Parameters.AddWithValue("@StoreId", shift.StoreId);
                cmd.Parameters.AddWithValue("@ShiftName", shift.ShiftName);
                cmd.Parameters.AddWithValue("@StartTime", shift.StartTime);
                cmd.Parameters.AddWithValue("@InitialCash", shift.InitialCash);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void CloseShift(int shiftId, decimal finalRevenue, int totalOrders)
        {
            string query = @"UPDATE Shifts 
                             SET EndTime = @EndTime, Revenue = @Revenue, TotalOrders = @TotalOrders, Status = 'Closed' 
                             WHERE Id = @Id";

            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EndTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@Revenue", finalRevenue);
                cmd.Parameters.AddWithValue("@TotalOrders", totalOrders);
                cmd.Parameters.AddWithValue("@Id", shiftId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<ShiftViewModel> GetShiftHistory(int staffId, int storeId)
        {
            var list = new List<ShiftViewModel>();
            string query = @"SELECT Id, ShiftName, StartTime, EndTime, InitialCash, Revenue, TotalOrders, Status 
                             FROM Shifts 
                             WHERE StaffId = @StaffId AND StoreId = @StoreId 
                             ORDER BY StartTime DESC";

            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@StaffId", staffId);
                cmd.Parameters.AddWithValue("@StoreId", storeId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ShiftViewModel
                        {
                            Id = reader.GetInt32(0),
                            ShiftName = reader.GetString(1),
                            StartTime = reader.GetDateTime(2),
                            EndTime = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                            InitialCash = reader.GetDecimal(4),
                            Revenue = reader.GetDecimal(5),
                            TotalOrders = reader.GetInt32(6),
                            Status = reader.GetString(7)
                        });
                    }
                }
            }
            return list;
        }
    
        public Shift? GetShiftById(int id)
        {
            Shift? shift = null;
            string query = @"SELECT Id, StaffId, StoreId, ShiftName, StartTime, EndTime, InitialCash, Revenue, TotalOrders, Status 
                             FROM Shifts 
                             WHERE Id = @Id";

            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        shift = new Shift
                        {
                            Id = reader.GetInt32(0),
                            StaffId = reader.GetInt32(1),
                            StoreId = reader.GetInt32(2),
                            ShiftName = reader.GetString(3),
                            StartTime = reader.GetDateTime(4),
                            EndTime = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                            InitialCash = reader.GetDecimal(6),
                            Revenue = reader.GetDecimal(7),
                            TotalOrders = reader.GetInt32(8),
                            Status = reader.GetString(9)
                        };
                    }
                }
            }
            return shift;
        }

        public List<ShiftViewModel> GetAllShifts()
        {
            var list = new List<ShiftViewModel>();
            string query = @"SELECT Id, StaffId as StaffId, ShiftName, StartTime, EndTime, InitialCash, Revenue, TotalOrders, Status 
                             FROM Shifts 
                             ORDER BY StartTime DESC";

            using (var conn = DbConnectionFactory.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ShiftViewModel
                        {
                            Id = reader.GetInt32(0),
                            StaffId = reader.GetInt32(1),
                            ShiftName = reader.GetString(2),
                            StartTime = reader.GetDateTime(3),
                            EndTime = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            InitialCash = reader.GetDecimal(5),
                            Revenue = reader.GetDecimal(6),
                            TotalOrders = reader.GetInt32(7),
                            Status = reader.GetString(8)
                        });
                    }
                }
            }
            return list;
        }
}
}
