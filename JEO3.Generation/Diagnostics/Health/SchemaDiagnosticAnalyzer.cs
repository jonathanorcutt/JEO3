using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Generation.Diagnostics
{
    public static class SchemaDiagnosticAnalyzer
    {
        public static SchemaDiagnostic Analyze(DatabaseContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var findings = new List<SchemaFinding>();

            findings.AddRange(AnalyzeTables(context));
            findings.AddRange(AnalyzeColumns(context));
            findings.AddRange(AnalyzeRelations(context));
            findings.AddRange(AnalyzeIndexes(context));
            findings.AddRange(AnalyzeStoredProcedures(context));
            findings.AddRange(AnalyzeViews(context));
            findings.AddRange(AnalyzeGraph(context));
            findings.AddRange(AnalyzeCheckConstraints(context));
            findings.AddRange(AnalyzeMissingIndexes(context));

            return new SchemaDiagnostic
            {
                Findings = findings,
                Score = CalculateScore(findings)
            };
        }

        private static IEnumerable<SchemaFinding> AnalyzeTables(DatabaseContext context)
        {
            if (context.Tables == null || !context.Tables.Any())
            {
                yield return Finding("SCHEMA", SchemaFindingSeverity.Warning, "No tables found", "The context does not contain any tables.", 2);
                yield break;
            }

            foreach (var table in context.Tables)
            {
                if (table.Name.StartsWith("tbl", StringComparison.OrdinalIgnoreCase))
                {
                    yield return Finding("NAMING", SchemaFindingSeverity.Info, "Legacy naming convention", "Table name uses legacy 'tbl' prefix.", 0, null, table);
                }
            }
        }

        public static SchemaDiagnostic AnalyzeTable(Table table, IReadOnlyCollection<Column> columns, IReadOnlyCollection<Relation> relations)
        {
            var findings = new List<SchemaFinding>();

            findings.AddRange(AnalyzeTableMetadata(table));
            findings.AddRange(AnalyzeColumnMetadata(table, columns));
            findings.AddRange(AnalyzeIndexCoverage(table, columns));
            findings.AddRange(AnalyzeKeyQuality(table, columns));
            findings.AddRange(AnalyzeRelationshipQuality(table, relations));
            findings.AddRange(AnalyzeTraversalRisk(table, relations));
            findings.AddRange(AnalyzeDataQuality(table, columns));
            findings.AddRange(AnalyzeSchemaRisk(table, columns));
            findings.AddRange(AnalyzeWorkloadBias(table, columns, relations));

            return new SchemaDiagnostic
            {
                Table = table,
                Findings = findings,
                Score = CalculateScore(findings)
            };
        }

        // ------------------------------------------------------------
        // TABLE METADATA
        // ------------------------------------------------------------

        private static IEnumerable<SchemaFinding> AnalyzeTableMetadata(Table table)
        {
            if (table.Rows == 0)
            {
                yield return Finding("DATA", SchemaFindingSeverity.Info, "Empty table", "Table contains no rows.", 0);
            }
            if (!table.Indexes.Any(i => i.IsClustered))
            {
                yield return new SchemaFinding
                {
                    IssueType = IssueType.HeapTable,
                    Category = "Performance",
                    Title = "Heap Table Detected",
                    Description = "This table does not have a clustered index. Data is stored unordered.",
                    Severity = SchemaFindingSeverity.Warning,
                    Score = 5,
                    Recommendation = "Consider adding a clustered index, typically on the primary key, to improve range scan and lookup performance."
                };
            }
            if (table.IsMemoryOptimized)
            {
                yield return Finding(
                "STORAGE", SchemaFindingSeverity.Info, "Memory optimized table", "Table uses In-Memory OLTP.", 1);
            }

            if (table.TemporalType != 0)
            {
                yield return Finding("TEMPORAL", SchemaFindingSeverity.Info, "Temporal table", "Table participates in system-versioned temporal data.", 1);
            }

            if (table.IsFileTable)
            {
                yield return Finding("STORAGE", SchemaFindingSeverity.Warning, "FileTable detected", "Table uses SQL Server FileTable functionality.", 2);
            }

            if (table.HistoryTableObjectId > 0)
            {
                yield return Finding("TEMPORAL", SchemaFindingSeverity.Info, "History table", "Table is associated with temporal history data.", 1);
            }
        }

        private static IEnumerable<SchemaFinding> AnalyzeColumns(DatabaseContext context)
        {
            foreach (var column in context.Columns)
            {
                // Check for deprecated data types
                if (string.Equals(column.DataType, "text", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(column.DataType, "ntext", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(column.DataType, "image", StringComparison.OrdinalIgnoreCase))
                {
                    yield return Finding("COLUMN", SchemaFindingSeverity.Warning, "Deprecated data type", $"Column '{column.Name}' uses deprecated data type '{column.DataType}'. Use varchar(max), nvarchar(max), or varbinary(max) instead.", 3, null, null, column.Name);
                }

                // Warn about wide fixed-length columns
                if (string.Equals(column.DataType, "char", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(column.DataType, "nchar", StringComparison.OrdinalIgnoreCase))
                {
                    if (column.MaximumLength > 100)
                    {
                        yield return Finding("COLUMN", SchemaFindingSeverity.Info, "Wide fixed-length column", $"Column '{column.Name}' is a wide fixed-length string. Consider variable length if data size varies.", 1, null, null, column.Name);
                    }
                }
            }
        }

        // ------------------------------------------------------------
        // COLUMN METADATA
        // ------------------------------------------------------------

        private static IEnumerable<SchemaFinding> AnalyzeColumnMetadata(Schema.ITable table, IReadOnlyCollection<IColumn> columns)
        {
            foreach (var column in columns)
            {
                if (column.IsComputed)
                {
                    yield return Finding("COLUMN", SchemaFindingSeverity.Info, $"Computed column: {column.Name}", "Column derives its value from a computed expression.", 0, null, table, column.Name);
                }

                if (column.IsPersisted)
                {
                    yield return Finding("COLUMN", SchemaFindingSeverity.Info, $"Persisted computed column: {column.Name}", "Computed value is persisted to storage.", 0, null, table, column.Name);
                }

                if (column.IsIdentity)
                {
                    yield return Finding("COLUMN", SchemaFindingSeverity.Info, $"Identity column: {column.Name}", "Column uses SQL Server identity generation.", 0, null, table, column.Name);
                }

                if (column.IsHidden)
                {
                    yield return Finding("COLUMN", SchemaFindingSeverity.Info, $"Hidden column: {column.Name}", "Column is hidden from normal metadata visibility.", 1, null, table, column.Name);
                }

                if (column.IsSparse)
                {
                    yield return Finding("COLUMN", SchemaFindingSeverity.Info, $"Sparse column: {column.Name}", "Column uses sparse storage.", 0, null, table, column.Name);
                }

                if (column.IsMasked)
                {
                    yield return Finding("SECURITY", SchemaFindingSeverity.Info, $"Masked column: {column.Name}", "Column uses dynamic data masking.", 0, null, table, column.Name);
                }

                if (column.EncryptionType != null)
                {
                    yield return Finding("SECURITY", SchemaFindingSeverity.Info, $"Encrypted column: {column.Name}", "Column uses SQL Server encryption metadata.", 0, null, table, column.Name);
                }
            }
        }
        private static IEnumerable<SchemaFinding> AnalyzeRelations(DatabaseContext context)
        {
            var relationGroups = context.Relations.GroupBy(r => r.ParentTable?.ObjectId ?? 0);

            foreach (var group in relationGroups)
            {
                if (group.Count() > 20)
                {
                    yield return Finding("RELATIONSHIP", SchemaFindingSeverity.Warning, "High number of inbound relationships", $"A table is the target of {group.Count()} foreign keys, potentially creating a bottleneck.", 3);
                }
            }
        }
        private static IEnumerable<SchemaFinding> AnalyzeMissingIndexes(DatabaseContext context)
        {
            if (context.MissingIndexes == null) yield break;

            foreach (var missing in context.MissingIndexes)
            {
                // Filter out low-value noise. You can tweak this threshold depending 
                // on the typical workload volume of the databases you are scanning.
                if (missing.ImprovementMeasure < 1000) continue;

                // High threshold for severe findings (e.g., massive table scans)
                var severity = missing.ImprovementMeasure > 50000
                    ? SchemaFindingSeverity.High
                    : SchemaFindingSeverity.Warning;

                var score = missing.ImprovementMeasure > 50000 ? 10 : 5;

                var description = $"Estimated impact: {missing.AvgUserImpact:F1}% | " +
                                  $"Improvement Measure: {missing.ImprovementMeasure:N0} | " +
                                  $"User Seeks: {missing.UserSeeks:N0}\n" +
                                  $"Suggested Keys: {missing.SuggestedKeyColumnsString}\n" +
                                  $"Includes: {missing.IncludedColumnsString ?? "None"}";

                yield return Finding(
                    category: "PERFORMANCE",
                    severity: severity,
                    title: "High-Impact Missing Index",
                    description: description,
                    score: score,
                    recommendation: $"Evaluate and test the following generated script:\n{missing.IndexCreationScript}",
                    table: missing.Table
                );
            }
        }

        private static IEnumerable<SchemaFinding> AnalyzeCheckConstraints(DatabaseContext context)
        {
            if (context.CheckConstraints == null) yield break;

            foreach (var constraint in context.CheckConstraints)
            {
                // Only flag constraints that fail the Ozar checks
                if (!constraint.IsDisabled && !constraint.IsNotTrusted) continue;

                var isCritical = constraint.IsDisabled;

                var severity = isCritical
                    ? SchemaFindingSeverity.Critical  // Bad data can get in
                    : SchemaFindingSeverity.Warning;  // Optimizer takes a performance hit

                var category = isCritical ? "DATA INTEGRITY" : "PERFORMANCE";
                var score = isCritical ? 8 : 4;

                var description = $"{constraint.IssueDescription}\nDefinition: {constraint.Definition}";

                yield return Finding(
                    category: category,
                    severity: severity,
                    title: isCritical ? "Disabled Check Constraint" : "Untrusted Check Constraint",
                    description: description,
                    score: score,
                    recommendation: $"Execute the following to fix the constraint:\n{constraint.RemediationScript}"
                // objectType: SchemaObjectType.CheckConstraint, // Assuming this exists in enum
                // objectName: constraint.Name,
                //schemaName: constraint.SchemaName,
                //databaseName: constraint.Name,
                );
            }
        }
        private static IEnumerable<SchemaFinding> AnalyzeIndexes(DatabaseContext context)
        {
            string[] prefixes = new string[] { "IX", "PK", "AK", "UK", "CX", "PXML", "UQ", "XMLPROPERTY", "XMLPATH", "IDX" };
            foreach (var index in context.Indexes)
            {
                // We're assuming the presence of an IsDisabled or similar flag based on SQL Server semantics
                // If this doesn't strictly match model, you can safely remove it.
                // Allow IDX Just For Legacy
                if (index.Name != null && prefixes.All(v => index.Name.StartsWith(v + "_") == false))
                {
                    yield return Finding("NAMING", SchemaFindingSeverity.Info, "Non-standard index naming", $"Index '{index.Name}' does not follow standard {string.Join("/", prefixes.Select(v => v + "_"))} naming prefixes.", 0);
                }
            }
        }

        private static IEnumerable<SchemaFinding> AnalyzeStoredProcedures(DatabaseContext context)
        {
            foreach (var procedure in context.Procedures)
            {
                if (procedure.SchemaName != "dbo" && procedure.Name.StartsWith("sp_", StringComparison.OrdinalIgnoreCase))
                {
                    yield return Finding("PROCEDURE", SchemaFindingSeverity.Warning, "Reserved prefix", $"Stored procedure '{procedure.Name}' uses the 'sp_' prefix which is reserved for system stored procedures.", 2);
                }

                if (string.IsNullOrWhiteSpace(procedure.Definition))
                {
                    yield return Finding("PROCEDURE", SchemaFindingSeverity.Info, "Empty or inaccessible definition", $"Definition for '{procedure.Name}' is missing or user lacks permissions.", 1);
                }
            }
        }

        private static IEnumerable<SchemaFinding> AnalyzeViews(DatabaseContext context)
        {
            foreach (var view in context.Views)
            {
                if (view.Name.StartsWith("vw", StringComparison.OrdinalIgnoreCase) || view.Name.StartsWith("viw", StringComparison.OrdinalIgnoreCase))
                {
                    yield return Finding("NAMING", SchemaFindingSeverity.Info, "Legacy view prefix", $"View '{view.Name}' uses legacy 'vw' prefix.", 0);
                }
            }
        }

        private static IEnumerable<SchemaFinding> AnalyzeGraph(DatabaseContext context)
        {
            foreach (var graphTable in context.Tables)
            {
                if (graphTable.ParentRelations?.Any() == false && graphTable.ChildRelations?.Any() == false && !context.Relations.Any(r => r.ParentTable?.ObjectId == graphTable.ObjectId || r.ReferencedTable?.ObjectId == graphTable.ObjectId))
                {
                    yield return Finding("GRAPH", SchemaFindingSeverity.Info, "Isolated Node", $"Graph Node '{graphTable.Name}' has no detected edges connected to it.", 1, null, graphTable);
                }
            }
        }


        // ------------------------------------------------------------
        // INDEX COVERAGE
        // ------------------------------------------------------------

        private static IEnumerable<SchemaFinding> AnalyzeIndexCoverage(ITable table, IReadOnlyCollection<IColumn> columns)
        {
            var foreignKeyColumns = columns
            .Where(x => x.IsForeignKey)
            .ToList();

            var unindexedForeignKeys = foreignKeyColumns
            .Where(x => !x.IsIndexed)
            .ToList();

            foreach (var column in unindexedForeignKeys)
            {
                yield return Finding(
                "INDEX",
                 SchemaFindingSeverity.Warning,
                $"Unindexed foreign key: {column.Name}",
                "Foreign key column does not appear to have usable index coverage.",
                3, null, table, column.Name);
            }

            if (foreignKeyColumns.Count > 0 &&
            unindexedForeignKeys.Count == 0)
            {
                yield return Finding("INDEX", SchemaFindingSeverity.Good, "Foreign key coverage", "Detected foreign key columns have usable index coverage.", -1, null, table);
            }
        }

        // ------------------------------------------------------------
        // KEY QUALITY
        // ------------------------------------------------------------

        private static IEnumerable<SchemaFinding> AnalyzeKeyQuality(ITable table, IReadOnlyCollection<IColumn> columns)
        {
            var primaryKeys = columns
            .Where(x => x.IsPrimaryKey)
            .ToList();

            if (primaryKeys.Count == 0)
            {
                yield return Finding("KEY", SchemaFindingSeverity.Warning, "No primary key detected", "Table does not expose a primary key.", 5);
            }

            if (primaryKeys.Count > 1)
            {
                yield return Finding("KEY", SchemaFindingSeverity.Info, "Composite primary key", $"Primary key contains {primaryKeys.Count} columns.", 1);
            }

            var nullablePrimaryKeys = primaryKeys
            .Where(x => x.IsNullable)
            .ToList();

            if (nullablePrimaryKeys.Count > 0)
            {
                yield return Finding("KEY", SchemaFindingSeverity.Critical, "Nullable primary key metadata", "One or more primary key columns are marked nullable.", 5);
            }
        }

        // ------------------------------------------------------------
        // RELATIONSHIP QUALITY
        // ------------------------------------------------------------

        private static IEnumerable<SchemaFinding> AnalyzeRelationshipQuality(ITable table, IReadOnlyCollection<Relation> relations)
        {
            if (relations.Count == 0)
            {
                yield return Finding("RELATIONSHIP", SchemaFindingSeverity.Info, "No relationships detected", "Table has no detected foreign-key relationships.", 0);

                yield break;
            }

            var nullableRelations = relations
            .Count(x => x.IsNullable);

            if (nullableRelations > 0)
            {
                yield return Finding("RELATIONSHIP", SchemaFindingSeverity.Info, "Nullable relationship paths", $"{nullableRelations} relationship(s) contain nullable foreign keys.", 1);
            }

            if (relations.Count >= 10)
            {
                yield return Finding("RELATIONSHIP", SchemaFindingSeverity.Warning, "Highly connected table", $"Table participates in {relations.Count} detected relationships.", 3);
            }
        }

        // ------------------------------------------------------------
        // TRAVERSAL RISK
        // ------------------------------------------------------------

        private static IEnumerable<SchemaFinding> AnalyzeTraversalRisk(ITable table, IReadOnlyCollection<Relation> relations)
        {
            if (relations.Count >= 10)
            {
                yield return Finding("GRAPH", SchemaFindingSeverity.Warning, "High graph connectivity", "High relationship density may significantly expand query generation paths.", 3);
            }

            var duplicateTargets = relations
            .GroupBy(x => x.ReferencedTable.ObjectId)
            .Where(x => x.Count() > 1)
            .ToList();

            if (duplicateTargets.Count > 0)
            {
                yield return Finding("GRAPH", SchemaFindingSeverity.Info, "Multiple relationship paths", "Multiple foreign-key paths target the same table.", 2);
            }
        }

        // ------------------------------------------------------------
        // DATA QUALITY
        // ------------------------------------------------------------

        private static IEnumerable<SchemaFinding> AnalyzeDataQuality(ITable table, IReadOnlyCollection<IColumn> columns)
        {
            if (table.Rows > 0 &&
            columns.Count(x => x.IsNullable) > columns.Count / 2)
            {
                yield return Finding("DATA", SchemaFindingSeverity.Warning, "Highly nullable table", "More than half of the table's columns allow NULL values.", 2);
            }

            var identityColumns = columns
            .Count(x => x.IsIdentity);

            if (identityColumns > 1)
            {
                yield return Finding("DATA", SchemaFindingSeverity.Warning, "Multiple identity columns", "Table contains multiple identity columns.", 2);
            }
        }

        // ------------------------------------------------------------
        // SCHEMA RISK
        // ------------------------------------------------------------

        private static IEnumerable<SchemaFinding> AnalyzeSchemaRisk(ITable table, IReadOnlyCollection<IColumn> columns)
        {
            if (table.Rows > 1_000_000 &&
            columns.Any(x => x.IsComputed))
            {
                yield return Finding("SCHEMA", SchemaFindingSeverity.Warning, "Large table with computed columns", "Computed expressions exist on a high-row-count table.", 2);
            }

            if (columns.Any(x => x.IsMasked || x.EncryptionType != null))
            {
                yield return Finding("SECURITY", SchemaFindingSeverity.Info, "Sensitive column metadata detected", "One or more columns contain security-related metadata.", 0);
            }
        }

        // ------------------------------------------------------------
        // WORKLOAD BIAS
        // ------------------------------------------------------------

        private static IEnumerable<SchemaFinding> AnalyzeWorkloadBias(ITable table, IReadOnlyCollection<IColumn> columns, IReadOnlyCollection<Relation> relations)
        {
            var name = table.Name.ToLowerInvariant();

            if (name.Contains("archive") ||
            name.Contains("history") ||
            name.Contains("audit") ||
            name.Contains("log"))
            {
                yield return Finding("BIAS", SchemaFindingSeverity.Info, "Potential historical or archival table", "Table name suggests historical, audit, archival, or logging usage.", -2);
            }
            if (name.Contains("test") || name.Contains("dev"))
            {
                yield return Finding("BIAS", SchemaFindingSeverity.Info, "Potential development environment table", "Table name suggests non-production use.", -1);
            }

            if (table.Rows == 0)
            {
                yield return Finding("BIAS", SchemaFindingSeverity.Info, "Empty table bias", "Empty table should receive reduced confidence during workload analysis.", -2);
            }

            if (table.TemporalType != 0)
            {
                yield return Finding("BIAS", SchemaFindingSeverity.Info, "Temporal workload bias", "Temporal metadata indicates specialized historical workload behavior.", -1);
            }

            if (relations.Count == 0 &&
            table.Rows == 0)
            {
                yield return Finding("BIAS", SchemaFindingSeverity.Info, "Low-confidence table", "Empty table with no relationships provides limited evidence for workload inference.", -3);
            }
        }

        // ------------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------------

        private static SchemaFinding Finding(string category, SchemaFindingSeverity severity, string title, string description, int score, string? recommendation = null,
            ITable? table = null, string? columnName = null, string? foreignKeyName = null)
        {
            return new SchemaFinding
            {
                DatabaseName = table?.DatabaseName,
                ObjectName = table?.Name,
                ObjectType = SchemaObjectType.Table,
                SchemaName = table?.SchemaName,
                Category = category,
                Severity = severity,
                Title = title,
                Description = description,
                Recommendation = recommendation,
                ObjectPath = table?.TablePath,
                ColumnName = columnName,
                ForeignKeyName = foreignKeyName,
                Score = score
            };
        }

        private static int CalculateScore(IEnumerable<SchemaFinding> findings)
        {
            return findings.Sum(x => x.Score);
        }
    }
}