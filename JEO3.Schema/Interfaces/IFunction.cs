namespace JEO3.Schema
{
    public interface IFunction : IObject
    {
        FunctionType FunctionType { get; }
        string? Definition { get; }
        string? ReturnType { get; }
        string? ProviderReturnType { get; }
        bool IsSystemObject { get; }
        bool IsDeterministic { get; }
        IReadOnlyList<IProcedureParameter> Parameters { get; }
        IReadOnlyList<IColumn> ReturnColumns { get; }
    }
}
