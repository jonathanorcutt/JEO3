using JEO3.Generation.Models;
using JEO3.Schema;


namespace JEO3.Generation
{
    /// <summary>
    /// Deep-climbing graph engine using symmetric backtracking memory management.
    /// Eliminates heap allocation during complex multi-schema structural routing.
    /// </summary>
    public sealed class BacktrackingSymmetricGenerator : IQueryGenerator
    {
        public BacktrackingSymmetricGenerator()
        {
        }

        public QueryGenerationResult Generate(ITable root) => Generate(root, QueryGenerationOptions.GetDefaultGenerationOptions());
        public QueryGenerationResult Generate(ITable root, QueryGenerationOptions options)
        {
            var tracker = new QueryTracker(root, options);

            if (options.EnableGraphGeneration == true)
            {
                // ONE single allocation for the entire lifecycle of the query generation run
                var sharedBranchPath = new HashSet<string>();

                // Phase 1: Climb up from the anchor root node
                TraverseUp(tracker.Root, options.Traversal.LevelsUp, sharedBranchPath, tracker);

                // Phase 2: Capture our safe upstream horizon checkpoint snapshot
                var upstreamNodes = tracker.Nodes.ToList();

                // Phase 3: Cascade downwards from each discovered architectural bridge node
                foreach (var upstreamNode in upstreamNodes)
                {
                    // Reset the shared path buffer for the next isolated descent pipeline
                    sharedBranchPath.Clear();
                    TraverseDown(upstreamNode, options.Traversal.LevelsDown, sharedBranchPath, tracker);
                }
            }

            return QueryResultBuilder.GenerateQueryResult(tracker, options, GenerationMethod.Backtracking);
        }

        private void TraverseUp(QueryTable current, int depthRemaining, HashSet<string> sharedBranchPath, QueryTracker tracker)
        {
            TraversalDirection direction = TraversalDirection.Up;
            if (tracker.CheckMaxDepthReached(current, direction, depthRemaining)) { return; }

            foreach (var relationship in current.Table.ParentRelations)
            {
                if (tracker.CheckStopTraversal(sharedBranchPath, current, relationship, direction)) { continue; }

                var node = tracker.TryAddVisit(current, relationship, direction, enforceGlobalDedup: false);

                // ========================================================
                // BACKTRACKING: Push constraint to our mutable context state
                // ========================================================
                sharedBranchPath.Add(relationship.KeyName);

                try
                {
                    // Fire deeper into the stack trace passing the exact same reference pointer
                    TraverseUp(node, depthRemaining - 1, sharedBranchPath, tracker);
                }
                finally
                {
                    // ========================================================
                    // BACKTRACKING: Pop constraint as the recursive stack frame unwinds
                    // ========================================================
                    sharedBranchPath.Remove(relationship.KeyName);
                }
            }
        }

        private void TraverseDown(QueryTable current, int depthRemaining, HashSet<string> sharedBranchPath, QueryTracker tracker)
        {
            TraversalDirection direction = TraversalDirection.Down;
            if (tracker.CheckMaxDepthReached(current, direction, depthRemaining)) { return; }

            foreach (var relationship in current.Table.ChildRelations)
            {
                if (tracker.CheckStopTraversal(sharedBranchPath, current, relationship, direction))
                {
                    continue;
                }

                var node = tracker.TryAddVisit(current, relationship, direction, enforceGlobalDedup: false);

                // ========================================================
                // BACKTRACKING: Push constraint
                // ========================================================
                sharedBranchPath.Add(relationship.KeyName);

                try
                {
                    TraverseDown(node, depthRemaining - 1, sharedBranchPath, tracker);
                }
                finally
                {
                    // ========================================================
                    // BACKTRACKING: Pop constraint
                    // ========================================================
                    sharedBranchPath.Remove(relationship.KeyName);
                }
            }
        }
    }
}
