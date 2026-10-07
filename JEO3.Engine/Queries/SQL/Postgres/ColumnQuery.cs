namespace JEO3.Engine
{
    internal static partial class PostgresQueries
    {
        internal const string ColumnQuery = @"
SELECT
    n.nspname AS SchemaName,
    t.relname AS TableName,
    a.attname AS ColumnName,
    a.attnum AS OrdinalPosition,
    NOT a.attnotnull AS IsNullable,
    (EXISTS (
        SELECT 1 
        FROM pg_index i 
        WHERE i.indrelid = t.oid 
          AND i.indisprimary = true 
          AND a.attnum = ANY(i.indkey)
    )) AS IsPrimaryKey,
    (pg_get_serial_sequence(n.nspname || '.' || t.relname, a.attname) IS NOT NULL 
     OR a.attidentity IN ('a', 'd')) AS IsIdentity,
    format_type(a.atttypid, NULL) AS DataType,
    CASE 
        WHEN a.atttypmod > 4 THEN a.atttypmod - 4 
        ELSE -1 
    END AS MaximumLength,
    CASE 
        WHEN a.atttypid IN (21, 23, 20, 1700) THEN 
            CASE 
                WHEN a.atttypid = 21 THEN 16  -- smallint
                WHEN a.atttypid = 23 THEN 32  -- integer
                WHEN a.atttypid = 20 THEN 64  -- bigint
                WHEN a.atttypmod >= 0 THEN ((a.atttypmod - 4) >> 16) & 65535
                ELSE NULL 
            END
        ELSE NULL 
    END AS Precision,
    CASE 
        WHEN a.atttypid = 1700 AND a.atttypmod >= 0 THEN (a.atttypmod - 4) & 65535
        ELSE NULL 
    END AS Scale,
    COALESCE(pg_get_expr(d.adbin, d.adrelid), '') AS ColumnDefault,
    COALESCE(t.reltuples::bigint, 0) AS Rows,
    CASE 
        WHEN n.nspname = 'public' THEN t.relname 
        ELSE n.nspname || '.' || t.relname 
    END AS TableFullName,
    n.nspname || '.' || t.relname || '.' || a.attname AS ColumnKey
FROM pg_attribute a
    INNER JOIN pg_class t ON t.oid = a.attrelid
    INNER JOIN pg_namespace n ON n.oid = t.relnamespace
    LEFT JOIN pg_attrdef d ON d.adrelid = a.attrelid AND d.adnum = a.attnum
WHERE 
    t.relkind = 'r' -- Only ordinary tables
    AND a.attnum > 0 -- Filter out system columns
    AND NOT a.attisdropped -- Filter out dropped columns
    AND n.nspname NOT IN ('pg_catalog', 'information_schema') -- Filter out system schemas
ORDER BY
    n.nspname,
    t.relname,
    a.attnum;";
    }
}

