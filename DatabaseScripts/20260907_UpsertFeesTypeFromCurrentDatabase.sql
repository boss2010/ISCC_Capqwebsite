/*
    FeesType production upsert generated from the current configured database.
    Default mode is preview only. Review the result, take a backup, then set
    @ApplyChanges = 1 and run again against the intended production database.

    Matching key: ID
    Existing IDs: UPDATE
    Missing IDs:  INSERT
    Production-only IDs: preserved (nothing is deleted)
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ApplyChanges bit = 0;

DECLARE @Source TABLE
(
    ID tinyint NOT NULL PRIMARY KEY,
    Name_Ar nvarchar(50) NULL,
    Name_En varchar(50) NULL,
    Full_Name nvarchar(250) NULL,
    Account_Type int NOT NULL,
    Price money NULL,
    Display_Order int NULL,
    IsActive bit NOT NULL
);

INSERT INTO @Source
(
    ID,
    Name_Ar,
    Name_En,
    Full_Name,
    Account_Type,
    Price,
    Display_Order,
    IsActive
)
VALUES
    (1, N'ثابتة', N'Treatment', N'ثابتة', -1, NULL, NULL, 1),
    (2, N'معالجة', N'Sample', N'معالجة', -1, NULL, NULL, 1),
    (3, N'نوبتجية ', N'Committee', N'نوبتجية ', -1, NULL, NULL, 1),
    (4, N'سحب', N'station accredation', N'سحب', -1, NULL, NULL, 1),
    (5, N'نبات', N' item', N'نبات', -1, NULL, NULL, 1),
    (6, N'اسم مختصر', N'short name', N'اسم مختصر', -1, NULL, NULL, 1),
    (7, N'رسم  لجنة جيشني', N'Geshnay', N'رسم  لجنة جيشني', -1, NULL, NULL, 1),
    (8, N'رسوم طلب الفحص', N'check recoust', N'رسوم طلب الفحص', -1, NULL, NULL, 1),
    (9, N'رسوم النبات لو ثابتة', N'plant', N'رسوم النبات لو ثابتة', -1, NULL, NULL, 1),
    (10, N'رسوم النبات لو متغير', N'Exception plant', N'رسوم النبات لو متغير', -1, NULL, NULL, 1),
    (11, N'رسوم معالجه ', N'Sample plant', N'رسوم معالجه ', -1, NULL, NULL, 1),
    (12, N'طلب اول مره مزرعة', NULL, N'طلب اول مره مزرعة', -1, NULL, NULL, 1),
    (13, N'طلب تجديد مزرعة', NULL, N'طلب تجديد مزرعة', -1, NULL, NULL, 1),
    (14, N'اعادة تقييم مزرعة', NULL, N'اعادة تقييم مزرعة', -1, NULL, NULL, 1),
    (15, N'طلبات خاصة مزرعة', NULL, N'طلبات خاصة مزرعة', -1, NULL, NULL, 1),
    (16, N'زيارة اضافية مزرعة', NULL, N'زيارة اضافية مزرعة', -1, NULL, NULL, 1),
    (17, N'تظلم مزرعة', NULL, N'تظلم مزرعة', -1, NULL, NULL, 1),
    (18, N'مسمي اعتماد محطة', NULL, N'مسمي اعتماد محطة', -1, NULL, NULL, 1),
    (19, N'لجنة محطة', NULL, N'لجنة محطة', -1, NULL, NULL, 1),
    (20, N'رسوم الفحص', NULL, N'رسوم الفحص', 138, NULL, NULL, 1),
    (21, N'نفقات نوباتجية', NULL, N'نفقات نوباتجية', 138, NULL, NULL, 1),
    (22, N'نفقات تبخير', NULL, N'نفقات تبخير', 138, NULL, 3, 1),
    (23, N'نفقات تطهير', NULL, N'نفقات تطهير', 138, NULL, 4, 1),
    (24, N'رسوم استخراج شهادة زراعية', NULL, N'رسوم استخراج شهادة زراعية', 138, NULL, NULL, 1),
    (25, N'رسوم صورة شهادة زراعية', NULL, N'رسوم صورة شهادة زراعية', 138, NULL, NULL, 1),
    (26, N'نفقات معاينه', NULL, N'نفقات معاينه', 138, 500.0000, 22, 1),
    (27, N'نفقات سحب عينة', NULL, N'نفقات سحب عينة', 139, 200.0000, 14, 1),
    (28, N'نفقات إذن استيراد', NULL, N'نفقات إذن استيراد', 138, NULL, NULL, 1),
    (29, N'نفقات صورة إذن استيراد', NULL, N'نفقات صورة إذن استيراد', 138, NULL, NULL, 1),
    (30, N'نفقات تجديد إذن استيراد', NULL, N'نفقات تجديد إذن استيراد', 138, NULL, NULL, 1),
    (31, N'نفقات مرفق شهادة زراعية', NULL, N'نفقات مرفق شهادة زراعية', 138, NULL, NULL, 1),
    (32, N'نفقات إستمارة فحص', NULL, N'نفقات إستمارة فحص', 138, NULL, NULL, 1),
    (33, N'نفقات بدل تالف شهادة زراعية', NULL, N'نفقات بدل تالف شهادة زراعية', 138, NULL, NULL, 1),
    (34, N'رسوم صندوق شهداء', NULL, N'رسوم صندوق شهداء', 0, NULL, NULL, 1),
    (35, N'رسوم فحص عينة معملية ', NULL, N'رسوم فحص عينة معملية ', 139, NULL, NULL, 1),
    (36, N'اﻋﺘﻤﺎد اﻟـﻤﺰرﻋﺔ اﻷﻗﻞ ﻣﻦ ﺧﻤﺴﺔ أﻓﺪﻧﺔ ', NULL, N'اﻋﺘﻤﺎد اﻟـﻤﺰرﻋﺔ اﻷﻗﻞ ﻣﻦ ﺧﻤﺴﺔ أﻓﺪﻧﺔ ', 139, 1000.0000, 2, 1),
    (37, N'اﻋﺘﻤﺎد اﻟـﻤﺰرﻋﺔ أﻛﺒﺮ ﻣﻦ ﺧﻤﺴﺔ أﻓﺪﻧﺔ ﺣﺘﻰ ٢٠ ﻓﺪان', NULL, N'اﻋﺘﻤﺎد اﻟـﻤﺰرﻋﺔ أﻛﺒﺮ ﻣﻦ ﺧﻤﺴﺔ أﻓﺪﻧﺔ ﺣﺘﻰ ٢٠ ﻓﺪان', 139, 2000.0000, 3, 1),
    (38, N'اﻋﺘﻤﺎد اﻟـﻤﺰرﻋﺔ أﻛﺒﺮ ﻣﻦ ﻋﺸﺮون ﻓﺪان ﺣﺘﻰ ١٠٠ ﻓﺪان ', NULL, N'اﻋﺘﻤﺎد اﻟـﻤﺰرﻋﺔ أﻛﺒﺮ ﻣﻦ ﻋﺸﺮون ﻓﺪان ﺣﺘﻰ ١٠٠ ﻓﺪان ', 139, 3500.0000, 4, 1),
    (39, N'اعتماد مزرعة أكبر من  ١٠٠ فدان حتى  ٥٠٠ فدان ', NULL, N'اعتماد مزرعة أكبر من  ١٠٠ فدان حتى  ٥٠٠ فدان ', 139, 5000.0000, 5, 1),
    (40, N'اﻋﺘﻤﺎد اﻟـﻤﺰرﻋﺔ ﻣﻦ ٥٠٠ ﻓﺪان', NULL, N'اﻋﺘﻤﺎد اﻟـﻤﺰرﻋﺔ ﻣﻦ ٥٠٠ ﻓﺪان', 139, 10000.0000, 6, 1),
    (41, N'اﻋﺘﻤﺎد ﻣﻨﺸﺄة ﺻﻨﺎﻋﻴﺔ', NULL, N'اﻋﺘﻤﺎد ﻣﻨﺸﺄة ﺻﻨﺎﻋﻴﺔ', 139, 5000.0000, 1, 1),
    (42, N'اعتماد كيانات تصديرية / رسوم زيارة إضافية', NULL, N'اعتماد كيانات تصديرية / رسوم زيارة إضافية', 139, 800.0000, 9, 1),
    (43, N'استخراج سجل تصدير', NULL, N'استخراج سجل تصدير', 139, 200.0000, 13, 1),
    (44, N'اﺳﺘﺨﺮاج ﺳﺠﻞ ﻣﻌﺎلجات ', NULL, N'اﺳﺘﺨﺮاج ﺳﺠﻞ ﻣﻌﺎلجات ', 139, 300.0000, 12, 1),
    (45, N'سحبة عينة', NULL, N'سحبة عينة', -1, 200.0000, NULL, 1),
    (46, N'ﺗﺴﺠﻴﻞ واﻋﺘﻤﺎد ﺷﺮﻛﺔ ﻣﺼﺪرة ', NULL, N'ﺗﺴﺠﻴﻞ واﻋﺘﻤﺎد ﺷﺮﻛﺔ ﻣﺼﺪرة ', 139, 500.0000, 10, 1),
    (47, N'اﻋﺘﻤﺎدوﺗﺴﺠﻴﻞﻣﻮردﻣﻨﺘﺠﺎت زراﻋﻴﺔﻟـﻤﺤﻄﺎت وﻣﺮاﻛﺰاﻟﺘﻌﺒﺌﺔ', NULL, N'اﻋﺘﻤﺎد و ﺗﺴﺠﻴﻞ ﻣﻮرد ﻣﻨﺘﺠﺎت زراﻋﻴﺔ ﻟـﻤﺤﻄﺎت وﻣﺮاﻛﺰ اﻟﺘﻌﺒﺌﺔ', 139, 1000.0000, NULL, 1),
    (48, N'استخراج شهادة بيانات', NULL, N'استخراج شهادة بيانات', 139, 100.0000, 15, 1),
    (49, N'بدلات انتقال', NULL, N'بدل انتقالات', 139, NULL, 21, 1),
    (50, N'رسوم فحص', NULL, N'رسوم فحص', 138, NULL, 1, 1),
    (51, N'نفقات سجل متابعه معالجه', NULL, N'نفقات سجل متابعه معالجه', 138, 300.0000, 19, 1),
    (52, N'نفقات استخراج شهاده زراعيه', NULL, N'نفقات استخراج شهاده زراعيه', 138, 100.0000, 5, 1),
    (53, N'نفقات بدل تالف شهاده زراعيه', NULL, N'نفقات بدل تالف شهاده زراعيه', 138, 500.0000, 9, 1),
    (54, N'نفقات بدل تالف شهاده معالجه', NULL, N'نفقات بدل تالف شهاده معالجه', 138, 200.0000, 16, 1),
    (55, N'نفقات تحرير استماره فحص', NULL, N'نفقات تحرير استماره فحص', 138, 10.0000, 20, 1),
    (56, N'نفقات دمغه الشهيد', NULL, N'نفقات دمغه الشهيد', 0, 5.0000, 23, 1),
    (57, N'نفقات سحب عينه', NULL, N'نفقات سحب عينه', 138, 200.0000, 21, 1),
    (58, N'نفقات شهاده اعتماد منشأه', NULL, N'نفقات شهاده اعتماد منشأه', 138, 200.0000, 18, 1),
    (59, N'نفقات شهاده معالجه', NULL, N'نفقات شهاده معالجه', 138, 100.0000, 14, 1),
    (60, N'نفقات صوره اذن استيراد', NULL, N'نفقات صوره اذن استيراد', 138, 10.0000, 13, 1),
    (61, N'نفقات صوره بدل تالف شهاده معالجه', NULL, N'نفقات صوره بدل تالف شهاده معالجه', 138, 100.0000, 17, 1),
    (62, N'نفقات صوره شهاده زراعيه', NULL, N'نفقات صوره شهاده زراعيه', 138, 20.0000, 7, 1),
    (63, N'نفقات صوره شهاده معالجه', NULL, N'نفقات صوره شهاده معالجه', 138, 50.0000, 15, 1),
    (64, N'نفقات صوره بدل تالف ملحق شهاده زراعيه', NULL, N'نفقات صوره بدل تالف ملحق شهاده زراعيه', 138, 50.0000, 12, 1),
    (65, N'نفقات صوره ملحق شهاده زراعيه', NULL, N'نفقات صوره ملحق شهاده زراعيه', 138, 20.0000, 8, 1),
    (66, N'نفقات بدل تالف ملحق شهاده زراعيه', NULL, N'نفقات بدل تالف ملحق شهاده زراعيه', 138, 250.0000, 10, 1),
    (67, N'نفقات ملحق شهاده زراعيه', NULL, N'نفقات ملحق شهاده زراعيه', 138, 50.0000, 6, 1),
    (68, N'نفقات نوباتجيه', NULL, N'نفقات نوباتجيه', 138, NULL, 2, 1),
    (69, N'نفقات صوره بدل تالف شهاده زراعيه', NULL, N'نفقات صوره بدل تالف شهاده زراعيه', 138, 50.0000, 11, 1),
    (70, N'رسوم ميكنة', NULL, N'رسوم ميكنة', 0, NULL, NULL, 1),
    (71, N'بند شهادة بيانات', NULL, N'بند شهادة بيانات', 138, 100.0000, 24, 1),
    (72, N'بند استخراج اذن استيراد', NULL, N'بند استخراج اذن استيراد', 138, 500.0000, 25, 1),
    (73, N'بند تجديد اذن استيراد', NULL, N'بند تجديد اذن استيراد', 138, 250.0000, 26, 1),
    (74, N'اعتماد محطه تعبئة / مركز تعبئة', NULL, N'اعتماد محطه تعبئة / مركز تعبئة', 139, 2000.0000, 7, 1),
    (75, N'تسجيل واعتماد موردين', NULL, N'تسجيل واعتماد موردين', 139, 1000.0000, 8, 1),
    (76, N'رسوم معاينة', NULL, N'رسوم معاينة', 139, 500.0000, 11, 1),
    (77, N'رسوم إعادة تقييم مزرعة', NULL, N'رسوم إعادة تقييم مزرعة', 139, NULL, 17, 1),
    (78, N'رسوم فحص عينة معملية (حشائش)', NULL, N'رسوم فحص عينة معملية (حشائش)', 139, NULL, 18, 1),
    (79, N'رسوم فحص عينة معملية (امراض)', NULL, N'رسوم فحص عينة معملية (امراض)', 139, NULL, 19, 1),
    (80, N'رسوم فحص عينة معملية (متبقيات مبيدات)', NULL, N'رسوم فحص عينة معملية (متبقيات مبيدات)', 139, NULL, 20, 1);

IF (SELECT COUNT(*) FROM @Source) <> 80
    THROW 50000, 'The embedded FeesType snapshot is incomplete.', 1;

SELECT
    source.ID,
    CASE WHEN target.ID IS NULL THEN N'INSERT' ELSE N'UPDATE' END AS PlannedAction,
    target.Name_Ar AS Current_Name_Ar,
    source.Name_Ar AS New_Name_Ar,
    target.Account_Type AS Current_Account_Type,
    source.Account_Type AS New_Account_Type,
    target.Price AS Current_Price,
    source.Price AS New_Price,
    target.Display_Order AS Current_Display_Order,
    source.Display_Order AS New_Display_Order,
    target.IsActive AS Current_IsActive,
    source.IsActive AS New_IsActive
FROM @Source AS source
LEFT JOIN dbo.FeesType AS target
    ON target.ID = source.ID
WHERE
    target.ID IS NULL
    OR ISNULL(target.Name_Ar, N'') <> ISNULL(source.Name_Ar, N'')
    OR ISNULL(target.Name_En, '') <> ISNULL(source.Name_En, '')
    OR ISNULL(target.Full_Name, N'') <> ISNULL(source.Full_Name, N'')
    OR target.Account_Type <> source.Account_Type
    OR ISNULL(target.Price, -1) <> ISNULL(source.Price, -1)
    OR ISNULL(target.Display_Order, -1) <> ISNULL(source.Display_Order, -1)
    OR target.IsActive <> source.IsActive
ORDER BY source.ID;

IF @ApplyChanges = 0
BEGIN
    PRINT N'Preview only: no changes were made. Set @ApplyChanges = 1 after reviewing the rows above.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    UPDATE target
    SET
        target.Name_Ar = source.Name_Ar,
        target.Name_En = source.Name_En,
        target.Full_Name = source.Full_Name,
        target.Account_Type = source.Account_Type,
        target.Price = source.Price,
        target.Display_Order = source.Display_Order,
        target.IsActive = source.IsActive,
        target.User_Updation_Date = GETDATE()
    FROM dbo.FeesType AS target
    INNER JOIN @Source AS source
        ON source.ID = target.ID;

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
        source.ID,
        source.Name_Ar,
        source.Name_En,
        source.Full_Name,
        source.Account_Type,
        source.Price,
        source.Display_Order,
        source.IsActive,
        1,
        GETDATE()
    FROM @Source AS source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.FeesType AS target WITH (UPDLOCK, HOLDLOCK)
        WHERE target.ID = source.ID
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT
    target.ID,
    target.Name_Ar,
    target.Name_En,
    target.Full_Name,
    target.Account_Type,
    target.Price,
    target.Display_Order,
    target.IsActive
FROM dbo.FeesType AS target
INNER JOIN @Source AS source
    ON source.ID = target.ID
ORDER BY target.ID;

