/* ============================================================
   RESET DATABASE - Chay file nay se xoa BookStoreDB cu va tao lai tu dau
   ============================================================ */
USE master;
GO

IF DB_ID(N'BookStoreDB') IS NOT NULL
BEGIN
    ALTER DATABASE BookStoreDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BookStoreDB;
END
GO

/* ============================================================
   BookStoreDB.sql
   Project: C# WinForms - Quan ly cua hang ban sach Kim Dong
   Database: SQL Server
   Chuc nang ho tro:
   - Quan ly tai khoan Admin / Staff
   - Quan ly sach, danh muc, tac gia, nha xuat ban, nha cung cap
   - Quan ly khach hang
   - Quan ly hoa don ban hang
   - Quan ly nhap sach
   - Quan ly kho, lich su kho
   - Quan ly doanh thu, sach ban chay, sach sap het hang
   ============================================================ */

/* ============================================================
   0. CREATE DATABASE
   ============================================================ */

USE master;
GO

IF DB_ID(N'BookStoreDB') IS NULL
BEGIN
    CREATE DATABASE BookStoreDB;
END
GO

USE BookStoreDB;
GO

/* ============================================================
   1. TABLES
   ============================================================ */

-- =========================
-- 1.1 ROLES
-- =========================
IF OBJECT_ID(N'Roles', N'U') IS NOT NULL DROP TABLE Roles;
GO

CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(255),

    CONSTRAINT CK_Roles_RoleName
    CHECK (RoleName IN ('Admin', 'Staff'))
);
GO

-- =========================
-- 1.2 USERS / ACCOUNTS
-- =========================
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoleId INT NOT NULL,

    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,

    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Users_Roles
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
GO

-- =========================
-- 1.3 CATEGORIES
-- =========================
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.4 AUTHORS
-- =========================
CREATE TABLE Authors (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    AuthorName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.5 PUBLISHERS
-- =========================
CREATE TABLE Publishers (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    PublisherName NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.6 SUPPLIERS
-- =========================
CREATE TABLE Suppliers (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    SupplierName NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.7 BOOKS
-- =========================
CREATE TABLE Books (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    BookCode NVARCHAR(50) NOT NULL UNIQUE,
    ISBN NVARCHAR(30) NULL,

    Title NVARCHAR(200) NOT NULL,

    CategoryId INT NOT NULL,
    AuthorId INT NULL,
    PublisherId INT NULL,

    SellingPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    MinStock INT NOT NULL DEFAULT 5,

    Description NVARCHAR(500),
    ImagePath NVARCHAR(255) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Books_Categories
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id),

    CONSTRAINT FK_Books_Authors
    FOREIGN KEY (AuthorId) REFERENCES Authors(Id),

    CONSTRAINT FK_Books_Publishers
    FOREIGN KEY (PublisherId) REFERENCES Publishers(Id),

    CONSTRAINT CK_Books_SellingPrice
    CHECK (SellingPrice >= 0),

    CONSTRAINT CK_Books_Quantity
    CHECK (Quantity >= 0),

    CONSTRAINT CK_Books_MinStock
    CHECK (MinStock >= 0)
);
GO

-- ISBN co the NULL nhieu dong, nhung neu co ISBN thi khong duoc trung
CREATE UNIQUE INDEX UX_Books_ISBN_NotNull
ON Books(ISBN)
WHERE ISBN IS NOT NULL;
GO

-- =========================
-- 1.8 CUSTOMERS
-- =========================
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),

    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.9 SALES ORDERS / INVOICES
-- =========================
CREATE TABLE SalesOrders (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    OrderCode NVARCHAR(50) NOT NULL UNIQUE,

    UserId INT NOT NULL,
    CustomerId INT NULL,

    OrderDate DATETIME2 DEFAULT SYSDATETIME(),

    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,

    PaymentMethod NVARCHAR(30) NOT NULL DEFAULT 'Cash',
    OrderStatus NVARCHAR(30) NOT NULL DEFAULT 'Completed',

    Note NVARCHAR(255),

    CONSTRAINT FK_SalesOrders_Users
    FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT FK_SalesOrders_Customers
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),

    CONSTRAINT CK_SalesOrders_TotalAmount
    CHECK (TotalAmount >= 0),

    CONSTRAINT CK_SalesOrders_PaymentMethod
    CHECK (PaymentMethod IN ('Cash', 'Banking', 'Card')),

    CONSTRAINT CK_SalesOrders_OrderStatus
    CHECK (OrderStatus IN ('Completed', 'Cancelled'))
);
GO

-- =========================
-- 1.10 SALES ORDER DETAILS
-- =========================
CREATE TABLE SalesOrderDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    SalesOrderId INT NOT NULL,
    BookId INT NOT NULL,

    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,

    LineTotal AS ((Quantity * UnitPrice) - DiscountAmount) PERSISTED,

    CONSTRAINT FK_SalesOrderDetails_SalesOrders
    FOREIGN KEY (SalesOrderId) REFERENCES SalesOrders(Id),

    CONSTRAINT FK_SalesOrderDetails_Books
    FOREIGN KEY (BookId) REFERENCES Books(Id),

    CONSTRAINT CK_SalesOrderDetails_Quantity
    CHECK (Quantity > 0),

    CONSTRAINT CK_SalesOrderDetails_UnitPrice
    CHECK (UnitPrice >= 0),

    CONSTRAINT CK_SalesOrderDetails_DiscountAmount
    CHECK (DiscountAmount >= 0),

    CONSTRAINT CK_SalesOrderDetails_LineTotal
    CHECK ((Quantity * UnitPrice) - DiscountAmount >= 0)
);
GO

-- =========================
-- 1.11 PURCHASE RECEIPTS
-- =========================
CREATE TABLE PurchaseReceipts (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    ReceiptCode NVARCHAR(50) NOT NULL UNIQUE,

    SupplierId INT NOT NULL,
    UserId INT NOT NULL,

    ImportDate DATETIME2 DEFAULT SYSDATETIME(),

    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,

    Note NVARCHAR(255),

    CONSTRAINT FK_PurchaseReceipts_Suppliers
    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),

    CONSTRAINT FK_PurchaseReceipts_Users
    FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT CK_PurchaseReceipts_TotalAmount
    CHECK (TotalAmount >= 0)
);
GO

-- =========================
-- 1.12 PURCHASE RECEIPT DETAILS
-- =========================
CREATE TABLE PurchaseReceiptDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    PurchaseReceiptId INT NOT NULL,
    BookId INT NOT NULL,

    Quantity INT NOT NULL,
    ImportPrice DECIMAL(18,2) NOT NULL,

    LineTotal AS (Quantity * ImportPrice) PERSISTED,

    CONSTRAINT FK_PurchaseReceiptDetails_PurchaseReceipts
    FOREIGN KEY (PurchaseReceiptId) REFERENCES PurchaseReceipts(Id),

    CONSTRAINT FK_PurchaseReceiptDetails_Books
    FOREIGN KEY (BookId) REFERENCES Books(Id),

    CONSTRAINT CK_PurchaseReceiptDetails_Quantity
    CHECK (Quantity > 0),

    CONSTRAINT CK_PurchaseReceiptDetails_ImportPrice
    CHECK (ImportPrice >= 0)
);
GO

-- =========================
-- 1.13 INVENTORY TRANSACTIONS
-- =========================
CREATE TABLE InventoryTransactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    BookId INT NOT NULL,
    UserId INT NULL,

    TransactionType NVARCHAR(30) NOT NULL,
    QuantityChange INT NOT NULL,

    ReferenceType NVARCHAR(30) NULL,
    ReferenceId INT NULL,

    Note NVARCHAR(255),

    CreatedAt DATETIME2 DEFAULT SYSDATETIME(),

    CONSTRAINT FK_InventoryTransactions_Books
    FOREIGN KEY (BookId) REFERENCES Books(Id),

    CONSTRAINT FK_InventoryTransactions_Users
    FOREIGN KEY (UserId) REFERENCES Users(Id),

    CONSTRAINT CK_InventoryTransactions_Type
    CHECK (TransactionType IN ('Import', 'Sale', 'Adjustment', 'CancelSale'))
);
GO

/* ============================================================
   2. TRIGGERS
   ============================================================ */

-- =========================
-- 2.1 TU TRU KHO KHI BAN SACH
-- =========================
CREATE TRIGGER TR_SalesOrderDetails_AfterInsert
ON SalesOrderDetails
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiem tra ton kho theo tung sach
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT BookId, SUM(Quantity) AS TotalQuantity
            FROM inserted
            GROUP BY BookId
        ) i
        JOIN Books b ON i.BookId = b.Id
        WHERE b.Quantity < i.TotalQuantity
    )
    BEGIN
        RAISERROR (N'So luong sach trong kho khong du de ban.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    -- Tru ton kho
    UPDATE b
    SET 
        b.Quantity = b.Quantity - i.TotalQuantity,
        b.UpdatedAt = SYSDATETIME()
    FROM Books b
    JOIN (
        SELECT BookId, SUM(Quantity) AS TotalQuantity
        FROM inserted
        GROUP BY BookId
    ) i ON b.Id = i.BookId;

    -- Cap nhat tong tien hoa don
    UPDATE so
    SET so.TotalAmount = x.TotalAmount
    FROM SalesOrders so
    JOIN (
        SELECT SalesOrderId, SUM(LineTotal) AS TotalAmount
        FROM SalesOrderDetails
        WHERE SalesOrderId IN (
            SELECT DISTINCT SalesOrderId FROM inserted
        )
        GROUP BY SalesOrderId
    ) x ON so.Id = x.SalesOrderId;

    -- Ghi lich su kho
    INSERT INTO InventoryTransactions (
        BookId,
        UserId,
        TransactionType,
        QuantityChange,
        ReferenceType,
        ReferenceId,
        Note
    )
    SELECT 
        i.BookId,
        so.UserId,
        'Sale',
        -i.Quantity,
        'SalesOrder',
        i.SalesOrderId,
        N'Ban sach'
    FROM inserted i
    JOIN SalesOrders so ON i.SalesOrderId = so.Id;
END;
GO

-- =========================
-- 2.2 TU CONG KHO KHI NHAP SACH
-- =========================
CREATE TRIGGER TR_PurchaseReceiptDetails_AfterInsert
ON PurchaseReceiptDetails
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Cong ton kho
    UPDATE b
    SET 
        b.Quantity = b.Quantity + i.TotalQuantity,
        b.UpdatedAt = SYSDATETIME()
    FROM Books b
    JOIN (
        SELECT BookId, SUM(Quantity) AS TotalQuantity
        FROM inserted
        GROUP BY BookId
    ) i ON b.Id = i.BookId;

    -- Cap nhat tong tien phieu nhap
    UPDATE pr
    SET pr.TotalAmount = x.TotalAmount
    FROM PurchaseReceipts pr
    JOIN (
        SELECT PurchaseReceiptId, SUM(LineTotal) AS TotalAmount
        FROM PurchaseReceiptDetails
        WHERE PurchaseReceiptId IN (
            SELECT DISTINCT PurchaseReceiptId FROM inserted
        )
        GROUP BY PurchaseReceiptId
    ) x ON pr.Id = x.PurchaseReceiptId;

    -- Ghi lich su kho
    INSERT INTO InventoryTransactions (
        BookId,
        UserId,
        TransactionType,
        QuantityChange,
        ReferenceType,
        ReferenceId,
        Note
    )
    SELECT 
        i.BookId,
        pr.UserId,
        'Import',
        i.Quantity,
        'PurchaseReceipt',
        i.PurchaseReceiptId,
        N'Nhap sach'
    FROM inserted i
    JOIN PurchaseReceipts pr ON i.PurchaseReceiptId = pr.Id;
END;
GO

-- =========================
-- 2.3 TU HOAN KHO KHI HUY HOA DON
-- Chi chay khi OrderStatus doi tu Completed sang Cancelled
-- =========================
CREATE TRIGGER TR_SalesOrders_AfterUpdate_Cancel
ON SalesOrders
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(OrderStatus)
    BEGIN
        -- Cong lai kho cho cac hoa don vua bi huy
        UPDATE b
        SET 
            b.Quantity = b.Quantity + x.TotalQuantity,
            b.UpdatedAt = SYSDATETIME()
        FROM Books b
        JOIN (
            SELECT sod.BookId, SUM(sod.Quantity) AS TotalQuantity
            FROM SalesOrderDetails sod
            JOIN inserted i ON sod.SalesOrderId = i.Id
            JOIN deleted d ON i.Id = d.Id
            WHERE i.OrderStatus = 'Cancelled'
              AND d.OrderStatus <> 'Cancelled'
            GROUP BY sod.BookId
        ) x ON b.Id = x.BookId;

        -- Ghi lich su hoan kho
        INSERT INTO InventoryTransactions (
            BookId,
            UserId,
            TransactionType,
            QuantityChange,
            ReferenceType,
            ReferenceId,
            Note
        )
        SELECT
            sod.BookId,
            i.UserId,
            'CancelSale',
            sod.Quantity,
            'SalesOrder',
            i.Id,
            N'Huy hoa don va hoan kho'
        FROM SalesOrderDetails sod
        JOIN inserted i ON sod.SalesOrderId = i.Id
        JOIN deleted d ON i.Id = d.Id
        WHERE i.OrderStatus = 'Cancelled'
          AND d.OrderStatus <> 'Cancelled';
    END
END;
GO

/* ============================================================
   3. VIEWS
   ============================================================ */

-- =========================
-- 3.1 DANH SACH SACH
-- =========================
CREATE VIEW vw_BookList
AS
SELECT 
    b.Id,
    b.BookCode,
    b.ISBN,
    b.Title,
    c.Id AS CategoryId,
    c.CategoryName,
    a.Id AS AuthorId,
    a.AuthorName,
    p.Id AS PublisherId,
    p.PublisherName,
    b.SellingPrice,
    b.Quantity,
    b.MinStock,
    CASE
        WHEN b.Quantity = 0 THEN N'Het hang'
        WHEN b.Quantity <= b.MinStock THEN N'Sap het'
        ELSE N'Con hang'
    END AS StockStatus,
    b.Description,
    b.ImagePath,
    b.IsActive,
    b.CreatedAt,
    b.UpdatedAt
FROM Books b
JOIN Categories c ON b.CategoryId = c.Id
LEFT JOIN Authors a ON b.AuthorId = a.Id
LEFT JOIN Publishers p ON b.PublisherId = p.Id;
GO

-- =========================
-- 3.2 CHI TIET HOA DON
-- =========================
CREATE VIEW vw_SalesOrderDetailFull
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
FROM SalesOrders so
JOIN Users u ON so.UserId = u.Id
LEFT JOIN Customers c ON so.CustomerId = c.Id
JOIN SalesOrderDetails sod ON so.Id = sod.SalesOrderId
JOIN Books b ON sod.BookId = b.Id;
GO

-- =========================
-- 3.3 SACH SAP HET HANG
-- =========================
CREATE VIEW vw_LowStockBooks
AS
SELECT 
    Id,
    BookCode,
    Title,
    Quantity,
    MinStock
FROM Books
WHERE Quantity <= MinStock
AND IsActive = 1;
GO

-- =========================
-- 3.4 DOANH THU THEO NGAY
-- =========================
CREATE VIEW vw_DailyRevenue
AS
SELECT
    CAST(OrderDate AS DATE) AS RevenueDate,
    COUNT(*) AS TotalOrders,
    SUM(TotalAmount) AS TotalRevenue
FROM SalesOrders
WHERE OrderStatus = 'Completed'
GROUP BY CAST(OrderDate AS DATE);
GO

-- =========================
-- 3.5 SACH BAN CHAY
-- =========================
CREATE VIEW vw_TopSellingBooks
AS
SELECT
    b.Id AS BookId,
    b.BookCode,
    b.Title,
    SUM(sod.Quantity) AS TotalSold,
    SUM(sod.LineTotal) AS TotalRevenue
FROM SalesOrderDetails sod
JOIN SalesOrders so ON sod.SalesOrderId = so.Id
JOIN Books b ON sod.BookId = b.Id
WHERE so.OrderStatus = 'Completed'
GROUP BY b.Id, b.BookCode, b.Title;
GO

-- =========================
-- 3.6 LICH SU KHO
-- =========================
CREATE VIEW vw_InventoryHistory
AS
SELECT
    it.Id,
    b.Id AS BookId,
    b.BookCode,
    b.Title,
    u.Id AS StaffId,
    u.FullName AS StaffName,
    it.TransactionType,
    it.QuantityChange,
    it.ReferenceType,
    it.ReferenceId,
    it.Note,
    it.CreatedAt
FROM InventoryTransactions it
JOIN Books b ON it.BookId = b.Id
LEFT JOIN Users u ON it.UserId = u.Id;
GO

-- =========================
-- 3.7 DANH SACH HOA DON
-- =========================
CREATE VIEW vw_SalesOrderList
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
LEFT JOIN Customers c ON so.CustomerId = c.Id;
GO

-- =========================
-- 3.8 DANH SACH PHIEU NHAP
-- =========================
CREATE VIEW vw_PurchaseReceiptList
AS
SELECT
    pr.Id,
    pr.ReceiptCode,
    pr.ImportDate,
    s.Id AS SupplierId,
    s.SupplierName,
    u.Id AS StaffId,
    u.FullName AS StaffName,
    pr.TotalAmount,
    pr.Note
FROM PurchaseReceipts pr
JOIN Suppliers s ON pr.SupplierId = s.Id
JOIN Users u ON pr.UserId = u.Id;
GO

/* ============================================================
   4. STORED PROCEDURE CHINH KHO THU CONG
   ============================================================ */

CREATE PROCEDURE sp_AdjustBookStock
    @BookId INT,
    @UserId INT,
    @QuantityChange INT,
    @Note NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Books WHERE Id = @BookId)
    BEGIN
        RAISERROR (N'Sach khong ton tai.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Id = @UserId)
    BEGIN
        RAISERROR (N'Nguoi dung khong ton tai.', 16, 1);
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM Books
        WHERE Id = @BookId
          AND Quantity + @QuantityChange < 0
    )
    BEGIN
        RAISERROR (N'So luong dieu chinh lam ton kho bi am.', 16, 1);
        RETURN;
    END;

    UPDATE Books
    SET Quantity = Quantity + @QuantityChange,
        UpdatedAt = SYSDATETIME()
    WHERE Id = @BookId;

    INSERT INTO InventoryTransactions (
        BookId,
        UserId,
        TransactionType,
        QuantityChange,
        ReferenceType,
        ReferenceId,
        Note
    )
    VALUES (
        @BookId,
        @UserId,
        'Adjustment',
        @QuantityChange,
        'Manual',
        NULL,
        ISNULL(@Note, N'Dieu chinh ton kho thu cong')
    );
END;
GO

/* ============================================================
   5. INDEXES
   ============================================================ */

CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_BookCode ON Books(BookCode);
CREATE INDEX IX_Books_CategoryId ON Books(CategoryId);
CREATE INDEX IX_Books_AuthorId ON Books(AuthorId);
CREATE INDEX IX_Books_PublisherId ON Books(PublisherId);

CREATE INDEX IX_SalesOrders_OrderDate ON SalesOrders(OrderDate);
CREATE INDEX IX_SalesOrders_UserId ON SalesOrders(UserId);
CREATE INDEX IX_SalesOrders_CustomerId ON SalesOrders(CustomerId);

CREATE INDEX IX_SalesOrderDetails_BookId ON SalesOrderDetails(BookId);
CREATE INDEX IX_SalesOrderDetails_SalesOrderId ON SalesOrderDetails(SalesOrderId);

CREATE INDEX IX_PurchaseReceipts_ImportDate ON PurchaseReceipts(ImportDate);
CREATE INDEX IX_PurchaseReceipts_SupplierId ON PurchaseReceipts(SupplierId);

CREATE INDEX IX_PurchaseReceiptDetails_BookId ON PurchaseReceiptDetails(BookId);
CREATE INDEX IX_PurchaseReceiptDetails_PurchaseReceiptId ON PurchaseReceiptDetails(PurchaseReceiptId);

CREATE INDEX IX_InventoryTransactions_BookId ON InventoryTransactions(BookId);
CREATE INDEX IX_InventoryTransactions_CreatedAt ON InventoryTransactions(CreatedAt);
GO

/* ============================================================
   6. SEED DATA
   ============================================================ */

-- Roles
INSERT INTO Roles (RoleName, Description)
VALUES
('Admin', N'Quan tri vien he thong'),
('Staff', N'Nhan vien ban hang');
GO

-- Users
-- Mat khau demo dang de plain text: 123456
-- Khi lam that nen hash mat khau.
INSERT INTO Users (
    RoleId,
    Username,
    PasswordHash,
    FullName,
    Phone,
    Email,
    Address
)
VALUES
(1, 'admin', '123456', N'Quan tri vien', '0900000001', 'admin@bookstore.com', N'Ha Noi'),
(2, 'staff', '123456', N'Nhan vien ban hang', '0900000002', 'staff@bookstore.com', N'Ha Noi');
GO

-- Categories
INSERT INTO Categories (CategoryName, Description)
VALUES
(N'Truyen tranh', N'Sach truyen tranh thieu nhi'),
(N'Van hoc thieu nhi', N'Sach van hoc danh cho thieu nhi'),
(N'Sach giao duc', N'Sach hoc tap va giao duc'),
(N'Ky nang song', N'Sach phat trien ky nang'),
(N'Lich su', N'Sach lich su danh cho thieu nhi');
GO

-- Authors
INSERT INTO Authors (AuthorName, Description)
VALUES
(N'To Hoai', N'Tac gia van hoc Viet Nam'),
(N'Nguyen Nhat Anh', N'Tac gia truyen thieu nhi'),
(N'Vo Quang', N'Tac gia van hoc thieu nhi'),
(N'Nhieu tac gia', N'Nhieu tac gia khac nhau');
GO

-- Publishers
INSERT INTO Publishers (
    PublisherName,
    Phone,
    Email,
    Address
)
VALUES
(N'Nha xuat ban Kim Dong', '0240000000', 'kimdong@example.com', N'Ha Noi');
GO

-- Suppliers
INSERT INTO Suppliers (
    SupplierName,
    Phone,
    Email,
    Address
)
VALUES
(N'Cong ty phat hanh sach Kim Dong', '0241111111', 'supplierkimdong@example.com', N'Ha Noi');
GO

-- Books
INSERT INTO Books (
    BookCode,
    ISBN,
    Title,
    CategoryId,
    AuthorId,
    PublisherId,
    SellingPrice,
    Quantity,
    MinStock,
    Description
)
VALUES
('BOOK001', '978000000001', N'De Men Phieu Luu Ky', 2, 1, 1, 65000, 30, 5, N'Tac pham thieu nhi noi tieng'),
('BOOK002', '978000000002', N'Kinh Van Hoa', 2, 2, 1, 90000, 20, 5, N'Truyen dai danh cho thieu nhi'),
('BOOK003', '978000000003', N'Que Noi', 2, 3, 1, 70000, 15, 5, N'Tac pham van hoc thieu nhi'),
('BOOK004', '978000000004', N'Truyen Tranh Thieu Nhi Tap 1', 1, 4, 1, 45000, 50, 10, N'Truyen tranh cho thieu nhi'),
('BOOK005', '978000000005', N'Lich Su Viet Nam Bang Tranh', 5, 4, 1, 80000, 12, 5, N'Sach lich su minh hoa cho thieu nhi');
GO

-- Customers
INSERT INTO Customers (
    FullName,
    Phone,
    Email,
    Address
)
VALUES
(N'Khach le', NULL, NULL, NULL),
(N'Nguyen Minh Anh', '0988888888', 'minhanh@example.com', N'Da Nang'),
(N'Le Hoang Nam', '0977777777', 'hoangnam@example.com', N'TP.HCM');
GO

/* ============================================================
   7. QUERY MAU DUNG TRONG C#
   ============================================================ */

/*
-- Dang nhap
SELECT 
    u.Id,
    u.Username,
    u.PasswordHash,
    u.FullName,
    r.RoleName,
    u.IsActive
FROM Users u
JOIN Roles r ON u.RoleId = r.Id
WHERE u.Username = @Username
AND u.PasswordHash = @Password
AND u.IsActive = 1;

-- Lay danh sach sach
SELECT *
FROM vw_BookList
WHERE IsActive = 1
ORDER BY Id DESC;

-- Tim kiem sach
SELECT *
FROM vw_BookList
WHERE IsActive = 1
AND (
    Title LIKE N'%' + @Keyword + N'%'
    OR BookCode LIKE N'%' + @Keyword + N'%'
    OR ISBN LIKE N'%' + @Keyword + N'%'
)
ORDER BY Id DESC;

-- Danh sach hoa don
SELECT *
FROM vw_SalesOrderList
ORDER BY OrderDate DESC;

-- Chi tiet hoa don
SELECT *
FROM vw_SalesOrderDetailFull
WHERE SalesOrderId = @SalesOrderId;

-- Doanh thu hom nay
SELECT ISNULL(SUM(TotalAmount), 0) AS TodayRevenue
FROM SalesOrders
WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)
AND OrderStatus = 'Completed';

-- Doanh thu thang nay
SELECT ISNULL(SUM(TotalAmount), 0) AS MonthRevenue
FROM SalesOrders
WHERE MONTH(OrderDate) = MONTH(GETDATE())
AND YEAR(OrderDate) = YEAR(GETDATE())
AND OrderStatus = 'Completed';

-- Sach sap het hang
SELECT *
FROM vw_LowStockBooks;

-- Sach ban chay
SELECT TOP 10 *
FROM vw_TopSellingBooks
ORDER BY TotalSold DESC;

-- Lich su kho
SELECT *
FROM vw_InventoryHistory
ORDER BY CreatedAt DESC;
*/

/* ============================================================
   8. TEST MAU
   Neu muon test thi bo comment doan duoi.
   ============================================================ */

/*
-- Test ban sach
INSERT INTO SalesOrders (
    OrderCode,
    UserId,
    CustomerId,
    PaymentMethod,
    Note
)
VALUES (
    'SO001',
    2,
    1,
    'Cash',
    N'Hoa don ban thu'
);
GO

INSERT INTO SalesOrderDetails (
    SalesOrderId,
    BookId,
    Quantity,
    UnitPrice,
    DiscountAmount
)
VALUES
(1, 1, 2, 65000, 0),
(1, 2, 1, 90000, 0);
GO

SELECT * FROM SalesOrders;
SELECT * FROM SalesOrderDetails;
SELECT * FROM Books;
SELECT * FROM InventoryTransactions;
GO

-- Test nhap sach
INSERT INTO PurchaseReceipts (
    ReceiptCode,
    SupplierId,
    UserId,
    Note
)
VALUES (
    'PR001',
    1,
    1,
    N'Nhap sach tu Kim Dong'
);
GO

INSERT INTO PurchaseReceiptDetails (
    PurchaseReceiptId,
    BookId,
    Quantity,
    ImportPrice
)
VALUES
(1, 1, 20, 40000),
(1, 2, 10, 60000);
GO

SELECT * FROM PurchaseReceipts;
SELECT * FROM PurchaseReceiptDetails;
SELECT * FROM Books;
SELECT * FROM InventoryTransactions;
GO

-- Test dieu chinh kho thu cong
EXEC sp_AdjustBookStock @BookId = 1, @UserId = 1, @QuantityChange = 5, @Note = N'Kiem ke tang 5 cuon';
GO

SELECT * FROM vw_InventoryHistory ORDER BY CreatedAt DESC;
GO
*/

PRINT N'Create BookStoreDB successfully.';
GO
