namespace JEO3.Schema
{
    public interface IUserDefinedType : IObject
    {
        UserDefinedTypeKind TypeKind { get; }
        string? BaseType { get; }
        string? ProviderBaseType { get; }
        int? MaxLength { get; }
        int? Precision { get; }
        int? Scale { get; }
        bool IsNullable { get; }
        string Definition { get; }
        byte[]? DefinitionHash { get; }
        DateTime? CreateDate { get; }
        DateTime? ModifyDate { get; }
        int SchemaId { get; }
        bool IsSystemObject { get; }
        bool IsDeterministic { get; }
        bool IsReplicated { get; }
        bool WithCheckOption { get; }
        IReadOnlyList<IUserDefinedTypeColumn> Columns { get; }
    }
}
