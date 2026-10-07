using JEO3.Schema;


namespace JEO3.Generation.Diagnostics
{
    public static class SchemaDriftAnalyzer
    {

        /// <summary>
        /// Compares a Source schema (e.g. Dev) against a Target schema (e.g. Prod) to find drift.
        /// </summary>
        public static SchemaDiffReport CompareContexts(DatabaseContext source, DatabaseContext target)
        {
            var report = new SchemaDiffReport();

            CompareTables(source, target, report);
            CompareColumns(source, target, report);
            CompareStoredProcedures(source, target, report);
            AnalyzeTables(source, target, report);
            AnalyzeProcedureSimilarity(target, report);

            // Added Comparisons
            CompareViews(source, target, report);
            CompareIndexes(source, target, report);
            CompareRelations(source, target, report);

            return report;
        }

        private static void CompareTables(DatabaseContext source, DatabaseContext target, SchemaDiffReport report)
        {
            var sourceTables = source.Tables.ToDictionary(t => t.TablePath, StringComparer.OrdinalIgnoreCase);
            var targetTables = target.Tables.ToDictionary(t => t.TablePath, StringComparer.OrdinalIgnoreCase);

            // 1. Missing in Target (Needs to be deployed)
            foreach (var key in sourceTables.Keys.Except(targetTables.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(
                DriftType.MissingInTarget,
                "Table",
                key,
                "Table exists in Source but is missing in Target."));
            }

            // 2. Missing in Source (Orphaned in Prod / Dropped in Dev)
            foreach (var key in targetTables.Keys.Except(sourceTables.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(
                DriftType.MissingInSource,
                "Table",
                key,
                "Table exists in Target but is missing in Source. Was it dropped?"));
            }
        }

        private static void CompareColumns(DatabaseContext source, DatabaseContext target, SchemaDiffReport report)
        {
            // Key format: "[Schema].[Table].[Column]"
            var sourceCols = source.Columns.ToDictionary(c => $"{c.TablePath}.{c.Name}", StringComparer.OrdinalIgnoreCase);
            var targetCols = target.Columns.ToDictionary(c => $"{c.TablePath}.{c.Name}", StringComparer.OrdinalIgnoreCase);

            foreach (var src in sourceCols.Values)
            {
                var key = $"{src.TablePath}.{src.Name}";

                // New columns
                if (!targetCols.TryGetValue(key, out var tgt))
                {
                    report.Drifts.Add(new SchemaDrift(
                    DriftType.MissingInTarget,
                    "Column",
                    key,
                    $"Column missing in target. Type: {src.DataType}({src.MaximumLength})"));
                    continue;
                }

                // MUTATION CHECKS (The stealthy bugs)
                var mutations = new List<string>();

                if (!string.Equals(src.DataType, tgt.DataType, StringComparison.OrdinalIgnoreCase))
                    mutations.Add($"Type changed from {tgt.DataType} to {src.DataType}");

                if (src.MaximumLength != tgt.MaximumLength)
                    mutations.Add($"Length changed from {tgt.MaximumLength} to {src.MaximumLength}");

                if (src.IsNullable != tgt.IsNullable)
                    mutations.Add($"Nullability changed from {(tgt.IsNullable ? "NULL" : "NOT NULL")} to {(src.IsNullable ? "NULL" : "NOT NULL")}");

                if (mutations.Any())
                {
                    report.Drifts.Add(new SchemaDrift(
                    DriftType.Modified,
                    "Column",
                    key,
                    string.Join(" | ", mutations)));
                }
            }

            // Columns dropped in Dev but still in Prod
            foreach (var key in targetCols.Keys.Except(sourceCols.Keys, StringComparer.OrdinalIgnoreCase))
            {
                // Ensure we aren't flagging columns for a table that was dropped entirely (cuts down on noise)
                var tablePath = targetCols[key].TablePath;
                if (source.Tables.Any(t => t.TablePath.Equals(tablePath, StringComparison.OrdinalIgnoreCase)))
                {
                    report.Drifts.Add(new SchemaDrift(
                    DriftType.MissingInSource,
                    "Column",
                    key,
                    "Column exists in Target but missing in Source. Pending DROP."));
                }
            }
        }

        private static void CompareStoredProcedures(DatabaseContext source, DatabaseContext target, SchemaDiffReport report)
        {
            // Use a composite key for fully qualified procedure identification
            var srcProcs = source.Procedures
            .ToDictionary(p => $"{p.SchemaName}.{p.Name}", StringComparer.OrdinalIgnoreCase);

            var tgtProcs = target.Procedures
            .ToDictionary(p => $"{p.SchemaName}.{p.Name}", StringComparer.OrdinalIgnoreCase);

            // 1. Missing in Target (Need to be created)
            foreach (var key in srcProcs.Keys.Except(tgtProcs.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(DriftType.MissingInTarget, "StoredProcedure", key, "Missing in target database."));
            }

            // 2. Missing in Source (Orphaned in Target)
            foreach (var key in tgtProcs.Keys.Except(srcProcs.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(DriftType.MissingInSource, "StoredProcedure", key, "Orphaned procedure in target database."));
            }
        }

        private static void CompareViews(DatabaseContext source, DatabaseContext target, SchemaDiffReport report)
        {
            var sourceViews = source.Views.ToDictionary(v => $"{v.SchemaName}.{v.Name}", StringComparer.OrdinalIgnoreCase);
            var targetViews = target.Views.ToDictionary(v => $"{v.SchemaName}.{v.Name}", StringComparer.OrdinalIgnoreCase);

            foreach (var key in sourceViews.Keys.Except(targetViews.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(DriftType.MissingInTarget, "View", key, "View missing in target database."));
            }

            foreach (var key in targetViews.Keys.Except(sourceViews.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(DriftType.MissingInSource, "View", key, "Orphaned view in target database."));
            }
        }

        private static void CompareIndexes(DatabaseContext source, DatabaseContext target, SchemaDiffReport report)
        {
            // Assuming index name combined with table path constructs uniqueness
            var sourceIndexes = source.Indexes.Where(i => i.Name != null).ToDictionary(i => $"{i.SchemaName}.{i.Name}", StringComparer.OrdinalIgnoreCase);
            var targetIndexes = target.Indexes.Where(i => i.Name != null).ToDictionary(i => $"{i.SchemaName}.{i.Name}", StringComparer.OrdinalIgnoreCase);

            foreach (var key in sourceIndexes.Keys.Except(targetIndexes.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(DriftType.MissingInTarget, "Index", key, "Index missing in target database."));
            }

            foreach (var key in targetIndexes.Keys.Except(sourceIndexes.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(DriftType.MissingInSource, "Index", key, "Index exists in target but missing in source. Pending DROP."));
            }
        }

        private static void CompareRelations(DatabaseContext source, DatabaseContext target, SchemaDiffReport report)
        {
            // Try to match foreign keys by name if available, otherwise fallback to relationship path signature.
            string BuildRelationKey(IRelation r)
            {
                return string.IsNullOrWhiteSpace(r.KeyName)
                ? $"FK_{r.ReferencedTable?.Name}_{r.ParentTable?.Name}"
                : r.KeyName;
            }

            var sourceRels = source.Relations.ToDictionary(BuildRelationKey, StringComparer.OrdinalIgnoreCase);
            var targetRels = target.Relations.ToDictionary(BuildRelationKey, StringComparer.OrdinalIgnoreCase);

            foreach (var key in sourceRels.Keys.Except(targetRels.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(DriftType.MissingInTarget, "ForeignKey", key, "Foreign key missing in target database."));
            }

            foreach (var key in targetRels.Keys.Except(sourceRels.Keys, StringComparer.OrdinalIgnoreCase))
            {
                report.Drifts.Add(new SchemaDrift(DriftType.MissingInSource, "ForeignKey", key, "Foreign key exists in target but missing in source."));
            }
        }

        private static void AnalyzeTables(DatabaseContext source, DatabaseContext target, SchemaDiffReport report)
        {
            foreach (var table in target.Tables)
            {
                if (table.Rows == 0)
                {
                    report.Drifts.Add(new SchemaDrift(
                    DriftType.Informational,
                    "EmptyTable",
                    table.TablePath,
                    "Table contains zero rows. Review whether it is still required."));
                }

                // Protect against potential nulls or unassigned TableName/Name depending on interface structure
                var name = (table.Name ?? table.Name ?? "").ToLowerInvariant();

                if (name.Contains("test") ||
                name.Contains("backup") ||
                name.Contains("temp"))
                {
                    report.Drifts.Add(new SchemaDrift(
                    DriftType.Warning,
                    "SuspiciousTable",
                    table.TablePath,
                    "Table name suggests a test, temporary, or backup object. Review for cleanup."));
                }
            }
        }

        private static void AnalyzeProcedureSimilarity(DatabaseContext context, SchemaDiffReport report)
        {
            var procedures = context.Procedures
            .Where(p => !string.IsNullOrWhiteSpace(p.Definition))
            .ToList();

            for (int i = 0; i < procedures.Count; i++)
            {
                for (int j = i + 1; j < procedures.Count; j++)
                {
                    var a = procedures[i];
                    var b = procedures[j];

                    if (a.SchemaName.Equals(b.SchemaName,
                    StringComparison.OrdinalIgnoreCase))
                        continue;

                    var score = FuzzySharp.Fuzz.WeightedRatio(
                    NormalizeSql(a.Definition!),
                    NormalizeSql(b.Definition!));

                    if (score >= 85)
                    {
                        report.Drifts.Add(new SchemaDrift(
                        DriftType.Warning,
                        "PotentialDuplicateProcedure",
                        $"{a.SchemaName}.{a.Name}",
                        $"{score}% similar to {b.SchemaName}.{b.Name}. Review for duplicate or misplaced procedure."));
                    }
                }
            }
        }

        private static string NormalizeSql(string sql)
        {
            return string.Join(" ",
            sql.Split(
            new[] { ' ', '\r', '\n', '\t' },
            StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
        }
    }

    public record SchemaDrift(DriftType Type, string ObjectType, string ObjectPath, string Description);
}