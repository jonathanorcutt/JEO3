using JEO3.Generation.ExecutionPlan;
using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Generation
{
    // Fork of PlanDrivenQueryGenerator, for diagram consumption only.
    // Everything below is a straight clone except one addition marked "NEW" -
    // a one-hop, non-recursed parent lookup off every node discovered during the
    // down-pass, surfacing lookup tables that hang off junction tables (e.g. the
    // parent of a bridge table you found as a child) without opening full
    // bidirectional recursion anywhere.
    //
    // Not used for SQL text generation. Consumers of QueryGenerationResult from
    // this generator should skip nodes where QueryTable.IsReferenceLeaf == true
    // when building executable query text - QueryResultBuilder itself is untouched
    // and does not know this concept, so that filtering is the caller's job.
    public sealed class DiagramDrivenGraphGenerator : IQueryGenerator
    {
        private ExecutionPlanStrategy _strategy;

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

        private void TraverseWide(QueryTable current, int depthRemaining, HashSet<string> sharedBranchPath, QueryTracker tracker, TraversalDirection direction, bool enforceGlobalDedup)
        {
            if (tracker.CheckMaxDepthReached(current, direction, depthRemaining))
            {
                return;
            }

            bool movingToChild = direction == TraversalDirection.Down;
            var rawRelationships = direction == TraversalDirection.Up ? current.Table.ParentRelations : current.Table.ChildRelations;

            var relationships = rawRelationships
                .Where(rel => tracker.IsPathAllowed(rel, movingToChild))
                .ToList();

            var prioritizedSteps = relationships
                .Select(rel =>
                {
                    var plan = TraversalOptimizer.EvaluateJoinQuality(rel, movingToChild);
                    int hazardPenalty = (plan.Efficiency == JoinEfficiency.UnindexedHazard) ? 100000 : 0;
                    int basePenalty = (_strategy.EdgePolicy.SortByIndexWeight ? plan.WeightPenalty : 0) + hazardPenalty;
                    return (Relation: rel, Efficiency: plan.Efficiency, Penalty: basePenalty);
                })
                .OrderBy(t => t.Penalty)
                .ThenBy(t => movingToChild ? t.Relation.ReferencedTable.Rows : t.Relation.ParentTable.Rows)
                .ToList();

            foreach (var step in prioritizedSteps)
            {
                if (tracker.CheckStopTraversal(sharedBranchPath, current, step.Relation, direction)) continue;

                long? targetTableId = movingToChild ? step.Relation.ReferencedObjectId : step.Relation.ParentObjectId;

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

                if (isDuplicateLane) continue;

                // NEW: one-hop parent lookup, down-pass only, no recursion.
                // Surfaces what this node's own parents are (lookup tables off a
                // junction table we just discovered as a child) without ever
                // walking further from them.
                if (movingToChild)
                {
                    AddReferenceLeaves(node, tracker);
                }

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

        // NEW: looks at `node`'s own parent relationships (Up direction) and adds
        // each allowed one as a terminal reference leaf. Skips the relation that
        // was just used to reach `node` in the first place, so we don't just
        // re-surface where we came from.
        private void AddReferenceLeaves(QueryTable node, QueryTracker tracker)
        {
            var cameFromRelationId = node.Relationship?.ObjectId;

            var parentRelations = node.Table.ParentRelations
                .Where(rel => rel.ObjectId != cameFromRelationId)
                .Where(rel => tracker.IsPathAllowed(rel, movingToChild: false))
                .ToList();

            foreach (var rel in parentRelations)
            {
                // Don't re-surface a table already sitting somewhere in this node's
                // own lineage - that's not new information for the diagram.
                bool alreadyInLineage = false;
                var lineageCheck = node;
                while (lineageCheck != null)
                {
                    if (lineageCheck.TableObjectId == rel.ParentObjectId)
                    {
                        alreadyInLineage = true;
                        break;
                    }
                    lineageCheck = lineageCheck.Parent;
                }
                if (alreadyInLineage) continue;

                tracker.TryAddReferenceLeaf(node, rel, TraversalDirection.Up);
            }
        }
    }
}
