namespace JEO3.Engine
{
    internal static partial class PostgresQueries
    {
        internal const string RelationQuery = @"
SELECT
    sp.nspname AS PrimarySchema,
    tp.relname AS PrimaryTableName,
    cp.attname AS PrimaryColumnName,
    sp.nspname || '.' || tp.relname || '.' || cp.attname AS PrimaryColumnKey,
    sf.nspname || '.' || tf.relname || '.' || cf.attname AS ForeignColumnKey,
    format_type(cp.atttypid, NULL) AS PrimaryColumnDataType,
    sf.nspname AS ForeignSchema,
    tf.relname AS ForeignTableName,
    cf.attname AS ForeignColumnName,
    format_type(cf.atttypid, NULL) AS ForeignColumnDataType,
    con.conname AS ForeignKeyName,
    NOT cf.attnotnull AS ForeignIsNullable,
    CASE WHEN cardinality(con.conkey) > 1 THEN 1 ELSE 0 END AS IsCompositeKey,
    CASE 
        WHEN sp.nspname = 'public' THEN tp.relname 
        ELSE sp.nspname || '.' || tp.relname 
    END AS PrimaryTableFullName,
    sp.nspname || '.' || tp.relname || '.' || cp.attname AS PrimaryColumnFullName,
    CASE 
        WHEN sf.nspname = 'public' THEN tf.relname 
        ELSE sf.nspname || '.' || tf.relname 
    END AS ForeignTableFullName,
    sf.nspname || '.' || tf.relname || '.' || cf.attname AS ForeignColumnFullName
FROM (
    SELECT 
        oid AS conid,
        conname,
        conrelid,
        confrelid,
        conkey,
        confkey,
        generate_series(1, cardinality(conkey)) AS key_index
    FROM pg_constraint
    WHERE contype = 'f'
) con
    INNER JOIN pg_class tf ON tf.oid = con.conrelid
    INNER JOIN pg_class tp ON tp.oid = con.confrelid
    INNER JOIN pg_namespace sf ON sf.oid = tf.relnamespace
    INNER JOIN pg_namespace sp ON sp.oid = tp.relnamespace
    INNER JOIN pg_attribute cf ON cf.attrelid = con.conrelid AND cf.attnum = con.conkey[con.key_index]
    INNER JOIN pg_attribute cp ON cp.attrelid = con.confrelid AND cp.attnum = con.confkey[con.key_index]
WHERE 
    sf.nspname NOT IN ('pg_catalog', 'information_schema')
    AND sp.nspname NOT IN ('pg_catalog', 'information_schema')
ORDER BY
    sp.nspname,
    tp.relname,
    cp.attname,
    sf.nspname,
    tf.relname,
    cf.attname;";
    }
}
