namespace JEO3.Schema
{
    public interface ITrigger : IObject
    {
        long? ParentObjectId { get; }
        bool IsDisabled { get; }
        bool IsInsteadOfTrigger { get; }
        string Definition { get; }
        byte[]? DefinitionHash { get; }
    }
}
