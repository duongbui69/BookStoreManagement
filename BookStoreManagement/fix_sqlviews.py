import os
import re

repo_path = r"c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreDB.sql"
if os.path.exists(repo_path):
    with open(repo_path, 'r', encoding='utf-8') as f:
        repo_content = f.read()

    # SalesOrderList
    old_list_sql = """CREATE VIEW [dbo].[vw_SalesOrderList]
AS
SELECT
    so.Id,
    so.OrderCode,
    so.OrderDate,
    u.Id AS StaffId,
    u.FullName AS StaffName,
    c.Id AS CustomerId,
    ISNULL(c.FullName, N'Khach le') AS CustomerName,
    so.TotalAmount,
    so.PaymentMethod,
    so.OrderStatus,
    so.Note
FROM SalesOrders so
JOIN Users u ON so.UserId = u.Id
LEFT JOIN Customers c ON so.CustomerId = c.Id;"""
    new_list_sql = """CREATE VIEW [dbo].[vw_SalesOrderList]
AS
SELECT
    so.Id,
    so.OrderCode,
    so.OrderDate,
    so.StoreId,
    st.StoreName,
    u.Id AS StaffId,
    u.FullName AS StaffName,
    c.Id AS CustomerId,
    ISNULL(c.FullName, N'Khach le') AS CustomerName,
    so.TotalAmount,
    so.PaymentMethod,
    so.OrderStatus,
    so.Note
FROM SalesOrders so
JOIN Stores st ON so.StoreId = st.Id
JOIN Users u ON so.UserId = u.Id
LEFT JOIN Customers c ON so.CustomerId = c.Id;"""
    repo_content = repo_content.replace(old_list_sql, new_list_sql)

    # SalesOrderDetailFull
    old_detail_sql = """CREATE VIEW [dbo].[vw_SalesOrderDetailFull]
AS
SELECT 
    so.Id AS SalesOrderId,
    so.OrderCode,
    so.OrderDate,

    u.Id AS StaffId,
    u.FullName AS StaffName,

    c.Id AS CustomerId,
    c.FullName AS CustomerName,

    b.Id AS BookId,
    b.BookCode,
    b.Title,

    sod.Quantity,
    sod.UnitPrice,
    sod.DiscountAmount,
    sod.LineTotal,

    so.TotalAmount,
    so.PaymentMethod,
    so.OrderStatus,
    so.Note
FROM SalesOrderDetails sod
JOIN SalesOrders so ON sod.SalesOrderId = so.Id
JOIN Books b ON sod.BookId = b.Id
JOIN Users u ON so.UserId = u.Id
LEFT JOIN Customers c ON so.CustomerId = c.Id;"""
    new_detail_sql = """CREATE VIEW [dbo].[vw_SalesOrderDetailFull]
AS
SELECT 
    so.Id AS SalesOrderId,
    so.OrderCode,
    so.OrderDate,
    so.StoreId,
    st.StoreName,
    u.Id AS StaffId,
    u.FullName AS StaffName,

    c.Id AS CustomerId,
    c.FullName AS CustomerName,

    b.Id AS BookId,
    b.BookCode,
    b.Title,

    sod.Quantity,
    sod.UnitPrice,
    sod.DiscountAmount,
    sod.LineTotal,

    so.TotalAmount,
    so.PaymentMethod,
    so.OrderStatus,
    so.Note
FROM SalesOrderDetails sod
JOIN SalesOrders so ON sod.SalesOrderId = so.Id
JOIN Stores st ON so.StoreId = st.Id
JOIN Books b ON sod.BookId = b.Id
JOIN Users u ON so.UserId = u.Id
LEFT JOIN Customers c ON so.CustomerId = c.Id;"""
    repo_content = repo_content.replace(old_detail_sql, new_detail_sql)

    with open(repo_path, 'w', encoding='utf-8') as f:
        f.write(repo_content)
    print("Updated BookStoreDB.sql")
else:
    print("BookStoreDB.sql not found")
