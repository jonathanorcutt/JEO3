using JEO3.Generation.Models;

namespace JEO3.Generation
{
    internal sealed class CardinalityEstimator
    {
        #region Properties

        private readonly TableStatisticsCatalog _statsCatalog;

        #endregion

        #region Initialization

        internal CardinalityEstimator(TableStatisticsCatalog statsCatalog)
        {
            _statsCatalog = statsCatalog ?? throw new ArgumentNullException(nameof(statsCatalog));
        }

        #endregion

        #region Estimation

        /// <summary>
        /// Analyzes a compiled list of QueryTable nodes to estimate output rows at each step.
        /// </summary>
        internal Dictionary<Guid, double> ComputeEstimates(List<QueryTable> nodes, EstimationOptions options)
        {
            var estimates = new Dictionary<Guid, double>();
            if (nodes == null || !nodes.Any()) return estimates;

            // Process Root Node (The entry node has no parent)
            var rootNode = nodes.First(n => n.Parent == null);
            var rootStats = _statsCatalog.Get(rootNode.Table.TablePath);

            double currentCardinality = options.RootMethod switch
            {
                RootTraversalMethod.PrimaryKeyLookup => 1.0,
                RootTraversalMethod.SelectiveFilteredScan => rootStats.TotalRowCount * options.CustomFilterSelectivity,
                RootTraversalMethod.BroadIndexScan => rootStats.TotalRowCount * 0.30, // 30% heuristic baseline
                RootTraversalMethod.FullTableScan => rootStats.TotalRowCount,
                _ => rootStats.TotalRowCount
            };

            estimates[rootNode.Id] = currentCardinality;

            // Process Downstream Joins Sequentially
            foreach (var node in nodes.Where(n => n.Parent != null))
            {
                var parentNode = node.Parent!;
                double parentEstimate = estimates.TryGetValue(parentNode.Id, out var pe) ? pe : 1.0;

                var rel = node.Relationship!;
                var tableStats = _statsCatalog.Get(node.Table.TablePath);

                // Determine if moving down the hierarchy (Parent -> Child) or up (Child -> Parent)
                bool isMovingDown = rel.ReferencedTable == node.Table;

                double nodeEstimate;

                if (isMovingDown)
                {
                    // Moving Down: One parent row can match multiple children.
                    // Multiply by the average fan-out / match factor of that foreign key.
                    var parentStats = _statsCatalog.Get(parentNode.Table.TablePath);
                    double matchFactor = parentStats.MatchFactors.TryGetValue(rel.KeyName, out var mf) ? mf : 2.5;

                    nodeEstimate = parentEstimate * matchFactor;
                }
                else
                {
                    // Moving Up: Multiple child rows collapse into exactly 1 unique primary parent record.
                    // Selectivity shrinks relative to the primary key unique constraints.
                    double innerJoinSelectivity = 1.0 / Math.Max(1, tableStats.TotalRowCount);
                    double joinResult = parentEstimate * tableStats.TotalRowCount * innerJoinSelectivity;

                    // If it's a left join, maintain at least the parent cardinality size
                    nodeEstimate = rel.IsNullable ? Math.Max(parentEstimate, joinResult) : joinResult;
                }

                estimates[node.Id] = nodeEstimate;
            }

            return estimates;
        }

        #endregion
    }
}
