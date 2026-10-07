using JEO3.Generation.ExecutionPlan;
using JEO3.Generation.Models;
using JEO3.Schema;


namespace JEO3.Generation
{
    public sealed class PlanDrivenQueryGenerator : IQueryGenerator
    {
        // Plan
        private ExecutionPlanStrategy _strategy;

        // Generate Basic
        public QueryGenerationResult Generate(ITable root) => Generate(root, QueryGenerationOptions.GetDefaultGenerationOptions());
        public QueryGenerationResult Generate(ITable root, QueryGenerationOptions options)
        {
            if (options == null || options.Traversal == null) return new();
            _strategy = StrategyCatalog.GetExecutionPlan(options);
            var tracker = new QueryTracker(root, options);

            if (options.EnableGraphGeneration == true)
            {
                var sharedBranchPath = new HashSet<string>();
                bool enforceGlobalDedup = _strategy.DiscoveryMode == DiscoveryMode.Strict;

                // PHASE 1: Upstream Parents Traversal Pass
                TraverseWide(tracker.Root, tracker.Options.Traversal.LevelsUp, sharedBranchPath, tracker, TraversalDirection.Up, enforceGlobalDedup);

                if (_strategy.DiscoveryMode == DiscoveryMode.WideNetExtended)
                {
                    var upstreamNodes = tracker.Nodes.ToList();
                    foreach (var upstreamNode in upstreamNodes)
                    {
                        sharedBranchPath.Clear(); // Keep branch paths pure between down-steps
                        TraverseWide(upstreamNode, tracker.Options.Traversal.LevelsDown, sharedBranchPath, tracker, TraversalDirection.Down, enforceGlobalDedup);
                    }
                }

                // PHASE 2: Post-Traversal Optimization / Tail Compression
                _strategy.PostTraversalCompression?.Invoke(tracker);
            }

            return QueryResultBuilder.GenerateQueryResult(tracker, options, GenerationMethod.PlanDrivenExecution);
        }

        // Jarvis
        private void TraverseWide(QueryTable current, int depthRemaining, HashSet<string> sharedBranchPath, QueryTracker tracker, TraversalDirection direction, bool enforceGlobalDedup)
        {
            if (tracker.CheckMaxDepthReached(current, direction, depthRemaining)) { return; }

            bool movingToChild = direction == TraversalDirection.Down;

            var rawRelationships = direction == TraversalDirection.Up
                ? current.Table.ParentRelations
                : current.Table.ChildRelations;

            // DYNAMIC PIPELINE FILTERING: Filter directly through the tracker's unified rule gate
            var relationships = rawRelationships
                .Where(rel => tracker.IsPathAllowed(rel, movingToChild))
                .ToList();

            // LEXICAL SEMANTIC BIAS SCORES
            var prioritizedSteps = relationships
                .Select(rel =>
                {
                    var plan = TraversalOptimizer.EvaluateJoinQuality(rel, movingToChild);

                    int hazardPenalty = (plan.Efficiency == JoinEfficiency.UnindexedHazard) ? 100000 : 0;
                    int basePenalty = (_strategy.EdgePolicy.SortByIndexWeight ? plan.WeightPenalty : 0) + hazardPenalty;
                    int semanticBonus = GetSemanticBiasScore(rel);

                    return (Relation: rel, Efficiency: plan.Efficiency, Penalty: basePenalty - semanticBonus);
                })
                .OrderBy(t => t.Penalty)
                .ThenBy(t => movingToChild ? t.Relation.ReferencedTable.Rows : t.Relation.ParentTable.Rows)
                .ToList();

            foreach (var step in prioritizedSteps)
            {
                if (tracker.CheckStopTraversal(sharedBranchPath, current, step.Relation, direction)) continue;

                long? targetTableId = movingToChild ? step.Relation.ReferencedObjectId : step.Relation.ParentObjectId;

                // Check if the target table exists in the active ancestral chain of the CURRENT node
                bool isDuplicateLane = false;
                var lineageCheck = current;
                while (lineageCheck != null)
                {
                    if (lineageCheck.TableObjectId == targetTableId)
                    {
                        isDuplicateLane = true;
                        break;
                    }
                    lineageCheck = lineageCheck.Parent;
                }

                var node = tracker.TryAddVisit(current, step.Relation, direction, enforceGlobalDedup);
                if (node == null) continue;

                // FIGMENTS: Safe Guard against recursive loops on the same path without starving adjacent branches from discovering their direct parent anchors
                if (isDuplicateLane) continue;

                // GOVERNING DYNAMICS TELEMETRY: Stunt duplicate chains to protect against join graph explosion
                //bool isDuplicateLane = tracker.Nodes.Any(n => n.TableObjectId == targetTableId);

                //var node = tracker.TryAddVisit(current, step.Relation, direction, enforceGlobalDedup);
                //if (node == null) continue;

                // NASH EQUILIBRIUM: Connect the structural anchor join line, but stop deep recursion
                //if (isDuplicateLane) continue;

                sharedBranchPath.Add(step.Relation.KeyName);
                try
                {
                    TraverseWide(node, depthRemaining - 1, sharedBranchPath, tracker, direction, enforceGlobalDedup);
                }
                finally
                {
                    sharedBranchPath.Remove(step.Relation.KeyName);
                }
            }
        }
        private int GetSemanticBiasScore(IRelation relation)
        {
            var name = relation.KeyName?.ToLowerInvariant() ?? string.Empty;
            return name.Contains("bill") || name.Contains("primary") || name.Contains("main")
                ? 500
                : name.Contains("ship") || name.Contains("alt") || name.Contains("second") ? 100 : 0;
        }
    }
}