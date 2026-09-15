USE [PlantQuarantine_New];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH(N'dbo.Fees_Altahsil', N'Used_By_User_Id') IS NULL
    BEGIN
        ALTER TABLE dbo.Fees_Altahsil
            ADD Used_By_User_Id smallint NULL;
    END;

    IF COL_LENGTH(N'dbo.Fees_Altahsil', N'Used_By_User_Name') IS NULL
    BEGIN
        ALTER TABLE dbo.Fees_Altahsil
            ADD Used_By_User_Name nvarchar(250) NULL;
    END;
    ELSE IF EXISTS
    (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.Fees_Altahsil')
          AND name = N'Used_By_User_Name'
          AND max_length < 500
    )
    BEGIN
        ALTER TABLE dbo.Fees_Altahsil
            ALTER COLUMN Used_By_User_Name nvarchar(250) NULL;
    END;

    COMMIT TRANSACTION;

    SELECT
        columnInfo.name AS ColumnName,
        TYPE_NAME(columnInfo.user_type_id) AS DataType,
        columnInfo.max_length AS MaxLength,
        columnInfo.is_nullable AS IsNullable
    FROM sys.columns AS columnInfo
    WHERE columnInfo.object_id = OBJECT_ID(N'dbo.Fees_Altahsil')
      AND columnInfo.name IN (N'Used_By_User_Id', N'Used_By_User_Name')
    ORDER BY columnInfo.column_id;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
