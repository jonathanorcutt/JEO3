namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string TriggerQuery = @"
SELECT 
    trg.oid::integer AS ObjectId,
    trg.tgrelid::integer AS ParentObjectId,
    trg.tgname AS Name,
    (trg.tgenabled = 'D') AS IsDisabled,
    ((trg.tgtype & 64) = 64) AS IsInsteadOfTrigger,
    pg_get_triggerdef(trg.oid) AS Definition,
    digest(pg_get_triggerdef(trg.oid), 'sha256') AS DefinitionHash
FROM pg_trigger trg
JOIN pg_class c ON c.oid = trg.tgrelid
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname NOT IN ('pg_catalog', 'information_schema')
  AND NOT trg.tgisinternal;
";
    }
}