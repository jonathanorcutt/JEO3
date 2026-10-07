namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string StatisticQuery = @"
SELECT 
    t.oid::integer AS ParentObjectId,
    st.oid::integer AS ObjectId,
    t.oid::integer AS ParentObjectId,
    st.oid::integer AS StatsId,
    st.stxname AS Name,
    FALSE AS IsAutoCreated,
    TRUE AS IsUserCreated,
    (st.stxexprs IS NOT NULL) AS HasFilter,
    pg_get_expr(st.stxexprs, st.stxrelid) AS FilterDefinition,
    NULL::timestamp AS CreateDate,
    GREATEST(stat.last_analyze, stat.last_autoanalyze) AS ModifyDate,
    NULL::varchar(128) AS CreatedBy
FROM pg_statistic_ext st
JOIN pg_class t ON t.oid = st.stxrelid
JOIN pg_namespace ns ON ns.oid = t.relnamespace
LEFT JOIN pg_stat_user_tables stat ON stat.relid = t.oid
WHERE ns.nspname NOT IN ('pg_catalog', 'information_schema')
  AND t.relkind IN ('r', 'p');
";
    }
}