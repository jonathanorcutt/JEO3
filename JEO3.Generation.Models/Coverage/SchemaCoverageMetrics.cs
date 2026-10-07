namespace JEO3.Generation.Models
{
    public sealed class SchemaCoverageMetrics
    {
        public string ReachableLabel => ReachableTables == null ? string.Empty : $"{ReachableTables?.Count} TABLE{(ReachableTables?.Count < 2 ? " " : "S")}";
        public double ReachablePercentage { get; set; }
        public double DisconnectedPercentage { get; set; }
        public List<string> ReachableTables { get; set; } = [];
        public List<string> DisconnectedTables { get; set; } = [];
        public int? ReachableTableCount => ReachableTables?.Count;
        public int? DisconnectedTableCount => DisconnectedTables?.Count;
        public double TotalTableCount { get; private init; }

        public SchemaCoverageMetrics()
        {
        }

        public SchemaCoverageMetrics(double reachablePercentage, double disconnectedPercentage, List<string>? reachableTables, List<string>? disconnectedTables, double totalTableCount)
        {
            ReachablePercentage = reachablePercentage;
            DisconnectedPercentage = disconnectedPercentage;
            ReachableTables = reachableTables;
            DisconnectedTables = disconnectedTables;
            TotalTableCount = totalTableCount;
        }
    }
}
