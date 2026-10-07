namespace JEO3.Providers.Extensions
{
    public sealed class EntityDescriptor
    {
        public static string DefaultSchemaOverride { get; set; }
        public string Schema { get; init; } = string.Empty;
        public string TableName { get; init; } = string.Empty;
        //public string FullTableName => $"[{Schema}].[{TableName}]";
        public string FullTableName => $"[{(!string.IsNullOrEmpty(DefaultSchemaOverride) ? DefaultSchemaOverride : Schema)}].[{TableName}]";
        public IReadOnlyList<ColumnDescriptor> Columns { get; init; } = Array.Empty<ColumnDescriptor>();

        // Pre-filtered collections for rapid query building
        public IReadOnlyList<ColumnDescriptor> PrimaryKeys => Columns.Where(c => c.IsPrimaryKey).ToList();
        public IReadOnlyList<ColumnDescriptor> InsertableColumns => Columns.Where(c => !c.IsDbGenerated).ToList();
        public IReadOnlyList<ColumnDescriptor> UpdatableColumns => Columns.Where(c => !c.IsPrimaryKey).ToList();
    }
}
