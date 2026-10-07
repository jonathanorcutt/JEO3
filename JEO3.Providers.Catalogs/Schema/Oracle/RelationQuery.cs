namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string RelationQuery = @"
SELECT
    ORA_HASH(fc.OWNER || '.' || fc.CONSTRAINT_NAME) AS ObjectId,
    CAST(NULL AS DATE) AS KeyCreatedDate,
    fc.LAST_CHANGE AS KeyModifiedDate,
    ix.OBJECT_ID AS KeyIndexId,
    'FOREIGN_KEY_CONSTRAINT' AS KeyType,
    fc.CONSTRAINT_NAME AS KeyName,
    CASE WHEN COUNT(*) OVER (PARTITION BY fcl.OWNER, fcl.CONSTRAINT_NAME) > 1 THEN 1 ELSE 0 END AS IsComposite,
    CASE WHEN fcol.NULLABLE = 'Y' THEN 1 ELSE 0 END AS IsNullable,
    CASE WHEN fc.STATUS = 'DISABLED' THEN 1 ELSE 0 END AS IsDisabled,
    CASE WHEN fc.VALIDATED = 'NOT VALIDATED' THEN 1 ELSE 0 END AS IsNotTrusted,
    CASE WHEN fcol.VIRTUAL_COLUMN = 'YES' THEN 1 ELSE 0 END AS IsComputed,
    REPLACE(fc.DELETE_RULE, ' ', '_') AS DeleteAction,
    'NO_ACTION' AS UpdateAction,
    pcl.OWNER AS PrimarySchema,
    pcl.TABLE_NAME AS PrimaryTableName,
    pcl.COLUMN_NAME AS PrimaryColumnName,
    pcol.DATA_TYPE AS PrimaryDataType,
    pcl.OWNER || '.' || pcl.TABLE_NAME || '.' || pcl.COLUMN_NAME AS PrimaryColumnPath,
    fcl.OWNER || '.' || fcl.TABLE_NAME || '.' || fcl.COLUMN_NAME AS ForeignColumnPath,
    fcl.OWNER AS ForeignSchema,
    fcl.TABLE_NAME AS ForeignTableName,
    fcl.COLUMN_NAME AS ForeignColumnName,
    fcol.DATA_TYPE AS ForeignDataType,
    pcl.OWNER || '.' || pcl.TABLE_NAME AS PrimaryTablePath,
    fcl.OWNER || '.' || fcl.TABLE_NAME AS ForeignTablePath
FROM all_constraints fc
INNER JOIN all_users u ON u.USERNAME = fc.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_cons_columns fcl ON fcl.OWNER = fc.OWNER AND fcl.CONSTRAINT_NAME = fc.CONSTRAINT_NAME
INNER JOIN all_constraints pc ON pc.OWNER = fc.R_OWNER";
    }
}
