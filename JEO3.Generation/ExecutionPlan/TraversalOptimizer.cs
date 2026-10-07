using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Generation.ExecutionPlan
{
    public static class TraversalOptimizer
    {
        public static EvaluatedEdge EvaluateJoinQuality(IRelation relation, bool movingToChild, bool bias = false)
        {
            try
            {
                var edge = new EvaluatedEdge
                {
                    Relationship = relation
                };

                var pairs = relation.ColumnPairs;

                if (!pairs.Any())
                {
                    edge.WeightPenalty = 1000;
                    edge.Score = 0;
                    edge.Efficiency = JoinEfficiency.UnindexedHazard;
                    edge.Reasons.Add("Relationship contains no join columns.");
                    return edge;
                }

                int penalty = 0;
                bool allIndexed = true;
                bool perfectSeek = true;

                foreach (var pair in pairs)
                {
                    var target = movingToChild ? pair.ReferencedColumn : pair.ParentColumn;
                    if (target == null)
                    {
                        bool stop = true;
                        continue;
                    }

                    // FATAL HAZARDS
                    if (pair.ReferencedColumn?.DataType != pair.ParentColumn?.DataType)
                    {
                        edge.HasDatatypeMismatch = true;
                        penalty += 150; // Massively increased: Destroys SARGability (Implicit Conversion)
                        edge.Reasons.Add($"{pair.ReferencedColumn?.Name}: datatype mismatch ({pair.ReferencedColumn?.DataType} vs {pair.ParentColumn?.DataType}).");
                    }

                    switch (target.DataType?.ToLowerInvariant())
                    {
                        case "varchar":
                        case "nvarchar":
                            if (target.MaximumLength == -1)
                            {
                                penalty += 300;
                                edge.Reasons.Add($"{target.Name}: MAX length join is fatal.");
                            }
                            break;
                        case "xml":
                            penalty += 500;
                            edge.Reasons.Add($"{target.Name}: XML column.");
                            break;
                    }

                    // INDEXING & SARGABILITY
                    if (!target.IsIndexed)
                    {
                        penalty += 100; // Increased: Forces a full scan
                        allIndexed = false;
                        perfectSeek = false;
                        edge.Reasons.Add($"{target.Name}: not indexed.");
                    }
                    else
                    {
                        edge.IsCoveringIndex = true;
                        if (!target.IsIndexKey || !target.IndexLinks.Any(i => i.OrdinalPosition == 1))
                        {
                            penalty += 40; // Increased: Forces an Index Scan instead of Index Seek
                            perfectSeek = false;
                            edge.Reasons.Add($"{target.Name}: indexed, but not leading key.");
                        }
                        else
                        {
                            penalty -= 20; // Increased reward for perfect index alignment
                            edge.Reasons.Add($"{target.Name}: leading index key seek.");
                        }
                    }

                    // 3. COLUMN PROPERTIES
                    if (target.IsNullable)
                    {
                        edge.UsesNullableForeignKey = true;
                        penalty += 10; // Consolidated from the double-check in previous code
                        edge.Reasons.Add($"{target.Name}: nullable.");
                    }

                    if (target.IsComputed)
                    {
                        edge.UsesComputedColumn = true;
                        penalty += 80;
                        edge.Reasons.Add($"{target.Name}: computed column overhead.");
                    }

                    if (target.IsIdentity)
                    {
                        edge.UsesIdentityKey = true;
                        penalty -= 20;
                        edge.Reasons.Add($"{target.Name}: clustered identity.");
                    }

                    // 4. DATATYPE EFFICIENCY REWARDS
                    switch (target.DataType.ToLowerInvariant())
                    {
                        case "tinyint":
                        case "smallint":
                        case "int":
                            penalty -= 10;
                            edge.Reasons.Add($"{target.Name}: optimal integer join.");
                            break;
                        case "bigint":
                            penalty -= 5;
                            break;
                        case "uniqueidentifier":
                            penalty += 15; // Increased slightly: GUID joins cause page splitting fragmentation
                            edge.Reasons.Add($"{target.Name}: GUID comparison overhead.");
                            break;
                    }
                }

                // Composite keys
                if (pairs.Count > 1)
                {
                    penalty += (pairs.Count - 1) * 15;
                    edge.UsesCompositeKey = true;
                    edge.Reasons.Add($"Composite key ({pairs.Count} columns).");
                }

                // Table size bias
                if (bias)
                {
                    var targetRows = movingToChild ? relation.ReferencedTable.Rows : relation.ParentTable.Rows;
                    var sourceRows = movingToChild ? relation.ParentTable.Rows : relation.ReferencedTable.Rows;

                    if (targetRows > sourceRows * 5) // Adjusted to 5x to prevent over-penalizing normal 1-to-Many relationships
                    {
                        penalty += 20;
                        edge.Reasons.Add("Target table significantly larger.");
                    }
                }

                // Floor the penalty for weight calculation (can't have negative weight in graph traversal)
                edge.WeightPenalty = Math.Max(0, penalty);

                // Calculate a 0-100% UI Score. 
                // A penalty of 0 = 100%. A penalty of 150 = 0%.
                double calculatedScore = 100.0 - (edge.WeightPenalty * (100.0 / 150.0));
                edge.Score = Math.Round(Math.Clamp(calculatedScore, 0, 100), 2);

                // Efficiency Buckets
                edge.Efficiency = edge.WeightPenalty switch
                {
                    <= 5 => JoinEfficiency.PerfectSeek, // Tightened constraint
                    <= 60 => JoinEfficiency.CoveredScan,
                    _ => JoinEfficiency.UnindexedHazard
                };

                return edge;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
