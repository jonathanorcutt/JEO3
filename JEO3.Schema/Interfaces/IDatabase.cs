namespace JEO3.Schema
{
    public interface IDatabase : IObject
    {
        DateTime? CreateDate { get; }
        string? CollationName { get; }
        IReadOnlyList<ITable> Tables { get; }
        IReadOnlyList<ISchema> Schemas { get; }
    }
}
