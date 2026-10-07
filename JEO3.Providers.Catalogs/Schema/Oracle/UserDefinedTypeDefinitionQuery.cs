namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string UserDefinedTypeDefinitionQuery = @"
SELECT
    CASE WHEN t.TYPECODE = 'COLLECTION' THEN CASE WHEN ct.COLL_TYPE = 'VARYING ARRAY' THEN 'VARRAY_TYPE' ELSE 'TABLE_TYPE' END ELSE 'OBJECT_TYPE' END AS FunctionType,
    'USER_DEFINED_TYPE' AS ObjectType,
    CASE WHEN t.TYPECODE = 'COLLECTION' THEN CASE WHEN ct.COLL_TYPE = 'VARYING ARRAY' THEN 'VARRAY(' || ct.UPPER_BOUND || ') OF ' ELSE 'TABLE OF ' END || ct.ELEM_TYPE_NAME
        ELSE (SELECT 'OBJECT (' || LISTAGG(a.ATTR_NAME || ' ' || a.ATTR_TYPE_NAME, ', ' ON OVERFLOW TRUNCATE '...' WITHOUT COUNT) WITHIN GROUP (ORDER BY a.ATTR_NO) || ')' FROM all_type_attrs a WHERE a.OWNER = t.OWNER AND a.TYPE_NAME = t.TYPE_NAME)
    END AS Definition,
    CASE WHEN t.TYPECODE = 'COLLECTION' THEN ct.ELEM_TYPE_NAME ELSE 'OBJECT' END AS ReturnType,
    t.TYPE_NAME AS ProviderReturnType,
    CASE WHEN ct.ELEM_TYPE_NAME IN ('VARCHAR2', 'NVARCHAR2', 'CHAR', 'NCHAR', 'RAW') THEN COALESCE(ct.LENGTH, -1) ELSE -1 END AS MaxLength,
    COALESCE(ct.PRECISION, 0) AS Precision,
    COALESCE(ct.SCALE, 0) AS Scale,
    1 AS IsNullable,
    u.USER_ID AS SchemaId,
    0 AS IsSystemObject,
    0 AS IsDeterministic,
    o.OBJECT_ID AS ObjectId,
    '""' || t.OWNER || '"".""' || t.TYPE_NAME || '""' AS Identifier,
    sys_context('USERENV', 'DB_NAME') AS DatabaseName,
    t.OWNER AS SchemaName,
    t.TYPE_NAME AS Name,
    sys_context('USERENV', 'DB_NAME') || '.' || t.OWNER || '.' || t.TYPE_NAME AS FullName
FROM all_types t
INNER JOIN all_users u ON u.USERNAME = t.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = t.OWNER AND o.OBJECT_NAME = t.TYPE_NAME AND o.OBJECT_TYPE = 'TYPE'
LEFT JOIN all_coll_types ct ON ct.OWNER = t.OWNER AND ct.TYPE_NAME = t.TYPE_NAME
WHERE t.TYPECODE IN ('OBJECT', 'COLLECTION')
    AND t.TYPE_NAME NOT LIKE 'BIN$%' AND t.TYPE_NAME NOT LIKE 'SYS\_PLSQL\_%' ESCAPE '\'
    --AND t.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY t.OWNER, t.TYPE_NAME";
    }
}