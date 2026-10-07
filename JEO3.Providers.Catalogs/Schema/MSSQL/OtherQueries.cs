namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        //        public const string MissingIndexQuery = @"
        //SELECT 
        //    CONVERT(decimal(18,2), user_seeks * avg_total_user_cost * (avg_user_impact * 0.01)) AS Improvement_Score,
        //    DB_NAME(mid.database_id) AS Database_Name,
        //    OBJECT_NAME(mid.object_id, mid.database_id) AS Table_Name,
        //    COALESCE(mid.equality_columns, '') AS Equality_Columns,
        //    COALESCE(mid.inequality_columns, '') AS Inequality_Columns,
        //    COALESCE(mid.included_columns, '') AS Included_Columns,
        //    migs.user_seeks AS User_Seeks,
        //    migs.user_scans AS User_Scans,
        //    CONVERT(decimal(5,2), migs.avg_user_impact) AS Avg_User_Impact_Percentage,
        //    migs.last_user_seek AS Last_User_Seek,
        //    'CREATE NONCLUSTERED INDEX [IX_' + OBJECT_NAME(mid.[object_id], mid.database_id) + '_' 
        //        + REPLACE(REPLACE(REPLACE(ISNULL(mid.equality_columns, mid.inequality_columns), '[', ''), ']', ''), ', ', '_') + ']'
        //        + ' ON ' + mid.[statement] 
        //        + ' (' + ISNULL(mid.equality_columns, '') 
        //        + CASE WHEN mid.equality_columns IS NOT NULL AND mid.inequality_columns IS NOT NULL THEN ', ' ELSE '' END 
        //        + ISNULL(mid.inequality_columns, '') + ')' 
        //        + CASE WHEN mid.included_columns IS NOT NULL THEN ' INCLUDE (' + mid.included_columns + ')' ELSE '' END AS Index_Creation_Script
        //FROM sys.dm_db_missing_index_groups mig
        //INNER JOIN sys.dm_db_missing_index_group_stats migs 
        //    ON migs.group_handle = mig.index_group_handle
        //INNER JOIN sys.dm_db_missing_index_details mid 
        //    ON mig.index_handle = mid.index_handle
        //WHERE mid.database_id = DB_ID() -- Filters to the current database context
        //ORDER BY Avg_User_Impact_Percentage desc, Improvement_Score DESC;";
    }
}
