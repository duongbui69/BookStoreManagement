/* ============================================================
   RESET DATABASE - BookStoreDB nang cap theo bai toan ban sach
   Chay file nay se xoa BookStoreDB cu va tao lai tu dau.

   Nang cap chinh:
   - Them cua hang Stores
   - Them CCCD/CMND va ma dinh danh cho khach hang, nhan vien
   - Quan ly ton kho sach theo tung cua hang bang StoreBookInventories
   - Hoa don ban hang co cua hang ban va nhan vien ban
   - Phieu tra hang / hoan tien ReturnReceipts, ReturnReceiptDetails
   - Lich su kho theo cua hang
   - Van giu Books.Quantity de tuong thich code cu: day la tong ton kho tat ca cua hang
   ============================================================ */

USE master;
GO

IF DB_ID(N'BookStoreDB') IS NOT NULL
BEGIN
    ALTER DATABASE BookStoreDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BookStoreDB;
END
GO

CREATE DATABASE BookStoreDB;
GO

USE BookStoreDB;
GO

/* ============================================================
   1. TABLES
   ============================================================ */

-- =========================
-- 1.1 ROLES
-- =========================
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(20) NOT NULL UNIQUE,
    Description NVARCHAR(255),

    CONSTRAINT CK_Roles_RoleName
    CHECK (RoleName IN ('Admin', 'Staff'))
);
GO

-- =========================
-- 1.2 STORES / CUA HANG
-- =========================
CREATE TABLE Stores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StoreCode NVARCHAR(50) NOT NULL UNIQUE,
    StoreName NVARCHAR(150) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.3 USERS / NHAN VIEN + TAI KHOAN
-- User trong project nay dong vai tro nhan vien co tai khoan dang nhap.
-- Admin co the khong thuoc cua hang nao, Staff nen thuoc mot cua hang.
-- =========================
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserCode NVARCHAR(50) NOT NULL UNIQUE DEFAULT ('EMP' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', '')),
    StoreId INT NULL,
    RoleId INT NOT NULL,

    IdentityNumber NVARCHAR(20) NULL UNIQUE,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,

    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    HireDate DATE NULL,

    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Users_Stores
    FOREIGN KEY (StoreId) REFERENCES Stores(Id),

    CONSTRAINT FK_Users_Roles
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
GO

-- =========================
-- 1.4 CUSTOMERS
-- =========================
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerCode NVARCHAR(50) NOT NULL UNIQUE DEFAULT ('CUS' + REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', '')),
    IdentityNumber NVARCHAR(20) NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.5 CATEGORIES
-- =========================
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.6 AUTHORS
-- =========================
CREATE TABLE Authors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AuthorName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.7 PUBLISHERS
-- =========================
CREATE TABLE Publishers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PublisherName NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.8 SUPPLIERS
-- =========================
CREATE TABLE Suppliers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SupplierName NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);
GO

-- =========================
-- 1.9 BOOKS / DAU SACH GOC
-- Quantity la tong ton kho tat ca cua hang, duoc trigger cap nhat khi ban/nhap/tra/dieu chinh.
-- Ton kho chi tiet theo cua hang nam o StoreBookInventories.
-- =========================
CREATE TABLE Books (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BookCode NVARCHAR(50) NOT NULL UNIQUE,
    ISBN NVARCHAR(30) NULL,
    Title NVARCHAR(200) NOT NULL,
    PublishYear INT NULL,
    PageCount INT NULL,

    CategoryId INT NOT NULL,
    AuthorId INT NULL,
    PublisherId INT NULL,

    SellingPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    MinStock INT NOT NULL DEFAULT 5,

    Description NVARCHAR(500),
    ImagePath NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_Books_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT FK_Books_Authors FOREIGN KEY (AuthorId) REFERENCES Authors(Id),
    CONSTRAINT FK_Books_Publishers FOREIGN KEY (PublisherId) REFERENCES Publishers(Id),
    CONSTRAINT CK_Books_PublishYear CHECK (PublishYear IS NULL OR PublishYear BETWEEN 1800 AND 2100),
    CONSTRAINT CK_Books_PageCount CHECK (PageCount IS NULL OR PageCount > 0),
    CONSTRAINT CK_Books_SellingPrice CHECK (SellingPrice >= 0),
    CONSTRAINT CK_Books_Quantity CHECK (Quantity >= 0),
    CONSTRAINT CK_Books_MinStock CHECK (MinStock >= 0)
);
GO

CREATE UNIQUE INDEX UX_Books_ISBN_NotNull
ON Books(ISBN)
WHERE ISBN IS NOT NULL;
GO

-- =========================
-- 1.10 STORE BOOK INVENTORIES / TON KHO THEO CUA HANG
-- =========================
CREATE TABLE StoreBookInventories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StoreId INT NOT NULL,
    BookId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    MinStock INT NOT NULL DEFAULT 5,
    ImportPrice DECIMAL(18,2) NULL,
    SellingPrice DECIMAL(18,2) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT FK_StoreBookInventories_Stores FOREIGN KEY (StoreId) REFERENCES Stores(Id),
    CONSTRAINT FK_StoreBookInventories_Books FOREIGN KEY (BookId) REFERENCES Books(Id),
    CONSTRAINT UQ_StoreBookInventories_Store_Book UNIQUE (StoreId, BookId),
    CONSTRAINT CK_StoreBookInventories_Quantity CHECK (Quantity >= 0),
    CONSTRAINT CK_StoreBookInventories_MinStock CHECK (MinStock >= 0),
    CONSTRAINT CK_StoreBookInventories_ImportPrice CHECK (ImportPrice IS NULL OR ImportPrice >= 0),
    CONSTRAINT CK_StoreBookInventories_SellingPrice CHECK (SellingPrice >= 0)
);
GO

-- =========================
-- 1.11 SALES ORDERS / HOA DON BAN HANG
-- StoreId co DEFAULT 1 de code cu neu chua truyen StoreId van test duoc.
-- Khi nang cap form, nen truyen StoreId theo CurrentSession.StoreId.
-- =========================
CREATE TABLE SalesOrders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderCode NVARCHAR(50) NOT NULL UNIQUE,
    StoreId INT NOT NULL DEFAULT 1,
    UserId INT NOT NULL,
    CustomerId INT NULL,
    OrderDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    PaymentMethod NVARCHAR(30) NOT NULL DEFAULT 'Cash',
    OrderStatus NVARCHAR(30) NOT NULL DEFAULT 'Completed',
    Note NVARCHAR(255),

    CONSTRAINT FK_SalesOrders_Stores FOREIGN KEY (StoreId) REFERENCES Stores(Id),
    CONSTRAINT FK_SalesOrders_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_SalesOrders_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT CK_SalesOrders_TotalAmount CHECK (TotalAmount >= 0),
    CONSTRAINT CK_SalesOrders_PaymentMethod CHECK (PaymentMethod IN ('Cash', 'Banking', 'Card')),
    CONSTRAINT CK_SalesOrders_OrderStatus CHECK (OrderStatus IN ('Completed', 'Cancelled'))
);
GO

-- =========================
-- 1.12 SALES ORDER DETAILS
-- =========================
CREATE TABLE SalesOrderDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SalesOrderId INT NOT NULL,
    BookId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    LineTotal AS ((Quantity * UnitPrice) - DiscountAmount) PERSISTED,

    CONSTRAINT FK_SalesOrderDetails_SalesOrders FOREIGN KEY (SalesOrderId) REFERENCES SalesOrders(Id),
    CONSTRAINT FK_SalesOrderDetails_Books FOREIGN KEY (BookId) REFERENCES Books(Id),
    CONSTRAINT CK_SalesOrderDetails_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_SalesOrderDetails_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_SalesOrderDetails_DiscountAmount CHECK (DiscountAmount >= 0),
    CONSTRAINT CK_SalesOrderDetails_LineTotal CHECK ((Quantity * UnitPrice) - DiscountAmount >= 0)
);
GO

-- =========================
-- 1.13 PURCHASE RECEIPTS / PHIEU NHAP SACH
-- =========================
CREATE TABLE PurchaseReceipts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReceiptCode NVARCHAR(50) NOT NULL UNIQUE,
    StoreId INT NOT NULL DEFAULT 1,
    SupplierId INT NOT NULL,
    UserId INT NOT NULL,
    ImportDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Note NVARCHAR(255),

    CONSTRAINT FK_PurchaseReceipts_Stores FOREIGN KEY (StoreId) REFERENCES Stores(Id),
    CONSTRAINT FK_PurchaseReceipts_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
    CONSTRAINT FK_PurchaseReceipts_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CK_PurchaseReceipts_TotalAmount CHECK (TotalAmount >= 0)
);
GO

-- =========================
-- 1.14 PURCHASE RECEIPT DETAILS
-- =========================
CREATE TABLE PurchaseReceiptDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PurchaseReceiptId INT NOT NULL,
    BookId INT NOT NULL,
    Quantity INT NOT NULL,
    ImportPrice DECIMAL(18,2) NOT NULL,
    SellingPrice DECIMAL(18,2) NULL,
    LineTotal AS (Quantity * ImportPrice) PERSISTED,

    CONSTRAINT FK_PurchaseReceiptDetails_PurchaseReceipts FOREIGN KEY (PurchaseReceiptId) REFERENCES PurchaseReceipts(Id),
    CONSTRAINT FK_PurchaseReceiptDetails_Books FOREIGN KEY (BookId) REFERENCES Books(Id),
    CONSTRAINT CK_PurchaseReceiptDetails_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_PurchaseReceiptDetails_ImportPrice CHECK (ImportPrice >= 0),
    CONSTRAINT CK_PurchaseReceiptDetails_SellingPrice CHECK (SellingPrice IS NULL OR SellingPrice >= 0)
);
GO

-- =========================
-- 1.15 RETURN RECEIPTS / PHIEU TRA HANG - HOAN TIEN
-- =========================
CREATE TABLE ReturnReceipts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReturnCode NVARCHAR(50) NOT NULL UNIQUE,
    SalesOrderId INT NULL,
    StoreId INT NOT NULL,
    CustomerId INT NULL,
    UserId INT NOT NULL,
    ReturnDate DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    TotalRefundAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Note NVARCHAR(255),

    CONSTRAINT FK_ReturnReceipts_SalesOrders FOREIGN KEY (SalesOrderId) REFERENCES SalesOrders(Id),
    CONSTRAINT FK_ReturnReceipts_Stores FOREIGN KEY (StoreId) REFERENCES Stores(Id),
    CONSTRAINT FK_ReturnReceipts_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
    CONSTRAINT FK_ReturnReceipts_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CK_ReturnReceipts_TotalRefundAmount CHECK (TotalRefundAmount >= 0)
);
GO

-- =========================
-- 1.16 RETURN RECEIPT DETAILS
-- =========================
CREATE TABLE ReturnReceiptDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReturnReceiptId INT NOT NULL,
    SalesOrderDetailId INT NULL,
    BookId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    RefundAmount AS (Quantity * UnitPrice) PERSISTED,
    ReturnReason NVARCHAR(255),
    IsRestock BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_ReturnReceiptDetails_ReturnReceipts FOREIGN KEY (ReturnReceiptId) REFERENCES ReturnReceipts(Id),
    CONSTRAINT FK_ReturnReceiptDetails_SalesOrderDetails FOREIGN KEY (SalesOrderDetailId) REFERENCES SalesOrderDetails(Id),
    CONSTRAINT FK_ReturnReceiptDetails_Books FOREIGN KEY (BookId) REFERENCES Books(Id),
    CONSTRAINT CK_ReturnReceiptDetails_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_ReturnReceiptDetails_UnitPrice CHECK (UnitPrice >= 0)
);
GO

-- =========================
-- 1.17 INVENTORY TRANSACTIONS / LICH SU KHO
-- =========================
CREATE TABLE InventoryTransactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StoreId INT NOT NULL,
    BookId INT NOT NULL,
    UserId INT NULL,
    TransactionType NVARCHAR(30) NOT NULL,
    QuantityChange INT NOT NULL,
    ReferenceType NVARCHAR(30) NULL,
    ReferenceId INT NULL,
    Note NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT FK_InventoryTransactions_Stores FOREIGN KEY (StoreId) REFERENCES Stores(Id),
    CONSTRAINT FK_InventoryTransactions_Books FOREIGN KEY (BookId) REFERENCES Books(Id),
    CONSTRAINT FK_InventoryTransactions_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CK_InventoryTransactions_Type CHECK (TransactionType IN ('Import', 'Sale', 'Adjustment', 'CancelSale', 'Return'))
);
GO

/* ============================================================
   2. TRIGGERS
   ============================================================ */

-- =========================
-- 2.0 TU TAO TON KHO MAC DINH KHI THEM SACH MOI
-- De code cu khi them sach co Quantity van co ton kho o cua hang 1.
-- Khi lam form nang cap, nen them ton kho theo StoreBookInventories rieng.
-- =========================
CREATE TRIGGER TR_Books_AfterInsert_CreateDefaultInventory
ON Books
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO StoreBookInventories (StoreId, BookId, Quantity, MinStock, SellingPrice, IsActive)
    SELECT 1, i.Id, i.Quantity, i.MinStock, i.SellingPrice, i.IsActive
    FROM inserted i
    WHERE i.Quantity > 0
      AND EXISTS (SELECT 1 FROM Stores WHERE Id = 1)
      AND NOT EXISTS (
          SELECT 1
          FROM StoreBookInventories sbi
          WHERE sbi.StoreId = 1 AND sbi.BookId = i.Id
      );
END;
GO

-- =========================
-- 2.1 TU TRU KHO THEO CUA HANG KHI BAN SACH
-- =========================
CREATE TRIGGER TR_SalesOrderDetails_AfterInsert
ON SalesOrderDetails
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM (
            SELECT so.StoreId, i.BookId, SUM(i.Quantity) AS TotalQuantity
            FROM inserted i
            JOIN SalesOrders so ON i.SalesOrderId = so.Id
            GROUP BY so.StoreId, i.BookId
        ) x
        LEFT JOIN StoreBookInventories sbi
            ON x.StoreId = sbi.StoreId AND x.BookId = sbi.BookId
        WHERE ISNULL(sbi.Quantity, 0) < x.TotalQuantity
    )
    BEGIN
        RAISERROR (N'So luong sach trong kho cua cua hang khong du de ban.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    UPDATE sbi
    SET sbi.Quantity = sbi.Quantity - x.TotalQuantity,
        sbi.UpdatedAt = SYSDATETIME()
    FROM StoreBookInventories sbi
    JOIN (
        SELECT so.StoreId, i.BookId, SUM(i.Quantity) AS TotalQuantity
        FROM inserted i
        JOIN SalesOrders so ON i.SalesOrderId = so.Id
        GROUP BY so.StoreId, i.BookId
    ) x ON sbi.StoreId = x.StoreId AND sbi.BookId = x.BookId;

    UPDATE b
    SET b.Quantity = b.Quantity - x.TotalQuantity,
        b.UpdatedAt = SYSDATETIME()
    FROM Books b
    JOIN (
        SELECT i.BookId, SUM(i.Quantity) AS TotalQuantity
        FROM inserted i
        GROUP BY i.BookId
    ) x ON b.Id = x.BookId;

    UPDATE so
    SET so.TotalAmount = x.TotalAmount
    FROM SalesOrders so
    JOIN (
        SELECT SalesOrderId, SUM(LineTotal) AS TotalAmount
        FROM SalesOrderDetails
        WHERE SalesOrderId IN (SELECT DISTINCT SalesOrderId FROM inserted)
        GROUP BY SalesOrderId
    ) x ON so.Id = x.SalesOrderId;

    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note)
    SELECT so.StoreId, i.BookId, so.UserId, 'Sale', -i.Quantity, 'SalesOrder', i.SalesOrderId, N'Ban sach'
    FROM inserted i
    JOIN SalesOrders so ON i.SalesOrderId = so.Id;
END;
GO

-- =========================
-- 2.2 TU CONG KHO THEO CUA HANG KHI NHAP SACH
-- =========================
CREATE TRIGGER TR_PurchaseReceiptDetails_AfterInsert
ON PurchaseReceiptDetails
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    MERGE StoreBookInventories AS target
    USING (
        SELECT
            pr.StoreId,
            i.BookId,
            SUM(i.Quantity) AS TotalQuantity,
            MAX(i.ImportPrice) AS ImportPrice,
            MAX(i.SellingPrice) AS SellingPrice
        FROM inserted i
        JOIN PurchaseReceipts pr ON i.PurchaseReceiptId = pr.Id
        GROUP BY pr.StoreId, i.BookId
    ) AS src
    ON target.StoreId = src.StoreId AND target.BookId = src.BookId
    WHEN MATCHED THEN
        UPDATE SET
            target.Quantity = target.Quantity + src.TotalQuantity,
            target.ImportPrice = src.ImportPrice,
            target.SellingPrice = ISNULL(src.SellingPrice, target.SellingPrice),
            target.UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (StoreId, BookId, Quantity, MinStock, ImportPrice, SellingPrice, IsActive)
        VALUES (
            src.StoreId,
            src.BookId,
            src.TotalQuantity,
            5,
            src.ImportPrice,
            ISNULL(src.SellingPrice, (SELECT SellingPrice FROM Books WHERE Id = src.BookId)),
            1
        );

    UPDATE b
    SET b.Quantity = b.Quantity + x.TotalQuantity,
        b.SellingPrice = ISNULL(x.SellingPrice, b.SellingPrice),
        b.UpdatedAt = SYSDATETIME()
    FROM Books b
    JOIN (
        SELECT i.BookId, SUM(i.Quantity) AS TotalQuantity, MAX(i.SellingPrice) AS SellingPrice
        FROM inserted i
        GROUP BY i.BookId
    ) x ON b.Id = x.BookId;

    UPDATE pr
    SET pr.TotalAmount = x.TotalAmount
    FROM PurchaseReceipts pr
    JOIN (
        SELECT PurchaseReceiptId, SUM(LineTotal) AS TotalAmount
        FROM PurchaseReceiptDetails
        WHERE PurchaseReceiptId IN (SELECT DISTINCT PurchaseReceiptId FROM inserted)
        GROUP BY PurchaseReceiptId
    ) x ON pr.Id = x.PurchaseReceiptId;

    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note)
    SELECT pr.StoreId, i.BookId, pr.UserId, 'Import', i.Quantity, 'PurchaseReceipt', i.PurchaseReceiptId, N'Nhap sach'
    FROM inserted i
    JOIN PurchaseReceipts pr ON i.PurchaseReceiptId = pr.Id;
END;
GO

-- =========================
-- 2.3 TU HOAN KHO KHI HUY HOA DON
-- =========================
CREATE TRIGGER TR_SalesOrders_AfterUpdate_Cancel
ON SalesOrders
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(OrderStatus)
    BEGIN
        UPDATE sbi
        SET sbi.Quantity = sbi.Quantity + x.TotalQuantity,
            sbi.UpdatedAt = SYSDATETIME()
        FROM StoreBookInventories sbi
        JOIN (
            SELECT i.StoreId, sod.BookId, SUM(sod.Quantity) AS TotalQuantity
            FROM SalesOrderDetails sod
            JOIN inserted i ON sod.SalesOrderId = i.Id
            JOIN deleted d ON i.Id = d.Id
            WHERE i.OrderStatus = 'Cancelled'
              AND d.OrderStatus <> 'Cancelled'
            GROUP BY i.StoreId, sod.BookId
        ) x ON sbi.StoreId = x.StoreId AND sbi.BookId = x.BookId;

        UPDATE b
        SET b.Quantity = b.Quantity + x.TotalQuantity,
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

        INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note)
        SELECT i.StoreId, sod.BookId, i.UserId, 'CancelSale', sod.Quantity, 'SalesOrder', i.Id, N'Huy hoa don va hoan kho'
        FROM SalesOrderDetails sod
        JOIN inserted i ON sod.SalesOrderId = i.Id
        JOIN deleted d ON i.Id = d.Id
        WHERE i.OrderStatus = 'Cancelled'
          AND d.OrderStatus <> 'Cancelled';
    END
END;
GO

-- =========================
-- 2.4 TU CONG KHO KHI TRA HANG HOP LE
-- =========================
CREATE TRIGGER TR_ReturnReceiptDetails_AfterInsert
ON ReturnReceiptDetails
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Khong cho tra vuot so luong da ban neu co lien ket chi tiet hoa don
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT rrd.SalesOrderDetailId, SUM(rrd.Quantity) AS TotalReturned
            FROM ReturnReceiptDetails rrd
            WHERE rrd.SalesOrderDetailId IN (
                SELECT SalesOrderDetailId
                FROM inserted
                WHERE SalesOrderDetailId IS NOT NULL
            )
            GROUP BY rrd.SalesOrderDetailId
        ) x
        JOIN SalesOrderDetails sod ON x.SalesOrderDetailId = sod.Id
        WHERE x.TotalReturned > sod.Quantity
    )
    BEGIN
        RAISERROR (N'So luong tra hang vuot qua so luong da mua.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;

    UPDATE rr
    SET rr.TotalRefundAmount = x.TotalRefundAmount
    FROM ReturnReceipts rr
    JOIN (
        SELECT ReturnReceiptId, SUM(RefundAmount) AS TotalRefundAmount
        FROM ReturnReceiptDetails
        WHERE ReturnReceiptId IN (SELECT DISTINCT ReturnReceiptId FROM inserted)
        GROUP BY ReturnReceiptId
    ) x ON rr.Id = x.ReturnReceiptId;

    MERGE StoreBookInventories AS target
    USING (
        SELECT rr.StoreId, i.BookId, SUM(i.Quantity) AS TotalQuantity, MAX(i.UnitPrice) AS UnitPrice
        FROM inserted i
        JOIN ReturnReceipts rr ON i.ReturnReceiptId = rr.Id
        WHERE i.IsRestock = 1
        GROUP BY rr.StoreId, i.BookId
    ) AS src
    ON target.StoreId = src.StoreId AND target.BookId = src.BookId
    WHEN MATCHED THEN
        UPDATE SET
            target.Quantity = target.Quantity + src.TotalQuantity,
            target.UpdatedAt = SYSDATETIME()
    WHEN NOT MATCHED THEN
        INSERT (StoreId, BookId, Quantity, MinStock, SellingPrice, IsActive)
        VALUES (src.StoreId, src.BookId, src.TotalQuantity, 5, src.UnitPrice, 1);

    UPDATE b
    SET b.Quantity = b.Quantity + x.TotalQuantity,
        b.UpdatedAt = SYSDATETIME()
    FROM Books b
    JOIN (
        SELECT i.BookId, SUM(i.Quantity) AS TotalQuantity
        FROM inserted i
        WHERE i.IsRestock = 1
        GROUP BY i.BookId
    ) x ON b.Id = x.BookId;

    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note)
    SELECT rr.StoreId, i.BookId, rr.UserId, 'Return', i.Quantity, 'ReturnReceipt', i.ReturnReceiptId, ISNULL(i.ReturnReason, N'Tra hang va nhap lai kho')
    FROM inserted i
    JOIN ReturnReceipts rr ON i.ReturnReceiptId = rr.Id
    WHERE i.IsRestock = 1;
END;
GO

/* ============================================================
   3. VIEWS
   ============================================================ */

CREATE VIEW vw_UserList
AS
SELECT
    u.Id,
    u.UserCode,
    u.StoreId,
    s.StoreName,
    u.RoleId,
    r.RoleName,
    u.IdentityNumber,
    u.Username,
    u.FullName,
    u.Phone,
    u.Email,
    u.Address,
    u.HireDate,
    u.IsActive,
    u.CreatedAt,
    u.UpdatedAt
FROM Users u
JOIN Roles r ON u.RoleId = r.Id
LEFT JOIN Stores s ON u.StoreId = s.Id;
GO

CREATE VIEW vw_BookList
AS
SELECT
    b.Id,
    b.BookCode,
    b.ISBN,
    b.Title,
    b.PublishYear,
    b.PageCount,
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

CREATE VIEW vw_StoreBookInventory
AS
SELECT
    sbi.Id,
    s.Id AS StoreId,
    s.StoreCode,
    s.StoreName,
    b.Id AS BookId,
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
    CASE
        WHEN sbi.Quantity = 0 THEN N'Het hang'
        WHEN sbi.Quantity <= sbi.MinStock THEN N'Sap het'
        ELSE N'Con hang'
    END AS StockStatus,
    sbi.IsActive,
    sbi.CreatedAt,
    sbi.UpdatedAt
FROM StoreBookInventories sbi
JOIN Stores s ON sbi.StoreId = s.Id
JOIN Books b ON sbi.BookId = b.Id
JOIN Categories c ON b.CategoryId = c.Id
LEFT JOIN Authors a ON b.AuthorId = a.Id
LEFT JOIN Publishers p ON b.PublisherId = p.Id;
GO

CREATE VIEW vw_SalesOrderDetailFull
AS
SELECT
    so.Id AS SalesOrderId,
    so.OrderCode,
    so.StoreId,
    st.StoreName,
    so.OrderDate,
    u.Id AS StaffId,
    u.FullName AS StaffName,
    c.Id AS CustomerId,
    ISNULL(c.FullName, N'Khach le') AS CustomerName,
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
JOIN Stores st ON so.StoreId = st.Id
JOIN Users u ON so.UserId = u.Id
LEFT JOIN Customers c ON so.CustomerId = c.Id
JOIN SalesOrderDetails sod ON so.Id = sod.SalesOrderId
JOIN Books b ON sod.BookId = b.Id;
GO

CREATE VIEW vw_SalesOrderList
AS
SELECT
    so.Id,
    so.OrderCode,
    so.StoreId,
    st.StoreName,
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
JOIN Stores st ON so.StoreId = st.Id
JOIN Users u ON so.UserId = u.Id
LEFT JOIN Customers c ON so.CustomerId = c.Id;
GO

CREATE VIEW vw_PurchaseReceiptList
AS
SELECT
    pr.Id,
    pr.ReceiptCode,
    pr.StoreId,
    st.StoreName,
    pr.ImportDate,
    s.Id AS SupplierId,
    s.SupplierName,
    u.Id AS StaffId,
    u.FullName AS StaffName,
    pr.TotalAmount,
    pr.Note
FROM PurchaseReceipts pr
JOIN Stores st ON pr.StoreId = st.Id
JOIN Suppliers s ON pr.SupplierId = s.Id
JOIN Users u ON pr.UserId = u.Id;
GO

CREATE VIEW vw_ReturnReceiptList
AS
SELECT
    rr.Id,
    rr.ReturnCode,
    rr.SalesOrderId,
    so.OrderCode,
    rr.StoreId,
    st.StoreName,
    rr.CustomerId,
    ISNULL(c.FullName, N'Khach le') AS CustomerName,
    rr.UserId AS StaffId,
    u.FullName AS StaffName,
    rr.ReturnDate,
    rr.TotalRefundAmount,
    rr.Note
FROM ReturnReceipts rr
LEFT JOIN SalesOrders so ON rr.SalesOrderId = so.Id
JOIN Stores st ON rr.StoreId = st.Id
LEFT JOIN Customers c ON rr.CustomerId = c.Id
JOIN Users u ON rr.UserId = u.Id;
GO

CREATE VIEW vw_ReturnReceiptDetailFull
AS
SELECT
    rr.Id AS ReturnReceiptId,
    rr.ReturnCode,
    rr.ReturnDate,
    rr.StoreId,
    st.StoreName,
    c.Id AS CustomerId,
    ISNULL(c.FullName, N'Khach le') AS CustomerName,
    u.Id AS StaffId,
    u.FullName AS StaffName,
    rrd.BookId,
    b.BookCode,
    b.Title,
    rrd.Quantity,
    rrd.UnitPrice,
    rrd.RefundAmount,
    rrd.ReturnReason,
    rrd.IsRestock,
    rr.TotalRefundAmount,
    rr.Note
FROM ReturnReceiptDetails rrd
JOIN ReturnReceipts rr ON rrd.ReturnReceiptId = rr.Id
JOIN Stores st ON rr.StoreId = st.Id
LEFT JOIN Customers c ON rr.CustomerId = c.Id
JOIN Users u ON rr.UserId = u.Id
JOIN Books b ON rrd.BookId = b.Id;
GO

CREATE VIEW vw_LowStockBooks
AS
SELECT
    b.Id,
    sbi.StoreId,
    s.StoreName,
    b.BookCode,
    b.Title,
    sbi.Quantity,
    sbi.MinStock
FROM StoreBookInventories sbi
JOIN Stores s ON sbi.StoreId = s.Id
JOIN Books b ON sbi.BookId = b.Id
WHERE sbi.Quantity <= sbi.MinStock
  AND sbi.IsActive = 1
  AND b.IsActive = 1;
GO

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

CREATE VIEW vw_DailyRevenueByStore
AS
SELECT
    so.StoreId,
    s.StoreName,
    CAST(so.OrderDate AS DATE) AS RevenueDate,
    COUNT(*) AS TotalOrders,
    SUM(so.TotalAmount) AS TotalRevenue
FROM SalesOrders so
JOIN Stores s ON so.StoreId = s.Id
WHERE so.OrderStatus = 'Completed'
GROUP BY so.StoreId, s.StoreName, CAST(so.OrderDate AS DATE);
GO

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

CREATE VIEW vw_InventoryHistory
AS
SELECT
    it.Id,
    it.StoreId,
    s.StoreName,
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
JOIN Stores s ON it.StoreId = s.Id
JOIN Books b ON it.BookId = b.Id
LEFT JOIN Users u ON it.UserId = u.Id;
GO

/* ============================================================
   4. STORED PROCEDURES
   ============================================================ */

CREATE PROCEDURE sp_AdjustBookStock
    @StoreId INT = 1,
    @BookId INT,
    @UserId INT,
    @QuantityChange INT,
    @Note NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Stores WHERE Id = @StoreId)
    BEGIN
        RAISERROR (N'Cua hang khong ton tai.', 16, 1);
        RETURN;
    END;

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

    IF NOT EXISTS (SELECT 1 FROM StoreBookInventories WHERE StoreId = @StoreId AND BookId = @BookId)
    BEGIN
        INSERT INTO StoreBookInventories (StoreId, BookId, Quantity, MinStock, SellingPrice, IsActive)
        SELECT @StoreId, @BookId, 0, MinStock, SellingPrice, 1
        FROM Books
        WHERE Id = @BookId;
    END;

    IF EXISTS (
        SELECT 1
        FROM StoreBookInventories
        WHERE StoreId = @StoreId
          AND BookId = @BookId
          AND Quantity + @QuantityChange < 0
    )
    BEGIN
        RAISERROR (N'So luong dieu chinh lam ton kho cua cua hang bi am.', 16, 1);
        RETURN;
    END;

    UPDATE StoreBookInventories
    SET Quantity = Quantity + @QuantityChange,
        UpdatedAt = SYSDATETIME()
    WHERE StoreId = @StoreId
      AND BookId = @BookId;

    UPDATE Books
    SET Quantity = Quantity + @QuantityChange,
        UpdatedAt = SYSDATETIME()
    WHERE Id = @BookId;

    INSERT INTO InventoryTransactions (StoreId, BookId, UserId, TransactionType, QuantityChange, ReferenceType, ReferenceId, Note)
    VALUES (@StoreId, @BookId, @UserId, 'Adjustment', @QuantityChange, 'Manual', NULL, ISNULL(@Note, N'Dieu chinh ton kho thu cong'));
END;
GO

/* ============================================================
   5. INDEXES
   ============================================================ */

CREATE INDEX IX_Users_StoreId ON Users(StoreId);
CREATE INDEX IX_Users_RoleId ON Users(RoleId);
CREATE INDEX IX_Customers_FullName ON Customers(FullName);
CREATE INDEX IX_Customers_Phone ON Customers(Phone);

CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_BookCode ON Books(BookCode);
CREATE INDEX IX_Books_CategoryId ON Books(CategoryId);
CREATE INDEX IX_Books_AuthorId ON Books(AuthorId);
CREATE INDEX IX_Books_PublisherId ON Books(PublisherId);

CREATE INDEX IX_StoreBookInventories_StoreId ON StoreBookInventories(StoreId);
CREATE INDEX IX_StoreBookInventories_BookId ON StoreBookInventories(BookId);

CREATE INDEX IX_SalesOrders_OrderDate ON SalesOrders(OrderDate);
CREATE INDEX IX_SalesOrders_StoreId ON SalesOrders(StoreId);
CREATE INDEX IX_SalesOrders_UserId ON SalesOrders(UserId);
CREATE INDEX IX_SalesOrders_CustomerId ON SalesOrders(CustomerId);

CREATE INDEX IX_SalesOrderDetails_BookId ON SalesOrderDetails(BookId);
CREATE INDEX IX_SalesOrderDetails_SalesOrderId ON SalesOrderDetails(SalesOrderId);

CREATE INDEX IX_PurchaseReceipts_ImportDate ON PurchaseReceipts(ImportDate);
CREATE INDEX IX_PurchaseReceipts_StoreId ON PurchaseReceipts(StoreId);
CREATE INDEX IX_PurchaseReceipts_SupplierId ON PurchaseReceipts(SupplierId);

CREATE INDEX IX_PurchaseReceiptDetails_BookId ON PurchaseReceiptDetails(BookId);
CREATE INDEX IX_PurchaseReceiptDetails_PurchaseReceiptId ON PurchaseReceiptDetails(PurchaseReceiptId);

CREATE INDEX IX_ReturnReceipts_ReturnDate ON ReturnReceipts(ReturnDate);
CREATE INDEX IX_ReturnReceipts_StoreId ON ReturnReceipts(StoreId);
CREATE INDEX IX_ReturnReceipts_CustomerId ON ReturnReceipts(CustomerId);

CREATE INDEX IX_InventoryTransactions_StoreId ON InventoryTransactions(StoreId);
CREATE INDEX IX_InventoryTransactions_BookId ON InventoryTransactions(BookId);
CREATE INDEX IX_InventoryTransactions_CreatedAt ON InventoryTransactions(CreatedAt);
GO

/* ============================================================
   6. SEED DATA
   ============================================================ */

INSERT INTO Roles (RoleName, Description)
VALUES
('Admin', N'Quan tri vien he thong'),
('Staff', N'Nhan vien ban hang');
GO

INSERT INTO Stores (StoreCode, StoreName, Address, Phone)
VALUES
('STORE001', N'Cua hang sach Kim Dong - Ha Noi', N'Ha Noi', '0240000001'),
('STORE002', N'Cua hang sach Kim Dong - TP.HCM', N'TP.HCM', '0280000002');
GO

-- Mat khau demo: 123456
-- PasswordHasher hien tai co fallback plain text nen van dang nhap duoc.
INSERT INTO Users (UserCode, StoreId, RoleId, IdentityNumber, Username, PasswordHash, FullName, Phone, Email, Address, HireDate)
VALUES
('EMP001', NULL, 1, '001000000001', 'admin', '123456', N'Quan tri vien', '0900000001', 'admin@bookstore.com', N'Ha Noi', '2024-01-01'),
('EMP002', 1, 2, '001000000002', 'staff', '123456', N'Nhan vien ban hang', '0900000002', 'staff@bookstore.com', N'Ha Noi', '2024-01-02'),
('EMP003', 2, 2, '001000000003', 'staffhcm', '123456', N'Nhan vien TP.HCM', '0900000003', 'staffhcm@bookstore.com', N'TP.HCM', '2024-01-03');
GO

INSERT INTO Customers (CustomerCode, IdentityNumber, FullName, Phone, Email, Address)
VALUES
('CUS000', NULL, N'Khach le', NULL, NULL, NULL),
('CUS001', '048000000001', N'Nguyen Minh Anh', '0988888888', 'minhanh@example.com', N'Da Nang'),
('CUS002', '079000000002', N'Le Hoang Nam', '0977777777', 'hoangnam@example.com', N'TP.HCM');
GO

INSERT INTO Categories (CategoryName, Description)
VALUES
(N'Truyen tranh', N'Sach truyen tranh thieu nhi'),
(N'Van hoc thieu nhi', N'Sach van hoc danh cho thieu nhi'),
(N'Sach giao duc', N'Sach hoc tap va giao duc'),
(N'Ky nang song', N'Sach phat trien ky nang'),
(N'Lich su', N'Sach lich su danh cho thieu nhi');
GO

INSERT INTO Authors (AuthorName, Description)
VALUES
(N'To Hoai', N'Tac gia van hoc Viet Nam'),
(N'Nguyen Nhat Anh', N'Tac gia truyen thieu nhi'),
(N'Vo Quang', N'Tac gia van hoc thieu nhi'),
(N'Nhieu tac gia', N'Nhieu tac gia khac nhau');
GO

INSERT INTO Publishers (PublisherName, Phone, Email, Address)
VALUES
(N'Nha xuat ban Kim Dong', '0240000000', 'kimdong@example.com', N'Ha Noi');
GO

INSERT INTO Suppliers (SupplierName, Phone, Email, Address)
VALUES
(N'Cong ty phat hanh sach Kim Dong', '0241111111', 'supplierkimdong@example.com', N'Ha Noi');
GO

DISABLE TRIGGER TR_Books_AfterInsert_CreateDefaultInventory ON Books;
GO

INSERT INTO Books (BookCode, ISBN, Title, PublishYear, PageCount, CategoryId, AuthorId, PublisherId, SellingPrice, Quantity, MinStock, Description)
VALUES
('BOOK001', '978000000001', N'De Men Phieu Luu Ky', 1941, 160, 2, 1, 1, 65000, 30, 5, N'Tac pham thieu nhi noi tieng'),
('BOOK002', '978000000002', N'Kinh Van Hoa', 1995, 220, 2, 2, 1, 90000, 20, 5, N'Truyen dai danh cho thieu nhi'),
('BOOK003', '978000000003', N'Que Noi', 1974, 180, 2, 3, 1, 70000, 15, 5, N'Tac pham van hoc thieu nhi'),
('BOOK004', '978000000004', N'Truyen Tranh Thieu Nhi Tap 1', 2020, 80, 1, 4, 1, 45000, 50, 10, N'Truyen tranh cho thieu nhi'),
('BOOK005', '978000000005', N'Lich Su Viet Nam Bang Tranh', 2021, 120, 5, 4, 1, 80000, 12, 5, N'Sach lich su minh hoa cho thieu nhi');
GO

INSERT INTO StoreBookInventories (StoreId, BookId, Quantity, MinStock, ImportPrice, SellingPrice)
VALUES
(1, 1, 20, 5, 40000, 65000),
(2, 1, 10, 5, 40000, 65000),
(1, 2, 15, 5, 60000, 90000),
(2, 2, 5, 5, 60000, 90000),
(1, 3, 10, 5, 45000, 70000),
(2, 3, 5, 5, 45000, 70000),
(1, 4, 30, 10, 25000, 45000),
(2, 4, 20, 10, 25000, 45000),
(1, 5, 8, 5, 50000, 80000),
(2, 5, 4, 5, 50000, 80000);
GO

ENABLE TRIGGER TR_Books_AfterInsert_CreateDefaultInventory ON Books;
GO

/* ============================================================
   7. QUERY TEST MAU
   ============================================================ */

/*
-- Test login
SELECT u.Id, u.Username, u.PasswordHash, u.FullName, r.RoleName, u.StoreId, s.StoreName, u.IsActive
FROM Users u
JOIN Roles r ON u.RoleId = r.Id
LEFT JOIN Stores s ON u.StoreId = s.Id
WHERE u.Username = 'admin' AND u.IsActive = 1;

-- Xem ton kho theo cua hang
SELECT * FROM vw_StoreBookInventory ORDER BY StoreId, BookId;

-- Test ban sach tai cua hang 1
INSERT INTO SalesOrders (OrderCode, StoreId, UserId, CustomerId, PaymentMethod, Note)
VALUES ('SO_TEST_001', 1, 2, 1, 'Cash', N'Hoa don ban thu');

INSERT INTO SalesOrderDetails (SalesOrderId, BookId, Quantity, UnitPrice, DiscountAmount)
VALUES (1, 1, 2, 65000, 0), (1, 2, 1, 90000, 0);

SELECT * FROM vw_SalesOrderList;
SELECT * FROM vw_StoreBookInventory WHERE StoreId = 1;
SELECT * FROM vw_InventoryHistory ORDER BY CreatedAt DESC;

-- Test tra hang
INSERT INTO ReturnReceipts (ReturnCode, SalesOrderId, StoreId, CustomerId, UserId, Note)
VALUES ('RT_TEST_001', 1, 1, 1, 2, N'Tra hang thu');

INSERT INTO ReturnReceiptDetails (ReturnReceiptId, SalesOrderDetailId, BookId, Quantity, UnitPrice, ReturnReason, IsRestock)
VALUES (1, 1, 1, 1, 65000, N'Khach doi tra', 1);

SELECT * FROM vw_ReturnReceiptList;
SELECT * FROM vw_StoreBookInventory WHERE StoreId = 1;
*/

PRINT N'Create upgraded BookStoreDB successfully.';
GO
