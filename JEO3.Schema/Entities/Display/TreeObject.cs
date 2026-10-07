namespace JEO3.Schema
{
    // Not Used Yet
    public class TreeObject : IObject
    {
        public int? ObjectId { get; set; }

        public string? Identifier { get; set; }

        public string? DatabaseName { get; set; }

        public string? SchemaName { get; set; }

        public string Name { get; set; }

        public string? FullName { get; set; }

        public DatabaseObjectType ObjectType { get; set; }

        public IObject? Parent { get; set; }

        public IEnumerable<IChildGrouping> Children { get; set; }
    }
}
