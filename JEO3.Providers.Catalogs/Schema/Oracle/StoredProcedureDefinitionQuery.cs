namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string StoredProcedureDefinitionQuery = @"
SELECT
    sys_context('USERENV', 'DB_NAME') AS DatabaseName,
    o.OBJECT_ID AS ObjectId,
    o.OBJECT_NAME AS Name,
    o.OWNER AS SchemaName,
    u.USER_ID AS SchemaId,
    o.CREATED AS CreateDate,
    o.LAST_DDL_TIME AS ModifyDate,
    0 AS IsAutoExecuted,
    0 AS IsExecutionReplicated,
    'CREATE OR REPLACE ' || src.Definition AS Definition
FROM all_objects o
INNER JOIN all_users u ON u.USERNAME = o.OWNER AND u.ORACLE_MAINTAINED = 'N'
LEFT JOIN (
    SELECT OWNER, NAME, DBMS_XMLGEN.CONVERT(XMLAGG(XMLELEMENT(e, TEXT) ORDER BY LINE).EXTRACT('//text()').GETCLOBVAL(), 1) AS Definition
    FROM all_source
    WHERE TYPE = 'PROCEDURE'
    GROUP BY OWNER, NAME
) src ON src.OWNER = o.OWNER AND src.NAME = o.OBJECT_NAME
WHERE o.OBJECT_TYPE = 'PROCEDURE'
    AND o.OBJECT_NAME NOT LIKE 'BIN$%'
    --AND o.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY o.OWNER, o.OBJECT_NAME";
    }
}
