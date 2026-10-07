namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string StatisticQuery = @"
SELECT
    o.OBJECT_ID AS ParentObjectId,
    tc.INTERNAL_COLUMN_ID AS ObjectId,
    tc.INTERNAL_COLUMN_ID AS StatsId,
    cs.COLUMN_NAME AS Name,
    CASE WHEN cs.USER_STATS = 'NO' AND COALESCE(se.CREATOR, 'SYSTEM') = 'SYSTEM' THEN 1 ELSE 0 END AS IsAutoCreated,
    CASE WHEN se.CREATOR = 'USER' THEN 1 ELSE 0 END AS IsUserCreated,
    0 AS HasFilter,
    CAST(NULL AS VARCHAR2(4000)) AS FilterDefinition,
    CAST(NULL AS TIMESTAMP) AS CreateDate,
    CAST(cs.LAST_ANALYZED AS TIMESTAMP) AS ModifyDate,
    CAST(NULL AS VARCHAR2(128)) AS CreatedBy
FROM all_tab_col_statistics cs
INNER JOIN all_users u ON u.USERNAME = cs.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = cs.OWNER AND o.OBJECT_NAME = cs.TABLE_NAME AND o.OBJECT_TYPE = 'TABLE'
INNER JOIN all_tab_cols tc ON tc.OWNER = cs.OWNER AND tc.TABLE_NAME = cs.TABLE_NAME AND tc.COLUMN_NAME = cs.COLUMN_NAME
LEFT JOIN all_stat_extensions se ON se.OWNER = cs.OWNER AND se.TABLE_NAME = cs.TABLE_NAME AND se.EXTENSION_NAME = cs.COLUMN_NAME
WHERE cs.TABLE_NAME NOT LIKE 'BIN$%'
    --AND cs.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY cs.OWNER, cs.TABLE_NAME, tc.INTERNAL_COLUMN_ID";
    }
}