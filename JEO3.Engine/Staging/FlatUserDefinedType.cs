using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatUserDefinedType
    {
        [JsonInclude]
        public int ObjectId { get; private set; }
        [JsonInclude]
        public string Name { get; private set; }
        [JsonInclude]
        public string FullName { get; private set; }
        [JsonInclude]
        public int SchemaId { get; private set; }
        [JsonInclude]
        /// Possible values:
        /// CLR_UDT, TABLE_TYPE, or ALIAS_UDT.
        /// </summary>
        public string FunctionType { get; private set; }
        [JsonInclude]
        public DateTime? CreateDate { get; private set; }
        [JsonInclude]
        public DateTime? ModifyDate { get; private set; }
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
        public string ReturnType { get; private set; }
        [JsonInclude]
        public string ProviderReturnType { get; private set; }
        [JsonInclude]
        public bool IsSystemObject { get; private set; }
        [JsonInclude]
        public bool IsDeterministic { get; private set; }
        [JsonInclude]
        public bool IsReplicated { get; private set; }
        [JsonInclude]
        public bool WithCheckOption { get; private set; }
        [JsonInclude]
        public string Definition { get; private set; }
        [JsonInclude]
        public byte[]? DefinitionHash { get; private set; }

        public static List<FlatUserDefinedType> Create(
            IEnumerable<FlatUserDefinedTypeDefinition> definitions,
            IEnumerable<FlatUserDefinedTypeColumn> columns)
        {
            return definitions.GroupJoin(
                columns,
                d => d.ObjectId,
                c => c.ObjectId,
                (definition, columnGroup) => new { definition, columnGroup })
            .SelectMany(
                x => x.columnGroup.DefaultIfEmpty(), // Performs a clean Left Join pattern
                (x, column) => new FlatUserDefinedType
                {
                    // Definition/Base Metadata (Always Populated)
                    ObjectId = x.definition.ObjectId,
                    DefinitionName = x.definition.Name,
                    DefinitionHash = x.definition.DefinitionHash,
                    FullName = x.definition.FullName,
                    Identifier = x.definition.Identifier,
                    SchemaId = x.definition.SchemaId > 0 ? x.definition.SchemaId : 0,
                    CreateDate = x.definition.CreateDate,
                    FunctionType = x.definition.FunctionType,
                    ModifyDate = x.definition.ModifyDate,
                    DatabaseName = x.definition.DatabaseName,
                    SchemaName = x.definition.SchemaName,
                    ReturnType = x.definition.ReturnType,
                    ProviderReturnType = x.definition.ProviderReturnType,
                    IsSystemObject = x.definition.IsSystemObject,
                    IsDeterministic = x.definition.IsDeterministic,
                    IsReplicated = x.definition.IsReplicated,
                    WithCheckOption = x.definition.WithCheckOption,
                    Definition = x.definition.Definition,

                    // Column Specific Data (Uses fallbacks when mapping an Alias/CLR type)
                    Name = column?.Name ?? x.definition.Name, // Fallback to type name if no distinct column exists
                    OrdinalPosition = column?.OrdinalPosition ?? 0,
                    DataType = column?.DataType ?? x.definition.ReturnType, // Uses underlying system type if alias
                    ProviderDataType = column?.ProviderDataType ?? x.definition.ProviderReturnType,
                    MaxLength = x.definition?.MaxLength ?? 0,
                    Precision = x.definition?.Precision ?? 0,
                    Scale = x.definition?.Scale ?? 0,
                    IsOutput = column?.IsOutput ?? false,
                    IsNullable = column?.IsNullable ?? false,
                    DefaultValue = column?.DefaultValue,
                    // Identifier
                })
            .ToList();
        }
    }
}
