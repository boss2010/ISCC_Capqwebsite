USE [PlantQuarantine_New];

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
    BEGIN
        THROW 50000, 'Table dbo.FeesType was not found.', 1;
    END;

    IF COL_LENGTH(N'dbo.FeesType', N'Price') IS NULL
    BEGIN
        ALTER TABLE dbo.FeesType
        ADD Price money NULL;
    END;

    DECLARE @ResetInitialDisplayOrder bit = 0;

    IF COL_LENGTH(N'dbo.FeesType', N'Display_Order') IS NULL
    BEGIN
        ALTER TABLE dbo.FeesType
        ADD Display_Order int NULL;
    END;
    ELSE
    BEGIN
        /* Remove the NOT NULL/default introduced by the first version of this script. */
        IF EXISTS
        (
            SELECT 1
            FROM sys.default_constraints dc
            INNER JOIN sys.columns c
                ON c.object_id = dc.parent_object_id
               AND c.column_id = dc.parent_column_id
            WHERE dc.parent_object_id = OBJECT_ID(N'dbo.FeesType')
              AND c.name = N'Display_Order'
              AND dc.name = N'DF_FeesType_Display_Order'
        )
        BEGIN
            SET @ResetInitialDisplayOrder = 1;
            ALTER TABLE dbo.FeesType
            DROP CONSTRAINT DF_FeesType_Display_Order;
        END;

        ALTER TABLE dbo.FeesType
        ALTER COLUMN Display_Order int NULL;

        IF @ResetInitialDisplayOrder = 1
        BEGIN
            EXEC sys.sp_executesql N'
                UPDATE dbo.FeesType
                SET Display_Order = NULL
                WHERE Display_Order = CONVERT(int, ID);';
        END;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

EXEC sys.sp_executesql N'
    SELECT
        ID,
        Name_Ar,
        Price,
        Display_Order
    FROM dbo.FeesType
    ORDER BY Display_Order, ID;';
