namespace JEO3.Generation.Models
{
    public sealed class EdgeSelectionPolicy
    {
        public bool SortByIndexWeight { get; init; } = true;

        public bool FilterUnindexedHazards { get; init; }

        public bool FilterComputedRelationships { get; init; }

        public bool PreferIdentityKeys { get; init; }
    }
}
