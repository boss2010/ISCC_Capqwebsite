SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.Fees_Altahsil', 'Is_Used') IS NULL
BEGIN
    ALTER TABLE dbo.Fees_Altahsil
        ADD Is_Used bit NOT NULL
            CONSTRAINT DF_Fees_Altahsil_Is_Used DEFAULT (0);
END;

IF COL_LENGTH('dbo.Fees_Altahsil', 'Used_Date') IS NULL
BEGIN
    ALTER TABLE dbo.Fees_Altahsil
        ADD Used_Date datetime2(0) NULL;
END;

COMMIT TRANSACTION;

SELECT
    c.name,
    TYPE_NAME(c.user_type_id) AS Data_Type,
    c.is_nullable
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.Fees_Altahsil')
  AND c.name IN (N'Is_Used', N'Used_Date')
ORDER BY c.column_id;
