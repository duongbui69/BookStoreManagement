-- Tắt kiểm tra khóa ngoại
EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- Tạo dữ liệu tồn kho ngẫu nhiên cho tất cả các sách
INSERT INTO StoreBookInventories (StoreId, BookId, Quantity, MinStock, SellingPrice, ShelfLocation, IsActive)
SELECT 
    1 AS StoreId,
    Id AS BookId,
    (ABS(CHECKSUM(NEWID())) % 100) + 10 AS Quantity,
    5 AS MinStock,
    ISNULL(SellingPrice, 50000) AS SellingPrice,
    'Kệ ' + CHAR(65 + (ABS(CHECKSUM(NEWID())) % 5)) + CAST((ABS(CHECKSUM(NEWID())) % 5) + 1 AS NVARCHAR) AS ShelfLocation,
    1 AS IsActive
FROM Books;

-- Bật lại kiểm tra khóa ngoại
EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
