namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string FunctionDefinitionQuery = @"
SELECT
    o.OBJECT_ID AS ObjectId,
    CASE WHEN pr.PIPELINED = 'YES' THEN 'PLSQL_PIPELINED_FUNCTION' ELSE 'PLSQL_FUNCTION' END AS FunctionType,
    'FUNCTION' AS ObjectType,
    'CREATE OR REPLACE ' || src.Definition AS Definition,
    CASE WHEN pr.PIPELINED = 'YES' THEN 'TABLE' ELSE COALESCE(ret.DATA_TYPE, 'UNKNOWN') END AS ReturnType,
    CASE WHEN pr.PIPELINED = 'YES' THEN 'TABLE' ELSE COALESCE(ret.TYPE_NAME, ret.DATA_TYPE, 'UNKNOWN') END AS ProviderReturnType,
    CASE WHEN u.ORACLE_MAINTAINED = 'Y' THEN 1 ELSE 0 END AS IsSystemObject,
    CASE WHEN pr.DETERMINISTIC = 'YES' THEN 1 ELSE 0 END AS IsDeterministic,
    '""' || o.OWNER || '"".""' || o.OBJECT_NAME || '""' AS Identifier,
    sys_context('USERENV', 'DB_NAME') AS DatabaseName,
    o.OWNER AS SchemaName,
    o.OBJECT_NAME AS Name,
    sys_context('USERENV', 'DB_NAME') || '.' || o.OWNER || '.' || o.OBJECT_NAME AS FullName
FROM all_objects o
INNER JOIN all_users u ON u.USERNAME = o.OWNER
LEFT JOIN all_procedures pr ON pr.OWNER = o.OWNER AND pr.OBJECT_NAME = o.OBJECT_NAME AND pr.OBJECT_TYPE = 'FUNCTION' AND pr.PROCEDURE_NAME IS NULL
LEFT JOIN all_arguments ret ON ret.OBJECT_ID = o.OBJECT_ID AND ret.POSITION = 0 AND ret.ARGUMENT_NAME IS NULL AND ret.DATA_LEVEL = 0
LEFT JOIN (
    SELECT OWNER, NAME, DBMS_XMLGEN.CONVERT(XMLAGG(XMLELEMENT(e, TEXT) ORDER BY LINE).EXTRACT('//text()').GETCLOBVAL(), 1) AS Definition
    FROM all_source
    WHERE TYPE = 'FUNCTION'
    GROUP BY OWNER, NAME
) src ON src.OWNER = o.OWNER AND src.NAME = o.OBJECT_NAME
WHERE o.OBJECT_TYPE = 'FUNCTION'
    AND o.OBJECT_NAME NOT LIKE 'BIN$%'
    AND u.ORACLE_MAINTAINED = 'N' -- drop this line to include system functions (IsSystemObject then flags them)
    --AND o.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY o.OWNER, o.OBJECT_NAME";
    }
}