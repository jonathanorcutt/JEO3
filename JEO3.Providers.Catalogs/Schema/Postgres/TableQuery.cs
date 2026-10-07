namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string TableQuery = @"
SELECT      
    t.oid::integer AS ObjectId,        
    current_database() AS DatabaseName,        
    s.nspname AS SchemaName,        
    concat(s.nspname, '.', t.relname) AS TablePath,        
    t.relname AS Name,        
    '' AS CreatedBy,
    NULL::timestamp AS CreatedDate,        
    NULL::timestamp AS DateLastModified,        
    CASE t.relpersistence
        WHEN 'u' THEN 1
        ELSE 0
    END AS Durability,
    CASE t.relpersistence
        WHEN 'u' THEN 'UNLOGGED'
        WHEN 't' THEN 'TEMPORARY'
        ELSE 'PERMANENT'
    END AS DurabilityDescription,        
    CASE t.relkind
        WHEN 'p' THEN 'PARTITIONED TABLE'
        ELSE 'USER_TABLE'
    END AS TableType,        
    FALSE AS IsView,        
    COALESCE(t.reltuples::bigint, 0) AS Rows,        
    s.oid::integer AS SchemaId,        
    FALSE AS IsMemoryOptimized,        
    0 AS TemporalType,        
    'NON_TEMPORAL_TABLE' AS TemporalTypeDescription,        
    FALSE AS IsFileTable,        
    0 AS LobDataSpaceId,        
    NULL::integer AS HistoryTableObjectId,        
    'TABLE' AS LockEscalationDescription,
    
    -- Total Space (Table + Indexes + TOAST) in KB
    ROUND(pg_total_relation_size(t.oid) / 1024.0)::bigint AS TotalSpaceKB,
    
    -- Used Space (Table main fork data) in KB
    ROUND(pg_relation_size(t.oid) / 1024.0)::bigint AS UsedSpaceKB,
    
    -- Unused / Index / TOAST Space in KB
    ROUND((pg_total_relation_size(t.oid) - pg_relation_size(t.oid)) / 1024.0)::bigint AS UnusedSpaceKB

FROM pg_class t
JOIN pg_namespace s ON s.oid = t.relnamespace
WHERE s.nspname NOT IN ('pg_catalog', 'information_schema')
  AND t.relkind IN ('r', 'p'); -- Ordinary and partitioned tables
";
    }
}