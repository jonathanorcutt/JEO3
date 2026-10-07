using JEO3.Generation.Models;

namespace JEO3.Generation.ExecutionPlan
{
    public static class StrategyCatalog
    {
        public static ExecutionPlanStrategy StrictPerformance => new()
        {
            StrategyName = "Strict Performance Pathfinder",
            DiscoveryMode = DiscoveryMode.Strict,
            EdgePolicy = new EdgeSelectionPolicy
            {
                SortByIndexWeight = true,
                FilterUnindexedHazards = true
            }
        };

        public static ExecutionPlanStrategy ExtendedDiscoveryCompression => new()
        {
            StrategyName = "Extended Discovery with Compression Pass",
            DiscoveryMode = DiscoveryMode.WideNetExtended,
            EdgePolicy = new EdgeSelectionPolicy
            {
                SortByIndexWeight = true,
                FilterUnindexedHazards = false
            },
            PostTraversalCompression = tracker =>
            {
                var tableGroups = tracker.Nodes
                    .Where(n => n.Direction != TraversalDirection.Root)
                    .GroupBy(n => n.TableObjectId)
                    .ToList();

                foreach (var group in tableGroups)
                {
                    if (group.Count() > 1)
                    {
                        var optimalNode = group
                            .Select(node =>
                            {
                                int totalPathPenalty = 0;
                                var current = node;
                                while (current?.Relationship != null)
                                {
                                    var eval = TraversalOptimizer.EvaluateJoinQuality(current.Relationship, current.Direction == TraversalDirection.Down);
                                    totalPathPenalty += eval.WeightPenalty;
                                    current = current.Parent;
                                }
                                return (Node: node, Depth: node.Depth, Penalty: totalPathPenalty);
                            })
                            .OrderBy(t => t.Penalty)
                            .ThenBy(t => t.Depth)
                            .Select(t => t.Node)
                            .First();

                        foreach (var redundantNode in group)
                        {
                            if (redundantNode != optimalNode) tracker.Nodes.Remove(redundantNode);
                        }
                    }
                }
            }
        };

        private static ExecutionPlanStrategy Jarvis(QueryGenerationOptions options)
        {
            return new ExecutionPlanStrategy()
            {
                StrategyName = "Jarvis Execution Plan Discovery",
                DiscoveryMode = DiscoveryMode.WideNetExtended,
                EdgePolicy = new EdgeSelectionPolicy()
                {
                    SortByIndexWeight = options.Traversal.BiasSmallerTableScans,
                    FilterUnindexedHazards = options.Traversal.StrictIndexMatchingOnly,
                },

                PostTraversalCompression = (tracker) =>
                {
                    // BIND LEAF COLLAPSE: Immediately exit if the user disabled leaf collapsing
                    if (!options.Traversal.EnforceStrictLeafCollapse)
                        return;

                    // Group all discovered nodes by their physical table ID
                    var tableGroups = tracker.Nodes
                        .Where(n => n.Direction != TraversalDirection.Root)
                        .GroupBy(n => n.TableObjectId)
                        .ToList();

                    foreach (var group in tableGroups)
                    {
                        if (group.Count() > 1)
                        {
                            // Calculate total path optimization penalty back to the root chain using local ValueTuples
                            var optimalNode = group
                                .Select(node =>
                                {
                                    int totalPathPenalty = 0;
                                    var current = node;
                                    while (current?.Relationship != null)
                                    {
                                        var evaluation = TraversalOptimizer.EvaluateJoinQuality(
                                            current.Relationship,
                                            current.Direction == TraversalDirection.Down
                                        );
                                        totalPathPenalty += evaluation.WeightPenalty;
                                        current = current.Parent;
                                    }
                                    return (Node: node, Depth: node.Depth, PathPenalty: totalPathPenalty);
                                })
                                // Pull the absolute winner based on index metrics, then depth
                                .OrderBy(tuple => tuple.PathPenalty)
                                .ThenBy(tuple => tuple.Depth)
                                .Select(tuple => tuple.Node)
                                .First();

                            // Cleanly remove any redundant duplicate nodes
                            foreach (var redundantNode in group)
                            {
                                if (redundantNode != optimalNode)
                                {
                                    tracker.Nodes.Remove(redundantNode);
                                }
                            }
                        }
                    }
                }
            };
        }

        public static ExecutionPlanStrategy GetExecutionPlan(QueryGenerationOptions options)
        {
            if (options.ExecutionPlanStrategy == null)
            {
                switch (options.ExecutionPlan)
                {
                    case DiscoveryMode.Strict:
                        options.ExecutionPlanStrategy = StrictPerformance;
                        break;
                    case DiscoveryMode.WideNetExtended:
                        options.ExecutionPlanStrategy = ExtendedDiscoveryCompression;
                        break;
                    case DiscoveryMode.Jarvis:
                        options.ExecutionPlanStrategy = Jarvis(options);
                        break;
                    default:
                        options.ExecutionPlanStrategy = Jarvis(options);
                        break;
                }
            }
            return options.ExecutionPlanStrategy;
        }
    }
}
