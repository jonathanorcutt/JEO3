namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string UserDefinedTypeDefinitionQuery = @"
SELECT 
    -- FunctionType & ObjectType
    CASE 
        WHEN t.is_assembly_type = 1 THEN 'CLR_UDT'
        WHEN t.is_table_type = 1 THEN 'TABLE_TYPE'
        ELSE 'ALIAS_UDT'
    END AS FunctionType, -- Mapped to fit property structure
    'USER_DEFINED_TYPE' AS ObjectType,

    -- Definition (Null or Table schema outline)
    CASE 
        WHEN t.is_table_type = 1 THEN 
            (SELECT 'TABLE (' + STRING_AGG(c.name + ' ' + TYPE_NAME(c.user_type_id), ', ') + ')'
             FROM sys.columns c WHERE c.object_id = t.default_object_id) -- Internal table object ID
        ELSE NULL 
    END AS [Definition],

    -- ReturnType & ProviderReturnType
    TYPE_NAME(t.system_type_id) AS ReturnType,
    t.name AS ProviderReturnType,

    -- IsSystemObject & IsDeterministic
    CAST(0 AS BIT) AS IsSystemObject, -- UDTs are user-defined by nature
    CAST(0 AS BIT) AS IsDeterministic,

    -- Identifiers & Names
    t.user_type_id AS ObjectId,
    QUOTENAME(SCHEMA_NAME(t.schema_id)) + '.' + QUOTENAME(t.name) AS Identifier,
    DB_NAME() AS DatabaseName,
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS [Name],
    DB_NAME() + '.' + SCHEMA_NAME(t.schema_id) + '.' + t.name AS FullName

FROM sys.types t WITH (NOLOCK)
WHERE 
    t.is_user_defined = 1;";
    }
}