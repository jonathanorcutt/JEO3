namespace JEO3.Generation.Models.Extensions
{
    public static class QueryTableExtensions
    {
        /// <summary>
        /// Creates a shallow clone of this node but severs the Parent reference.
        /// This is used during CTE compilation so the node can act as a local root 
        /// without compiling joins back to the parent query scope.
        /// </summary>
        public static QueryTable CloneAsLocalRoot(this QueryTable original)
        {
            return original == null
                ? throw new ArgumentNullException(nameof(original))
                : new QueryTable
                {
                    Table = original.Table,
                    Alias = original.Alias,
                    Depth = original.Depth,
                    Direction = original.Direction,
                    Relationship = original.Relationship,
                    RequiresLeftJoin = original.RequiresLeftJoin,
                    Parent = null // Sever parent link so join engines treat this as anchor
                };
        }

        /// <summary>
        /// Collects all downstream descendants of this node in the execution graph,
        /// avoiding circular traversal and skipping the global query root.
        /// </summary>
        public static void GatherDescendants(
            this QueryTable current,
            List<QueryTable> allNodes,
            List<QueryTable> branch,
            HashSet<QueryTable> visited)
        {
            if (current == null || visited.Contains(current)) return;

            visited.Add(current);
            branch.Add(current);

            var children = allNodes.Where(n => n.Parent == current);
            foreach (var child in children)
            {
                child.GatherDescendants(allNodes, branch, visited);
            }
        }

        /// <summary>
        /// Determines if a table acts as a logical junction hub based on 
        /// the number of radiating active children in the current compilation plan.
        /// </summary>
        public static bool IsLogicalHub(this QueryTable node, List<QueryTable> allNodes, int branchThreshold = 3)
        {
            if (node == null) return false;

            int childCount = allNodes.Count(n => n.Parent == node);
            return childCount >= branchThreshold;
        }
    }
}
