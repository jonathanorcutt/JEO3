using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Generation
{
    public sealed class CoverageCalculator
    {
        public static SchemaCoverageMetrics CalculateCoverage(ITable startTable, IEnumerable<ITable> tables, IEnumerable<IRelation> relations, bool strictTraversal = false)
        {
            // Map total tables directly to a HashSet for O(1) existence checks
            var totalTableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in tables)
            {
                totalTableNames.Add($"{t.SchemaName}.{t.Name}");
            }

            // Build Adjacency Matrix dynamically (No intermediate models or .GroupBy needed)
            var graphMatrix = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var tableName in totalTableNames)
            {
                graphMatrix[tableName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            // Populate edges in a single highly optimized pass
            foreach (var r in relations)
            {
                if (r.IsDisabled) continue;
                if (strictTraversal && r.IsNullable) continue;

                string parentName = $"{r.ParentTable?.SchemaName}.{r.ParentTable?.Name}";
                string childName = $"{r.ReferencedTable?.SchemaName}.{r.ReferencedTable?.Name}";

                // Skip self-referencing loops and null checks
                if (string.IsNullOrEmpty(parentName) || string.IsNullOrEmpty(childName)) continue;
                if (parentName.Equals(childName, StringComparison.OrdinalIgnoreCase)) continue;

                // Ensure both ends exist in our known tables, HashSet inherently deduplicates composite keys
                if (graphMatrix.TryGetValue(parentName, out var parentEdges) &&
                    graphMatrix.TryGetValue(childName, out var childEdges))
                {
                    parentEdges.Add(childName);
                    childEdges.Add(parentName);
                }
            }

            // Determine start node (Fallback intact)
            string startTableName = startTable != null
                ? $"{startTable.SchemaName}.{startTable.Name}"
                : "Person.Person";

            // Execute Directed Breadth-First Search (BFS)
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queue = new Queue<string>();

            if (graphMatrix.ContainsKey(startTableName))
            {
                visited.Add(startTableName);
                queue.Enqueue(startTableName);
            }

            while (queue.Count > 0)
            {
                string currentTable = queue.Dequeue();

                foreach (string neighbor in graphMatrix[currentTable])
                {
                    // HashSet.Add returns false if the item is already present
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Compute Final Metrics
            double totalCount = totalTableNames.Count;
            double reachableCount = visited.Count;

            double coveragePct = totalCount > 0 ? reachableCount / totalCount * 100.0 : 0.0;
            double disconnectedPct = totalCount > 0 ? (totalCount - reachableCount) / totalCount * 100.0 : 0.0;

            var reachableList = visited.OrderBy(t => t).ToList();

            // Much faster than LINQ Except
            var disconnectedList = totalTableNames
                .Where(t => !visited.Contains(t))
                .OrderBy(t => t)
                .ToList();

            return new SchemaCoverageMetrics(
                Math.Round(coveragePct, 1),
                Math.Round(disconnectedPct, 1),
                reachableList,
                disconnectedList,
                totalCount);
        }
    }
}