namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string ColumnQuery = @"
SELECT
    c.OWNER AS SchemaName,
    c.TABLE_NAME AS TableName,
    c.COLUMN_NAME AS ColumnName,
    c.COLUMN_ID AS OrdinalPosition,
    CASE WHEN c.NULLABLE = 'Y' THEN 1 ELSE 0 END AS IsNullable,
    COALESCE(k.IsPk, 0) AS IsPrimaryKey,
    COALESCE(k.IsUq, 0) AS IsUnique,
    COALESCE(k.IsFk, 0) AS IsForeignKey,
    CASE WHEN ix.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsIndexed,
    CASE WHEN c.IDENTITY_COLUMN = 'YES' THEN 1 ELSE 0 END AS IsIdentity,
    CASE WHEN c.VIRTUAL_COLUMN = 'YES' THEN 1 ELSE 0 END AS IsComputed,
    c.DATA_TYPE AS DataType,
    CASE WHEN c.DATA_TYPE IN ('VARCHAR2', 'NVARCHAR2', 'CHAR', 'NCHAR', 'RAW') THEN c.DATA_LENGTH ELSE -1 END AS MaximumLength,
    COALESCE(c.DATA_PRECISION, 0) AS Precision,
    COALESCE(c.DATA_SCALE, 0) AS Scale,
    c.CHARACTER_SET_NAME AS CharacterSetName,
    c.DATA_DEFAULT AS ColumnDefault,
    cc.COMMENTS AS Description,
    COALESCE(t.NUM_ROWS, 0) AS RowCount,
    c.OWNER || '.' || c.TABLE_NAME AS TableFullName,
    c.OWNER || '.' || c.TABLE_NAME || '.' || c.COLUMN_NAME AS ColumnKey
FROM all_tab_cols c
INNER JOIN all_tables t ON t.OWNER = c.OWNER AND t.TABLE_NAME = c.TABLE_NAME AND t.NESTED = 'NO'
INNER JOIN all_users u ON u.USERNAME = c.OWNER AND u.ORACLE_MAINTAINED = 'N'
LEFT JOIN (
    SELECT cl.OWNER, cl.TABLE_NAME, cl.COLUMN_NAME,
        MAX(CASE WHEN cn.CONSTRAINT_TYPE = 'P' THEN 1 ELSE 0 END) IsPk,
        MAX(CASE WHEN cn.CONSTRAINT_TYPE = 'U' THEN 1 ELSE 0 END) IsUq,
        MAX(CASE WHEN cn.CONSTRAINT_TYPE = 'R' THEN 1 ELSE 0 END) IsFk
    FROM all_constraints cn
    INNER JOIN all_cons_columns cl ON cl.OWNER = cn.OWNER AND cl.CONSTRAINT_NAME = cn.CONSTRAINT_NAME
    WHERE cn.CONSTRAINT_TYPE IN ('P', 'U', 'R')
    GROUP BY cl.OWNER, cl.TABLE_NAME, cl.COLUMN_NAME
) k ON k.OWNER = c.OWNER AND k.TABLE_NAME = c.TABLE_NAME AND k.COLUMN_NAME = c.COLUMN_NAME
LEFT JOIN (
    SELECT TABLE_OWNER, TABLE_NAME, COLUMN_NAME FROM all_ind_columns GROUP BY TABLE_OWNER, TABLE_NAME, COLUMN_NAME
) ix ON ix.TABLE_OWNER = c.OWNER AND ix.TABLE_NAME = c.TABLE_NAME AND ix.COLUMN_NAME = c.COLUMN_NAME
LEFT JOIN all_col_comments cc ON cc.OWNER = c.OWNER AND cc.TABLE_NAME = c.TABLE_NAME AND cc.COLUMN_NAME = c.COLUMN_NAME
WHERE c.HIDDEN_COLUMN = 'NO'
    --AND c.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY c.OWNER, c.TABLE_NAME, c.COLUMN_ID";
    }
}