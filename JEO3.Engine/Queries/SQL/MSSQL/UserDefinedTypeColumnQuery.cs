namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string UserDefinedTypeColumnQuery = @"
SELECT 
    tt.user_type_id AS ObjectId,
    c.column_id AS OrdinalPosition,
    c.name AS [Name],
    st.name AS DataType,
    t.name AS ProviderDataType,
    c.max_length AS MaxLength,
    CAST(c.precision AS INT) AS Precision,
    CAST(c.scale AS INT) AS Scale,
    CAST(0 AS BIT) AS IsOutput,
    CAST(c.is_nullable AS BIT) AS IsNullable,
    CAST(NULL AS SQL_VARIANT) AS DefaultValue
FROM sys.table_types tt WITH (NOLOCK)
INNER JOIN sys.columns c WITH (NOLOCK) 
    ON tt.type_table_object_id = c.object_id
LEFT JOIN sys.types t WITH (NOLOCK) 
    ON c.user_type_id = t.user_type_id
LEFT JOIN sys.types st WITH (NOLOCK) 
    ON c.system_type_id = st.system_type_id 
    AND st.user_type_id = st.system_type_id;";
    }
}