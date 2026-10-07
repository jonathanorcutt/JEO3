namespace JEO3.Generation.Models
{
    public interface IQueryTracker
    {
        QueryTable Root { get; }
        List<QueryTable> Nodes { get; }
        List<TraversalEvent> Events { get; }
        QueryGenerationOptions Options { get; init; }
    }
}