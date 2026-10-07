namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string FunctionParameterQuery = @"
SELECT 
    p.oid::integer AS ObjectId,
    params.ordinal_position::integer AS OrdinalPosition,
    COALESCE(params.parameter_name, concat('$'$'', params.ordinal_position)) AS Name,
    format_type(params.type_oid, NULL) AS DataType,
    format_type(params.type_oid, NULL) AS ProviderDataType,
    
    CASE 
        WHEN information_schema._pg_char_max_len(params.type_oid, -1) IS NOT NULL 
            THEN information_schema._pg_char_max_len(params.type_oid, -1)
        ELSE -1 
    END AS MaxLength,

    information_schema._pg_numeric_precision(params.type_oid, -1)::integer AS Precision,
    information_schema._pg_numeric_scale(params.type_oid, -1)::integer AS Scale,

    (params.parameter_mode IN ('o', 'b', 't')) AS IsOutput,
    TRUE AS IsNullable,
    (params.ordinal_position > (params.total_args - params.default_args)) AS HasDefaultValue

FROM pg_proc p
JOIN pg_namespace n ON n.oid = p.pronamespace
CROSS JOIN LATERAL (
    SELECT 
        idx.ordinal_position,
        p.proargnames[idx.ordinal_position] AS parameter_name,
        COALESCE(p.proallargtypes[idx.ordinal_position], p.proargtypes[idx.ordinal_position - 1]) AS type_oid,
        COALESCE(p.proargmodes[idx.ordinal_position], 'i') AS parameter_mode,
        COALESCE(cardinality(p.proallargtypes), cardinality(p.proargtypes), 0) AS total_args,
        p.pronargs - p.pronargdefaults AS default_args
    FROM generate_series(1, GREATEST(cardinality(p.proallargtypes), cardinality(p.proargtypes), 0)) WITH ORDINALITY AS idx(unused, ordinal_position)
) params
WHERE n.nspname NOT IN ('pg_catalog', 'information_schema')
  AND p.prokind = 'f'
  AND params.ordinal_position > 0;
";
    }
}
