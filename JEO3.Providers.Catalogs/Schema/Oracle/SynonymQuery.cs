namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string SynonymQuery = @"
SELECT
    o.OBJECT_ID AS Id,
    u.USER_ID AS SchemaId,
    s.OWNER AS SchemaName,
    s.SYNONYM_NAME AS Name,
    COALESCE(s.TABLE_OWNER, s.OWNER) AS TargetSchemaName,
    s.TABLE_NAME AS TargetTableName,
    COALESCE(s.TABLE_OWNER || '.', '') || s.TABLE_NAME || CASE WHEN s.DB_LINK IS NOT NULL THEN '@' || s.DB_LINK ELSE '' END AS BaseObjectName,
    CAST(NULL AS NUMBER) AS PrincipalId,
    0 AS ParentObjectId,
    'SN' AS Type,
    'SYNONYM' AS TypeDesc,
    o.CREATED AS CreateDate,
    o.LAST_DDL_TIME AS ModifyDate,
    0 AS IsMsShipped,
    0 AS IsPublished,
    0 AS IsSchemaPublished
FROM all_synonyms s
LEFT JOIN all_users u ON u.USERNAME = s.OWNER
LEFT JOIN all_users tu ON tu.USERNAME = s.TABLE_OWNER
LEFT JOIN all_objects o ON o.OWNER = s.OWNER AND o.OBJECT_NAME = s.SYNONYM_NAME AND o.OBJECT_TYPE = 'SYNONYM'
WHERE (u.ORACLE_MAINTAINED = 'N' OR (s.OWNER = 'PUBLIC' AND tu.ORACLE_MAINTAINED = 'N'))
    AND s.SYNONYM_NAME NOT LIKE 'BIN$%'
    --AND s.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY s.OWNER, s.SYNONYM_NAME";
    }
}