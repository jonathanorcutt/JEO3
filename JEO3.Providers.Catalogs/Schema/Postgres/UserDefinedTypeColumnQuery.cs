namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string UserDefinedTypeColumnQuery = @"
SELECT 
    t.oid::integer AS ObjectId,
    a.attnum::integer AS OrdinalPosition,
    a.attname AS Name,
    format_type(a.atttypid, NULL) AS DataType,
    format_type(a.atttypid, NULL) AS ProviderDataType,
    
    CASE 
        WHEN information_schema._pg_char_max_len(a.atttypid, a.atttypmod) IS NOT NULL 
            THEN information_schema._pg_char_max_len(a.atttypid, a.atttypmod)
        ELSE -1 
    END AS MaxLength,

    information_schema._pg_numeric_precision(a.atttypid, a.atttypmod)::integer AS Precision,
    information_schema._pg_numeric_scale(a.atttypid, a.atttypmod)::integer AS Scale,

    FALSE AS IsOutput,
    NOT a.attnotnull AS IsNullable,
    NULL::text AS DefaultValue

FROM pg_type t
JOIN pg_class c ON c.oid = t.typrelid
JOIN pg_attribute a ON a.attrelid = c.oid
JOIN pg_namespace n ON n.oid = t.typnamespace
WHERE n.nspname NOT IN ('pg_catalog', 'information_schema')
  AND c.relkind = 'c' -- Filter for composite types (UDTs)
  AND a.attnum > 0    -- Exclude system attributes
  AND NOT a.attisdropped;
";
    }
}
