-- Tắt kiểm tra khóa ngoại
EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'StoreBookInventories')
BEGIN
    CREATE TABLE StoreBookInventories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StoreId INT NOT NULL FOREIGN KEY REFERENCES Stores(Id),
        BookId INT NOT NULL FOREIGN KEY REFERENCES Books(Id),
        Quantity INT NOT NULL DEFAULT 0,
        MinStock INT NOT NULL DEFAULT 5,
        ImportPrice DECIMAL(18,2) NULL,
        SellingPrice DECIMAL(18,2) NOT NULL,
        ShelfLocation NVARCHAR(50) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        UpdatedAt DATETIME2 NULL
    );
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ExportReceipts')
BEGIN
    CREATE TABLE ExportReceipts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        StoreId INT NOT NULL FOREIGN KEY REFERENCES Stores(Id),
        ReceiptCode NVARCHAR(50) NOT NULL UNIQUE,
        CreatedById INT NULL FOREIGN KEY REFERENCES Users(Id),
        Reason NVARCHAR(255) NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Hoàn thành',
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ExportReceiptDetails')
BEGIN
    CREATE TABLE ExportReceiptDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ExportReceiptId INT NOT NULL FOREIGN KEY REFERENCES ExportReceipts(Id) ON DELETE CASCADE,
        BookId INT NOT NULL FOREIGN KEY REFERENCES Books(Id),
        Quantity INT NOT NULL
    );
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'VoucherTypes')
BEGIN
    CREATE TABLE VoucherTypes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Code NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(500) NULL,
        GroupType NVARCHAR(50) NOT NULL DEFAULT 'Thu',
        Status BIT NOT NULL DEFAULT 1
    );
END

-- Create view
EXEC('
CREATE OR ALTER VIEW vw_StoreBookInventory AS
SELECT 
    sbi.Id,
    sbi.StoreId,
    s.StoreCode,
    s.StoreName,
    sbi.BookId,
    b.BookCode,
    b.ISBN,
    b.Title,
    c.CategoryName,
    a.AuthorName,
    p.PublisherName,
    sbi.Quantity,
    sbi.MinStock,
    sbi.ImportPrice,
    sbi.SellingPrice,
    sbi.ShelfLocation,
    CASE 
        WHEN sbi.Quantity = 0 THEN N''Hết hàng''
        WHEN sbi.Quantity <= sbi.MinStock THEN N''Sắp hết''
        ELSE N''Còn hàng''
    END AS StockStatus,
    sbi.IsActive,
    sbi.CreatedAt,
    sbi.UpdatedAt
FROM StoreBookInventories sbi
JOIN Stores s ON sbi.StoreId = s.Id
JOIN Books b ON sbi.BookId = b.Id
LEFT JOIN Categories c ON b.CategoryId = c.Id
LEFT JOIN Authors a ON b.AuthorId = a.Id
LEFT JOIN Publishers p ON b.PublisherId = p.Id;
');

-- Bật lại kiểm tra khóa ngoại
EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
