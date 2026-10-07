namespace JEO3.Schema
{
    public interface IForeignKey : IRelation
    {
        string? DeleteAction { get; }
        string? UpdateAction { get; }
        bool IsSelfReferencing { get; }
        bool IsComposite { get; }

    }
}
