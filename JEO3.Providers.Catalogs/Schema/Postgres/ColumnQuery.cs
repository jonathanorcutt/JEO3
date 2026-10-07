namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string ColumnQuery = @"
SELECT
    c.attrelid AS ObjectId,
    concat(s.nspname, '.', t.relname) AS TablePath,
    concat(s.nspname, '.', t.relname, '.', c.attname) AS Path,
    s.nspname AS SchemaName,
    t.relname AS TableName,
    c.attname AS Name,
    c.attnum AS OrdinalPosition,
    NOT c.attnotnull AS IsNullable,

    (EXISTS (
        SELECT 1
        FROM pg_constraint con
        WHERE con.conrelid = c.attrelid
          AND con.contype = 'p'
          AND c.attnum = ANY(con.conkey)
    )) AS IsPrimaryKey,

    (c.attidentity IN ('a', 'd') OR pg_get_serial_sequence(concat('""', s.nspname, '"".""', t.relname, '""'), c.attname) IS NOT NULL) AS IsIdentity,
    format_type(c.atttypid, NULL) AS DataType,

    CASE 
        WHEN isc.character_maximum_length IS NOT NULL THEN isc.character_maximum_length
        ELSE -1
    END AS MaximumLength,

    isc.numeric_precision AS Precision,
    isc.numeric_scale AS Scale,
    c.attgenerated <> '' AS IsComputed,
    COALESCE(pg_get_expr(def.adbin, def.adrelid), '') AS ColumnDefault,

    c.attnum AS ColumnId,
    c.atttypid AS UserTypeId,
    c.atttypid AS SystemTypeId,
    CASE 
        WHEN c.attgenerated <> '' THEN pg_get_expr(def.adbin, def.adrelid)
        ELSE NULL 
    END AS ComputedDefinition,
    (c.attgenerated = 's') AS IsPersisted,
    c.attidentity AS GeneratedAlwaysType,
    CASE c.attidentity
        WHEN 'a' THEN 'ALWAYS'
        WHEN 'd' THEN 'BY DEFAULT'
        ELSE 'NONE'
    END AS GeneratedAlwaysTypeDescription,
    (c.attnum < 0) AS IsHidden,
    FALSE AS IsSparse,
    FALSE AS IsColumnSet,
    FALSE AS IsRowGuid,
    col.collname AS CollationName,
    NULL AS EncryptionType,
    NULL AS EncryptionTypeDescription,
    FALSE AS IsMasked,
    NULL::integer AS XmlCollectionId,
    d.description AS Description,
    CASE WHEN typ.typtype = 'e' OR typ.typtype = 'd' THEN typ.typname ELSE NULL END AS UserTypeName,
    (typ.typtype = 'c') AS IsTableType,
    format_type(c.atttypid, NULL) AS SystemDataType,

    (EXISTS (
        SELECT 1
        FROM pg_constraint con
        WHERE con.conrelid = c.attrelid
          AND con.contype = 'f'
          AND c.attnum = ANY(con.conkey)
    )) AS IsForeignKey,

    (EXISTS (
        SELECT 1
        FROM pg_index idx
        WHERE idx.indrelid = c.attrelid
          AND c.attnum = ANY(idx.indkey)
    )) AS IsIndexedInDatabase,

    FALSE AS IsFileStream,

    format_type(c.atttypid, NULL) AS ProviderDataType,

    (EXISTS (
        SELECT 1
        FROM pg_constraint con
        WHERE con.conrelid = c.attrelid
          AND con.contype IN ('p', 'u')
          AND c.attnum = ANY(con.conkey)
    )) AS IsUnique,

    COALESCE(pg_get_expr(def.adbin, def.adrelid), '') AS DefaultValue,
    CASE 
        WHEN c.attgenerated <> '' THEN pg_get_expr(def.adbin, def.adrelid)
        ELSE NULL 
    END AS ComputedExpression,
    isc.character_set_name AS CharacterSetName

FROM pg_attribute c
INNER JOIN pg_class t ON t.oid = c.attrelid
INNER JOIN pg_namespace s ON s.oid = t.relnamespace
INNER JOIN pg_type typ ON typ.oid = c.atttypid
LEFT JOIN pg_attrdef def ON def.adrelid = c.attrelid AND def.adnum = c.attnum
LEFT JOIN pg_description d ON d.objoid = c.attrelid AND d.objsubid = c.attnum
LEFT JOIN pg_collation col ON col.oid = c.attcollation
LEFT JOIN information_schema.columns isc 
    ON isc.table_schema = s.nspname 
   AND isc.table_name = t.relname 
   AND isc.column_name = c.attname

WHERE t.relkind IN ('r', 'p')               -- Ordinary and partitioned tables
  AND s.nspname NOT IN ('pg_catalog', 'information_schema') -- Exclude system schemas
  AND c.attnum > 0                          -- Exclude system columns (oid, tableoid, etc.)
  AND NOT c.attisdropped;                  -- Exclude dropped columns
";
    }
}

