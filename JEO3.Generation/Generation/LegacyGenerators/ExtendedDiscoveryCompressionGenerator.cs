using JEO3.Generation.ExecutionPlan;
using JEO3.Generation.Models;
using JEO3.Schema;


namespace JEO3.Generation
{
    /// <summary>
    /// Advanced engine that uses duplicate paths for deep extended discovery,
    /// then collapses the final tree to guarantee exactly one node per physical table.
    /// </summary>
    public sealed class ExtendedDiscoveryCompressionGenerator : IQueryGenerator
    {
        public ExtendedDiscoveryCompressionGenerator()
        {
        }

        public QueryGenerationResult Generate(ITable root) => Generate(root, QueryGenerationOptions.GetDefaultGenerationOptions());
        public QueryGenerationResult Generate(ITable root, QueryGenerationOptions options)
        {
            var tracker = new QueryTracker(root, options);

            if (options.EnableGraphGeneration == true)
            {
                var sharedBranchPath = new HashSet<string>();

                // PHASE 1: Run wide-net discovery with execution-plan metrics determining the sorting paths
                TraverseUpWide(tracker.Root, options.Traversal.LevelsUp, sharedBranchPath, tracker, options);

                var upstreamNodes = tracker.Nodes.ToList();
                foreach (var upstreamNode in upstreamNodes)
                {
                    sharedBranchPath.Clear();
                    TraverseDownWide(upstreamNode, options.Traversal.LevelsDown, sharedBranchPath, tracker, options);
                }

                // PHASE 2: Trim the fat! Collapse duplicates down to 1 node per table based on optimal index weight.
                CollapseDuplicateNodes(tracker);
            }

            var result = QueryResultBuilder.GenerateQueryResult(tracker, options, GenerationMethod.ExtendedDiscovery);
            return result;
        }

        /// <summary>
        /// Post-processing compression pass. Keeps only the optimal entry path for each unique table.
        /// </summary>
        private void CollapseDuplicateNodes(IQueryTracker tracker)
        {
            // Group all discovered nodes by their physical table ID
            var tableGroups = tracker.Nodes
                .Where(n => n.Direction != TraversalDirection.Root)
                .GroupBy(n => n.TableObjectId)
                .ToList();

            foreach (var group in tableGroups)
            {
                if (group.Count() > 1)
                {
                    // VALUE-TUPLE GREASE: Calculate path weight penalty for each duplicate node local loop
                    var optimalNode = group
                        .Select(node =>
                        {
                            // Calculate total index/join penalty score from this node back up to the parent chain
                            int totalPathPenalty = 0;
                            var current = node;
                            while (current?.Relationship != null)
                            {
                                // Pass true if relationship was a child direction link
                                var evaluation = TraversalOptimizer.EvaluateJoinQuality(current.Relationship, current.Direction == TraversalDirection.Down);
                                totalPathPenalty += evaluation.WeightPenalty;
                                current = current.Parent;
                            }

                            // Return an on-the-fly ValueTuple mapping the candidate node to its depth and structural quality score
                            return (Node: node, Depth: node.Depth, PathPenalty: totalPathPenalty);
                        })
                        // Sort by the best index path first, then fall back to lowest depth tie-breaker
                        .OrderBy(tuple => tuple.PathPenalty)
                        .ThenBy(tuple => tuple.Depth)
                        .Select(tuple => tuple.Node)
                        .First();

                    // Evict the redundant, sub-optimal duplicate nodes from the final execution graph
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

        private void TraverseUpWide(QueryTable current, int depthRemaining, HashSet<string> sharedBranchPath, QueryTracker tracker, QueryGenerationOptions options)
        {
            TraversalDirection direction = TraversalDirection.Up;
            if (tracker.CheckMaxDepthReached(current, direction, depthRemaining)) { return; }

            // VALUE-TUPLE GREASE: Evaluate cost metrics before visiting paths to prevent unindexed explosions
            var prioritizedParents = current.Table.ParentRelations
                .Select(rel =>
                {
                    var plan = TraversalOptimizer.EvaluateJoinQuality(rel, movingToChild: false);
                    return (Relation: rel, Efficiency: plan.Efficiency, Penalty: plan.WeightPenalty);
                })
                // Skip completely unindexed pathways if engine configuration demands optimization strictness
                .Where(t => !options.Traversal.StrictIndexMatchingOnly || t.Efficiency != JoinEfficiency.UnindexedHazard)
                .OrderBy(t => t.Penalty)
                .ToList();

            foreach (var step in prioritizedParents)
            {
                if (tracker.CheckStopTraversal(sharedBranchPath, current, step.Relation, direction))
                {
                    continue;
                }

                var node = tracker.TryAddVisit(current, step.Relation, direction, enforceGlobalDedup: false);
                if (node == null) continue; // Safety bounce if tracker blocks allocation

                sharedBranchPath.Add(step.Relation.KeyName);
                try
                {
                    TraverseUpWide(node, depthRemaining - 1, sharedBranchPath, tracker, options);
                }
                finally
                {
                    sharedBranchPath.Remove(step.Relation.KeyName);
                }
            }
        }

        private void TraverseDownWide(QueryTable current, int depthRemaining, HashSet<string> sharedBranchPath, QueryTracker tracker, QueryGenerationOptions options)
        {
            TraversalDirection direction = TraversalDirection.Down;
            if (tracker.CheckMaxDepthReached(current, direction, depthRemaining)) { return; }

            // VALUE-TUPLE GREASE: Rank structural efficiency metrics locally before allocating deep discovery tracks
            var prioritizedChildren = current.Table.ChildRelations
                .Select(rel =>
                {
                    var plan = TraversalOptimizer.EvaluateJoinQuality(rel, movingToChild: true);
                    return (Relation: rel, Efficiency: plan.Efficiency, Penalty: plan.WeightPenalty);
                })
                .Where(t => !options.Traversal.StrictIndexMatchingOnly || t.Efficiency != JoinEfficiency.UnindexedHazard)
                .OrderBy(t => t.Penalty)
                .ThenBy(t => t.Relation.ReferencedTable.Rows) // Smaller tables break index ties
                .ToList();

            foreach (var step in prioritizedChildren)
            {
                if (tracker.CheckStopTraversal(sharedBranchPath, current, step.Relation, direction))
                {
                    continue;
                }

                var node = tracker.TryAddVisit(current, step.Relation, direction, enforceGlobalDedup: false);
                if (node == null) continue;

                sharedBranchPath.Add(step.Relation.KeyName);
                try
                {
                    TraverseDownWide(node, depthRemaining - 1, sharedBranchPath, tracker, options);
                }
                finally
                {
                    sharedBranchPath.Remove(step.Relation.KeyName);
                }
            }
        }
    }
}