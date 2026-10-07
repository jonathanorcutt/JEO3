namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        internal const string UserDefinedTypeDefinitionQuery = @"
SELECT 
    CASE 
        WHEN t.is_assembly_type = 1 THEN 'CLR_UDT'
        WHEN t.is_table_type = 1 THEN 'TABLE_TYPE'
        ELSE 'ALIAS_UDT'
    END AS FunctionType,
    'USER_DEFINED_TYPE' AS ObjectType,

    CASE 
        WHEN t.is_table_type = 1 THEN 
            (SELECT 'TABLE (' + STRING_AGG(c.name + ' ' + TYPE_NAME(c.user_type_id), ', ') + ')'
             FROM sys.table_types tt
             INNER JOIN sys.columns c ON tt.type_table_object_id = c.object_id
             WHERE tt.user_type_id = t.user_type_id)
        ELSE NULL 
    END AS [Definition],

    TYPE_NAME(t.system_type_id) AS ReturnType,
    t.name AS ProviderReturnType,

    -- NEW: only meaningful for alias types (table types get this per-column from the second query)
    t.max_length AS MaxLength,
    t.precision AS Precision,
    t.scale AS Scale,
    t.is_nullable AS IsNullable,
    t.schema_id AS SchemaId,

    CAST(0 AS BIT) AS IsSystemObject,
    CAST(0 AS BIT) AS IsDeterministic,

    t.user_type_id AS ObjectId,
    QUOTENAME(SCHEMA_NAME(t.schema_id)) + '.' + QUOTENAME(t.name) AS Identifier,
    DB_NAME() AS DatabaseName,
    SCHEMA_NAME(t.schema_id) AS SchemaName,
    t.name AS [Name],
    DB_NAME() + '.' + SCHEMA_NAME(t.schema_id) + '.' + t.name AS FullName

FROM sys.types t WITH (NOLOCK)
WHERE t.is_user_defined = 1;";
    }
}