using JEO3.Schema;

namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        // [KEPT] This is the canonical MissingIndexQuery — it matches
        // FlatMissingIndex field-for-field. The duplicate previously
        // defined in OtherQueries.cs has been removed (see that file).
        internal const string MissingIndexQuery = @"
    SELECT
        mid.database_id AS " + nameof(IMissingIndex.DatabaseId) + @",
        DB_NAME(mid.database_id) AS " + nameof(IMissingIndex.DatabaseName) + @",

        mid.object_id AS " + nameof(IMissingIndex.ObjectId) + @",
        OBJECT_SCHEMA_NAME(mid.object_id, mid.database_id) AS " + nameof(IMissingIndex.SchemaName) + @",
        OBJECT_NAME(mid.object_id, mid.database_id) AS " + nameof(IMissingIndex.TableName) + @",

        mig.index_group_handle AS " + nameof(IMissingIndex.IndexGroupHandle) + @",
        mid.index_handle AS " + nameof(IMissingIndex.IndexHandle) + @",

        -- Usage / impact
        migs.user_seeks AS " + nameof(IMissingIndex.UserSeeks) + @",
        migs.user_scans AS " + nameof(IMissingIndex.UserScans) + @",
        migs.user_seeks + migs.user_scans AS " + nameof(IMissingIndex.UserReads) + @",
        migs.avg_user_impact AS " + nameof(IMissingIndex.AvgUserImpact) + @",
        migs.avg_total_user_cost AS " + nameof(IMissingIndex.AvgTotalUserCost) + @",
        migs.last_user_seek AS " + nameof(IMissingIndex.LastUserSeek) + @",

        -- Brent-style prioritization
        CONVERT(
            decimal(18,2),
            migs.user_seeks
                * migs.avg_total_user_cost
                * (migs.avg_user_impact * 0.01)
        ) AS " + nameof(IMissingIndex.ImprovementMeasure) + @",

mid.equality_columns AS " + nameof(IMissingIndex.EqualityColumnsDisplayString) + @",
mid.inequality_columns AS " + nameof(IMissingIndex.InequalityColumnsDisplayString) + @",
mid.included_columns AS " + nameof(IMissingIndex.IncludedColumnsDisplayString) + @",
        -- Human-readable recommendation
        CASE
            WHEN mid.equality_columns IS NOT NULL
                 AND mid.inequality_columns IS NOT NULL
                THEN mid.equality_columns + ', ' + mid.inequality_columns

            WHEN mid.equality_columns IS NOT NULL
                THEN mid.equality_columns

            ELSE mid.inequality_columns
        END AS " + nameof(IMissingIndex.SuggestedKeyColumnsDisplayString) + @",



        -- Suggested key columns
        REPLACE(REPLACE(REPLACE(REPLACE(mid.equality_columns, ',', '|'), ' ', ''), '[', ''), ']', '') AS " + nameof(IMissingIndex.EqualityColumnsString) + @",
        REPLACE(REPLACE(REPLACE(REPLACE(mid.inequality_columns, ',', '|'), ' ', ''), '[', ''), ']', '') AS " + nameof(IMissingIndex.InequalityColumnsString) + @",
        REPLACE(REPLACE(REPLACE(REPLACE(mid.included_columns, ',', '|'), ' ', ''), '[', ''), ']', '') AS " + nameof(IMissingIndex.IncludedColumnsString) + @",

        -- Human-readable recommendation
        CASE
            WHEN mid.equality_columns IS NOT NULL
                 AND mid.inequality_columns IS NOT NULL
                THEN REPLACE(REPLACE(REPLACE(REPLACE(mid.equality_columns + ', ' + mid.inequality_columns, ',', '|'), ' ', ''), '[', ''), ']', '')

            WHEN mid.equality_columns IS NOT NULL
                THEN REPLACE(REPLACE(REPLACE(REPLACE(mid.equality_columns, ',', '|'), ' ', ''), '[', ''), ']', '')

            ELSE REPLACE(REPLACE(REPLACE(REPLACE(mid.inequality_columns, ',', '|'), ' ', ''), '[', ''), ']', '')
        END AS " + nameof(IMissingIndex.SuggestedKeyColumnsString) + @",

        -- Generated CREATE INDEX statement
        'CREATE NONCLUSTERED INDEX [IX_'
            + OBJECT_NAME(mid.object_id, mid.database_id)
            + '_MissingIndex_'
            + CONVERT(varchar(20), mid.index_handle)
            + '] ON '
            + mid.statement
            + ' ('
            + CASE
                WHEN mid.equality_columns IS NOT NULL
                     AND mid.inequality_columns IS NOT NULL
                    THEN mid.equality_columns + ', ' + mid.inequality_columns

                WHEN mid.equality_columns IS NOT NULL
                    THEN mid.equality_columns

                ELSE mid.inequality_columns
              END
            + ')'
            + CASE
                WHEN mid.included_columns IS NOT NULL
                    THEN ' INCLUDE (' + mid.included_columns + ')'
                ELSE ''
              END
            + ';' AS " + nameof(IMissingIndex.IndexCreationScript) + @"

    FROM sys.dm_db_missing_index_groups AS mig
        INNER JOIN sys.dm_db_missing_index_group_stats AS migs ON migs.group_handle = mig.index_group_handle
        INNER JOIN sys.dm_db_missing_index_details AS mid ON mid.index_handle = mig.index_handle
    WHERE mid.database_id = DB_ID()

    --ORDER BY
    --    ImprovementMeasure DESC,
    --    AvgUserImpact DESC,
    --    UserSeeks DESC;
";
    }
}

