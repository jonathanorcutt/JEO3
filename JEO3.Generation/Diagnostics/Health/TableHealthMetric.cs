using JEO3.Schema;

namespace JEO3.Generation.Models
{
    /// <summary>
    /// One row in the heatmap: a table plus its four normalized (0-100, higher = worse) severity
    /// scores. Raw values are also kept so the UI can show real numbers in tooltips.
    /// </summary>
    public sealed class TableHealthMetric
    {
        public required int? ObjectId { get; init; }
        public required string TablePath { get; init; }
        public required string SchemaName { get; init; }
        public required string Name { get; init; }

        public double RawFragmentationPercent { get; init; }
        public double RawUnindexedFkCount { get; init; }
        public double RawCoveragePercent { get; init; }
        public int RawMissingIndexCount { get; init; }

        public double FragmentationSeverity { get; init; }
        public double UnindexedFkSeverity { get; init; }
        public double CoverageSeverity { get; init; }
        public double MissingIndexSeverity { get; init; }
    }

    public static class TableHealthMetricBuilder
    {
        /// <summary>
        /// Builds one row per table with normalized severity scores.
        /// NOTE: the normalization choices below are judgment calls, not derived from data -
        /// adjust the divisors/caps to whatever actually separates "fine" from "bad" in schema.
        /// </summary>
        public static IReadOnlyList<TableHealthMetric> Build(IDatabaseContext context)
        {
            var tables = context.Tables;

            // Cap missing-index count normalization at the highest count seen, so one outlier table
            // doesn't compress everything else toward 0. Falls back to 1 to avoid divide-by-zero.
            var maxMissingIndexes = tables.Count == 0 ? 1 : Math.Max(1, tables.Max(t => t.MissingIndexes.Count));

            return tables.Select(table =>
            {
                var fragmentation = table.Indexes.Count == 0 ? 0 : table.Indexes.Average(i => i.FragmentationPercentage);

                var fkColumns = table.Columns.Where(c => c.IsForeignKey).ToList();
                var unindexedFkCount = fkColumns.Count(c => c.Indexes.Count == 0);
                var unindexedFkRatio = fkColumns.Count == 0 ? 0 : (double)unindexedFkCount / fkColumns.Count * 100;

                var indexableColumns = table.Columns.Count(c => c.IsForeignKey || c.IsPrimaryKey);
                var coveredColumns = table.Columns.Count(c => (c.IsForeignKey || c.IsPrimaryKey) && c.Indexes.Count > 0);
                var coveragePercent = indexableColumns == 0 ? 100 : (double)coveredColumns / indexableColumns * 100;

                var missingIndexCount = table.MissingIndexes.Count;

                return new TableHealthMetric
                {
                    ObjectId = table.ObjectId,
                    Name = table.Name,
                    SchemaName = table.SchemaName,
                    TablePath = $"{table.SchemaName}.{table.Name}",
                    RawFragmentationPercent = fragmentation ?? 0,
                    RawUnindexedFkCount = unindexedFkCount,
                    RawCoveragePercent = coveragePercent,
                    RawMissingIndexCount = missingIndexCount,

                    FragmentationSeverity = Math.Clamp(fragmentation ?? 0, 0, 100),
                    UnindexedFkSeverity = Math.Clamp(unindexedFkRatio, 0, 100),
                    CoverageSeverity = Math.Clamp(100 - coveragePercent, 0, 100), // inverted: low coverage = high severity
                    MissingIndexSeverity = Math.Clamp((double)missingIndexCount / maxMissingIndexes * 100, 0, 100)
                };
            })
            .OrderByDescending(m => m.FragmentationSeverity + m.UnindexedFkSeverity + m.CoverageSeverity + m.MissingIndexSeverity)
            .ToList();
        }
    }
}
