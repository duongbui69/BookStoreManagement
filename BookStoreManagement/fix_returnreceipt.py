import os

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Repositories\ReturnReceiptRepository.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

# Replace async CreateReturnAsync SQL
old_async_sql = """
                    INSERT INTO ReturnReceipts (ReturnCode, SalesOrderId, StoreId, CustomerId, UserId, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@ReturnCode, @SalesOrderId, @StoreId, @CustomerId, @UserId, @Note);
"""
new_async_sql = """
                    INSERT INTO ReturnReceipts (ReturnCode, SalesOrderId, StoreId, CustomerId, UserId, TotalRefundAmount, ReturnStatus, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@ReturnCode, @SalesOrderId, @StoreId, @CustomerId, @UserId, @TotalRefundAmount, @ReturnStatus, @Note);
"""
repo_content = repo_content.replace(old_async_sql, new_async_sql)

# Replace async ExecuteScalarAsync
old_async_params = """
                returnReceiptId = await Dapper.SqlMapper.ExecuteScalarAsync<int>(connection, insertReceiptSql, new {
                    receipt.ReturnCode,
                    receipt.SalesOrderId,
                    receipt.StoreId,
                    receipt.CustomerId,
                    receipt.UserId,
                    receipt.Note
                }, transaction);
"""
new_async_params = """
                returnReceiptId = await Dapper.SqlMapper.ExecuteScalarAsync<int>(connection, insertReceiptSql, new {
                    receipt.ReturnCode,
                    receipt.SalesOrderId,
                    receipt.StoreId,
                    receipt.CustomerId,
                    receipt.UserId,
                    receipt.TotalRefundAmount,
                    receipt.ReturnStatus,
                    receipt.Note
                }, transaction);
"""
repo_content = repo_content.replace(old_async_params, new_async_params)


# Replace sync CreateReturn SQL
old_sync_sql = """
                    INSERT INTO ReturnReceipts (ReturnCode, SalesOrderId, StoreId, CustomerId, UserId, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@ReturnCode, @SalesOrderId, @StoreId, @CustomerId, @UserId, @Note);
"""
repo_content = repo_content.replace(old_sync_sql, new_async_sql)

# Replace sync AddParameter
old_sync_params = """
                AddParameter(receiptCommand, "@ReturnCode", receipt.ReturnCode);
                AddParameter(receiptCommand, "@SalesOrderId", receipt.SalesOrderId);
                AddParameter(receiptCommand, "@StoreId", receipt.StoreId);
                AddParameter(receiptCommand, "@CustomerId", receipt.CustomerId);
                AddParameter(receiptCommand, "@UserId", receipt.UserId);
                AddParameter(receiptCommand, "@Note", receipt.Note);
"""
new_sync_params = """
                AddParameter(receiptCommand, "@ReturnCode", receipt.ReturnCode);
                AddParameter(receiptCommand, "@SalesOrderId", receipt.SalesOrderId);
                AddParameter(receiptCommand, "@StoreId", receipt.StoreId);
                AddParameter(receiptCommand, "@CustomerId", receipt.CustomerId);
                AddParameter(receiptCommand, "@UserId", receipt.UserId);
                AddParameter(receiptCommand, "@TotalRefundAmount", receipt.TotalRefundAmount);
                AddParameter(receiptCommand, "@ReturnStatus", receipt.ReturnStatus);
                AddParameter(receiptCommand, "@Note", receipt.Note);
"""
repo_content = repo_content.replace(old_sync_params, new_sync_params)

with open(repo_path, 'w', encoding='utf-8') as f:
    f.write(repo_content)
print("Updated ReturnReceiptRepository.cs")


srv_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Services\ReturnReceiptService.cs"
with open(srv_path, 'r', encoding='utf-8') as f:
    srv_content = f.read()

old_receipt_init_sync = """
            var receipt = new ReturnReceipt
            {
                ReturnCode = code,
                SalesOrderId = salesOrderId,
                StoreId = resolvedStoreId,
                CustomerId = customerId,
                UserId = CurrentSession.UserId,
                Note = TrimNullable(note)
            };
"""
new_receipt_init_sync = """
            decimal totalRefund = 0;
            if (details != null)
            {
                foreach (var detail in details) totalRefund += detail.Quantity * detail.UnitPrice;
            }

            var receipt = new ReturnReceipt
            {
                ReturnCode = code,
                SalesOrderId = salesOrderId,
                StoreId = resolvedStoreId,
                CustomerId = customerId,
                UserId = CurrentSession.UserId,
                TotalRefundAmount = totalRefund,
                ReturnStatus = "Processing",
                Note = TrimNullable(note)
            };
"""
srv_content = srv_content.replace(old_receipt_init_sync, new_receipt_init_sync)
with open(srv_path, 'w', encoding='utf-8') as f:
    f.write(srv_content)
print("Updated ReturnReceiptService.cs")
