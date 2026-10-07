using JEO3.Generation.Models;
using JEO3.Schema;


namespace JEO3.Generation
{
    public sealed class JEO3Generator : IQueryGenerator
    {
        #region Initialization

        public JEO3Generator()
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

            // Get Boundary Nodes Variable
            var boundaryNodes = tracker.Nodes
                    .GroupBy(n => n.Direction)
                    .SelectMany(g =>
                    {
                        var maxDepth = g.Max(x => x.Depth);
                        return g.Where(n => n.Depth == maxDepth || !tracker.Nodes.Any(x => x.Parent == n));
                    }).ToList();

            // Other Boundary Options To Explore:
            // --------------------------------------------------------
            // “Boundary Nodes = furthest reached per direction branch”
            // --------------------------------------------------------
            //var boundaryNodes =
            //_nodes
            //    .GroupBy(n => n.Direction)
            //    .SelectMany(g =>
            //        g.Where(n => n.Depth == g.Max(x => x.Depth))
            //    )
            //    .ToList();
            // --------------------------------------------------------
            // Stricter - only real leaves per direction — i.e., nodes that are never a Parent of another node in the same set
            // --------------------------------------------------------
            //var boundaryNodes =
            //    _nodes
            //        .Where(n => !_nodes.Any(x => x.Parent == n))
            //        .ToList();
            // --------------------------------------------------------

            // Create QueryTable Queue
            var frontier = new Queue<QueryTable>(boundaryNodes);

            // Explore Beyond Horizon
            TraverseHorizon(frontier, tracker);

            // Populate Query Result
            var result = QueryResultBuilder.GenerateQueryResult(tracker, options, GenerationMethod.JEO3);
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

        private void TraverseHorizon(Queue<QueryTable> frontier, QueryTracker tracker)
        {
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                foreach (var relationship in current.Table.ParentRelations)
                {
                    var target = relationship.ParentTable;
                    var reason = tracker.IsRelationExcluded(relationship);
                    if (reason != TraversalStopReason.None) { tracker.Events.Add(new TraversalEvent(current.TableObjectId, target.ObjectId.GetValueOrDefault(), relationship.ObjectId, reason, TraversalDirection.Up, current.Depth)); continue; }

                    // KEY RULE: uniqueness is TABLE-based here
                    // if (!tracker.VisitedTables.Add(target.ObjectId)) { continue; }

                    var node = tracker.TryAddVisit(current, relationship, TraversalDirection.Up, enforceGlobalDedup: false);

                    frontier.Enqueue(node);
                }

                foreach (var relationship in current.Table.ChildRelations)
                {
                    var target = relationship.ReferencedTable;

                    var reason = tracker.IsRelationExcluded(relationship);
                    if (reason != TraversalStopReason.None)
                    {
                        tracker.Events.Add(new TraversalEvent(current.TableObjectId, target.ObjectId.GetValueOrDefault(), relationship.ObjectId, reason, TraversalDirection.Down, current.Depth));
                        continue;
                    }

                    //if (!tracker.VisitedTables.Add(target.ObjectId))
                    //continue;

                    var node = tracker.TryAddVisit(current, relationship, TraversalDirection.Down, enforceGlobalDedup: false);

                    frontier.Enqueue(node);
                }
            }
        }

        #endregion
    }
}