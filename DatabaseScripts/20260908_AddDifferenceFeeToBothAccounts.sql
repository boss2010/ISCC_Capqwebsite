SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
        THROW 50000, 'Table dbo.FeesType was not found.', 1;

    IF OBJECT_ID(N'dbo.Fees_Altahsil_Detiles', N'U') IS NULL
        THROW 50001, 'Table dbo.Fees_Altahsil_Detiles was not found.', 1;

    IF COL_LENGTH(N'dbo.Fees_Altahsil_Detiles', N'Fee_Description') IS NULL
    BEGIN
        ALTER TABLE dbo.Fees_Altahsil_Detiles
            ADD Fee_Description nvarchar(250) NULL;
    END;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.FeesType
        WHERE ID = 81
          AND ISNULL(Name_Ar, N'') <> N'فرق رسوم'
          AND ISNULL(Full_Name, N'') <> N'فرق رسوم'
    )
        THROW 50002, 'Fee ID 81 is occupied by another fee. No changes were made.', 1;

    IF EXISTS (SELECT 1 FROM dbo.FeesType WHERE ID = 81)
    BEGIN
        UPDATE dbo.FeesType
        SET Name_Ar = N'فرق رسوم',
            Full_Name = N'فرق رسوم',
            Account_Type = 0,
            Price = NULL,
            Display_Order = 27,
            IsActive = 1,
            User_Deletion_Id = NULL,
            User_Deletion_Date = NULL,
            User_Updation_Date = GETDATE()
        WHERE ID = 81;
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
            81,
            N'فرق رسوم',
            NULL,
            N'فرق رسوم',
            0,
            NULL,
            27,
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
WHERE ID = 81;

SELECT
    COL_LENGTH(
        N'dbo.Fees_Altahsil_Detiles',
        N'Fee_Description') AS FeeDescriptionColumnLength;
