using JEO3.Generation.Models;
using JEO3.Schema;


namespace JEO3.Generation
{
    /// <summary>
    /// When traversing from the root: Only recurse downward from the root and recurse upward from the root separately
    /// </summary>
    public sealed class RootSplitGenerator : IQueryGenerator
    {
        #region Initialization

        public RootSplitGenerator()
        {
        }

        #endregion

        #region Query Generation

        public QueryGenerationResult Generate(ITable root) => Generate(root, QueryGenerationOptions.GetDefaultGenerationOptions());
        public QueryGenerationResult Generate(ITable root, QueryGenerationOptions options)
        {
            var tracker = new QueryTracker(root, options);

            // If GenerateTableJoins = False - Skip Query Joins And Go Straight To Output
            if (options.EnableGraphGeneration == true)
            {
                // Execute the isolated recursive runs out from the root node
                TraverseUp(tracker.Root, options.Traversal.LevelsUp, [], tracker);
                TraverseDown(tracker.Root, options.Traversal.LevelsDown, [], tracker);
            }

            // Populate Query Result
            var result = QueryResultBuilder.GenerateQueryResult(tracker, options, GenerationMethod.RootSplit);
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

                TraverseUp(node, depthRemaining - 1, [.. path, relationship.KeyName], tracker);
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

                TraverseDown(node, depthRemaining - 1, [.. path, relationship.KeyName], tracker);
            }
        }

        #endregion
    }
}