namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string IndexQuery = @"
SELECT 
    s.nspname AS SchemaName,
    t.relname AS TableName,
    CASE 
        WHEN i.indisunique AND i.indisprimary THEN 'PRIMARY KEY'
        WHEN i.indisunique THEN 'UNIQUE'
        WHEN am.amname = 'btree' THEN 'BTREE'
        WHEN am.amname = 'hash' THEN 'HASH'
        WHEN am.amname = 'gin' THEN 'GIN'
        WHEN am.amname = 'gist' THEN 'GIST'
        WHEN am.amname = 'spgist' THEN 'SPGIST'
        WHEN am.amname = 'brin' THEN 'BRIN'
        ELSE UPPER(am.amname)
    END AS IndexType,
    idx_cls.relname AS Name,
    idx_cls.oid::integer AS IndexId,
    c.attname AS ColumnName,
    c.attnum::integer AS ColumnId,
    pos.ordinal_position::integer AS IndexColumnId,
    
    -- Key columns retain order; included columns get 0
    CASE 
        WHEN pos.ordinal_position <= i.indnkeyatts THEN pos.ordinal_position 
        ELSE 0 
    END AS IndexOrdinalPosition,

    (am.amname = 'heap' OR idx_cls.relkind = 'p') AS IsClustered,
    format_type(c.atttypid, NULL) AS DataType,
    i.indisunique AS IsUnique,
    (EXISTS (
        SELECT 1 
        FROM pg_constraint con 
        WHERE con.conindid = i.indexrelid AND con.contype = 'u'
    )) AS IsUniqueConstraint,
    i.indisprimary AS IsPrimaryKey,
    (pos.ordinal_position > i.indnkeyatts) AS IsIncluded,
    NOT idx_cls.relispartition AND NOT i.indisready AS IsDisabled,

    -- High-performance Bloat / Fragmentation Estimate (0.0 to 100.0%)
    COALESCE(
        ROUND(
            GREATEST(0.0, 
                (1.0 - (
                    (idx_cls.reltuples * 8) / 
                    NULLIF(idx_cls.relpages * (current_setting('block_size')::numeric / 1024), 0)
                )) * 100.0
            )::numeric, 2
        ), 0.0
    )::double precision AS FragmentationPercentage,

    idx_cls.relpages::bigint AS PageCount

FROM pg_index i
JOIN pg_class t ON t.oid = i.indrelid
JOIN pg_namespace s ON s.oid = t.relnamespace
JOIN pg_class idx_cls ON idx_cls.oid = i.indexrelid
JOIN pg_am am ON am.oid = idx_cls.relam

-- Unnest index key positions to map columns and included columns
CROSS JOIN LATERAL generate_series(1, cardinality(i.indkey)) WITH ORDINALITY AS pos(attnum_val, ordinal_position)
JOIN pg_attribute c ON c.attrelid = t.oid AND c.attnum = i.indkey[pos.ordinal_position - 1]

WHERE s.nspname NOT IN ('pg_catalog', 'information_schema')
  AND t.relkind IN ('r', 'p') -- Ordinary and partitioned tables
  AND c.attnum > 0;
";
    }
}
