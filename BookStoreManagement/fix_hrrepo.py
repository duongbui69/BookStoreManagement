import os

filepath = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Repositories\HRRepository.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix the broken SQL query in GetSalaryReportAsync
content = content.replace("ISNULL(s.StoreName, N'T?t c? chi nhnh') as StoreName,", "N'Tất cả chi nhánh' as StoreName,")
content = content.replace("LEFT JOIN Stores s ON u.StoreId = s.Id", "")
content = content.replace("GROUP BY u.Id, u.UserCode, u.FullName, s.StoreName, u.HourlyRate", "GROUP BY u.Id, u.FullName, u.HourlyRate")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
