USE [PlantQuarantine_New];
GO

/*
    تجهيز أعمدة FeesType على Production فقط.
    لا يحذف الجدول، لا يغير IDs، ولا يعيد توزيع الرسوم على الحسابات.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
        THROW 50000, 'Table dbo.FeesType was not found.', 1;

    IF COL_LENGTH(N'dbo.FeesType', N'Account_Type') IS NULL
    BEGIN
        ALTER TABLE dbo.FeesType
        ADD Account_Type int NOT NULL
            CONSTRAINT DF_FeesType_Account_Type_Production DEFAULT (-1) WITH VALUES;
    END;

    IF COL_LENGTH(N'dbo.FeesType', N'Full_Name') IS NULL
    BEGIN
        ALTER TABLE dbo.FeesType
        ADD Full_Name nvarchar(250) NULL;
    END;

    IF COL_LENGTH(N'dbo.FeesType', N'Price') IS NULL
    BEGIN
        ALTER TABLE dbo.FeesType
        ADD Price money NULL;
    END;

    IF COL_LENGTH(N'dbo.FeesType', N'Display_Order') IS NULL
    BEGIN
        ALTER TABLE dbo.FeesType
        ADD Display_Order int NULL;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

SELECT
    columnInfo.name AS Column_Name,
    TYPE_NAME(columnInfo.user_type_id) AS Data_Type,
    columnInfo.max_length AS Max_Length,
    columnInfo.is_nullable AS Is_Nullable
FROM sys.columns AS columnInfo
WHERE columnInfo.object_id = OBJECT_ID(N'dbo.FeesType')
  AND columnInfo.name IN
      (N'Account_Type', N'Full_Name', N'Price', N'Display_Order')
ORDER BY columnInfo.name;

PRINT 'FeesType production schema preparation completed successfully.';
GO
