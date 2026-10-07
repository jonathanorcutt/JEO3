namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string ViewDefinitionQuery = @"
SELECT
    sys_context('USERENV', 'DB_NAME') AS DatabaseName,
    o.OBJECT_ID AS ObjectId,
    v.VIEW_NAME AS Name,
    v.OWNER AS SchemaName,
    u.USER_ID AS SchemaId,
    o.CREATED AS CreateDate,
    o.LAST_DDL_TIME AS ModifyDate,
    0 AS IsReplicated,
    CASE WHEN UPPER(v.TEXT_VC) LIKE '%WITH CHECK OPTION%' THEN 1 ELSE 0 END AS WithCheckOption,
    'CREATE OR REPLACE VIEW ""' || v.OWNER || '"".""' || v.VIEW_NAME || '"" AS ' || v.TEXT_VC AS Definition
FROM all_views v
INNER JOIN all_users u ON u.USERNAME = v.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = v.OWNER AND o.OBJECT_NAME = v.VIEW_NAME AND o.OBJECT_TYPE = 'VIEW'
WHERE v.VIEW_NAME NOT LIKE 'BIN$%'
    --AND v.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY v.OWNER, v.VIEW_NAME";
    }
}
