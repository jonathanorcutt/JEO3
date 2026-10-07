
namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string TableQuery = @"
SELECT
    sys_context('USERENV', 'DB_NAME') ""DatabaseName"",
    t.OWNER ""SchemaName"",
    t.TABLE_NAME ""TableName"",
    t.OWNER || '.' || t.TABLE_NAME ""TableFullName"",
    COALESCE(t.NUM_ROWS, 0) ""Rows"",
    CASE WHEN t.TEMPORARY = 'Y' THEN 1 ELSE 0 END ""IsTemporary"",
    o.CREATED ""CreatedDate"",
    o.LAST_DDL_TIME ""DateLastModified"",
    tc.COMMENTS ""Description""
FROM all_tables t
INNER JOIN all_users u ON u.USERNAME = t.OWNER AND u.ORACLE_MAINTAINED = 'N'
LEFT JOIN all_objects o ON o.OWNER = t.OWNER AND o.OBJECT_NAME = t.TABLE_NAME AND o.OBJECT_TYPE = 'TABLE'
LEFT JOIN all_tab_comments tc ON tc.OWNER = t.OWNER AND tc.TABLE_NAME = t.TABLE_NAME
WHERE t.NESTED = 'NO'
    --AND t.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY t.OWNER, t.TABLE_NAME";
    }
}
