namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string ViewColumnQuery = @"
SELECT 
    v.oid::integer AS ObjectId,
    v.relname AS ViewName,
    s.nspname AS SchemaName,
    c.attname AS ColumnName,
    format_type(c.atttypid, NULL) AS DataType,
    format_type(c.atttypid, NULL) AS ProviderDataType,
    c.attnum::integer AS OrdinalPosition,
    NOT c.attnotnull AS IsNullable,
    
    CASE 
        WHEN information_schema._pg_char_max_len(c.atttypid, c.atttypmod) IS NOT NULL 
            THEN information_schema._pg_char_max_len(c.atttypid, c.atttypmod)
        ELSE -1 
    END AS MaxLength,

    information_schema._pg_numeric_precision(c.atttypid, c.atttypmod)::integer AS Precision,
    information_schema._pg_numeric_scale(c.atttypid, c.atttypmod)::integer AS Scale,

    FALSE AS IsOutput

FROM pg_class v
JOIN pg_namespace s ON s.oid = v.relnamespace
JOIN pg_attribute c ON c.attrelid = v.oid
WHERE s.nspname NOT IN ('pg_catalog', 'information_schema')
  AND v.relkind = 'v' -- Filter specifically for views ('m' for materialized views)
  AND c.attnum > 0    -- Exclude system attributes
  AND NOT c.attisdropped;
";
    }
}
