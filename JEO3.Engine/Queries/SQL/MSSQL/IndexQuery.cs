using JEO3.Engine.Models;

namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string IndexQuery = @"


SET DEADLOCK_PRIORITY LOW;
SET XACT_ABORT OFF; -- errors caught per-table must not abort the whole batch

DECLARE @FragStats TABLE (ObjectId INT, IndexId INT, AvgFragmentationPercent FLOAT NULL, TotalPageCount BIGINT NULL);

DECLARE @ObjectId INT, @TableName NVARCHAR(256);
DECLARE @StartTime DATETIME2 = SYSUTCDATETIME();
DECLARE @MaxRuntimeMs INT = 15000; -- hard cap for the whole loop; tune to sit safely under client-side command timeout

DECLARE tbl_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT t.object_id, t.name FROM sys.tables t WHERE t.is_ms_shipped = 0;

OPEN tbl_cursor;
FETCH NEXT FROM tbl_cursor INTO @ObjectId, @TableName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- bail out of remaining tables once we're near the budget, instead of
    -- letting per-table timeouts stack up into an unbounded total
    IF DATEDIFF(MILLISECOND, @StartTime, SYSUTCDATETIME()) > @MaxRuntimeMs
        BREAK;

    BEGIN TRY
        SET LOCK_TIMEOUT 500;

        INSERT INTO @FragStats (ObjectId, IndexId, AvgFragmentationPercent, TotalPageCount)
        SELECT object_id, index_id,
               SUM(avg_fragmentation_in_percent * page_count) / NULLIF(SUM(page_count), 0),
               SUM(page_count)
        FROM sys.dm_db_index_physical_stats(DB_ID(), @ObjectId, NULL, NULL, 'LIMITED')
        WHERE index_level = 0 AND alloc_unit_type_desc = 'IN_ROW_DATA'
        GROUP BY object_id, index_id;
    END TRY
    BEGIN CATCH
        -- locked / timed out / any other per-table error - skip it, leave no row
    END CATCH;

    FETCH NEXT FROM tbl_cursor INTO @ObjectId, @TableName;
END

CLOSE tbl_cursor;
DEALLOCATE tbl_cursor;

SET LOCK_TIMEOUT -1;

SELECT DISTINCT
    s.name AS " + nameof(FlatIndex.SchemaName) + @",
    t.name AS " + nameof(FlatIndex.TableName) + @",
    i.type_desc AS " + nameof(FlatIndex.IndexType) + @",
    i.name AS [" + nameof(FlatIndex.Name) + @"],
    i.index_id AS " + nameof(FlatIndex.IndexId) + @",
    c.name AS " + nameof(FlatIndex.ColumnName) + @",
    ic.column_id AS " + nameof(FlatIndex.ColumnId) + @",
    ic.index_column_id AS " + nameof(FlatIndex.IndexColumnId) + @",
    ic.key_ordinal AS " + nameof(FlatIndex.IndexOrdinalPosition) + @",
    (CASE WHEN i.type = 1 THEN 1 ELSE 0 END) AS " + nameof(FlatIndex.IsClustered) + @",
    ty.name AS " + nameof(FlatIndex.DataType) + @",
    i.is_unique AS " + nameof(FlatIndex.IsUnique) + @",
    i.is_unique_constraint AS " + nameof(FlatIndex.IsUniqueConstraint) + @",
    i.is_primary_key AS " + nameof(FlatIndex.IsPrimaryKey) + @",
    ic.is_included_column AS " + nameof(FlatIndex.IsIncluded) + @",
    i.is_disabled AS " + nameof(FlatIndex.IsDisabled) + @",
    ps.AvgFragmentationPercent AS " + nameof(FlatIndex.FragmentationPercentage) + @",
    ps.TotalPageCount AS " + nameof(FlatIndex.PageCount) + @"
FROM sys.indexes i WITH (NOLOCK)
INNER JOIN sys.objects t WITH (NOLOCK) ON i.object_id = t.object_id
INNER JOIN sys.schemas s WITH (NOLOCK) ON t.schema_id = s.schema_id
INNER JOIN sys.index_columns ic WITH (NOLOCK) ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c WITH (NOLOCK) ON ic.object_id = c.object_id AND ic.column_id = c.column_id
INNER JOIN sys.types ty WITH (NOLOCK) ON ty.user_type_id = c.user_type_id
LEFT JOIN @FragStats ps ON ps.ObjectId = i.object_id AND ps.IndexId = i.index_id
WHERE t.is_ms_shipped = 0
  AND i.is_hypothetical = 0;


";


        //-- ============================================================
        //-- Per-table fragmentation capture (lock-timeout-safe).
        //-- Each table gets its own LOCK_TIMEOUT window so one locked
        //-- table can never stall or fail the whole hydration run.
        //-- ============================================================

        //CREATE TABLE #FragStats (ObjectId INT, IndexId INT, AvgFragmentationPercent FLOAT NULL, TotalPageCount BIGINT NULL);

        //DECLARE @ObjectId INT, @TableName NVARCHAR(256);
        //DECLARE tbl_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT t.object_id, t.name FROM sys.tables t WHERE t.is_ms_shipped = 0;

        //OPEN tbl_cursor;
        //FETCH NEXT FROM tbl_cursor INTO @ObjectId, @TableName;

        //WHILE @@FETCH_STATUS = 0
        //BEGIN
        //    BEGIN TRY
        //        SET LOCK_TIMEOUT 500;

        //        INSERT INTO #FragStats (ObjectId, IndexId, AvgFragmentationPercent, TotalPageCount)
        //        SELECT object_id, index_id, SUM(avg_fragmentation_in_percent * page_count) / NULLIF(SUM(page_count), 0), SUM(page_count)
        //        FROM sys.dm_db_index_physical_stats(DB_ID(), @ObjectId, NULL, NULL, 'LIMITED')
        //        WHERE index_level = 0 AND alloc_unit_type_desc = 'IN_ROW_DATA'
        //        GROUP BY object_id, index_id;
        //    END TRY
        //    BEGIN CATCH
        //        -- table was locked / timed out - skip it, leave no row for this ObjectId
        //    END CATCH;

        //    FETCH NEXT FROM tbl_cursor INTO @ObjectId, @TableName;
        //END

        //CLOSE tbl_cursor;
        //DEALLOCATE tbl_cursor;

        //SET LOCK_TIMEOUT -1; -- reset to default for the rest of the session

        //-- ============================================================
        //-- Main index/column metadata output, joined against the
        //-- pre-captured fragmentation stats instead of calling the DMF
        //-- directly (avoids a single blocked table failing the whole query).
        //-- ============================================================

        //SELECT DISTINCT
        //    s.name AS [SchemaName],
        //    t.name AS [Table],
        //    i.type_desc AS [IndexType],
        //    i.name AS [Name],
        //    i.index_id [IndexId],
        //    c.name AS [ColumnName],
        //    ic.column_id [ColumnId],
        //    ic.index_column_id [IndexColumnId],
        //    ic.key_ordinal [IndexOrdinalPosition],
        //    (CASE WHEN i.type = 1 THEN 1 ELSE 0 END) AS IsClustered,
        //    ty.name AS [DataType],
        //    i.is_unique AS IsUnique,
        //    i.is_unique_constraint AS IsUniqueConstraint,
        //    i.is_primary_key AS IsPrimaryKey,
        //    ic.is_included_column AS IsIncluded,
        //    i.is_disabled AS IsDisabled,
        //    ps.AvgFragmentationPercent AS FragmentationPercentage,
        //    ps.TotalPageCount AS PageCount
        //FROM sys.indexes i WITH (NOLOCK)
        //INNER JOIN sys.objects t WITH (NOLOCK) ON i.object_id = t.object_id
        //INNER JOIN sys.schemas s WITH (NOLOCK) ON t.schema_id = s.schema_id
        //INNER JOIN sys.index_columns ic WITH (NOLOCK) ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        //INNER JOIN sys.columns c WITH (NOLOCK) ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        //INNER JOIN sys.types ty WITH (NOLOCK) ON ty.user_type_id = c.user_type_id
        //LEFT JOIN #FragStats ps ON ps.ObjectId = i.object_id AND ps.IndexId = i.index_id
        //WHERE t.is_ms_shipped = 0
        //  AND i.is_hypothetical = 0;

        //DROP TABLE #FragStats;





        //--        SELECT DISTINCT
        //--            s.name AS [SchemaName],
        //--            t.name AS [Table],
        //--            i.type_desc AS [IndexType],
        //--            i.name AS [Name],
        //--            i.index_id [IndexId],
        //--            c.name AS [ColumnName],
        //--            ic.column_id [ColumnId],
        //--            ic.index_column_id [IndexColumnId],
        //--            ic.key_ordinal [IndexOrdinalPosition],
        //--            (CASE WHEN i.type = 1 THEN 1 ELSE 0 END) AS IsClustered,
        //--            ty.name AS [DataType],
        //--            i.is_unique AS IsUnique,
        //--            i.is_unique_constraint AS IsUniqueConstraint,
        //--            i.is_primary_key AS IsPrimaryKey,
        //--            ic.is_included_column AS IsIncluded,
        //--            i.is_disabled AS IsDisabled,
        //--            ISNULL(ps.AvgFragmentationPercent, 0) AS FragmentationPercentage,
        //--            ISNULL(ps.TotalPageCount, 0) AS PageCount
        //--        FROM sys.indexes i
        //--        INNER JOIN sys.objects t ON i.object_id = t.object_id
        //--        INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
        //--        INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        //--        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        //--        INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        //--        LEFT JOIN (
        //--            SELECT
        //--                object_id,
        //--                index_id,
        //--                SUM(avg_fragmentation_in_percent * page_count) / NULLIF(SUM(page_count), 0) AS AvgFragmentationPercent,
        //--                SUM(page_count) AS TotalPageCount
        //--            FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED')
        //--            WHERE index_level = 0
        //--              AND alloc_unit_type_desc = 'IN_ROW_DATA'
        //--            GROUP BY object_id, index_id
        //--        ) ps ON ps.object_id = i.object_id AND ps.index_id = i.index_id
        //--        WHERE t.is_ms_shipped = 0
        //--          AND i.is_hypothetical = 0;

        //";
    }
}
