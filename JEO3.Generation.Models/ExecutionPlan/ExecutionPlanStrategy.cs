using JEO3.Generation.Models;

namespace JEO3.Generation.Models
{
    public sealed class ExecutionPlanStrategy
    {
        public required string StrategyName { get; init; }
        public required DiscoveryMode DiscoveryMode { get; init; }
        public EdgeSelectionPolicy EdgePolicy { get; init; } = new();

        // Let the strategy pass an optional custom compression function if it uses WideNetExtended
        public Action<IQueryTracker>? PostTraversalCompression { get; init; }
    }

}
