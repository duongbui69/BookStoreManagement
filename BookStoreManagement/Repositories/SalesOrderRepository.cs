using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class SalesOrderRepository
    {
        public void CreateSalesOrder(SalesOrder order, List<SalesOrderDetail> details)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Insert SalesOrder
                const string sqlOrder = @"INSERT INTO SalesOrders (OrderCode, UserId, CustomerId, OrderDate, TotalAmount, PaymentMethod, OrderStatus, Note)
                                          OUTPUT INSERTED.Id
                                          VALUES (@OrderCode, @UserId, @CustomerId, GETDATE(), @TotalAmount, @PaymentMethod, @OrderStatus, @Note)";
                using var cmdOrder = new SqlCommand(sqlOrder, connection, transaction);
                cmdOrder.Parameters.AddWithValue("@OrderCode", order.OrderCode);
                cmdOrder.Parameters.AddWithValue("@UserId", order.UserId);
                cmdOrder.Parameters.AddWithValue("@CustomerId", order.CustomerId ?? (object)DBNull.Value);
                cmdOrder.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                cmdOrder.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod);
                cmdOrder.Parameters.AddWithValue("@OrderStatus", order.OrderStatus);
                cmdOrder.Parameters.AddWithValue("@Note", order.Note ?? (object)DBNull.Value);

                int orderId = (int)cmdOrder.ExecuteScalar();
                order.Id = orderId;

                // 2. Insert Details & Update Inventory
                const string sqlDetail = @"INSERT INTO SalesOrderDetails (SalesOrderId, BookId, Quantity, UnitPrice)
                                           VALUES (@SalesOrderId, @BookId, @Quantity, @UnitPrice)";
                
                const string sqlUpdateInventory = "UPDATE Books SET Quantity = Quantity - @Quantity WHERE Id = @BookId";

                const string sqlInventoryLog = @"INSERT INTO InventoryTransactions (BookId, TransactionType, Quantity, ReferenceId, UserId, TransactionDate)
                                                 VALUES (@BookId, 'Sale', -@Quantity, @ReferenceId, @UserId, GETDATE())";

                foreach (var detail in details)
                {
                    // Insert Detail
                    using var cmdDetail = new SqlCommand(sqlDetail, connection, transaction);
                    cmdDetail.Parameters.AddWithValue("@SalesOrderId", orderId);
                    cmdDetail.Parameters.AddWithValue("@BookId", detail.BookId);
                    cmdDetail.Parameters.AddWithValue("@Quantity", detail.Quantity);
                    cmdDetail.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
                    cmdDetail.ExecuteNonQuery();

                    // Note: The database has triggers (TR_SalesOrderDetails_AfterInsert) 
                    // which might automatically update the inventory and log the transaction.
                    // However, if the triggers do it, we don't need to do it here manually.
                    // Let's assume we do it manually if triggers were dropped, but the script had triggers.
                    // Assuming the triggers from BookStoreDB_clean_reset.sql are active, we ONLY insert details.
                    // The trigger TR_SalesOrderDetails_AfterInsert will handle Books.Quantity and InventoryTransactions.
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
