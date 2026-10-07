namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string UserDefinedTypeColumnQuery = @"
SELECT
    o.OBJECT_ID AS ObjectId,
    a.ATTR_NO AS OrdinalPosition,
    a.ATTR_NAME AS Name,
    a.ATTR_TYPE_NAME AS DataType,
    a.ATTR_TYPE_NAME AS ProviderDataType,
    CASE WHEN a.ATTR_TYPE_NAME IN ('VARCHAR2', 'NVARCHAR2', 'CHAR', 'NCHAR', 'RAW') THEN COALESCE(a.LENGTH, -1) ELSE -1 END AS MaxLength,
    COALESCE(a.PRECISION, 0) AS Precision,
    COALESCE(a.SCALE, 0) AS Scale,
    0 AS IsOutput,
    1 AS IsNullable,
    CAST(NULL AS VARCHAR2(4000)) AS DefaultValue
FROM all_type_attrs a
INNER JOIN all_users u ON u.USERNAME = a.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = a.OWNER AND o.OBJECT_NAME = a.TYPE_NAME AND o.OBJECT_TYPE = 'TYPE'
WHERE a.TYPE_NAME NOT LIKE 'BIN$%' AND a.TYPE_NAME NOT LIKE 'SYS\_PLSQL\_%' ESCAPE '\'
UNION ALL
SELECT
    o.OBJECT_ID AS ObjectId,
    1 AS OrdinalPosition,
    'COLUMN_VALUE' AS Name,
    ct.ELEM_TYPE_NAME AS DataType,
    ct.ELEM_TYPE_NAME AS ProviderDataType,
    CASE WHEN ct.ELEM_TYPE_NAME IN ('VARCHAR2', 'NVARCHAR2', 'CHAR', 'NCHAR', 'RAW') THEN COALESCE(ct.LENGTH, -1) ELSE -1 END AS MaxLength,
    COALESCE(ct.PRECISION, 0) AS Precision,
    COALESCE(ct.SCALE, 0) AS Scale,
    0 AS IsOutput,
    1 AS IsNullable,
    CAST(NULL AS VARCHAR2(4000)) AS DefaultValue
FROM all_coll_types ct
INNER JOIN all_users u ON u.USERNAME = ct.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = ct.OWNER AND o.OBJECT_NAME = ct.TYPE_NAME AND o.OBJECT_TYPE = 'TYPE'
WHERE ct.TYPE_NAME NOT LIKE 'BIN$%' AND ct.TYPE_NAME NOT LIKE 'SYS\_PLSQL\_%' ESCAPE '\'
ORDER BY ObjectId, OrdinalPosition";
    }
}