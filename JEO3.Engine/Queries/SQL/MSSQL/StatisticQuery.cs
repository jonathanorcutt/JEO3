using JEO3.Schema;

namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string StatisticQuery = @"
SELECT 
   s.object_id AS " + nameof(IStatistic.ParentObjectId) + @",
    s.stats_id AS " + nameof(IStatistic.ObjectId) + @",
    s.object_id AS " + nameof(IStatistic.ParentObjectId) + @",
    s.stats_id AS " + nameof(IStatistic.StatsId) + @",
    s.name AS [" + nameof(IStatistic.Name) + @"],
    s.auto_created AS " + nameof(IStatistic.IsAutoCreated) + @",
    s.user_created AS " + nameof(IStatistic.IsUserCreated) + @",
    s.has_filter AS " + nameof(IStatistic.HasFilter) + @",
    s.filter_definition AS " + nameof(IStatistic.FilterDefinition) + @",
    -- Standard audit properties (system stats catalog does not store create date or creator)
    CAST(NULL AS datetime2) AS " + nameof(IStatistic.CreateDate) + @",
    STATS_DATE(s.object_id, s.stats_id) AS " + nameof(IStatistic.ModifyDate) + @",
    CAST(NULL AS nvarchar(128)) AS " + nameof(IStatistic.CreatedBy) + @"
FROM sys.stats s WITH (NOLOCK)
INNER JOIN sys.tables t WITH (NOLOCK) ON s.object_id = t.object_id
WHERE t.is_ms_shipped = 0;";

    }
}