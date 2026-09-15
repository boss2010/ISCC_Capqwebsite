IF COL_LENGTH('dbo.Fees_Altahsil_Detiles', 'Quantity') IS NULL
BEGIN
    ALTER TABLE dbo.Fees_Altahsil_Detiles
        ADD Quantity int NULL;
END;

IF EXISTS
(
    SELECT 1
    FROM sys.extended_properties ep
    INNER JOIN sys.columns c
        ON c.object_id = ep.major_id
       AND c.column_id = ep.minor_id
    WHERE ep.name = N'MS_Description'
      AND ep.major_id = OBJECT_ID(N'dbo.Fees_Altahsil_Detiles')
      AND c.name = N'Quantity'
)
BEGIN
    EXEC sys.sp_updateextendedproperty
        @name = N'MS_Description', @value = N'العدد',
        @level0type = N'SCHEMA', @level0name = N'dbo',
        @level1type = N'TABLE',  @level1name = N'Fees_Altahsil_Detiles',
        @level2type = N'COLUMN', @level2name = N'Quantity';
END
ELSE
BEGIN
    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description', @value = N'العدد',
        @level0type = N'SCHEMA', @level0name = N'dbo',
        @level1type = N'TABLE',  @level1name = N'Fees_Altahsil_Detiles',
        @level2type = N'COLUMN', @level2name = N'Quantity';
END;
