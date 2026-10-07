using JEO3.Schema;

namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string TableQuery = @"
SELECT      
    T.object_id AS " + nameof(ITable.ObjectId) + @",        
    DB_NAME() AS " + nameof(ITable.DatabaseName) + @",        
    S.name AS " + nameof(ITable.SchemaName) + @",        
    CONCAT(S.name, '.', T.name) AS " + nameof(ITable.TablePath) + @",        
    T.name AS [" + nameof(ITable.Name) + @"],        
    '' AS " + nameof(ITable.CreatedBy) + @", -- Placeholder for principal tracking        
    T.create_date AS " + nameof(ITable.CreatedDate) + @",        
    T.modify_date AS " + nameof(ITable.DateLastModified) + @",        
    T.durability AS " + nameof(ITable.Durability) + @",        
    T.durability_desc AS " + nameof(ITable.DurabilityDescription) + @",        
    T.type_desc AS " + nameof(ITable.TableType) + @",        
    CAST(0 AS bit) AS " + nameof(ITable.IsView) + @",        
    ISNULL(P.TotalRows, 0) AS " + nameof(ITable.Rows) + @",        
    T.schema_id AS " + nameof(ITable.SchemaId) + @",        
    T.is_memory_optimized AS " + nameof(ITable.IsMemoryOptimized) + @",        
    T.temporal_type AS " + nameof(ITable.TemporalType) + @",        
    T.temporal_type_desc AS " + nameof(ITable.TemporalTypeDescription) + @",        
    T.is_filetable AS " + nameof(ITable.IsFileTable) + @",        
    T.lob_data_space_id AS " + nameof(ITable.LobDataSpaceId) + @",        
    T.history_table_id AS " + nameof(ITable.HistoryTableObjectId) + @",        
    T.lock_escalation_desc AS " + nameof(ITable.LockEscalationDescription) + @",
    ISNULL(SU.TotalSpaceKB, 0) AS " + nameof(ITable.TotalSpaceKB) + @",     
    ISNULL(SU.UsedSpaceKB, 0) AS " + nameof(ITable.UsedSpaceKB) + @",     
    ISNULL(SU.UnusedSpaceKB, 0) AS " + nameof(ITable.UnusedSpaceKB) + @"    
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