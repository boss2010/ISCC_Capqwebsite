USE [PlantQuarantine_New];
GO

/*
    إضافة نوع رسوم الميكنة للدفع الحكومي والخاص بدون حذف أو تعديل أي رسم آخر.
    يظل Display_Order فارغًا حتى لا يظهر كرسم يختاره المستخدم؛ التطبيق يحسبه تلقائيًا.
    السكربت قابل لإعادة التشغيل بأمان.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
    THROW 50000, 'Table dbo.FeesType was not found.', 1;

IF COL_LENGTH(N'dbo.FeesType', N'Account_Type') IS NULL OR
   COL_LENGTH(N'dbo.FeesType', N'Price') IS NULL OR
   COL_LENGTH(N'dbo.FeesType', N'Display_Order') IS NULL
    THROW 50001, 'Required FeesType columns are missing.', 1;

DECLARE @MatchCount int;
DECLARE @MechanizationFeeId tinyint;

SELECT
    @MatchCount = COUNT(*),
    @MechanizationFeeId = MIN(ID)
FROM dbo.FeesType
WHERE Name_Ar = N'رسوم ميكنة'
   OR Full_Name = N'رسوم ميكنة';

IF @MatchCount > 1
    THROW 50002, 'Duplicate mechanization fee rows were found. No changes were made.', 1;

IF @MatchCount = 0
BEGIN
    ;WITH Numbers AS
    (
        SELECT 1 AS ID
        UNION ALL
        SELECT ID + 1 FROM Numbers WHERE ID < 255
    )
    SELECT TOP (1) @MechanizationFeeId = CONVERT(tinyint, Numbers.ID)
    FROM Numbers
    LEFT JOIN dbo.FeesType existing ON existing.ID = Numbers.ID
    WHERE existing.ID IS NULL
    ORDER BY Numbers.ID
    OPTION (MAXRECURSION 255);

    IF @MechanizationFeeId IS NULL
        THROW 50003, 'No available tinyint ID exists in FeesType.', 1;

    INSERT INTO dbo.FeesType
    (
        ID,
        Name_Ar,
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
        @MechanizationFeeId,
        N'رسوم ميكنة',
        N'رسوم ميكنة',
        0,
        NULL,
        NULL,
        1,
        1,
        GETDATE()
    );
END
ELSE
BEGIN
    UPDATE dbo.FeesType
    SET
        Name_Ar = N'رسوم ميكنة',
        Full_Name = N'رسوم ميكنة',
        Account_Type = 0,
        Price = NULL,
        Display_Order = NULL,
        IsActive = 1,
        User_Deletion_Id = NULL,
        User_Deletion_Date = NULL
    WHERE ID = @MechanizationFeeId;
END;

COMMIT TRANSACTION;

SELECT
    ID,
    Name_Ar,
    Full_Name,
    Account_Type,
    Price,
    Display_Order,
    IsActive
FROM dbo.FeesType
WHERE ID = @MechanizationFeeId;
GO
