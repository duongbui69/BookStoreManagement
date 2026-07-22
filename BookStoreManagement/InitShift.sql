CREATE TABLE Shifts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StaffId INT NOT NULL,
    StoreId INT NOT NULL,
    ShiftName NVARCHAR(50) NOT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NULL,
    InitialCash DECIMAL(18, 2) NOT NULL DEFAULT 0,
    Revenue DECIMAL(18, 2) NOT NULL DEFAULT 0,
    TotalOrders INT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Open',
    FOREIGN KEY (StaffId) REFERENCES Users(Id),
    FOREIGN KEY (StoreId) REFERENCES Stores(Id)
);

-- Seed some mock data for history
INSERT INTO Shifts (StaffId, StoreId, ShiftName, StartTime, EndTime, InitialCash, Revenue, TotalOrders, Status)
VALUES 
(2, 1, N'Sáng', DATEADD(day, -1, GETDATE()), DATEADD(hour, 6, DATEADD(day, -1, GETDATE())), 500000, 10200000, 42, 'Closed'),
(2, 1, N'Chiều', DATEADD(hour, 6, DATEADD(day, -2, GETDATE())), DATEADD(hour, 14, DATEADD(day, -2, GETDATE())), 500000, 8950000, 35, 'Closed'),
(2, 1, N'Sáng', DATEADD(day, -3, GETDATE()), DATEADD(hour, 6, DATEADD(day, -3, GETDATE())), 500000, 11100000, 48, 'Closed'),
(2, 1, N'Chiều', DATEADD(hour, 6, DATEADD(day, -4, GETDATE())), DATEADD(hour, 14, DATEADD(day, -4, GETDATE())), 500000, 9200000, 38, 'Closed'),
(2, 1, N'Sáng', DATEADD(day, -5, GETDATE()), DATEADD(hour, 6, DATEADD(day, -5, GETDATE())), 500000, 10500000, 45, 'Closed');
