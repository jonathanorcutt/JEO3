namespace JEO3.Schema
{
    /// <summary>
    /// Provider-agnostic representation of a database object.
    /// </summary>
    public interface IObject
    {
        /// <summary>
        /// Provider-specific object identifier, when available.
        /// </summary>
        int? ObjectId { get; }
        /// <summary>
        /// Provider-independent stable identifier, when available.
        /// </summary>
        string? Identifier { get; }
        /// <summary>
        /// Database containing the object.
        /// </summary>
        string? DatabaseName { get; }
        /// <summary>
        /// Schema containing the object.
        /// </summary>
        string? SchemaName { get; }
        /// <summary>
        /// Object name.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Fully qualified object name.
        /// </summary>
        string? FullName { get; }
        /// <summary>
        /// Type of database object.
        /// </summary>
        DatabaseObjectType ObjectType { get; }

        /// <summary>
        /// The object this one is nested under in the schema hierarchy (e.g. a column's table), or null at the root.
        /// </summary>
        IObject? Parent { get; }

        /// <summary>
        /// Objects nested directly under this one (e.g. a table's columns), empty if none.
        /// </summary>
        IEnumerable<IChildGrouping> Children { get; }
    }
}