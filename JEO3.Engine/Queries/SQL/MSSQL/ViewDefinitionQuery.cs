namespace JEO3.Engine
{

    internal static partial class MSSQLQueries
    {
        internal const string ViewDefinitionQuery = @"
SELECT 
    DB_NAME() AS DatabaseName,
    v.object_id AS ObjectId,
    v.name AS Name,
    s.name AS SchemaName,
    v.schema_id AS SchemaId,
    v.create_date AS CreateDate,
    v.modify_date AS ModifyDate,
    v.is_replicated AS IsReplicated,
    v.with_check_option AS WithCheckOption,
    m.definition AS Definition
FROM sys.views v WITH (NOLOCK)
    INNER JOIN sys.schemas s WITH (NOLOCK) ON v.schema_id = s.schema_id
    LEFT JOIN sys.sql_modules m WITH (NOLOCK) ON v.object_id = m.object_id
--ORDER BY
--    s.name,
--    v.name;";
    }
}