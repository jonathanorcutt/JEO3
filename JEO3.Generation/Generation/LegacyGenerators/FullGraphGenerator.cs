using JEO3.Generation.Models;
using JEO3.Schema;


namespace JEO3.Generation
{
    /// <summary>
    /// When traversing from the root: Up and down independently from every discovered table (true graph traversal)
    /// </summary>
    public sealed class FullGraphGenerator : IQueryGenerator
    {
        #region Initialization

        public FullGraphGenerator()
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
                Traverse(tracker.Root, options.Traversal.LevelsUp, options.Traversal.LevelsDown, [], tracker);
            }

            var result = QueryResultBuilder.GenerateQueryResult(tracker, options, GenerationMethod.FullGraph);
            return result;
        }

        private async Task<(IQueryTracker tracker, QueryGenerationResult result)> Traverse(Table root, QueryGenerationOptions options, Func<(QueryTable current, int levelsUpRemaining, int levelsDownRemaining, HashSet<string> fkPath, QueryTracker tracker), QueryGenerationResult> func)
        {
            var tracker = new QueryTracker(root, options);

            // If GenerateTableJoins = False - Skip Query Joins And Go Straight To Output
            if (options.EnableGraphGeneration == true)
            {
                var resultX = func.Invoke((tracker.Root, options.Traversal.LevelsUp, options.Traversal.LevelsDown, new HashSet<string>(), tracker));
                return (tracker, resultX);
            }

            var result = QueryResultBuilder.GenerateQueryResult(tracker, options, GenerationMethod.FullGraph);
            return (tracker, result);
        }

        private void Traverse(QueryTable current, int levelsUpRemaining, int levelsDownRemaining, HashSet<string> fkPath, QueryTracker tracker)
        {
            // UP
            TraversalDirection direction = TraversalDirection.Up;
            if (tracker.CheckMaxDepthReached(current, direction, levelsUpRemaining) == false)
            {
                foreach (var relationship in current.Table.ParentRelations)
                {
                    if (tracker.CheckStopTraversal(fkPath, current, relationship, direction)) { continue; }

                    var node = tracker.TryAddVisit(current, relationship, direction, enforceGlobalDedup: false);

                    Traverse(node, levelsUpRemaining - 1, levelsDownRemaining, [.. fkPath, relationship.KeyName], tracker);
                }
            }

            // DOWN
            direction = TraversalDirection.Down;
            if (tracker.CheckMaxDepthReached(current, direction, levelsDownRemaining) == false)
            {
                foreach (var relationship in current.Table.ChildRelations)
                {
                    if (tracker.CheckStopTraversal(fkPath, current, relationship, direction)) { continue; }

                    var node = tracker.TryAddVisit(current, relationship, direction, enforceGlobalDedup: false);

                    Traverse(node, levelsUpRemaining, levelsDownRemaining - 1, [.. fkPath, relationship.KeyName], tracker);
                }
            }
        }

        #endregion
    }
}