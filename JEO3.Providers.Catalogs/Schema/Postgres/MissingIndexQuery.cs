namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string MissingIndexQuery = @"
WITH missing_index_candidates AS (
    SELECT
        d.oid::integer AS DatabaseId,
        current_database() AS DatabaseName,
        t.oid::integer AS ObjectId,
        s.nspname AS SchemaName,
        t.relname AS TableName,
        
        -- Synthesized handles using table OID
        t.oid::integer AS IndexGroupHandle,
        t.oid::integer AS IndexHandle,

        -- Scan / Usage statistics
        st.seq_scan::bigint AS UserSeeks,
        st.idx_scan::bigint AS UserScans,
        (st.seq_scan + st.idx_scan)::bigint AS UserReads,
        
        -- Percentage of total scans that were sequential (unindexed)
        ROUND(
            (st.seq_scan::numeric / NULLIF(st.seq_scan + st.idx_scan, 0)) * 100.0, 2
        ) AS AvgUserImpact,

        -- Average cost estimation based on tuples fetched per sequential scan
        ROUND(
            (st.seq_tup_read::numeric / NULLIF(st.seq_scan, 0)), 2
        ) AS AvgTotalUserCost,

        st.last_seq_scan AS LastUserSeek,

        -- Best primary key / constraint candidate columns to suggest for the index
        (
            SELECT string_agg(quote_ident(a.attname), ', ')
            FROM pg_attribute a
            WHERE a.attrelid = t.oid 
              AND a.attnum > 0 
              AND NOT a.attisdropped
              AND a.attnotnull
            LIMIT 2
        ) AS SuggestedColsRaw

    FROM pg_stat_user_tables st
    JOIN pg_class t ON t.oid = st.relid
    JOIN pg_namespace s ON s.oid = t.relnamespace
    CROSS JOIN pg_database d
    WHERE d.datname = current_database()
      AND st.seq_scan > 0
      AND (st.seq_scan > st.idx_scan OR st.idx_scan = 0)
)
SELECT
    DatabaseId,
    DatabaseName,
    ObjectId,
    SchemaName,
    TableName,

    IndexGroupHandle,
    IndexHandle,

    UserSeeks,
    UserScans,
    UserReads,
    COALESCE(AvgUserImpact, 0.0) AS AvgUserImpact,
    COALESCE(AvgTotalUserCost, 0.0) AS AvgTotalUserCost,
    LastUserSeek,

    -- Brent Ozar style Improvement Measure projection
    ROUND(
        (UserSeeks * COALESCE(AvgTotalUserCost, 1.0) * (COALESCE(AvgUserImpact, 1.0) * 0.01))::numeric, 2
    ) AS ImprovementMeasure,

    -- Columns Display Strings
    COALESCE(SuggestedColsRaw, 'id') AS EqualityColumnsDisplayString,
    NULL::text AS InequalityColumnsDisplayString,
    NULL::text AS IncludedColumnsDisplayString,
    COALESCE(SuggestedColsRaw, 'id') AS SuggestedKeyColumnsDisplayString,

    -- Delimiter-replaced strings (matches SQL Server REPLACE logic -> '|' separated)
    REPLACE(COALESCE(SuggestedColsRaw, 'id'), ', ', '|') AS EqualityColumnsString,
    NULL::text AS InequalityColumnsString,
    NULL::text AS IncludedColumnsString,
    REPLACE(COALESCE(SuggestedColsRaw, 'id'), ', ', '|') AS SuggestedKeyColumnsString,

    -- Generated PostgreSQL CREATE INDEX statement
    concat(
        'CREATE INDEX ""idx_', TableName, '_missing_', IndexHandle, '"" ON ""', 
        SchemaName, '"".""', TableName, '"" (', 
        COALESCE(SuggestedColsRaw, 'id'), ');'
    ) AS IndexCreationScript

FROM missing_index_candidates;
";
    }
}