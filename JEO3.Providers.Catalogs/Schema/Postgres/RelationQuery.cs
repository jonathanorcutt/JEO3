namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string RelationQuery = @"
SELECT DISTINCT
    con.oid::integer AS ObjectId,
    NULL::timestamp AS KeyCreatedDate,
    NULL::timestamp AS KeyModifiedDate,
    con.conindid::integer AS KeyIndexId,
    'FOREIGN_KEY_CONSTRAINT' AS KeyType,
    con.conname AS KeyName,
    (cardinality(con.conkey) > 1) AS IsComposite,
    NOT cf.attnotnull AS IsNullable,
    NOT con.convalidated AS IsDisabled,
    NOT con.convalidated AS IsNotTrusted,
    cf.attgenerated <> '' AS IsComputed,
    CASE con.confdeltype
        WHEN 'a' THEN 'NO_ACTION'
        WHEN 'r' THEN 'RESTRICT'
        WHEN 'c' THEN 'CASCADE'
        WHEN 'n' THEN 'SET_NULL'
        WHEN 'd' THEN 'SET_DEFAULT'
        ELSE 'NO_ACTION'
    END AS DeleteAction,
    CASE con.confupdtype
        WHEN 'a' THEN 'NO_ACTION'
        WHEN 'r' THEN 'RESTRICT'
        WHEN 'c' THEN 'CASCADE'
        WHEN 'n' THEN 'SET_NULL'
        WHEN 'd' THEN 'SET_DEFAULT'
        ELSE 'NO_ACTION'
    END AS UpdateAction,
    sp.nspname AS PrimarySchema,
    tp.relname AS PrimaryTableName,
    cp.attname AS PrimaryColumnName,
    format_type(cp.atttypid, NULL) AS PrimaryDataType,
    concat(sp.nspname, '.', tp.relname, '.', cp.attname) AS PrimaryColumnPath,
    concat(sf.nspname, '.', tf.relname, '.', cf.attname) AS ForeignColumnPath,
    sf.nspname AS ForeignSchema,
    tf.relname AS ForeignTableName,
    cf.attname AS ForeignColumnName,
    format_type(cf.atttypid, NULL) AS ForeignDataType,
    concat(sp.nspname, '.', tp.relname) AS PrimaryTablePath,
    concat(sf.nspname, '.', tf.relname) AS ForeignTablePath

FROM pg_constraint con
-- Foreign / Child table & schema
JOIN pg_class tf ON tf.oid = con.conrelid
JOIN pg_namespace sf ON sf.oid = tf.relnamespace

-- Primary / Referenced table & schema
JOIN pg_class tp ON tp.oid = con.confrelid
JOIN pg_namespace sp ON sp.oid = tp.relnamespace

-- Unnest foreign key column positions in parallel
CROSS JOIN LATERAL generate_series(1, cardinality(con.conkey)) AS keys(idx)

-- Foreign column
JOIN pg_attribute cf 
    ON cf.attrelid = con.conrelid 
   AND cf.attnum = con.conkey[keys.idx]

-- Primary column
JOIN pg_attribute cp 
    ON cp.attrelid = con.confrelid 
   AND cp.attnum = con.confkey[keys.idx]

WHERE con.contype = 'f'
  AND sf.nspname NOT IN ('pg_catalog', 'information_schema')
  AND sp.nspname NOT IN ('pg_catalog', 'information_schema');
";
    }
}
