SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.FeesType', N'U') IS NULL
    THROW 50000, 'Table dbo.FeesType was not found.', 1;

IF COL_LENGTH(N'dbo.FeesType', N'Account_Type') IS NULL
    THROW 50001, 'Column dbo.FeesType.Account_Type was not found.', 1;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.FeesType
    WHERE ID = 56
      AND
      (
          Name_Ar LIKE N'%شهيد%'
          OR Full_Name LIKE N'%شهيد%'
      )
)
    THROW 50002, 'Fee ID 56 is not the martyr fee. No changes were made.', 1;

/* Account_Type = 0 means the fee is available in both accounts. */
UPDATE dbo.FeesType
SET Account_Type = 0,
    IsActive = 1,
    User_Deletion_Id = NULL,
    User_Deletion_Date = NULL
WHERE ID = 56;

/* Disable the old private martyr row only when it is actually a martyr fee. */
UPDATE dbo.FeesType
SET Account_Type = -1
WHERE ID = 34
  AND
  (
      Name_Ar LIKE N'%شهيد%'
      OR Full_Name LIKE N'%شهيد%'
  );

COMMIT TRANSACTION;

SELECT ID, Name_Ar, Full_Name, Account_Type, Price, IsActive
FROM dbo.FeesType
WHERE ID IN (34, 56)
ORDER BY ID;
