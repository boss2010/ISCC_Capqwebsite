USE [PlantQuarantine_New];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.Fees_Altahsil', N'U') IS NULL
    THROW 50000, 'Table dbo.Fees_Altahsil was not found.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dbo.Fees_Altahsil', N'Department') IS NULL
    BEGIN
        ALTER TABLE dbo.Fees_Altahsil
            ADD Department nvarchar(250) NULL;
    END;

    IF COL_LENGTH(N'dbo.Fees_Altahsil', N'Item') IS NULL
    BEGIN
        ALTER TABLE dbo.Fees_Altahsil
            ADD Item nvarchar(250) NULL;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

SELECT
    columnInfo.name AS ColumnName,
    typeInfo.name AS DataType,
    columnInfo.max_length AS MaxLengthInBytes,
    columnInfo.is_nullable AS IsNullable
FROM sys.columns AS columnInfo
INNER JOIN sys.types AS typeInfo
    ON typeInfo.user_type_id = columnInfo.user_type_id
WHERE columnInfo.object_id = OBJECT_ID(N'dbo.Fees_Altahsil')
  AND columnInfo.name IN (N'Department', N'Item')
ORDER BY columnInfo.column_id;
GO
