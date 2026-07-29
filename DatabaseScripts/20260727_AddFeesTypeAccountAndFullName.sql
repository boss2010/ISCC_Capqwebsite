USE [PlantQuarantine_New];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
BEGIN
    THROW 50000, 'Table dbo.FeesType was not found in PlantQuarantine_New.', 1;
END;
GO

/* إضافة نوع الحساب:
   138 = حكومي
   139 = خاص
   0   = يظهر في الحسابين
  -1   = غير معروض في شاشات الدفع الإلكتروني
*/
IF COL_LENGTH(N'dbo.FeesType', N'Account_Type') IS NULL
BEGIN
    ALTER TABLE dbo.FeesType
    ADD Account_Type int NOT NULL DEFAULT (-1);
END;
GO

/* اسم كامل أكبر من Name_Ar لاستخدامه في شاشة الدفع */
IF COL_LENGTH(N'dbo.FeesType', N'Full_Name') IS NULL
BEGIN
    ALTER TABLE dbo.FeesType
    ADD Full_Name nvarchar(250) NULL;
END;
GO

/* توقف برسالة واضحة إذا لم تتم إضافة الأعمدة */
IF COL_LENGTH(N'dbo.FeesType', N'Account_Type') IS NULL
BEGIN
    THROW 50001, 'Column dbo.FeesType.Account_Type was not created.', 1;
END;

IF COL_LENGTH(N'dbo.FeesType', N'Full_Name') IS NULL
BEGIN
    THROW 50002, 'Column dbo.FeesType.Full_Name was not created.', 1;
END;
GO

/* إعادة ضبط أنواع الحساب للرسوم التي تديرها شاشة الدفع */
UPDATE dbo.FeesType
SET Account_Type = -1
WHERE ID IN
(
    20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
    30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
    40, 41, 42, 43, 44, 46, 47, 48, 49, 50
);

/* رسوم الحساب الحكومي */
UPDATE dbo.FeesType
SET Account_Type = 138
WHERE ID IN
(
    20, 21, 22, 23, 24, 25,
    28, 29, 30, 31, 32, 33
);

/* رسوم الحساب الخاص */
UPDATE dbo.FeesType
SET Account_Type = 139
WHERE ID IN
(
    26, 27, 35, 36, 37, 38, 39, 40, 41,
    42, 43, 44, 46, 47, 48, 49, 50
);

/* رسوم صندوق الشهداء تظهر في الحسابين */
UPDATE dbo.FeesType
SET Account_Type = 0
WHERE ID = 34;
GO

/* تهيئة الاسم الكامل من الاسم العربي الحالي */
UPDATE dbo.FeesType
SET Full_Name = Name_Ar
WHERE NULLIF(LTRIM(RTRIM(Full_Name)), N'') IS NULL;

/* يمكن استبدال النص التالي بالاسم الكامل المطلوب */
UPDATE dbo.FeesType
SET Full_Name = N'نفقات نوباتجية'
WHERE Name_Ar LIKE N'%نوباتجية%';
GO

/* نتيجة التحقق النهائية */
SELECT
    ID,
    Name_Ar,
    Full_Name,
    Account_Type,
    CASE Account_Type
        WHEN 138 THEN N'حساب حكومي'
        WHEN 139 THEN N'حساب خاص'
        WHEN 0 THEN N'حكومي وخاص'
        ELSE N'غير معروض'
    END AS Account_Name,
    IsActive,
    User_Deletion_Id
FROM dbo.FeesType
WHERE Account_Type IN (0, 138, 139)
ORDER BY Account_Type, ID;
GO
