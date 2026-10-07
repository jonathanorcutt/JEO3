namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        internal const string TriggerQuery = @"
SELECT 
    t.object_id AS ObjectId,
    t.parent_id AS ParentObjectId,
    t.name AS Name,
    t.is_disabled AS IsDisabled,
    t.is_instead_of_trigger AS IsInsteadOfTrigger,
    sm.definition AS Definition,
    HASHBYTES('SHA2_256', sm.definition) AS DefinitionHash
FROM sys.triggers t WITH (NOLOCK)
INNER JOIN sys.sql_modules sm WITH (NOLOCK) ON t.object_id = sm.object_id
WHERE t.parent_class = 1 -- 1 = Object or column (Table/View triggers)
AND t.is_ms_shipped = 0;";
    }
}