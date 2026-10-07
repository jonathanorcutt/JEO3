namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        internal const string ColumnQuery = @"
SELECT
    C.object_id AS ObjectId,
    CONCAT(S.name, '.', T.name) AS TablePath,
    CONCAT(S.name, '.', T.name, '.', C.name) AS Path,
    S.name AS SchemaName,
    T.name AS TableName,
    C.name AS [Name],
    C.column_id AS OrdinalPosition,
    C.is_nullable AS IsNullable,
    
    CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM sys.index_columns IC
        INNER JOIN sys.indexes IX ON IC.object_id = IX.object_id AND IC.index_id = IX.index_id
        WHERE IC.object_id = C.object_id AND IC.column_id = C.column_id AND IX.is_primary_key = 1
    ) THEN 1 ELSE 0 END AS bit) AS IsPrimaryKey,
    
    C.is_identity AS IsIdentity,
    TY.name AS DataType,

    -- [FIX 1] The Unicode Trap: convert byte length to char length for nchar/nvarchar
    CASE 
        WHEN C.max_length = -1 THEN -1 
        WHEN TY.name IN ('nchar', 'nvarchar', 'sysname') THEN C.max_length / 2 
        ELSE C.max_length 
    END AS MaximumLength,

    C.precision AS Precision,
    C.scale AS Scale,
    C.is_computed AS IsComputed,
    COALESCE(OBJECT_DEFINITION(C.default_object_id), '') AS ColumnDefault,
    
    -- [FIX 2] Removed the `Rows` join (see explanation below)

    C.column_id AS ColumnId,
    C.user_type_id AS UserTypeId,
    TY.system_type_id AS SystemTypeId,
    CC.definition AS ComputedDefinition,
    CC.is_persisted AS IsPersisted,
    C.generated_always_type AS GeneratedAlwaysType,
    C.generated_always_type_desc AS GeneratedAlwaysTypeDescription,
    C.is_hidden AS IsHidden,
    C.is_sparse AS IsSparse,
    C.is_column_set AS IsColumnSet,
    C.is_rowguidcol AS IsRowGuid,
    C.collation_name AS CollationName,
    C.encryption_type AS EncryptionType,
    C.encryption_type_desc AS EncryptionTypeDescription,
    C.is_masked AS IsMasked,
    C.xml_collection_id AS XmlCollectionId,
    EP.value AS Description,
    UT.name AS UserTypeName,
    UT.is_table_type AS IsTableType,
    BT.name AS SystemDataType,

    CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM sys.foreign_key_columns FKC WITH (NOLOCK)
        WHERE FKC.parent_object_id = C.object_id AND FKC.parent_column_id = C.column_id
    ) THEN 1 ELSE 0 END AS bit) AS IsForeignKey,
    
    CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM sys.index_columns IC WITH (NOLOCK)
        INNER JOIN sys.indexes IX WITH (NOLOCK) ON IX.object_id = IC.object_id AND IX.index_id = IC.index_id
        WHERE IC.object_id = C.object_id AND IC.column_id = C.column_id
    ) THEN 1 ELSE 0 END AS bit) AS IsIndexedInDatabase,
    
    C.is_filestream AS IsFileStream,

    -- Newly added properties
    TY.name AS ProviderDataType,
    --C.is_generated AS IsGenerated,
    CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM sys.index_columns IC WITH (NOLOCK)
        INNER JOIN sys.indexes IX WITH (NOLOCK) ON IC.object_id = IX.object_id AND IC.index_id = IX.index_id
        WHERE IC.object_id = C.object_id AND IC.column_id = C.column_id AND (IX.is_unique = 1 OR IX.is_unique_constraint = 1)
    ) THEN 1 ELSE 0 END AS bit) AS IsUnique,
    COALESCE(OBJECT_DEFINITION(C.default_object_id), '') AS DefaultValue,
    CC.definition AS  ComputedExpression,
    CAST(NULL AS nvarchar(128)) AS CharacterSetName
FROM sys.columns C WITH (NOLOCK)
INNER JOIN sys.tables T WITH (NOLOCK) ON T.object_id = C.object_id
INNER JOIN sys.schemas S WITH (NOLOCK) ON S.schema_id = T.schema_id
INNER JOIN sys.types TY WITH (NOLOCK) ON TY.user_type_id = C.user_type_id
LEFT JOIN sys.types BT WITH (NOLOCK) ON BT.system_type_id = C.system_type_id AND BT.user_type_id = BT.system_type_id
LEFT JOIN sys.computed_columns CC WITH (NOLOCK) ON CC.object_id = C.object_id AND CC.column_id = C.column_id
LEFT JOIN sys.extended_properties EP WITH (NOLOCK) ON EP.major_id = C.object_id AND EP.minor_id = C.column_id AND EP.name = 'MS_Description'
LEFT JOIN sys.types UT WITH (NOLOCK) ON UT.user_type_id = C.user_type_id AND UT.is_user_defined = 1
WHERE T.is_ms_shipped = 0 
--ORDER BY S.name, T.name, C.column_id;
";
    }
}

