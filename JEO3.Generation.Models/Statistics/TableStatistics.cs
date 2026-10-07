namespace JEO3.Generation.Models
{
    public sealed class TableStatistics
    {
        #region Properties

        public long TotalRowCount { get; set; } = 1;

        // Tracks the density/uniqueness profile of a given foreign key column pair
        // Key: ForeignKeyName, Value: Average matching rows per primary key row
        public Dictionary<string, double> MatchFactors { get; } = [];

        #endregion
    }
}
