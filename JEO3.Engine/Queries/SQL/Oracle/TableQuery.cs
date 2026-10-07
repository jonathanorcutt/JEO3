
namespace JEO3.Engine
{
    internal static partial class OracleQueries
    {
        internal const string TableQuery = @"
SELECT 
    sys_context('USERENV', 'DB_NAME') ""DatabaseName"",
    t.OWNER ""SchemaName"",
    t.TABLE_NAME ""TableName"",
    t.OWNER || '.' || t.TABLE_NAME ""TableFullName"",
    COALESCE(t.NUM_ROWS, 0) ""Rows""
FROM all_tables t
WHERE 
    t.OWNER NOT IN ('SYS', 'SYSTEM', 'XDB', 'OUTLN', 'MDSYS', 'APEX_040200', 'LBACSYS', 'DVSYS', 'GSMADMIN_INTERNAL')
    AND t.OWNER = 'ORDDATA' -- HARDCODE FOR LAPTOP

    --AND t.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
    --sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY 
    t.OWNER, 
    t.TABLE_NAME";
    }
}
