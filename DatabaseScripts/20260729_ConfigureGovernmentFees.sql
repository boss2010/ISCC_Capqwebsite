SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @DamagedCertificateCopyId tinyint;

SELECT @DamagedCertificateCopyId = ID
FROM dbo.FeesType
WHERE Name_Ar = N'نفقات صوره بدل تالف شهاده زراعيه';

IF @DamagedCertificateCopyId IS NULL
BEGIN
    SELECT @DamagedCertificateCopyId = CONVERT(tinyint, MAX(CONVERT(int, ID)) + 1)
    FROM dbo.FeesType;

    INSERT INTO dbo.FeesType
    (
        ID,
        Name_Ar,
        Full_Name,
        IsActive,
        User_Creation_Id,
        User_Creation_Date,
        Account_Type,
        Price,
        Display_Order
    )
    VALUES
    (
        @DamagedCertificateCopyId,
        N'نفقات صوره بدل تالف شهاده زراعيه',
        N'نفقات صوره بدل تالف شهاده زراعيه',
        1,
        1,
        GETDATE(),
        138,
        50,
        11
    );
END;

DECLARE @GovernmentFees TABLE
(
    ID tinyint NOT NULL PRIMARY KEY,
    FeeName nvarchar(250) NOT NULL,
    Price money NULL,
    DisplayOrder int NOT NULL
);

INSERT INTO @GovernmentFees (ID, FeeName, Price, DisplayOrder)
VALUES
    (50, N'رسوم فحص', NULL, 1),
    (68, N'نفقات نوباتجيه', NULL, 2),
    (22, N'نفقات تبخير', NULL, 3),
    (23, N'نفقات تطهير', NULL, 4),
    (52, N'نفقات استخراج شهاده زراعيه', 100, 5),
    (67, N'نفقات ملحق شهاده زراعيه', 50, 6),
    (62, N'نفقات صوره شهاده زراعيه', 20, 7),
    (65, N'نفقات صوره ملحق شهاده زراعيه', 20, 8),
    (53, N'نفقات بدل تالف شهاده زراعيه', 500, 9),
    (66, N'نفقات بدل تالف ملحق شهاده زراعيه', 250, 10),
    (@DamagedCertificateCopyId, N'نفقات صوره بدل تالف شهاده زراعيه', 50, 11),
    (64, N'نفقات صوره بدل تالف ملحق شهاده زراعيه', 50, 12),
    (60, N'نفقات صوره اذن استيراد', 250, 13),
    (59, N'نفقات شهاده معالجه', 100, 14),
    (63, N'نفقات صوره شهاده معالجه', 50, 15),
    (54, N'نفقات بدل تالف شهاده معالجه', 200, 16),
    (61, N'نفقات صوره بدل تالف شهاده معالجه', 100, 17),
    (58, N'نفقات شهاده اعتماد منشأه', 200, 18),
    (51, N'نفقات سجل متابعه معالجه', 300, 19),
    (55, N'نفقات تحرير استماره فحص', 10, 20),
    (57, N'نفقات سحب عينه', 200, 21),
    (26, N'نفقات معاينه', 500, 22),
    (56, N'نفقات دمغه الشهيد', 5, 23);

UPDATE ft
SET
    ft.Name_Ar = source.FeeName,
    ft.Full_Name = source.FeeName,
    ft.Account_Type = 138,
    ft.Price = source.Price,
    ft.Display_Order = source.DisplayOrder
FROM dbo.FeesType ft
INNER JOIN @GovernmentFees source ON source.ID = ft.ID;

IF (SELECT COUNT(*) FROM dbo.FeesType WHERE Account_Type = 138 AND Display_Order BETWEEN 1 AND 23) <> 23
BEGIN
    THROW 50001, 'Government fees configuration is incomplete.', 1;
END;

COMMIT TRANSACTION;

SELECT ID, Name_Ar, Account_Type, Price, Display_Order
FROM dbo.FeesType
WHERE Account_Type = 138
  AND Display_Order BETWEEN 1 AND 23
ORDER BY Display_Order;
