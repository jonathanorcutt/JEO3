namespace JEO3.Schema
{
    public interface ISchema : IObject
    {
        short CompatibilityLevel { get; }
        bool? IsReadSnapshotCommitted { get; }
        IReadOnlyList<ITable> Tables { get; }
    }
}
