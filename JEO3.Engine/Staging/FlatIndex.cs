using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatIndex
    {
        [JsonInclude]
        public string SchemaName { get; private set; } = string.Empty;
        [JsonInclude]
        public string TableName { get; private set; } = string.Empty;
        [JsonInclude]
        public string IndexType { get; private set; } = string.Empty;
        [JsonInclude]
        public string Name { get; private set; } = string.Empty;
        [JsonInclude]
        public int IndexId { get; private set; }
        [JsonInclude]
        public string ColumnName { get; private set; } = string.Empty;
        [JsonInclude]
        public int ColumnId { get; private set; }
        [JsonInclude]
        public int IndexColumnId { get; private set; }
        [JsonInclude]
        public int IndexOrdinalPosition { get; private set; }
        [JsonInclude]
        public string DataType { get; private set; } = string.Empty;
        [JsonInclude]
        public bool IsClustered { get; private set; } // Maps to sys.indexes.type == 1
        [JsonInclude]
        public bool IsUnique { get; private set; } // Maps to sys.indexes.is_unique == 1
        [JsonInclude]
        public bool IsUniqueConstraint { get; private set; } // Maps to sys.indexes.is_unique_constraint == 1
        [JsonInclude]
        public bool IsIncluded { get; private set; }
        [JsonInclude]
        public bool IsDisabled { get; private set; }
        [JsonInclude]
        public bool IsPrimaryKey { get; private set; }
        [JsonInclude]
        public double? FragmentationPercentage { get; private set; }
        [JsonInclude]
        public long? PageCount { get; private set; }

        public string Key => $"{SchemaName}.{TableName}.{ColumnName}";
    }
}
