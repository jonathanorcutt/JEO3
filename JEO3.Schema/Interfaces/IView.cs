namespace JEO3.Schema
{
    public interface IView : IObject
    {
        string? Definition { get; }
        bool IsSystemObject { get; }
        bool IsEncrypted { get; }
        bool IsMaterialized { get; }
        IReadOnlyList<IColumn> Columns { get; }
        IReadOnlyList<IRelation> Relations { get; }
    }
}
