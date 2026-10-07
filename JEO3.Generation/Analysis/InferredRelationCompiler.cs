using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Generation
{
    public sealed class InferredRelationCompiler
    {
        /// <summary>
        /// Analyzes the flat schema architecture and discovers high-probability 
        /// relationships that lack formal database foreign keys.
        /// </summary>
        public static IEnumerable<InferredCandidate> CompileSuggestedRelations(
            IEnumerable<Table> cachedTables,
            IEnumerable<Column> cachedColumns,
            IEnumerable<GraphRelationConstraint> activePhysicalConstraints)
        {
            var suggestions = new List<InferredCandidate>();

            // Convert active constraints to a fast-lookup hash set to prevent suggesting duplicates
            var existingKeys = activePhysicalConstraints
                .Select(c => $"{c.PrimaryTable}.{c.PrimaryColumns}->{c.ForeignTable}.{c.ForeignColumns}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var table in cachedTables)
            {
                // Find columns that look like implicit keys (e.g., Ending in "Id", "Guid", "Code")
                var foreignKeyCandidates = cachedColumns
                    .Where(c => c.TableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase) &&
                                IsCandidateKeyName(c.Name));

                foreach (var col in foreignKeyCandidates)
                {
                    // Look for a target table matching the stripped prefix (e.g., "CustomerId" -> "Customer")
                    var targetTableName = InferTargetTableName(col.Name);
                    var targetTable = cachedTables.FirstOrDefault(t => t.Name.Equals(targetTableName, StringComparison.OrdinalIgnoreCase));

                    if (targetTable != null)
                    {
                        // SKIP SELF-MATCHING KEYS
                        if (table.Name.Equals(targetTable.Name, StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Match against the primary key anchor of that target table (defaulting to "Id")
                        var targetCol = cachedColumns.FirstOrDefault(c =>
                            c.TableName.Equals(targetTable.Name, StringComparison.OrdinalIgnoreCase) &&
                            (c.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) || c.Name.Equals($"{targetTable.Name}Id", StringComparison.OrdinalIgnoreCase)));

                        if (targetCol != null)
                        {
                            // Enforce Type-Safety sanity check
                            if (!AreTypesCompatible(col.DataType, targetCol.DataType))
                                continue;

                            // Skip if this edge is already explicitly defined in the physical schema
                            var signature = $"{targetTable.Name}.{targetCol.Name}->{table.Name}.{col.Name}";
                            if (existingKeys.Contains(signature))
                                continue;

                            // Create the pure engine constraint using Option 1 factory method!
                            var constraint = GraphRelationConstraint.CreateManual(
                                primaryTable: targetTable.Name,
                                primaryColumn: targetCol.Name,
                                foreignTable: table.Name,
                                foreignColumn: col.Name,
                                signature
                            );

                            suggestions.Add(new InferredCandidate
                            {
                                Constraint = constraint,
                                MatchingRule = "Target Table Suffix Match",
                                ConfidenceScore = 0.90,
                                Explanation = $"Implicit naming convention match: Mapped {table.Name}.{col.Name} to Primary Anchor {targetTable.Name}.{targetCol.Name}."
                            });
                        }
                    }
                }
            }

            return suggestions;
        }

        #region Private Heuristic Helpers

        private static bool IsCandidateKeyName(string columnName)
        {
            return columnName.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ||
            columnName.EndsWith("_id", StringComparison.OrdinalIgnoreCase) ||
            columnName.EndsWith("Guid", StringComparison.OrdinalIgnoreCase) ||
            columnName.EndsWith("_guid", StringComparison.OrdinalIgnoreCase);
        }

        private static string InferTargetTableName(string columnName)
        {
            string clean = columnName;

            // Strip out potential ID suffixes
            if (clean.EndsWith("Id", StringComparison.OrdinalIgnoreCase)) clean = clean[..^2];
            else if (clean.EndsWith("_id", StringComparison.OrdinalIgnoreCase)) clean = clean[..^3];
            else if (clean.EndsWith("Guid", StringComparison.OrdinalIgnoreCase)) clean = clean[..^4];
            else if (clean.EndsWith("_guid", StringComparison.OrdinalIgnoreCase)) clean = clean[..^5];

            // Trim any dangling underscores left over from snake_case formatting
            return clean.TrimEnd('_');
        }

        private static bool AreTypesCompatible(string sourceType, string targetType)
        {
            return sourceType.Equals(targetType, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
