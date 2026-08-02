import os

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Repositories\SalesOrderRepository.cs"
with open(repo_path, 'r', encoding='utf-8') as f:
    repo_content = f.read()

# Replace async CreateOrderAsync SQL
old_async_sql = """
                    INSERT INTO SalesOrders (OrderCode, UserId, CustomerId, PaymentMethod, OrderStatus, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderCode, @UserId, @CustomerId, @PaymentMethod, @OrderStatus, @Note);
"""
new_async_sql = """
                    INSERT INTO SalesOrders (OrderCode, UserId, CustomerId, StoreId, TotalAmount, PaymentMethod, OrderStatus, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderCode, @UserId, @CustomerId, @StoreId, @TotalAmount, @PaymentMethod, @OrderStatus, @Note);
"""
repo_content = repo_content.replace(old_async_sql, new_async_sql)

# Replace async ExecuteScalarAsync
old_async_params = """
                    orderId = await connection.ExecuteScalarAsync<int>(insertOrderSql, new
                    {
                        OrderCode = order.OrderCode,
                        UserId = order.UserId,
                        CustomerId = order.CustomerId,
                        PaymentMethod = order.PaymentMethod,
                        OrderStatus = order.OrderStatus,
                        Note = order.Note
                    }, transaction);
"""
new_async_params = """
                    orderId = await connection.ExecuteScalarAsync<int>(insertOrderSql, new
                    {
                        OrderCode = order.OrderCode,
                        UserId = order.UserId,
                        CustomerId = order.CustomerId,
                        StoreId = order.StoreId,
                        TotalAmount = order.TotalAmount,
                        PaymentMethod = order.PaymentMethod,
                        OrderStatus = order.OrderStatus,
                        Note = order.Note
                    }, transaction);
"""
repo_content = repo_content.replace(old_async_params, new_async_params)


# Replace sync CreateOrder SQL
old_sync_sql = """
                    INSERT INTO SalesOrders (OrderCode, UserId, CustomerId, PaymentMethod, OrderStatus, Note)
                    OUTPUT INSERTED.Id
                    VALUES (@OrderCode, @UserId, @CustomerId, @PaymentMethod, @OrderStatus, @Note);
"""
repo_content = repo_content.replace(old_sync_sql, new_async_sql)

# Replace sync AddParameter
old_sync_params = """
                AddParameter(orderCommand, "@OrderCode", order.OrderCode);
                AddParameter(orderCommand, "@UserId", order.UserId);
                AddParameter(orderCommand, "@CustomerId", order.CustomerId);
                AddParameter(orderCommand, "@PaymentMethod", order.PaymentMethod);
                AddParameter(orderCommand, "@OrderStatus", order.OrderStatus);
                AddParameter(orderCommand, "@Note", order.Note);
"""
new_sync_params = """
                AddParameter(orderCommand, "@OrderCode", order.OrderCode);
                AddParameter(orderCommand, "@UserId", order.UserId);
                AddParameter(orderCommand, "@CustomerId", order.CustomerId);
                AddParameter(orderCommand, "@StoreId", order.StoreId);
                AddParameter(orderCommand, "@TotalAmount", order.TotalAmount);
                AddParameter(orderCommand, "@PaymentMethod", order.PaymentMethod);
                AddParameter(orderCommand, "@OrderStatus", order.OrderStatus);
                AddParameter(orderCommand, "@Note", order.Note);
"""
repo_content = repo_content.replace(old_sync_params, new_sync_params)

with open(repo_path, 'w', encoding='utf-8') as f:
    f.write(repo_content)
print("Updated SalesOrderRepository.cs")


srv_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Services\SalesOrderService.cs"
with open(srv_path, 'r', encoding='utf-8') as f:
    srv_content = f.read()

old_order_init = """
            var order = new SalesOrder
            {
                OrderCode = orderCode,
                StoreId = resolvedStoreId,
                UserId = CurrentSession.UserId,
                CustomerId = customerId,
                PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? AppConstants.PaymentMethods.Cash : paymentMethod.Trim(),
                OrderStatus = AppConstants.OrderStatuses.Completed,
                Note = TrimNullable(note)
            };
"""
new_order_init = """
            decimal totalAmount = 0;
            if (details != null)
            {
                foreach (var detail in details) totalAmount += detail.LineTotal;
            }

            var order = new SalesOrder
            {
                OrderCode = orderCode,
                StoreId = resolvedStoreId,
                UserId = CurrentSession.UserId,
                CustomerId = customerId,
                TotalAmount = totalAmount,
                PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? AppConstants.PaymentMethods.Cash : paymentMethod.Trim(),
                OrderStatus = AppConstants.OrderStatuses.Completed,
                Note = TrimNullable(note)
            };
"""
srv_content = srv_content.replace(old_order_init, new_order_init)
with open(srv_path, 'w', encoding='utf-8') as f:
    f.write(srv_content)
print("Updated SalesOrderService.cs")
