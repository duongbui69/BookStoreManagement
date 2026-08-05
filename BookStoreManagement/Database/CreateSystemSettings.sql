USE BookStoreDB;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SystemSettings] (
        [SettingKey] NVARCHAR(50) NOT NULL PRIMARY KEY,
        [SettingValue] NVARCHAR(255) NULL,
        [Description] NVARCHAR(255) NULL
    );

    -- Insert default settings
    INSERT INTO [dbo].[SystemSettings] ([SettingKey], [SettingValue], [Description]) VALUES 
    ('StoreName', 'BookStore Management', 'Tên cửa hàng'),
    ('Address', '123 Đường Sách, TP.HCM', 'Địa chỉ cửa hàng'),
    ('Phone', '0123456789', 'Số điện thoại'),
    ('MinStockThreshold', '10', 'Ngưỡng cảnh báo sắp hết hàng'),
    ('InvoicePrefix', 'HD', 'Tiền tố hóa đơn');
END
GO
