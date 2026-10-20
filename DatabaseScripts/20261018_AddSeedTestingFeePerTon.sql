SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
        THROW 50000, 'Table dbo.FeesType was not found.', 1;

    IF OBJECT_ID(N'dbo.Fees_Altahsil_Detiles', N'U') IS NULL
        THROW 50001, 'Table dbo.Fees_Altahsil_Detiles was not found.', 1;

    IF COL_LENGTH(N'dbo.Fees_Altahsil_Detiles', N'Quantity') IS NULL
        THROW 50002, 'Column dbo.Fees_Altahsil_Detiles.Quantity was not found.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.columns AS columnsInfo
        INNER JOIN sys.types AS typesInfo
            ON typesInfo.user_type_id = columnsInfo.user_type_id
        WHERE columnsInfo.object_id = OBJECT_ID(N'dbo.Fees_Altahsil_Detiles')
          AND columnsInfo.name = N'Quantity'
          AND typesInfo.name = N'decimal'
          AND columnsInfo.precision = 18
          AND columnsInfo.scale = 3
    )
        ALTER TABLE dbo.Fees_Altahsil_Detiles
            ALTER COLUMN Quantity decimal(18, 3) NULL;

    -- Compatibility cleanup in case the earlier separate-column script
    -- was applied to any environment before this unified design.
    IF COL_LENGTH(N'dbo.Fees_Altahsil_Detiles', N'Quantity_Ton') IS NOT NULL
    BEGIN
        EXEC sys.sp_executesql N'
            UPDATE dbo.Fees_Altahsil_Detiles
            SET Quantity = COALESCE(Quantity_Ton, Quantity)
            WHERE Quantity_Ton IS NOT NULL;';

        ALTER TABLE dbo.Fees_Altahsil_Detiles
            DROP COLUMN Quantity_Ton;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.FeesType
        WHERE ID = 83
          AND ISNULL(Name_Ar, N'') <> N'إختبارات وخدمات التقاوى'
          AND ISNULL(Full_Name, N'') <> N'إختبارات وخدمات التقاوى'
    )
        THROW 50003, 'Fee ID 83 is occupied by another fee. No changes were made.', 1;

    IF EXISTS (SELECT 1 FROM dbo.FeesType WHERE ID = 83)
    BEGIN
        UPDATE dbo.FeesType
        SET Name_Ar = N'إختبارات وخدمات التقاوى',
            Full_Name = N'إختبارات وخدمات التقاوى',
            Account_Type = 139,
            Price = 15.50,
            Display_Order = 22,
            IsActive = 1,
            User_Deletion_Id = NULL,
            User_Deletion_Date = NULL,
            User_Updation_Date = GETDATE()
        WHERE ID = 83;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.FeesType
        (
            ID,
            Name_Ar,
            Name_En,
            Full_Name,
            Account_Type,
            Price,
            Display_Order,
            IsActive,
            User_Creation_Id,
            User_Creation_Date
        )
        VALUES
        (
            83,
            N'إختبارات وخدمات التقاوى',
            NULL,
            N'إختبارات وخدمات التقاوى',
            139,
            15.50,
            22,
            1,
            1,
            GETDATE()
        );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    ID,
    Name_Ar,
    Full_Name,
    Account_Type,
    Price,
    Display_Order,
    IsActive
FROM dbo.FeesType
WHERE ID = 83;

SELECT
    typesInfo.name AS QuantityDataType,
    columnsInfo.precision AS QuantityPrecision,
    columnsInfo.scale AS QuantityScale
FROM sys.columns AS columnsInfo
INNER JOIN sys.types AS typesInfo
    ON typesInfo.user_type_id = columnsInfo.user_type_id
WHERE columnsInfo.object_id = OBJECT_ID(N'dbo.Fees_Altahsil_Detiles')
  AND columnsInfo.name = N'Quantity';
