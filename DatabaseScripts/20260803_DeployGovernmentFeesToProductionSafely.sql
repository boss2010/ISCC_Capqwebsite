USE [PlantQuarantine_New];
GO

/*
    نشر إعدادات الرسوم الحكومية على Production بدون حذف FeesType أو تغيير IDs الحالية.

    طريقة الاستخدام الآمنة:
    1) خذ Full Backup لقاعدة البيانات.
    2) شغّل أولاً:
       - 20260727_AddFeesTypeAccountAndFullName.sql
       - 20260729_AddPriceAndDisplayOrderToFeesType.sql
    3) اترك @ApplyChanges = 0 وشغّل هذا الملف لمراجعة خطة التنفيذ.
    4) بعد التأكد من عمود Action وعدم وجود أخطاء، غيّر القيمة إلى 1 وأعد التشغيل.

    ملاحظات:
    - لا يتم حذف أي صف.
    - الصف الموجود يُطابق بالاسم ويحتفظ بالـ ID الخاص به.
    - الصف الناقص فقط يتم إدخاله في ID متاح.
    - رسم دمغة الشهيد الحكومي يجب أن يكون ID = 56 لأن التطبيق يعتمد عليه حاليًا.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ApplyChanges bit = 0; -- 0 = معاينة فقط، 1 = تنفيذ فعلي

IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
    THROW 50000, 'Table dbo.FeesType was not found.', 1;

IF COL_LENGTH(N'dbo.FeesType', N'Full_Name') IS NULL
    THROW 50001, 'Column Full_Name is missing. Run 20260727_AddFeesTypeAccountAndFullName.sql first.', 1;

IF COL_LENGTH(N'dbo.FeesType', N'Account_Type') IS NULL
    THROW 50002, 'Column Account_Type is missing. Run 20260727_AddFeesTypeAccountAndFullName.sql first.', 1;

IF COL_LENGTH(N'dbo.FeesType', N'Price') IS NULL
    THROW 50003, 'Column Price is missing. Run 20260729_AddPriceAndDisplayOrderToFeesType.sql first.', 1;

IF COL_LENGTH(N'dbo.FeesType', N'Display_Order') IS NULL
    THROW 50004, 'Column Display_Order is missing. Run 20260729_AddPriceAndDisplayOrderToFeesType.sql first.', 1;

DECLARE @Definitions TABLE
(
    CanonicalName nvarchar(250) NOT NULL PRIMARY KEY,
    AlternateName1 nvarchar(250) NULL,
    AlternateName2 nvarchar(250) NULL,
    Price money NULL,
    DisplayOrder int NOT NULL UNIQUE,
    PreferredId tinyint NULL,
    RequiredExactId bit NOT NULL DEFAULT (0)
);

INSERT INTO @Definitions
    (CanonicalName, AlternateName1, AlternateName2, Price, DisplayOrder, PreferredId, RequiredExactId)
VALUES
    (N'رسوم فحص', NULL, NULL, NULL, 1, 50, 0),
    (N'نفقات نوباتجيه', N'نفقات نوباتجية', NULL, NULL, 2, 68, 0),
    (N'نفقات تبخير', NULL, NULL, NULL, 3, 22, 0),
    (N'نفقات تطهير', NULL, NULL, NULL, 4, 23, 0),
    (N'نفقات استخراج شهاده زراعيه', N'نفقات استخراج شهادة زراعية', NULL, 100, 5, 52, 0),
    (N'نفقات ملحق شهاده زراعيه', N'نفقات ملحق شهادة زراعية', NULL, 50, 6, 67, 0),
    (N'نفقات صوره شهاده زراعيه', N'نفقات صورة شهادة زراعية', NULL, 20, 7, 62, 0),
    (N'نفقات صوره ملحق شهاده زراعيه', N'نفقات صورة ملحق شهادة زراعية', NULL, 20, 8, 65, 0),
    (N'نفقات بدل تالف شهاده زراعيه', N'نفقات بدل تالف شهادة زراعية', NULL, 500, 9, 53, 0),
    (N'نفقات بدل تالف ملحق شهاده زراعيه', N'نفقات ملحق بدل تالف شهاده زراعيه', N'نفقات بدل تالف ملحق شهادة زراعية', 250, 10, 66, 0),
    (N'نفقات صوره بدل تالف شهاده زراعيه', N'نفقات صورة بدل تالف شهادة زراعية', NULL, 50, 11, NULL, 0),
    (N'نفقات صوره بدل تالف ملحق شهاده زراعيه', N'نفقات صوره ملحق بدل تالف شهاده زراعيه', N'نفقات صورة بدل تالف ملحق شهادة زراعية', 50, 12, 64, 0),
    (N'نفقات صوره اذن استيراد', N'نفقات صورة إذن استيراد', NULL, 250, 13, 60, 0),
    (N'نفقات شهاده معالجه', N'نفقات شهادة معالجة', NULL, 100, 14, 59, 0),
    (N'نفقات صوره شهاده معالجه', N'نفقات صورة شهادة معالجة', NULL, 50, 15, 63, 0),
    (N'نفقات بدل تالف شهاده معالجه', N'نفقات بدل تالف شهادة معالجة', NULL, 200, 16, 54, 0),
    (N'نفقات صوره بدل تالف شهاده معالجه', N'نفقات صورة بدل تالف شهادة معالجة', NULL, 100, 17, 61, 0),
    (N'نفقات شهاده اعتماد منشأه', N'نفقات شهادة اعتماد منشأة', NULL, 200, 18, 58, 0),
    (N'نفقات سجل متابعه معالجه', N'نفقات استخراج سجل متابعه معالجه', N'نفقات سجل متابعة معالجة', 300, 19, 51, 0),
    (N'نفقات تحرير استماره فحص', N'نفقات تحرير استمارة فحص', NULL, 10, 20, 55, 0),
    (N'نفقات سحب عينه', N'نفقات سحب عينة', NULL, 200, 21, 57, 0),
    (N'نفقات معاينه', N'نفقات معاينة', NULL, 500, 22, 26, 0),
    (N'نفقات دمغه الشهيد', N'نفقات دمغة الشهيد', N'رسوم صندوق الشهداء', 5, 23, 56, 1);

DECLARE @Plan TABLE
(
    CanonicalName nvarchar(250) NOT NULL PRIMARY KEY,
    ExistingId tinyint NULL,
    TargetId tinyint NULL,
    MatchCount int NOT NULL,
    Price money NULL,
    DisplayOrder int NOT NULL,
    PreferredId tinyint NULL,
    RequiredExactId bit NOT NULL,
    ActionName nvarchar(30) NULL
);

INSERT INTO @Plan
    (CanonicalName, ExistingId, MatchCount, Price, DisplayOrder, PreferredId, RequiredExactId)
SELECT
    definition.CanonicalName,
    matches.ExistingId,
    matches.MatchCount,
    definition.Price,
    definition.DisplayOrder,
    definition.PreferredId,
    definition.RequiredExactId
FROM @Definitions AS definition
CROSS APPLY
(
    SELECT
        COUNT(*) AS MatchCount,
        MIN(fee.ID) AS ExistingId
    FROM dbo.FeesType AS fee
    WHERE
        NULLIF(LTRIM(RTRIM(fee.Name_Ar)), N'') IN
            (definition.CanonicalName, definition.AlternateName1, definition.AlternateName2)
        OR NULLIF(LTRIM(RTRIM(fee.Full_Name)), N'') IN
            (definition.CanonicalName, definition.AlternateName1, definition.AlternateName2)
) AS matches;

IF EXISTS (SELECT 1 FROM @Plan WHERE MatchCount > 1)
BEGIN
    SELECT
        planRow.CanonicalName,
        planRow.MatchCount,
        fee.ID,
        fee.Name_Ar,
        fee.Full_Name,
        fee.Account_Type,
        fee.Price,
        fee.Display_Order,
        fee.IsActive,
        fee.User_Deletion_Id,
        CASE
            WHEN LTRIM(RTRIM(fee.Name_Ar)) = definition.CanonicalName
              OR LTRIM(RTRIM(fee.Full_Name)) = definition.CanonicalName
                THEN N'Canonical name'
            WHEN LTRIM(RTRIM(fee.Name_Ar)) = definition.AlternateName1
              OR LTRIM(RTRIM(fee.Full_Name)) = definition.AlternateName1
                THEN N'Alternate name 1'
            WHEN LTRIM(RTRIM(fee.Name_Ar)) = definition.AlternateName2
              OR LTRIM(RTRIM(fee.Full_Name)) = definition.AlternateName2
                THEN N'Alternate name 2'
            ELSE N'Unknown'
        END AS MatchReason
    FROM @Plan AS planRow
    INNER JOIN @Definitions AS definition
        ON definition.CanonicalName = planRow.CanonicalName
    INNER JOIN dbo.FeesType AS fee
        ON
            NULLIF(LTRIM(RTRIM(fee.Name_Ar)), N'') IN
                (definition.CanonicalName, definition.AlternateName1, definition.AlternateName2)
            OR NULLIF(LTRIM(RTRIM(fee.Full_Name)), N'') IN
                (definition.CanonicalName, definition.AlternateName1, definition.AlternateName2)
    WHERE planRow.MatchCount > 1
    ORDER BY planRow.CanonicalName, fee.ID;

    THROW 50005, 'Duplicate FeesType rows matched the same government fee. No changes were made.', 1;
END;

/* ID 56 عقد حالي مع التطبيق لرسم الشهيد الحكومي. */
IF EXISTS
(
    SELECT 1
    FROM @Plan
    WHERE RequiredExactId = 1
      AND ExistingId IS NOT NULL
      AND ExistingId <> PreferredId
)
BEGIN
    SELECT CanonicalName, ExistingId, PreferredId AS RequiredId
    FROM @Plan
    WHERE RequiredExactId = 1
      AND ExistingId IS NOT NULL
      AND ExistingId <> PreferredId;

    THROW 50006, 'Government martyr fee exists under a different ID. Application code and database must be aligned first.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM @Plan AS planRow
    INNER JOIN dbo.FeesType AS fee ON fee.ID = planRow.PreferredId
    WHERE planRow.RequiredExactId = 1
      AND planRow.ExistingId IS NULL
)
BEGIN
    SELECT fee.ID, fee.Name_Ar, fee.Full_Name
    FROM @Plan AS planRow
    INNER JOIN dbo.FeesType AS fee ON fee.ID = planRow.PreferredId
    WHERE planRow.RequiredExactId = 1
      AND planRow.ExistingId IS NULL;

    THROW 50007, 'Required government martyr fee ID 56 is occupied by another row. No changes were made.', 1;
END;

UPDATE @Plan
SET
    TargetId = ExistingId,
    ActionName = N'UPDATE'
WHERE ExistingId IS NOT NULL;

DECLARE
    @CanonicalName nvarchar(250),
    @PreferredId tinyint,
    @RequiredExactId bit,
    @SelectedId int;

DECLARE MissingFeeCursor CURSOR LOCAL FAST_FORWARD FOR
SELECT CanonicalName, PreferredId, RequiredExactId
FROM @Plan
WHERE ExistingId IS NULL
ORDER BY DisplayOrder;

OPEN MissingFeeCursor;
FETCH NEXT FROM MissingFeeCursor INTO @CanonicalName, @PreferredId, @RequiredExactId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @SelectedId = NULL;

    IF @PreferredId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.FeesType WHERE ID = @PreferredId)
       AND NOT EXISTS (SELECT 1 FROM @Plan WHERE TargetId = @PreferredId)
    BEGIN
        SET @SelectedId = CONVERT(int, @PreferredId);
    END;

    IF @SelectedId IS NULL AND @RequiredExactId = 1
        THROW 50008, 'A required exact FeesType ID is unavailable. No changes were made.', 1;

    IF @SelectedId IS NULL
    BEGIN
        ;WITH Numbers AS
        (
            SELECT 1 AS Number
            UNION ALL
            SELECT Number + 1
            FROM Numbers
            WHERE Number < 255
        )
        SELECT TOP (1) @SelectedId = Number
        FROM Numbers
        WHERE NOT EXISTS
            (SELECT 1 FROM dbo.FeesType WHERE ID = CONVERT(tinyint, Number))
          AND NOT EXISTS
            (SELECT 1 FROM @Plan WHERE TargetId = CONVERT(tinyint, Number))
        ORDER BY Number
        OPTION (MAXRECURSION 255);
    END;

    IF @SelectedId IS NULL
        THROW 50009, 'No free tinyint ID is available in dbo.FeesType.', 1;

    UPDATE @Plan
    SET
        TargetId = CONVERT(tinyint, @SelectedId),
        ActionName = N'INSERT'
    WHERE CanonicalName = @CanonicalName;

    FETCH NEXT FROM MissingFeeCursor INTO @CanonicalName, @PreferredId, @RequiredExactId;
END;

CLOSE MissingFeeCursor;
DEALLOCATE MissingFeeCursor;

/* تقرير المعاينة قبل أي تعديل. */
SELECT
    planRow.ActionName AS Action,
    planRow.TargetId AS ID,
    planRow.CanonicalName AS New_Name,
    fee.Name_Ar AS Current_Name,
    fee.Account_Type AS Current_Account_Type,
    fee.Price AS Current_Price,
    fee.Display_Order AS Current_Display_Order,
    138 AS New_Account_Type,
    planRow.Price AS New_Price,
    planRow.DisplayOrder AS New_Display_Order
FROM @Plan AS planRow
LEFT JOIN dbo.FeesType AS fee ON fee.ID = planRow.ExistingId
ORDER BY planRow.DisplayOrder;

IF @ApplyChanges = 0
BEGIN
    PRINT 'PREVIEW ONLY: No database changes were made. Set @ApplyChanges = 1 after reviewing the plan.';
    RETURN;
END;

DECLARE @IdentityInsertEnabled bit = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE fee
    SET
        fee.Name_Ar = LEFT(planRow.CanonicalName, 50),
        fee.Full_Name = planRow.CanonicalName,
        fee.Account_Type = 138,
        fee.Price = planRow.Price,
        fee.Display_Order = planRow.DisplayOrder,
        fee.IsActive = 1,
        fee.User_Deletion_Id = NULL,
        fee.User_Deletion_Date = NULL,
        fee.User_Updation_Id = 1,
        fee.User_Updation_Date = GETDATE()
    FROM dbo.FeesType AS fee
    INNER JOIN @Plan AS planRow ON planRow.ExistingId = fee.ID
    WHERE planRow.ActionName = N'UPDATE';

    IF EXISTS (SELECT 1 FROM @Plan WHERE ActionName = N'INSERT')
    BEGIN
        IF COLUMNPROPERTY(OBJECT_ID(N'dbo.FeesType'), N'ID', 'IsIdentity') = 1
        BEGIN
            SET IDENTITY_INSERT dbo.FeesType ON;
            SET @IdentityInsertEnabled = 1;
        END;

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
        SELECT
            planRow.TargetId,
            LEFT(planRow.CanonicalName, 50),
            NULL,
            planRow.CanonicalName,
            138,
            planRow.Price,
            planRow.DisplayOrder,
            1,
            1,
            GETDATE()
        FROM @Plan AS planRow
        WHERE planRow.ActionName = N'INSERT';

        IF @IdentityInsertEnabled = 1
        BEGIN
            SET IDENTITY_INSERT dbo.FeesType OFF;
            SET @IdentityInsertEnabled = 0;
        END;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.FeesType
        WHERE Account_Type = 138
          AND Display_Order BETWEEN 1 AND 23
          AND IsActive = 1
          AND User_Deletion_Id IS NULL
    ) <> 23
        THROW 50010, 'Final government fees count is not exactly 23. Transaction was rolled back.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.FeesType
        WHERE ID = 56
          AND Account_Type = 138
          AND Price = 5
          AND Display_Order = 23
          AND IsActive = 1
          AND User_Deletion_Id IS NULL
    )
        THROW 50011, 'Government martyr fee validation failed. Transaction was rolled back.', 1;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @IdentityInsertEnabled = 1
    BEGIN TRY
        SET IDENTITY_INSERT dbo.FeesType OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    IF XACT_STATE() <> 0
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
    IsActive,
    User_Deletion_Id
FROM dbo.FeesType
WHERE Account_Type = 138
  AND Display_Order BETWEEN 1 AND 23
ORDER BY Display_Order;

PRINT 'Government fees deployment completed successfully.';
GO
