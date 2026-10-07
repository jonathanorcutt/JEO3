namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string CheckConstraintQuery = @"
SELECT
    TO_NUMBER(sys_context('USERENV', 'CON_ID')) AS DatabaseId,
    sys_context('USERENV', 'DB_NAME') AS DatabaseName,
    o.OBJECT_ID AS TableObjectId,
    CAST(NULL AS NUMBER) AS ObjectId,
    o.OBJECT_ID AS ParentObjectId,
    cn.OWNER AS SchemaName,
    cn.TABLE_NAME AS TableName,
    CASE WHEN cl.ColumnCount = 1 THEN cl.ColumnName ELSE NULL END AS ColumnName,
    cn.CONSTRAINT_NAME AS Name,
    cn.SEARCH_CONDITION_VC AS Definition,
    CASE WHEN cn.STATUS = 'DISABLED' THEN 1 ELSE 0 END AS IsDisabled,
    CASE WHEN cn.VALIDATED = 'NOT VALIDATED' THEN 1 ELSE 0 END AS IsNotTrusted,
    CASE
        WHEN cn.STATUS = 'DISABLED' THEN 'Constraint is disabled. Data integrity is not enforced on new writes.'
        WHEN cn.VALIDATED = 'NOT VALIDATED' THEN 'Constraint is not validated. Existing rows may violate it and the optimizer cannot rely on it.'
        ELSE 'Healthy'
    END AS IssueDescription,
    CASE
        WHEN cn.STATUS = 'DISABLED' THEN 'ALTER TABLE ""' || cn.OWNER || '"".""' || cn.TABLE_NAME || '"" ENABLE CONSTRAINT ""' || cn.CONSTRAINT_NAME || '"";'
        WHEN cn.VALIDATED = 'NOT VALIDATED' THEN 'ALTER TABLE ""' || cn.OWNER || '"".""' || cn.TABLE_NAME || '"" MODIFY CONSTRAINT ""' || cn.CONSTRAINT_NAME || '"" VALIDATE;'
        ELSE NULL
    END AS RemediationScript,
    cn.OWNER || '.' || cn.TABLE_NAME || CASE WHEN cl.ColumnCount = 1 THEN '.' || cl.ColumnName ELSE '' END AS Path
FROM all_constraints cn
INNER JOIN all_users u ON u.USERNAME = cn.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = cn.OWNER AND o.OBJECT_NAME = cn.TABLE_NAME AND o.OBJECT_TYPE = 'TABLE'
LEFT JOIN (
    SELECT OWNER, CONSTRAINT_NAME, COUNT(*) AS ColumnCount, MIN(COLUMN_NAME) AS ColumnName
    FROM all_cons_columns
    GROUP BY OWNER, CONSTRAINT_NAME
) cl ON cl.OWNER = cn.OWNER AND cl.CONSTRAINT_NAME = cn.CONSTRAINT_NAME
WHERE cn.CONSTRAINT_TYPE = 'C'
    AND cn.TABLE_NAME NOT LIKE 'BIN$%'
    AND NOT REGEXP_LIKE(cn.SEARCH_CONDITION_VC, '^""[^""]+"" IS NOT NULL$')
    --AND cn.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY cn.OWNER, cn.TABLE_NAME, cn.CONSTRAINT_NAME";
    }
}