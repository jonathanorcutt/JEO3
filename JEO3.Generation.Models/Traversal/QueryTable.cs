
using JEO3.Schema;

namespace JEO3.Generation.Models
{
    public sealed class QueryTable
    {
        #region Properties

        public Guid Id { get; init; } = Guid.NewGuid();

        // Helpers - Expose
        public int TableObjectId => Table?.ObjectId ?? 0;
        public int RelationshipObjectId => Relationship?.ObjectId ?? 0;

        public ITable Table { get; init; }

        public QueryTable? Parent { get; init; }

        public IRelation? Relationship { get; init; }

        public TraversalDirection Direction { get; init; }
        public bool RequiresLeftJoin { get; init; }

        public string Alias { get; set; }

        public int Depth { get; init; }


        // NEW: additive only, defaults false. Every existing node in every existing
        // generator ends up false here since nothing else sets it. Marks a node that
        // was added as a one-hop, non-recursed parent lookup for diagram purposes,
        // and is not meant to be joined into generated SQL text.
        public bool IsReferenceLeaf { get; init; } = false;

        #endregion

        #region Functions

        public void Deconstruct(out Guid id, out Guid? parentId, out string alias, out int depth)
        {
            id = this.Id;
            parentId = this.Parent?.Id;
            alias = this.Alias;
            depth = this.Depth;
        }

        public HashSet<ITable> GetAllTables(HashSet<ITable>? results = null)
        {
            results ??= [];

            results.Add(this.Table);

            // Bubble up to the root
            Parent?.GetAllTables(results);

            return results;
        }

        public List<QueryTable> GetNodesToRoot(List<QueryTable>? results = null)
        {
            results ??= [];

            results.Add(this);

            // Bubble up to the root
            Parent?.GetNodesToRoot(results);

            return results;
        }

        #endregion
    }
}
