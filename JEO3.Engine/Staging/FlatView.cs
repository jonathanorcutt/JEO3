using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatView : FlatObject
    {
        [JsonInclude]
        public int OrdinalPosition { get; private set; }
        [JsonInclude]
        public string? DataType { get; private set; }
        [JsonInclude]
        public string? ProviderDataType { get; private set; }
        [JsonInclude]
        public int? MaxLength { get; private set; }
        [JsonInclude]
        public int? Precision { get; private set; }
        [JsonInclude]
        public int? Scale { get; private set; }
        [JsonInclude]
        public bool IsOutput { get; private set; }
        [JsonInclude]
        public bool IsNullable { get; private set; }
        [JsonInclude]
        public string? DefaultValue { get; private set; }
        [JsonInclude]
        public string Identifier { get; private set; }
        [JsonInclude]
        public string DefinitionName { get; private set; }
        [JsonInclude]
        public string DatabaseName { get; private set; }
        [JsonInclude]
        public string SchemaName { get; private set; }
        [JsonInclude]
        public bool IsReplicated { get; private set; }
        [JsonInclude]
        public bool WithCheckOption { get; private set; }
        [JsonInclude]
        public string Definition { get; private set; }
        [JsonInclude]
        public byte[]? DefinitionHash { get; private set; }

        public static List<FlatView> Create(
            IEnumerable<FlatViewDefinition> definitions,
            IEnumerable<FlatViewColumn> columns)
        {
            return definitions.GroupJoin(
                columns,
                d => d.ObjectId,
                c => c.ObjectId,
                (definition, columnGroup) => new { definition, columnGroup })
            .SelectMany(
                x => x.columnGroup.DefaultIfEmpty(), // Implements SQL Left Join behavior
                (x, column) => new FlatView
                {
                    // View Metadata (Always Populated)
                    ObjectId = x.definition.ObjectId,
                    DefinitionName = x.definition.Name,
                    DefinitionHash = x.definition.DefinitionHash,
                    FullName = x.definition.FullName,
                    SchemaId = x.definition.SchemaId,
                    CreateDate = x.definition.CreateDate,
                    ModifyDate = x.definition.ModifyDate,
                    DatabaseName = x.definition.DatabaseName,
                    SchemaName = x.definition.SchemaName,
                    IsReplicated = x.definition.IsReplicated,
                    WithCheckOption = x.definition.WithCheckOption,
                    Definition = x.definition.Definition,

                    // Column Metadata (Safe fallback mapping if column is null)
                    Name = column?.Name,
                    OrdinalPosition = column?.OrdinalPosition ?? 0,
                    DataType = column?.DataType,
                    ProviderDataType = column?.ProviderDataType,
                    MaxLength = column?.MaxLength,
                    Precision = column?.Precision,
                    Scale = column?.Scale,
                    IsOutput = column?.IsOutput ?? false,
                    IsNullable = column?.IsNullable ?? false,
                    DefaultValue = column?.DefaultValue, // Views don't have default constraints, usually null
                                                         // Identifier = x.
                })
            .ToList();
        }
    }
}
