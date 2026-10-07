namespace JEO3.Engine
{
    internal static partial class OracleQueries
    {
        internal const string FunctionParameterQuery = @"
SELECT 
    p.object_id AS ObjectId,
    p.parameter_id AS OrdinalPosition,
    p.name AS Name,
    st.name AS DataType,
    t.name AS ProviderDataType,
    p.max_length AS MaxLength,
    CAST(p.precision AS INT) AS Precision,
    CAST(p.scale AS INT) AS Scale,
    CAST(p.is_output AS BIT) AS IsOutput,
    CAST(p.is_nullable AS BIT) AS IsNullable,
    p.default_value AS DefaultValue
FROM sys.parameters p
INNER JOIN sys.types t ON p.user_type_id = t.user_type_id
INNER JOIN sys.types st ON p.system_type_id = st.system_type_id 
    AND st.system_type_id = st.user_type_id
--WHERE p.object_id IN (/* Pass Function ObjectIds from Pass 1 */);";
    }
}