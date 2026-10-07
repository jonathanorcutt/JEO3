namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string ExtendedPropertyQuery = @"
SELECT
    o.OBJECT_ID AS MajorId,
    0 AS MinorId,
    1 AS Class,
    'OBJECT_OR_COLUMN' AS ClassDescription,
    'MS_Description' AS PropertyName,
    tc.COMMENTS AS Value,
    tc.OWNER AS SchemaName,
    o.OBJECT_ID AS ObjectId,
    tc.TABLE_NAME AS Name,
    CASE tc.TABLE_TYPE WHEN 'VIEW' THEN 'VIEW' ELSE 'USER_TABLE' END AS ObjectTypeDescription,
    CAST(NULL AS NUMBER) AS ParentObjectId,
    CAST(NULL AS VARCHAR2(128)) AS ParentObjectName,
    CAST(NULL AS VARCHAR2(128)) AS ColumnName,
    CAST(NULL AS VARCHAR2(128)) AS IndexName
FROM all_tab_comments tc
INNER JOIN all_users u ON u.USERNAME = tc.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = tc.OWNER AND o.OBJECT_NAME = tc.TABLE_NAME AND o.OBJECT_TYPE IN ('TABLE', 'VIEW')
WHERE tc.COMMENTS IS NOT NULL AND tc.TABLE_NAME NOT LIKE 'BIN$%'
UNION ALL
SELECT
    o.OBJECT_ID AS MajorId,
    c.COLUMN_ID AS MinorId,
    1 AS Class,
    'OBJECT_OR_COLUMN' AS ClassDescription,
    'MS_Description' AS PropertyName,
    cc.COMMENTS AS Value,
    cc.OWNER AS SchemaName,
    o.OBJECT_ID AS ObjectId,
    cc.TABLE_NAME AS Name,
    CASE o.OBJECT_TYPE WHEN 'VIEW' THEN 'VIEW' ELSE 'USER_TABLE' END AS ObjectTypeDescription,
    o.OBJECT_ID AS ParentObjectId,
    cc.TABLE_NAME AS ParentObjectName,
    cc.COLUMN_NAME AS ColumnName,
    CAST(NULL AS VARCHAR2(128)) AS IndexName
FROM all_col_comments cc
INNER JOIN all_users u ON u.USERNAME = cc.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = cc.OWNER AND o.OBJECT_NAME = cc.TABLE_NAME AND o.OBJECT_TYPE IN ('TABLE', 'VIEW')
INNER JOIN all_tab_cols c ON c.OWNER = cc.OWNER AND c.TABLE_NAME = cc.TABLE_NAME AND c.COLUMN_NAME = cc.COLUMN_NAME AND c.HIDDEN_COLUMN = 'NO'
WHERE cc.COMMENTS IS NOT NULL AND cc.TABLE_NAME NOT LIKE 'BIN$%'
    --AND cc.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY SchemaName, Name, MinorId";
    }
}