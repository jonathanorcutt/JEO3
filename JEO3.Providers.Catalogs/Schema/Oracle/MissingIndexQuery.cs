namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string MissingIndexQuery = @"
WITH fk AS (
    SELECT cn.OWNER, cn.TABLE_NAME, cn.CONSTRAINT_NAME, COUNT(*) AS ColCount,
        LISTAGG('""' || cl.COLUMN_NAME || '""', ', ') WITHIN GROUP (ORDER BY cl.POSITION) AS DisplayCols,
        LISTAGG(cl.COLUMN_NAME, '|') WITHIN GROUP (ORDER BY cl.POSITION) AS PlainCols
    FROM all_constraints cn
    INNER JOIN all_cons_columns cl ON cl.OWNER = cn.OWNER AND cl.CONSTRAINT_NAME = cn.CONSTRAINT_NAME
    WHERE cn.CONSTRAINT_TYPE = 'R'
    GROUP BY cn.OWNER, cn.TABLE_NAME, cn.CONSTRAINT_NAME
)
SELECT
    TO_NUMBER(sys_context('USERENV', 'CON_ID')) AS DatabaseId,
    sys_context('USERENV', 'DB_NAME') AS DatabaseName,
    o.OBJECT_ID AS ObjectId,
    fk.OWNER AS SchemaName,
    fk.TABLE_NAME AS TableName,
    ORA_HASH(fk.OWNER || '.' || fk.CONSTRAINT_NAME) AS IndexGroupHandle,
    ORA_HASH(fk.OWNER || '.' || fk.CONSTRAINT_NAME) AS IndexHandle,
    0 AS UserSeeks,
    0 AS UserScans,
    0 AS UserReads,
    0 AS AvgUserImpact,
    0 AS AvgTotalUserCost,
    CAST(NULL AS DATE) AS LastUserSeek,
    COALESCE(t.NUM_ROWS, 0) AS ImprovementMeasure,
    fk.DisplayCols AS EqualityColumnsDisplayString,
    CAST(NULL AS VARCHAR2(4000)) AS InequalityColumnsDisplayString,
    CAST(NULL AS VARCHAR2(4000)) AS IncludedColumnsDisplayString,
    fk.DisplayCols AS SuggestedKeyColumnsDisplayString,
    fk.PlainCols AS EqualityColumnsString,
    CAST(NULL AS VARCHAR2(4000)) AS InequalityColumnsString,
    CAST(NULL AS VARCHAR2(4000)) AS IncludedColumnsString,
    fk.PlainCols AS SuggestedKeyColumnsString,
    'CREATE INDEX ""' || fk.OWNER || '"".""IX_' || fk.CONSTRAINT_NAME || '"" ON ""' || fk.OWNER || '"".""' || fk.TABLE_NAME || '"" (' || fk.DisplayCols || ')' AS IndexCreationScript
FROM fk
INNER JOIN all_users u ON u.USERNAME = fk.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = fk.OWNER AND o.OBJECT_NAME = fk.TABLE_NAME AND o.OBJECT_TYPE = 'TABLE'
LEFT JOIN all_tables t ON t.OWNER = fk.OWNER AND t.TABLE_NAME = fk.TABLE_NAME
WHERE fk.TABLE_NAME NOT LIKE 'BIN$%'
    AND NOT EXISTS (
        SELECT 1 FROM all_indexes i
        WHERE i.TABLE_OWNER = fk.OWNER AND i.TABLE_NAME = fk.TABLE_NAME
            AND fk.ColCount = (
                SELECT COUNT(*) FROM all_ind_columns ic
                INNER JOIN all_cons_columns fc ON fc.OWNER = fk.OWNER AND fc.CONSTRAINT_NAME = fk.CONSTRAINT_NAME AND fc.COLUMN_NAME = ic.COLUMN_NAME
                WHERE ic.INDEX_OWNER = i.OWNER AND ic.INDEX_NAME = i.INDEX_NAME AND ic.COLUMN_POSITION <= fk.ColCount))
    --AND fk.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY ImprovementMeasure DESC, fk.OWNER, fk.TABLE_NAME";
    }
}
