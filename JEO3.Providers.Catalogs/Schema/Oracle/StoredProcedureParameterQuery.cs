namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string StoredProcedureParameterQuery = @"
SELECT
    a.OBJECT_ID AS ObjectId,
    a.POSITION AS OrdinalPosition,
    a.ARGUMENT_NAME AS Name,
    a.DATA_TYPE AS DataType,
    COALESCE(a.TYPE_NAME, a.DATA_TYPE) AS ProviderDataType,
    CASE WHEN a.DATA_TYPE IN ('VARCHAR2', 'NVARCHAR2', 'CHAR', 'NCHAR', 'RAW') THEN COALESCE(a.DATA_LENGTH, -1) ELSE -1 END AS MaxLength,
    COALESCE(a.DATA_PRECISION, 0) AS Precision,
    COALESCE(a.DATA_SCALE, 0) AS Scale,
    CASE WHEN a.IN_OUT IN ('OUT', 'IN/OUT') THEN 1 ELSE 0 END AS IsOutput,
    1 AS IsNullable,
    0 AS IsReadOnly,
    CASE WHEN a.DEFAULTED = 'Y' THEN 1 ELSE 0 END AS HasDefaultValue
FROM all_arguments a
INNER JOIN all_objects o ON o.OBJECT_ID = a.OBJECT_ID AND o.OBJECT_TYPE = 'PROCEDURE'
INNER JOIN all_users u ON u.USERNAME = o.OWNER AND u.ORACLE_MAINTAINED = 'N'
WHERE a.PACKAGE_NAME IS NULL
    AND a.DATA_LEVEL = 0
    AND a.POSITION > 0
    AND a.ARGUMENT_NAME IS NOT NULL
    --AND o.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY a.OBJECT_ID, a.POSITION";
    }
}
