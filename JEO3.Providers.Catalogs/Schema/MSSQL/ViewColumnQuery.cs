namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        internal const string ViewColumnQuery = @"
SELECT 
    v.object_id AS ObjectId,
    v.name AS ViewName,
    s.name AS SchemaName,
    c.name AS ColumnName,
    st.name AS DataType,
    t.name AS ProviderDataType,
    c.column_id AS OrdinalPosition,
    CAST(c.is_nullable AS BIT) AS IsNullable,
    c.max_length AS MaxLength,
    CAST(c.precision AS INT) AS Precision,
    CAST(c.scale AS INT) AS Scale,
    -- Default values for view columns don't exist, hardcoding false/null or removing is best
    CAST(0 AS BIT) AS IsOutput 
FROM sys.views v WITH (NOLOCK)
INNER JOIN sys.schemas s WITH (NOLOCK) 
    ON v.schema_id = s.schema_id
LEFT JOIN sys.columns c WITH (NOLOCK) 
    ON v.object_id = c.object_id
LEFT JOIN sys.types t WITH (NOLOCK) 
    ON c.user_type_id = t.user_type_id
LEFT JOIN sys.types st WITH (NOLOCK) 
    ON c.system_type_id = st.system_type_id 
    AND st.user_type_id = st.system_type_id;
";
    }
}
