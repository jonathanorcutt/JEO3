namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        // [FIX] Previously this const held a copy of the trigger query
        // (queried sys.triggers / IsInsteadOfTrigger). It now queries
        // sys.check_constraints and matches FlatCheckConstraint 1:1.
        internal const string CheckConstraintQuery = @"
SELECT 
    DB_ID() AS DatabaseId,
    DB_NAME() AS DatabaseName,
    t.object_id AS TableObjectId,
    cc.object_id AS ObjectId,
    cc.parent_object_id AS ParentObjectId,
    OBJECT_SCHEMA_NAME(cc.parent_object_id) AS SchemaName,
    OBJECT_NAME(cc.parent_object_id) AS TableName,
    -- parent_column_id is 0 if the constraint applies to the table level
    CASE WHEN cc.parent_column_id <> 0 THEN COL_NAME(cc.parent_object_id, cc.parent_column_id) ELSE NULL END AS ColumnName,
    cc.name AS [Name],
    cc.definition AS Definition,
    cc.is_disabled AS IsDisabled,
    cc.is_not_trusted AS IsNotTrusted,
    
    -- Brent-style human readable reason
    CASE 
        WHEN cc.is_disabled = 1 THEN 'Constraint is disabled. Data integrity is not enforced on new writes.'
        WHEN cc.is_not_trusted = 1 THEN 'Constraint is not trusted. The query optimizer will ignore it for execution plans.'
        ELSE 'Healthy'
    END AS IssueDescription,
    
    -- Generated ALTER statement for quick remediation
    CASE
        WHEN cc.is_disabled = 1 
            THEN 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(cc.parent_object_id) + '].[' + OBJECT_NAME(cc.parent_object_id) + '] CHECK CONSTRAINT [' + cc.name + '];'
        WHEN cc.is_not_trusted = 1
            THEN 'ALTER TABLE [' + OBJECT_SCHEMA_NAME(cc.parent_object_id) + '].[' + OBJECT_NAME(cc.parent_object_id) + '] WITH CHECK CHECK CONSTRAINT [' + cc.name + '];'
        ELSE NULL
    END AS RemediationScript,

    -- Retaining graph Path convention
    CONCAT(
        OBJECT_SCHEMA_NAME(cc.parent_object_id), '.', 
        OBJECT_NAME(cc.parent_object_id), 
        CASE WHEN cc.parent_column_id <> 0 THEN CONCAT('.', COL_NAME(cc.parent_object_id, cc.parent_column_id)) ELSE '' END
    ) AS [Path]
FROM sys.check_constraints AS cc WITH (NOLOCK)
INNER JOIN sys.tables t WITH (NOLOCK) ON t.object_id = cc.parent_object_id
INNER JOIN sys.columns c WITH (NOLOCK) ON cc.parent_column_id = c.column_id AND t.object_id = c.object_id
WHERE cc.is_ms_shipped = 0;
--ORDER BY t.name, cc.name;
";
    }
}

