ALTER VIEW vw_StoreBookInventory AS
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
        WHEN sbi.Quantity = 0 THEN N'Hết hàng'
        WHEN sbi.Quantity <= sbi.MinStock THEN N'Sắp hết'
        ELSE N'Còn hàng'
    END AS StockStatus,
    sbi.IsActive,
    sbi.CreatedAt,
    sbi.UpdatedAt
FROM StoreBookInventories sbi
JOIN Stores s ON sbi.StoreId = s.Id
JOIN Books b ON sbi.BookId = b.Id
LEFT JOIN Categories c ON b.CategoryId = c.Id
LEFT JOIN Authors a ON b.AuthorId = a.Id
LEFT JOIN Publishers p ON b.PublisherId = p.Id
;
