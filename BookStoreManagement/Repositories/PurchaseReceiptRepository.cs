using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookStoreManagement.Models;
using BookStoreManagement.Database;

namespace BookStoreManagement.Repositories
{
    public class PurchaseReceiptRepository
    {
        public void CreatePurchaseReceipt(PurchaseReceipt receipt, List<PurchaseReceiptDetail> details)
        {
            using var connection = DbConnectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Insert PurchaseReceipt
                const string sqlReceipt = @"INSERT INTO PurchaseReceipts (ReceiptCode, UserId, SupplierId, ReceiptDate, TotalAmount, Note)
                                            OUTPUT INSERTED.Id
                                            VALUES (@ReceiptCode, @UserId, @SupplierId, GETDATE(), @TotalAmount, @Note)";
                using var cmdReceipt = new SqlCommand(sqlReceipt, connection, transaction);
                cmdReceipt.Parameters.AddWithValue("@ReceiptCode", receipt.ReceiptCode);
                cmdReceipt.Parameters.AddWithValue("@UserId", receipt.UserId);
                cmdReceipt.Parameters.AddWithValue("@SupplierId", receipt.SupplierId);
                cmdReceipt.Parameters.AddWithValue("@TotalAmount", receipt.TotalAmount);
                cmdReceipt.Parameters.AddWithValue("@Note", receipt.Note ?? (object)DBNull.Value);

                int receiptId = (int)cmdReceipt.ExecuteScalar();
                receipt.Id = receiptId;

                // 2. Insert Details
                // Trigger TR_PurchaseReceiptDetails_AfterInsert will handle Books.Quantity and InventoryTransactions update.
                const string sqlDetail = @"INSERT INTO PurchaseReceiptDetails (PurchaseReceiptId, BookId, Quantity, ImportPrice)
                                           VALUES (@PurchaseReceiptId, @BookId, @Quantity, @ImportPrice)";
                
                foreach (var detail in details)
                {
                    using var cmdDetail = new SqlCommand(sqlDetail, connection, transaction);
                    cmdDetail.Parameters.AddWithValue("@PurchaseReceiptId", receiptId);
                    cmdDetail.Parameters.AddWithValue("@BookId", detail.BookId);
                    cmdDetail.Parameters.AddWithValue("@Quantity", detail.Quantity);
                    cmdDetail.Parameters.AddWithValue("@ImportPrice", detail.ImportPrice);
                    cmdDetail.ExecuteNonQuery();
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
