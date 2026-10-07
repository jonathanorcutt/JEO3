namespace JEO3.Engine
{
    internal static partial class PostgresQueries
    {
        internal const string CheckConstraintQuery = @"
SELECT 
    object_id AS ObjectId,
    parent_object_id AS ParentObjectId,
    parent_column_id AS ParentColumnId,
    name AS Name,
    definition AS Definition,
    is_disabled AS IsDisabled,
    is_not_trusted AS IsNotTrusted
FROM sys.check_constraints
WHERE is_ms_shipped = 0;";
    }
}
