USE [PlantQuarantine_New];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
    THROW 50000, 'Table dbo.FeesType was not found.', 1;

IF COL_LENGTH(N'dbo.FeesType', N'Account_Type') IS NULL
    THROW 50001, 'Run 20260727_AddFeesTypeAccountAndFullName.sql first.', 1;

IF COL_LENGTH(N'dbo.FeesType', N'Full_Name') IS NULL
    THROW 50002, 'Column dbo.FeesType.Full_Name was not found.', 1;
GO

CREATE TABLE #GovernmentFees
(
    RequestedName nvarchar(250) NOT NULL PRIMARY KEY
);

INSERT INTO #GovernmentFees (RequestedName)
VALUES
    (N'رسوم فحص'),
    (N'نفقات نوباتجيه'),
    (N'نفقات تبخير'),
    (N'نفقات تطهير'),
    (N'نفقات استخراج شهاده زراعيه'),
    (N'نفقات ملحق شهاده زراعيه'),
    (N'نفقات صوره شهاده زراعيه'),
    (N'نفقات صوره ملحق شهاده زراعيه'),
    (N'نفقات بدل تالف شهاده زراعيه'),
    (N'نفقات ملحق بدل تالف شهاده زراعيه'),
    (N'نفقات صوره ملحق بدل تالف شهاده زراعيه'),
    (N'نفقات صوره اذن استيراد'),
    (N'نفقات شهاده معالجه'),
    (N'نفقات صوره شهاده معالجه'),
    (N'نفقات بدل تالف شهاده معالجه'),
    (N'نفقات صوره بدل تالف شهاده معالجه'),
    (N'نفقات شهاده اعتماد منشأه'),
    (N'نفقات استخراج سجل متابعه معالجه'),
    (N'نفقات تحرير استماره فحص'),
    (N'نفقات سحب عينه'),
    (N'نفقات دمغه الشهيد');

BEGIN TRANSACTION;

/* لو الاسم موجود بالفعل: فعّله واربطه بالحساب الحكومي */
UPDATE fee
SET
    fee.Account_Type = 138,
    fee.IsActive = 1,
    fee.User_Deletion_Id = NULL,
    fee.User_Deletion_Date = NULL,
    fee.Full_Name = requested.RequestedName
FROM dbo.FeesType AS fee
INNER JOIN #GovernmentFees AS requested
    ON LTRIM(RTRIM(fee.Name_Ar)) = requested.RequestedName
    OR LTRIM(RTRIM(fee.Full_Name)) = requested.RequestedName;

DECLARE @IsIdentity bit =
    CONVERT(bit, COLUMNPROPERTY(
        OBJECT_ID(N'dbo.FeesType'),
        N'ID',
        'IsIdentity'));

/*
لو ID Identity لا نرسله.
لو ليس Identity نولّد IDs بعد أكبر ID حالي داخل transaction.
*/
IF @IsIdentity = 1
BEGIN
    INSERT INTO dbo.FeesType
    (
        Name_Ar,
        Name_En,
        Full_Name,
        Account_Type,
        IsActive,
        User_Creation_Id,
        User_Creation_Date
    )
    SELECT
        LEFT(requested.RequestedName, 50),
        NULL,
        requested.RequestedName,
        138,
        1,
        1,
        GETDATE()
    FROM #GovernmentFees AS requested
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.FeesType AS fee
        WHERE
            LTRIM(RTRIM(fee.Name_Ar)) = requested.RequestedName
            OR LTRIM(RTRIM(fee.Full_Name)) = requested.RequestedName
    );
END;
ELSE
BEGIN
    DECLARE @CurrentMaximumId int;
    DECLARE @NewRowsCount int;

    SELECT @CurrentMaximumId = ISNULL(MAX(CONVERT(int, ID)), 0)
    FROM dbo.FeesType WITH (TABLOCKX, HOLDLOCK);

    SELECT @NewRowsCount = COUNT(*)
    FROM #GovernmentFees AS requested
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.FeesType AS fee
        WHERE
            LTRIM(RTRIM(fee.Name_Ar)) = requested.RequestedName
            OR LTRIM(RTRIM(fee.Full_Name)) = requested.RequestedName
    );

    IF @CurrentMaximumId + @NewRowsCount > 255
        THROW 50003, 'There are not enough available tinyint IDs in dbo.FeesType.', 1;

    ;WITH MissingFees AS
    (
        SELECT
            requested.RequestedName,
            ROW_NUMBER() OVER (ORDER BY requested.RequestedName) AS RowNumber
        FROM #GovernmentFees AS requested
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM dbo.FeesType AS fee
            WHERE
                LTRIM(RTRIM(fee.Name_Ar)) = requested.RequestedName
                OR LTRIM(RTRIM(fee.Full_Name)) = requested.RequestedName
        )
    )
    INSERT INTO dbo.FeesType
    (
        ID,
        Name_Ar,
        Name_En,
        Full_Name,
        Account_Type,
        IsActive,
        User_Creation_Id,
        User_Creation_Date
    )
    SELECT
        CONVERT(tinyint, @CurrentMaximumId + RowNumber),
        LEFT(RequestedName, 50),
        NULL,
        RequestedName,
        138,
        1,
        1,
        GETDATE()
    FROM MissingFees;
END;

COMMIT TRANSACTION;

/* النتيجة: يجب أن تظهر الأسماء كلها بالحساب 138 */
SELECT
    fee.ID,
    fee.Name_Ar,
    fee.Full_Name,
    fee.Account_Type,
    fee.IsActive
FROM dbo.FeesType AS fee
INNER JOIN #GovernmentFees AS requested
    ON LTRIM(RTRIM(fee.Name_Ar)) = requested.RequestedName
    OR LTRIM(RTRIM(fee.Full_Name)) = requested.RequestedName
ORDER BY fee.ID;

DROP TABLE #GovernmentFees;
GO
