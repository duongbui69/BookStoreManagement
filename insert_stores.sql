SET IDENTITY_INSERT Stores ON;
INSERT INTO Stores (Id, StoreCode, StoreName, Address, Phone, IsActive, CreatedAt) 
VALUES (1, 'S01', N'Chi nhánh Trung tâm', N'123 Đường A, Quận 1, TP.HCM', '0123456789', 1, GETDATE());
SET IDENTITY_INSERT Stores OFF;

-- Bật lại kiểm tra khóa ngoại
EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
