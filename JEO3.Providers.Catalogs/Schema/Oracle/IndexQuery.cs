namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string IndexQuery = @"
SELECT
    i.TABLE_OWNER AS SchemaName,
    i.TABLE_NAME AS TableName,
    i.INDEX_TYPE AS IndexType,
    i.INDEX_NAME AS Name,
    o.OBJECT_ID AS IndexId,
    ic.COLUMN_NAME AS ColumnName,
    c.COLUMN_ID AS ColumnId,
    ic.COLUMN_POSITION AS IndexColumnId,
    ic.COLUMN_POSITION AS IndexOrdinalPosition,
    CASE WHEN i.INDEX_TYPE = 'IOT - TOP' THEN 1 ELSE 0 END AS IsClustered,
    c.DATA_TYPE AS DataType,
    CASE WHEN i.UNIQUENESS = 'UNIQUE' THEN 1 ELSE 0 END AS IsUnique,
    COALESCE(k.IsUq, 0) AS IsUniqueConstraint,
    COALESCE(k.IsPk, 0) AS IsPrimaryKey,
    0 AS IsIncluded,
    CASE WHEN i.STATUS = 'UNUSABLE' THEN 1 ELSE 0 END AS IsDisabled,
    CAST(NULL AS NUMBER) AS FragmentationPercentage,
    i.LEAF_BLOCKS AS PageCount
FROM all_indexes i
INNER JOIN all_users u ON u.USERNAME = i.TABLE_OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_ind_columns ic ON ic.INDEX_OWNER = i.OWNER AND ic.INDEX_NAME = i.INDEX_NAME
INNER JOIN all_tab_cols c ON c.OWNER = i.TABLE_OWNER AND c.TABLE_NAME = i.TABLE_NAME AND c.COLUMN_NAME = ic.COLUMN_NAME
INNER JOIN all_objects o ON o.OWNER = i.OWNER AND o.OBJECT_NAME = i.INDEX_NAME AND o.OBJECT_TYPE = 'INDEX' AND o.SUBOBJECT_NAME IS NULL
LEFT JOIN (
    SELECT INDEX_OWNER, INDEX_NAME,
        MAX(CASE WHEN CONSTRAINT_TYPE = 'P' THEN 1 ELSE 0 END) AS IsPk,
        MAX(CASE WHEN CONSTRAINT_TYPE = 'U' THEN 1 ELSE 0 END) AS IsUq
    FROM all_constraints
    WHERE CONSTRAINT_TYPE IN ('P', 'U') AND INDEX_NAME IS NOT NULL
    GROUP BY INDEX_OWNER, INDEX_NAME
) k ON k.INDEX_OWNER = i.OWNER AND k.INDEX_NAME = i.INDEX_NAME
WHERE i.INDEX_TYPE <> 'LOB'
    AND i.TABLE_NAME NOT LIKE 'BIN$%'
    --AND i.TABLE_OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY i.TABLE_OWNER, i.TABLE_NAME, i.INDEX_NAME, ic.COLUMN_POSITION";
    }
}
