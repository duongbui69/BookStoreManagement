USE [BookStoreDB];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReturnReceipts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ReturnReceipts](
        [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ReturnCode] [nvarchar](50) NOT NULL UNIQUE,
        [SalesOrderId] [int] NULL,
        [StoreId] [int] NOT NULL,
        [CustomerId] [int] NULL,
        [UserId] [int] NOT NULL,
        [ReturnDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [TotalRefundAmount] [decimal](18, 2) NOT NULL DEFAULT 0,
        [Note] [nvarchar](max) NULL,
        [ReturnStatus] [nvarchar](50) NOT NULL DEFAULT 'Chờ xử lý'
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReturnReceiptDetails]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ReturnReceiptDetails](
        [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ReturnReceiptId] [int] NOT NULL FOREIGN KEY REFERENCES [dbo].[ReturnReceipts](Id) ON DELETE CASCADE,
        [BookId] [int] NOT NULL,
        [Quantity] [int] NOT NULL,
        [UnitPrice] [decimal](18, 2) NOT NULL,
        [RefundAmount] [decimal](18, 2) NOT NULL,
        [ReturnReason] [nvarchar](max) NULL,
        [IsRestock] [bit] NOT NULL DEFAULT 1
    );
END
GO

IF OBJECT_ID('vw_ReturnReceiptList', 'V') IS NOT NULL
    DROP VIEW vw_ReturnReceiptList;
GO

CREATE VIEW vw_ReturnReceiptList AS
SELECT 
    r.Id,
    r.ReturnCode,
    r.SalesOrderId,
    so.OrderCode,
    r.StoreId,
    s.StoreName,
    r.CustomerId,
    ISNULL(c.FullName, 'Khách vãng lai') AS CustomerName,
    r.UserId AS StaffId,
    u.FullName AS StaffName,
    r.ReturnDate,
    r.TotalRefundAmount,
    r.Note,
    r.ReturnStatus
FROM 
    ReturnReceipts r
LEFT JOIN SalesOrders so ON r.SalesOrderId = so.Id
LEFT JOIN Stores s ON r.StoreId = s.Id
LEFT JOIN Customers c ON r.CustomerId = c.Id
LEFT JOIN Users u ON r.UserId = u.Id;
GO

IF OBJECT_ID('vw_ReturnReceiptDetailFull', 'V') IS NOT NULL
    DROP VIEW vw_ReturnReceiptDetailFull;
GO

CREATE VIEW vw_ReturnReceiptDetailFull AS
SELECT 
    rd.ReturnReceiptId,
    r.ReturnCode,
    r.ReturnDate,
    r.StoreId,
    s.StoreName,
    r.CustomerId,
    ISNULL(c.FullName, 'Khách vãng lai') AS CustomerName,
    r.UserId AS StaffId,
    u.FullName AS StaffName,
    rd.BookId,
    b.BookCode,
    b.Title,
    rd.Quantity,
    rd.UnitPrice,
    rd.RefundAmount,
    rd.ReturnReason,
    rd.IsRestock,
    r.TotalRefundAmount,
    r.Note,
    r.ReturnStatus
FROM 
    ReturnReceiptDetails rd
INNER JOIN ReturnReceipts r ON rd.ReturnReceiptId = r.Id
LEFT JOIN Books b ON rd.BookId = b.Id
LEFT JOIN Stores s ON r.StoreId = s.Id
LEFT JOIN Customers c ON r.CustomerId = c.Id
LEFT JOIN Users u ON r.UserId = u.Id;
GO
