namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string StoredProcedureParameterQuery = @"
SELECT 
    p.object_id AS ObjectId,
    p.parameter_id AS OrdinalPosition,
    p.name AS [Name],
    st.name AS DataType,
    t.name AS ProviderDataType,
    p.max_length AS MaxLength,
    CAST(p.precision AS INT) AS Precision,
    CAST(p.scale AS INT) AS Scale,
    CAST(p.is_output AS BIT) AS IsOutput,
    CAST(p.is_nullable AS BIT) AS IsNullable,
    CAST(p.is_readonly AS BIT) AS IsReadOnly,
    -- Tracks if a default value exists (T-SQL values must be parsed from sys.sql_modules)
    CAST(p.has_default_value AS BIT) AS HasDefaultValue 
FROM sys.parameters p WITH (NOLOCK)
LEFT JOIN sys.types t WITH (NOLOCK) 
    ON p.user_type_id = t.user_type_id
LEFT JOIN sys.types st WITH (NOLOCK) 
    ON p.system_type_id = st.system_type_id 
    AND st.user_type_id = st.system_type_id;";
    }
}