namespace JEO3.Schema
{
    public interface IExtendedProperty : IObject
    {
        int MajorId { get; }
        int MinorId { get; }
        int Class { get; }
        string? ClassDescription { get; }
        string? PropertyName { get; }
        string? Value { get; }
        string? ObjectTypeDescription { get; }
        int? ParentObjectId { get; }
        string? ParentObjectName { get; }
        string? ColumnName { get; }
        string? IndexName { get; }
    }
}
