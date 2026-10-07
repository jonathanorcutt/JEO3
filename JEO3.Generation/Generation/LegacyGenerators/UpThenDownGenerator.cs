using JEO3.Generation.Models;
using JEO3.Schema;


namespace JEO3.Generation
{
    /// <summary>
    /// When traversing from the root: Up traversal first, then down traversal from every discovered node
    /// </summary>
    public sealed class UpThenDownGenerator : IQueryGenerator
    {
        #region Initialization

        public UpThenDownGenerator()
        {
        }

        #endregion

        #region Query Generation

        public QueryGenerationResult Generate(ITable root) => Generate(root, QueryGenerationOptions.GetDefaultGenerationOptions());
        public QueryGenerationResult Generate(ITable root, QueryGenerationOptions options)
        {
            var tracker = new QueryTracker(root, options);

            if (options.EnableGraphGeneration == true)
            {
                // Up
                TraverseUp(tracker.Root, options.Traversal.LevelsUp, [], tracker);

                var upstreamNodes = tracker.Nodes.ToList();

                // Branch downwards from every single node captured upstream
                foreach (var n in upstreamNodes)
                {
                    // Down
                    TraverseDown(n, options.Traversal.LevelsDown, [], tracker);
                }
            }

            // Populate Query Result
            var result = QueryResultBuilder.GenerateQueryResult(tracker, options, GenerationMethod.UpThenDown);
            return result;
        }

        private void TraverseUp(QueryTable current, int depthRemaining, HashSet<string> path, QueryTracker tracker)
        {
            TraversalDirection direction = TraversalDirection.Up;
            if (tracker.CheckMaxDepthReached(current, direction, depthRemaining)) { return; }

            foreach (var relationship in current.Table.ParentRelations)
            {
                if (tracker.CheckStopTraversal(path, current, relationship, direction)) { continue; }

                var node = tracker.TryAddVisit(current, relationship, direction, enforceGlobalDedup: false);

                TraverseUp(node, depthRemaining - 1, [.. path], tracker);
            }
        }

        private void TraverseDown(QueryTable current, int depthRemaining, HashSet<string> path, QueryTracker tracker)
        {
            TraversalDirection direction = TraversalDirection.Down;
            if (tracker.CheckMaxDepthReached(current, direction, depthRemaining)) { return; }

            foreach (var relationship in current.Table.ChildRelations)
            {
                if (tracker.CheckStopTraversal(path, current, relationship, direction)) { continue; }

                var node = tracker.TryAddVisit(current, relationship, direction, enforceGlobalDedup: false);

                TraverseDown(node, depthRemaining - 1, [.. path], tracker);
            }
        }

        #endregion
    }
}