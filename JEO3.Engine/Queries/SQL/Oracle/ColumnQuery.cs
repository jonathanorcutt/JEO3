namespace JEO3.Engine
{
    internal static partial class OracleQueries
    {
        internal const string ColumnQuery = @"
    SELECT DISTINCT
    c.OWNER AS SchemaName,
    c.TABLE_NAME AS TableName,
    c.COLUMN_NAME AS ColumnName,
    c.COLUMN_ID AS OrdinalPosition,
    (CASE WHEN c.NULLABLE = 'Y' THEN '1' ELSE '0' END) AS IsNullable,
    (CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END) AS IsPrimaryKey,
    (CASE WHEN c.IDENTITY_COLUMN = 'YES' THEN 1 ELSE 0 END) AS IsIdentity,
    c.DATA_TYPE AS DataType,
    CASE WHEN c.DATA_TYPE IN ('VARCHAR2', 'CHAR', 'RAW') THEN COALESCE(c.DATA_LENGTH, -1)  ELSE -1 END MaximumLength,
    COALESCE(c.DATA_PRECISION, 0) AS Precision,
    COALESCE(c.DATA_SCALE, 0) AS Scale,
    '' AS ColumnDefault,
    --COALESCE(c.DATA_DEFAULT, '') AS ColumnDefault,
    COALESCE(t.NUM_ROWS, 0) AS RowCount,
    (c.OWNER || '.' || c.TABLE_NAME) AS TableFullName,
    (c.OWNER || '.' || c.TABLE_NAME || '.' || c.COLUMN_NAME) AS ColumnKey
FROM all_tab_cols c
INNER JOIN all_tables t ON t.OWNER = c.OWNER AND t.TABLE_NAME = c.TABLE_NAME
LEFT JOIN (
    SELECT cons.OWNER, cols.TABLE_NAME, cols.COLUMN_NAME
    FROM all_constraints cons
    INNER JOIN all_cons_columns cols ON cons.CONSTRAINT_NAME = cols.CONSTRAINT_NAME AND cons.OWNER = cols.OWNER
    WHERE cons.CONSTRAINT_TYPE = 'P'
    AND cons.OWNER = 'ORDDATA' -- HARDCODE FOR LAPTOP
        --AND cons.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
) pk ON pk.OWNER = c.OWNER AND pk.TABLE_NAME = c.TABLE_NAME AND pk.COLUMN_NAME = c.COLUMN_NAME
WHERE 
    c.OWNER NOT IN ('SYS', 'SYSTEM', 'XDB', 'OUTLN', 'MDSYS', 'APEX_040200', 'LBACSYS', 'DVSYS', 'GSMADMIN_INTERNAL') AND c.HIDDEN_COLUMN = 'NO'
    AND c.OWNER = 'ORDDATA'-- HARDCODE FOR LAPTOP

    --sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY 
    c.OWNER, 
    c.TABLE_NAME, 
    c.COLUMN_ID";
    }
}