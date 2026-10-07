namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        internal const string StatisticQuery = @"
SELECT 
   s.object_id AS ParentObjectId,
    s.stats_id AS ObjectId,
    s.object_id AS ParentObjectId,
    s.stats_id AS StatsId,
    s.name AS [Name],
    s.auto_created AS IsAutoCreated,
    s.user_created AS IsUserCreated,
    s.has_filter AS HasFilter,
    s.filter_definition AS FilterDefinition,
    -- Standard audit properties (system stats catalog does not store create date or creator)
    CAST(NULL AS datetime2) AS CreateDate,
    STATS_DATE(s.object_id, s.stats_id) AS ModifyDate,
    CAST(NULL AS nvarchar(128)) AS CreatedBy
FROM sys.stats s WITH (NOLOCK)
INNER JOIN sys.tables t WITH (NOLOCK) ON s.object_id = t.object_id
WHERE t.is_ms_shipped = 0;";

    }
}