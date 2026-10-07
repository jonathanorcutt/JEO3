namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        internal const string TableQuery = @"
SELECT      
    T.object_id AS ObjectId,        
    DB_NAME() AS DatabaseName,        
    S.name AS SchemaName,        
    CONCAT(S.name, '.', T.name) AS TablePath,        
    T.name AS [Name],        
    '' AS CreatedBy, -- Placeholder for principal tracking        
    T.create_date AS CreatedDate,        
    T.modify_date AS DateLastModified,        
    T.durability AS Durability,        
    T.durability_desc AS DurabilityDescription,        
    T.type_desc AS TableType,        
    CAST(0 AS bit) AS IsView,        
    ISNULL(P.TotalRows, 0) AS Rows,        
    T.schema_id AS SchemaId,        
    T.is_memory_optimized AS IsMemoryOptimized,        
    T.temporal_type AS TemporalType,        
    T.temporal_type_desc AS TemporalTypeDescription,        
    T.is_filetable AS IsFileTable,        
    T.lob_data_space_id AS LobDataSpaceId,        
    T.history_table_id AS HistoryTableObjectId,        
    T.lock_escalation_desc AS LockEscalationDescription,
    ISNULL(SU.TotalSpaceKB, 0) AS TotalSpaceKB,     
    ISNULL(SU.UsedSpaceKB, 0) AS UsedSpaceKB,     
    ISNULL(SU.UnusedSpaceKB, 0) AS UnusedSpaceKB
FROM sys.tables T WITH (NOLOCK)
INNER JOIN sys.schemas S WITH (NOLOCK) ON T.schema_id = S.schema_id    
LEFT JOIN (        
    -- Subquery for Row Counts
    SELECT object_id, SUM(rows) AS TotalRows         
    FROM sys.partitions WITH (NOLOCK)       
    WHERE index_id IN (0, 1)         
    GROUP BY object_id    
) P ON P.object_id = T.object_id  
LEFT JOIN (
    -- Subquery for Space Usage
    SELECT 
        p.object_id,
        SUM(a.total_pages) * 8 AS TotalSpaceKB, 
        SUM(a.used_pages) * 8 AS UsedSpaceKB, 
        (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
    FROM sys.indexes i WITH (NOLOCK)
    INNER JOIN sys.partitions p WITH (NOLOCK) ON i.object_id = p.object_id AND i.index_id = p.index_id
    INNER JOIN sys.allocation_units a WITH (NOLOCK) ON p.partition_id = a.container_id
    GROUP BY p.object_id
) SU ON SU.object_id = T.object_id
WHERE T.is_ms_shipped = 0
--ORDER BY S.name, T.name;";
    }
}