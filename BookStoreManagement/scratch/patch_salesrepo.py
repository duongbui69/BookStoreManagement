import re
file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Repositories/SalesOrderRepository.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

# Replace GetByDateRange
old_sync = '''        public List<SalesOrderListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            var orders = new List<SalesOrderListViewModel>();
            string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE OrderDate >= @FromDate
                  AND OrderDate < DATEADD(DAY, 1, @ToDate)
            ";
            // if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY OrderDate DESC;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@FromDate", fromDate.Date);
                AddParameter(parameters, "@ToDate", toDate.Date);
                // if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }'''

new_sync = '''        public List<SalesOrderListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            var orders = new List<SalesOrderListViewModel>();
            string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE OrderDate >= @FromDate
                  AND OrderDate <= @ToDate
            ";
            // if (storeId.HasValue) sql += " AND StoreId = @StoreId";
            sql += " ORDER BY OrderDate DESC;";

            return ExecuteQuery(command =>
            {
                using var reader = command.ExecuteReader();
                while (reader.Read()) orders.Add(MapOrderList(reader));
                return orders;
            }, sql, parameters =>
            {
                AddParameter(parameters, "@FromDate", fromDate);
                AddParameter(parameters, "@ToDate", toDate);
                // if (storeId.HasValue) AddParameter(parameters, "@StoreId", storeId.Value);
            });
        }'''
text = text.replace(old_sync, new_sync)

old_async = '''        public async Task<List<SalesOrderListViewModel>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE OrderDate >= @FromDate
                  AND OrderDate < DATEADD(DAY, 1, @ToDate)
            ";
            sql += " ORDER BY OrderDate DESC;";
            var items = await QueryAsync<SalesOrderListViewModel>(sql, new { FromDate = fromDate.Date, ToDate = toDate.Date });
            return System.Linq.Enumerable.ToList(items);
        }'''

new_async = '''        public async Task<List<SalesOrderListViewModel>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            string sql = @"
                SELECT * FROM vw_SalesOrderList
                WHERE OrderDate >= @FromDate
                  AND OrderDate <= @ToDate
            ";
            sql += " ORDER BY OrderDate DESC;";
            var items = await QueryAsync<SalesOrderListViewModel>(sql, new { FromDate = fromDate, ToDate = toDate });
            return System.Linq.Enumerable.ToList(items);
        }'''
text = text.replace(old_async, new_async)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
