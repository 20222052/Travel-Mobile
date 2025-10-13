-- =====================================================================
-- Script to update Order table with missing columns
-- This script adds TourId, Email, Gender columns to Order table
-- Status values: 0=Chờ xác nhận, 1=Đã xác nhận, 2=Đang giao, 3=Đã giao, 4=Đã hủy
-- =====================================================================

USE Travel;
GO

PRINT '========================================';
PRINT 'Starting Order table update...';
PRINT '========================================';

-- Check if columns exist before adding them
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[Order]') AND name = 'TourId')
BEGIN
    ALTER TABLE [Order] ADD TourId INT NULL;
    PRINT '✓ Added TourId column';
    
    -- Add foreign key constraint
    ALTER TABLE [Order] ADD CONSTRAINT FK_Order_Tour FOREIGN KEY (TourId) REFERENCES Tour(Id);
    PRINT '✓ Added FK_Order_Tour foreign key constraint';
END
ELSE
BEGIN
    PRINT '✓ TourId column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[Order]') AND name = 'Email')
BEGIN
    ALTER TABLE [Order] ADD Email NVARCHAR(100) NULL;
    PRINT '✓ Added Email column';
END
ELSE
BEGIN
    PRINT '✓ Email column already exists';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[Order]') AND name = 'Gender')
BEGIN
    ALTER TABLE [Order] ADD Gender NVARCHAR(10) NULL;
    PRINT '✓ Added Gender column';
END
ELSE
BEGIN
    PRINT '✓ Gender column already exists';
END

PRINT '';
PRINT '========================================';
PRINT 'Order table structure:';
PRINT '========================================';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Order'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '========================================';
PRINT 'Order status values:';
PRINT '0 = Chờ xác nhận (Pending)';
PRINT '1 = Đã xác nhận (Confirmed)';
PRINT '2 = Đang giao (Shipping)';
PRINT '3 = Đã giao (Delivered)';
PRINT '4 = Đã hủy (Cancelled)';
PRINT '========================================';
PRINT '';
PRINT '✓ Order table update completed successfully!';
GO
