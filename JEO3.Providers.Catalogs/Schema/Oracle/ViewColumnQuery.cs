namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string ViewColumnQuery = @"
SELECT
    o.OBJECT_ID AS ObjectId,
    v.VIEW_NAME AS ViewName,
    v.OWNER AS SchemaName,
    c.COLUMN_NAME AS ColumnName,
    c.DATA_TYPE AS DataType,
    c.DATA_TYPE AS ProviderDataType,
    c.COLUMN_ID AS OrdinalPosition,
    CASE WHEN c.NULLABLE = 'Y' THEN 1 ELSE 0 END AS IsNullable,
    CASE WHEN c.DATA_TYPE IN ('VARCHAR2', 'NVARCHAR2', 'CHAR', 'NCHAR', 'RAW') THEN COALESCE(c.DATA_LENGTH, -1) ELSE -1 END AS MaxLength,
    COALESCE(c.DATA_PRECISION, 0) AS Precision,
    COALESCE(c.DATA_SCALE, 0) AS Scale,
    0 AS IsOutput
FROM all_views v
INNER JOIN all_users u ON u.USERNAME = v.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = v.OWNER AND o.OBJECT_NAME = v.VIEW_NAME AND o.OBJECT_TYPE = 'VIEW'
INNER JOIN all_tab_cols c ON c.OWNER = v.OWNER AND c.TABLE_NAME = v.VIEW_NAME AND c.HIDDEN_COLUMN = 'NO'
WHERE v.VIEW_NAME NOT LIKE 'BIN$%'
    --AND v.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY v.OWNER, v.VIEW_NAME, c.COLUMN_ID";
    }
}
