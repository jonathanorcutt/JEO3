namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string UserDefinedTypeDefinitionQuery = @"
SELECT 
    CASE t.typtype
        WHEN 'c' THEN 'TABLE_TYPE'
        WHEN 'd' THEN 'ALIAS_UDT'
        WHEN 'e' THEN 'ENUM_UDT'
        WHEN 'r' THEN 'RANGE_UDT'
        ELSE 'ALIAS_UDT'
    END AS FunctionType,
    'USER_DEFINED_TYPE' AS ObjectType,

    -- Reconstructs definition string for composite types
    CASE t.typtype
        WHEN 'c' THEN (
            SELECT concat('TABLE (', string_agg(concat(a.attname, ' ', format_type(a.atttypid, a.atttypmod)), ', ' ORDER BY a.attnum), ')')
            FROM pg_attribute a
            WHERE a.attrelid = t.typrelid
              AND a.attnum > 0 
              AND NOT a.attisdropped
        )
        WHEN 'd' THEN format_type(t.typbasetype, t.typtypmod)
        ELSE NULL 
    END AS Definition,

    -- Underlying Base Type (for domain/alias types) or self
    CASE 
        WHEN t.typtype = 'd' THEN format_type(t.typbasetype, NULL)
        ELSE format_type(t.oid, NULL)
    END AS ReturnType,
    t.typname AS ProviderReturnType,

    -- MaxLength, Precision, Scale for domain/alias types
    CASE 
        WHEN information_schema._pg_char_max_len(COALESCE(NULLIF(t.typbasetype, 0), t.oid), COALESCE(NULLIF(t.typtypmod, -1), t.typtypmod)) IS NOT NULL 
            THEN information_schema._pg_char_max_len(COALESCE(NULLIF(t.typbasetype, 0), t.oid), COALESCE(NULLIF(t.typtypmod, -1), t.typtypmod))
        ELSE -1 
    END AS MaxLength,

    information_schema._pg_numeric_precision(COALESCE(NULLIF(t.typbasetype, 0), t.oid), COALESCE(NULLIF(t.typtypmod, -1), t.typtypmod))::integer AS Precision,
    information_schema._pg_numeric_scale(COALESCE(NULLIF(t.typbasetype, 0), t.oid), COALESCE(NULLIF(t.typtypmod, -1), t.typtypmod))::integer AS Scale,
    
    NOT t.typnotnull AS IsNullable,
    n.oid::integer AS SchemaId,

    FALSE AS IsSystemObject,
    FALSE AS IsDeterministic,

    t.oid::integer AS ObjectId,
    concat('""', n.nspname, '"".""', t.typname, '""') AS Identifier,
    current_database() AS DatabaseName,
    n.nspname AS SchemaName,
    t.typname AS Name,
    concat(current_database(), '.', n.nspname, '.', t.typname) AS FullName

FROM pg_type t
JOIN pg_namespace n ON n.oid = t.typnamespace
WHERE n.nspname NOT IN ('pg_catalog', 'information_schema')
  -- Filter for user-defined domains ('d'), composites ('c'), enums ('e'), and ranges ('r')
  AND t.typtype IN ('d', 'c', 'e', 'r')
  -- Exclude internal table-backing composite types automatically created for every table
  AND NOT EXISTS (
      SELECT 1 FROM pg_class c 
      WHERE c.oid = t.typrelid AND c.relkind IN ('r', 'p', 'v', 'm', 'f')
  );
";
    }
}
