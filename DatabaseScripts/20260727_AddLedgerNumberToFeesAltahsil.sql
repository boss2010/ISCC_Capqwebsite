USE [PlantQuarantine_New];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.Fees_Altahsil', N'U') IS NULL
    THROW 50000, 'Table dbo.Fees_Altahsil was not found.', 1;
GO

IF COL_LENGTH(N'dbo.Fees_Altahsil', N'Ledger_Number') IS NULL
BEGIN
    ALTER TABLE dbo.Fees_Altahsil
    ADD Ledger_Number nvarchar(100) NULL;
END;
GO

SELECT
    COL_LENGTH(N'dbo.Fees_Altahsil', N'Ledger_Number') AS LedgerNumberColumnLength;
GO
