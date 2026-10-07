namespace JEO3.Providers.Catalogs
{

    internal static partial class MSSQLQueries
    {
        internal const string StoredProcedureDefinitionQuery = @"
SELECT 
    DB_NAME() AS DatabaseName,
    p.object_id AS ObjectId,
    p.name AS [Name],
    s.name AS SchemaName,
    p.schema_id AS SchemaId,
    p.create_date AS CreateDate,
    p.modify_date AS ModifyDate,
    p.is_auto_executed AS IsAutoExecuted,
    p.is_execution_replicated AS IsExecutionReplicated,
    m.definition AS Definition
FROM sys.procedures p WITH (NOLOCK)
    INNER JOIN sys.schemas s WITH (NOLOCK) ON p.schema_id = s.schema_id
    LEFT JOIN sys.sql_modules m WITH (NOLOCK) ON p.object_id = m.object_id
--ORDER BY
--    s.name,
--    p.name;";
    }
}