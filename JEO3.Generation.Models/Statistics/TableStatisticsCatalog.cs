namespace JEO3.Generation.Models
{
    public sealed class TableStatisticsCatalog
    {
        #region Properties

        public Dictionary<string, TableStatistics> Tables { get; } = [];

        #endregion

        #region Functions

        public TableStatistics Get(string fullName)
        {
            return Tables.TryGetValue(fullName, out var stats) ? stats : new TableStatistics();
        }

        #endregion
    }
}
