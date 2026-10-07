namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {

        internal const string FunctionDefinitionQuery = @"
SELECT 
    -- FunctionType & ObjectType
    o.type_desc AS FunctionType,
    'FUNCTION' AS ObjectType,

    -- Definition
    m.definition AS [Definition],

    -- ReturnType & ProviderReturnType
    CASE 
        WHEN o.type IN ('IF', 'TF') THEN 'TABLE'
        ELSE COALESCE(TYPE_NAME(p.system_type_id), 'UNKNOWN')
    END AS ReturnType,
    CASE 
        WHEN o.type IN ('IF', 'TF') THEN 'TABLE'
        ELSE COALESCE(TYPE_NAME(p.user_type_id), 'UNKNOWN')
    END AS ProviderReturnType,

    -- IsSystemObject
    o.is_ms_shipped AS IsSystemObject,

    -- IsDeterministic
    CAST(OBJECTPROPERTY(o.object_id, 'IsDeterministic') AS BIT) AS IsDeterministic,

    -- Identifiers & Names
    o.object_id AS ObjectId,
    QUOTENAME(SCHEMA_NAME(o.schema_id)) + '.' + QUOTENAME(o.name) AS Identifier,
    DB_NAME() AS DatabaseName,
    SCHEMA_NAME(o.schema_id) AS SchemaName,
    o.name AS [Name],
    DB_NAME() + '.' + SCHEMA_NAME(o.schema_id) + '.' + o.name AS FullName

FROM 
    (
        -- Combines user-defined functions and system/builtin functions
        SELECT object_id, name, schema_id, type, type_desc, is_ms_shipped FROM sys.objects WITH (NOLOCK)
        UNION ALL
        SELECT object_id, name, schema_id, type, type_desc, is_ms_shipped FROM sys.system_objects WITH (NOLOCK)
    ) o
LEFT JOIN sys.sql_modules m WITH (NOLOCK) ON o.object_id = m.object_id
LEFT JOIN sys.parameters p WITH (NOLOCK) ON o.object_id = p.object_id AND p.parameter_id = 0 -- 0 represents the return value for scalar functions
WHERE 
    o.type IN ('FN', 'IF', 'TF', 'FS', 'FT')
    AND o.object_id > 0; -- Scalar, Inline TVF, Table TVF, CLR Scalar, CLR Table UDF";
    }
}
