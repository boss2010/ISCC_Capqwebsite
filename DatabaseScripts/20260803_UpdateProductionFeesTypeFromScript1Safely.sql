USE [PlantQuarantine_New];
GO

/*
    تحديث رسوم الحساب الحكومي اعتماداً على نسخة Production الموجودة في script1.sql.

    لا يحذف هذا السكربت أي صف ولا يعيد إنشاء جدول FeesType.
    يتم تحديث 22 صفاً موجوداً بالـ IDs الحالية، وإضافة الصف الوحيد الناقص بالـ ID = 69.

    التشغيل الآمن:
      1) اترك @ApplyChanges = 0 وشغّل السكربت لمراجعة خطة التنفيذ.
      2) بعد مراجعة النتائج، غيّر @ApplyChanges إلى 1 وشغّله مرة أخرى للتنفيذ.

    السكربت قابل لإعادة التشغيل، وكل التعديلات الفعلية تتم داخل Transaction واحدة.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ApplyChanges bit = 0; -- 0 = معاينة فقط، 1 = تنفيذ فعلي

IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
    THROW 50000, 'Table dbo.FeesType was not found.', 1;

IF COL_LENGTH(N'dbo.FeesType', N'Account_Type') IS NULL
   OR COL_LENGTH(N'dbo.FeesType', N'Full_Name') IS NULL
   OR COL_LENGTH(N'dbo.FeesType', N'Price') IS NULL
   OR COL_LENGTH(N'dbo.FeesType', N'Display_Order') IS NULL
BEGIN
    THROW 50001, 'FeesType does not contain the required columns.', 1;
END;

DECLARE @Fees TABLE
(
    ID tinyint NOT NULL PRIMARY KEY,
    CurrentName nvarchar(250) NOT NULL,
    TargetName nvarchar(250) NOT NULL,
    Price money NULL,
    DisplayOrder int NOT NULL UNIQUE
);

INSERT INTO @Fees (ID, CurrentName, TargetName, Price, DisplayOrder)
VALUES
    (50, N'رسوم فحص', N'رسوم فحص', NULL, 1),
    (68, N'نفقات نوباتجيه', N'نفقات نوباتجيه', NULL, 2),
    (22, N'نفقات تبخير', N'نفقات تبخير', NULL, 3),
    (23, N'نفقات تطهير', N'نفقات تطهير', NULL, 4),
    (52, N'نفقات استخراج شهاده زراعيه', N'نفقات استخراج شهاده زراعيه', 100, 5),
    (67, N'نفقات ملحق شهاده زراعيه', N'نفقات ملحق شهاده زراعيه', 50, 6),
    (62, N'نفقات صوره شهاده زراعيه', N'نفقات صوره شهاده زراعيه', 20, 7),
    (65, N'نفقات صوره ملحق شهاده زراعيه', N'نفقات صوره ملحق شهاده زراعيه', 20, 8),
    (53, N'نفقات بدل تالف شهاده زراعيه', N'نفقات بدل تالف شهاده زراعيه', 500, 9),
    (66, N'نفقات ملحق بدل تالف شهاده زراعيه', N'نفقات بدل تالف ملحق شهاده زراعيه', 250, 10),
    (64, N'نفقات صوره ملحق بدل تالف شهاده زراعيه', N'نفقات صوره بدل تالف ملحق شهاده زراعيه', 50, 12),
    (60, N'نفقات صوره اذن استيراد', N'نفقات صوره اذن استيراد', 250, 13),
    (59, N'نفقات شهاده معالجه', N'نفقات شهاده معالجه', 100, 14),
    (63, N'نفقات صوره شهاده معالجه', N'نفقات صوره شهاده معالجه', 50, 15),
    (54, N'نفقات بدل تالف شهاده معالجه', N'نفقات بدل تالف شهاده معالجه', 200, 16),
    (61, N'نفقات صوره بدل تالف شهاده معالجه', N'نفقات صوره بدل تالف شهاده معالجه', 100, 17),
    (58, N'نفقات شهاده اعتماد منشأه', N'نفقات شهاده اعتماد منشأه', 200, 18),
    (51, N'نفقات استخراج سجل متابعه معالجه', N'نفقات سجل متابعه معالجه', 300, 19),
    (55, N'نفقات تحرير استماره فحص', N'نفقات تحرير استماره فحص', 10, 20),
    (57, N'نفقات سحب عينه', N'نفقات سحب عينه', 200, 21),
    (26, N'نفقات معاينة', N'نفقات معاينه', 500, 22),
    (56, N'نفقات دمغه الشهيد', N'نفقات دمغه الشهيد', 5, 23);

DECLARE @MissingFeeId tinyint = 69;
DECLARE @MissingFeeName nvarchar(250) = N'نفقات صوره بدل تالف شهاده زراعيه';

/* يجب أن تكون كل IDs المتفق عليها موجودة. */
IF EXISTS
(
    SELECT 1
    FROM @Fees AS expected
    LEFT JOIN dbo.FeesType AS actual ON actual.ID = expected.ID
    WHERE actual.ID IS NULL
)
BEGIN
    SELECT expected.ID, expected.CurrentName AS ExpectedName
    FROM @Fees AS expected
    LEFT JOIN dbo.FeesType AS actual ON actual.ID = expected.ID
    WHERE actual.ID IS NULL;

    THROW 50002, 'One or more expected FeesType IDs are missing. No changes were made.', 1;
END;

/* منع تحديث صف غير متوقع إذا تغير محتوى Production بعد استخراج script1.sql. */
IF EXISTS
(
    SELECT 1
    FROM @Fees AS expected
    INNER JOIN dbo.FeesType AS actual ON actual.ID = expected.ID
    WHERE LTRIM(RTRIM(actual.Name_Ar)) NOT IN (expected.CurrentName, expected.TargetName)
)
BEGIN
    SELECT
        expected.ID,
        expected.CurrentName AS ExpectedCurrentName,
        expected.TargetName,
        actual.Name_Ar AS ActualName
    FROM @Fees AS expected
    INNER JOIN dbo.FeesType AS actual ON actual.ID = expected.ID
    WHERE LTRIM(RTRIM(actual.Name_Ar)) NOT IN (expected.CurrentName, expected.TargetName);

    THROW 50003, 'An expected ID contains a different fee. No changes were made.', 1;
END;

/* البند الناقص إما غير موجود، أو موجود بالفعل بالـ ID المحجوز له عند إعادة التشغيل. */
IF EXISTS
(
    SELECT 1
    FROM dbo.FeesType
    WHERE (Name_Ar = @MissingFeeName OR Full_Name = @MissingFeeName)
      AND ID <> @MissingFeeId
)
BEGIN
    SELECT ID, Name_Ar, Full_Name
    FROM dbo.FeesType
    WHERE (Name_Ar = @MissingFeeName OR Full_Name = @MissingFeeName)
      AND ID <> @MissingFeeId;

    THROW 50004, 'The missing fee now exists under another ID. No changes were made.', 1;
END;

IF EXISTS
(
    SELECT 1
    FROM dbo.FeesType
    WHERE ID = @MissingFeeId
      AND LTRIM(RTRIM(Name_Ar)) <> @MissingFeeName
)
BEGIN
    SELECT ID, Name_Ar, Full_Name
    FROM dbo.FeesType
    WHERE ID = @MissingFeeId;

    THROW 50005, 'ID 69 is occupied by another fee. No changes were made.', 1;
END;

/* منع تكرار ترتيب ظاهر من صفوف أخرى غير الصفوف المستهدفة. */
IF EXISTS
(
    SELECT 1
    FROM dbo.FeesType AS actual
    WHERE actual.Account_Type = 138
      AND actual.Display_Order BETWEEN 1 AND 23
      AND actual.ID <> @MissingFeeId
      AND NOT EXISTS (SELECT 1 FROM @Fees AS expected WHERE expected.ID = actual.ID)
)
BEGIN
    SELECT ID, Name_Ar, Account_Type, Price, Display_Order
    FROM dbo.FeesType AS actual
    WHERE actual.Account_Type = 138
      AND actual.Display_Order BETWEEN 1 AND 23
      AND actual.ID <> @MissingFeeId
      AND NOT EXISTS (SELECT 1 FROM @Fees AS expected WHERE expected.ID = actual.ID)
    ORDER BY Display_Order;

    THROW 50006, 'Another fee already uses a government display order from 1 to 23. No changes were made.', 1;
END;

/* وضع المعاينة: لا توجد أي كتابة في قاعدة البيانات. */
IF @ApplyChanges = 0
BEGIN
    SELECT
        expected.ID,
        actual.Name_Ar AS CurrentName,
        expected.TargetName,
        actual.Account_Type AS CurrentAccountType,
        CAST(138 AS int) AS TargetAccountType,
        actual.Price AS CurrentPrice,
        expected.Price AS TargetPrice,
        actual.Display_Order AS CurrentDisplayOrder,
        expected.DisplayOrder AS TargetDisplayOrder,
        N'UPDATE' AS PlannedAction
    FROM @Fees AS expected
    INNER JOIN dbo.FeesType AS actual ON actual.ID = expected.ID

    UNION ALL

    SELECT
        @MissingFeeId,
        actual.Name_Ar,
        @MissingFeeName,
        actual.Account_Type,
        138,
        actual.Price,
        CONVERT(money, 50),
        actual.Display_Order,
        11,
        CASE WHEN actual.ID IS NULL THEN N'INSERT' ELSE N'UPDATE' END
    FROM (SELECT 1 AS Dummy) AS source
    LEFT JOIN dbo.FeesType AS actual ON actual.ID = @MissingFeeId

    ORDER BY TargetDisplayOrder;

    PRINT N'Preview only: no database changes were made. Set @ApplyChanges = 1 to apply.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE actual
    SET
        actual.Name_Ar = expected.TargetName,
        actual.Full_Name = expected.TargetName,
        actual.Account_Type = 138,
        actual.Price = expected.Price,
        actual.Display_Order = expected.DisplayOrder
    FROM dbo.FeesType AS actual
    INNER JOIN @Fees AS expected ON expected.ID = actual.ID;

    IF NOT EXISTS (SELECT 1 FROM dbo.FeesType WHERE ID = @MissingFeeId)
    BEGIN
        INSERT INTO dbo.FeesType
        (
            ID,
            Name_Ar,
            Name_En,
            IsActive,
            User_Updation_Id,
            User_Updation_Date,
            User_Deletion_Id,
            User_Deletion_Date,
            User_Creation_Id,
            User_Creation_Date,
            Account_Type,
            Full_Name,
            Price,
            Display_Order
        )
        VALUES
        (
            @MissingFeeId,
            @MissingFeeName,
            NULL,
            1,
            NULL,
            NULL,
            NULL,
            NULL,
            1,
            GETDATE(),
            138,
            @MissingFeeName,
            50,
            11
        );
    END
    ELSE
    BEGIN
        UPDATE dbo.FeesType
        SET
            Name_Ar = @MissingFeeName,
            Full_Name = @MissingFeeName,
            Account_Type = 138,
            Price = 50,
            Display_Order = 11
        WHERE ID = @MissingFeeId;
    END;

    /* تحقق كامل قبل تثبيت الـ Transaction. */
    IF EXISTS
    (
        SELECT 1
        FROM @Fees AS expected
        INNER JOIN dbo.FeesType AS actual ON actual.ID = expected.ID
        WHERE actual.Name_Ar <> expected.TargetName
           OR ISNULL(actual.Full_Name, N'') <> expected.TargetName
           OR actual.Account_Type <> 138
           OR actual.Display_Order <> expected.DisplayOrder
           OR (actual.Price <> expected.Price)
           OR (actual.Price IS NULL AND expected.Price IS NOT NULL)
           OR (actual.Price IS NOT NULL AND expected.Price IS NULL)
    )
    BEGIN
        THROW 50007, 'Verification of the 22 updated fees failed.', 1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.FeesType
        WHERE ID = @MissingFeeId
          AND Name_Ar = @MissingFeeName
          AND Full_Name = @MissingFeeName
          AND Account_Type = 138
          AND Price = 50
          AND Display_Order = 11
    )
    BEGIN
        THROW 50008, 'Verification of the inserted fee failed.', 1;
    END;

    IF
    (
        SELECT COUNT(*)
        FROM dbo.FeesType
        WHERE Account_Type = 138
          AND Display_Order BETWEEN 1 AND 23
    ) <> 23
    BEGIN
        THROW 50009, 'The final government fee list does not contain exactly 23 rows.', 1;
    END;

    IF EXISTS
    (
        SELECT Display_Order
        FROM dbo.FeesType
        WHERE Account_Type = 138
          AND Display_Order BETWEEN 1 AND 23
        GROUP BY Display_Order
        HAVING COUNT(*) <> 1
    )
    BEGIN
        THROW 50010, 'Duplicate government display orders were detected.', 1;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

SELECT ID, Name_Ar, Full_Name, Account_Type, Price, Display_Order
FROM dbo.FeesType
WHERE Account_Type = 138
  AND Display_Order BETWEEN 1 AND 23
ORDER BY Display_Order;

PRINT N'Government fees were updated successfully. No FeesType rows were deleted.';
GO
