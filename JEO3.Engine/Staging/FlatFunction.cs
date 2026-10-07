using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatFunction
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
        public DateTime CreateDate { get; private set; }
        [JsonInclude]
        public DateTime ModifyDate { get; private set; }
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
        public string DefinitionName { get; private set; }
        [JsonInclude]
        public byte[]? DefinitionHash { get; private set; }
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
        public string FunctionType { get; private set; }
        [JsonInclude]
        public string ObjectType { get; private set; }
        [JsonInclude]
        public string ReturnType { get; private set; }
        [JsonInclude]
        public string ProviderReturnType { get; private set; }
        [JsonInclude]
        public string Identifier { get; private set; }
        [JsonInclude]
        public bool IsSystemObject { get; private set; }
        [JsonInclude]
        public bool IsDeterministic { get; private set; }

        public static List<FlatFunction> Create(
            IEnumerable<FlatFunctionDefinition> definitions,
            IEnumerable<FlatFunctionParameter> parameters)
        {
            return definitions.GroupJoin(
                parameters,
                d => d.ObjectId,
                p => p.ObjectId,
                (definition, paramGroup) => new { definition, paramGroup })
            .SelectMany(
                x => x.paramGroup.DefaultIfEmpty(), // Performs the Left Join behavior
                (x, parameter) => new FlatFunction
                {
                    // Definition/Base Metadata (Always Populated)
                    ObjectId = x.definition.ObjectId,
                    DefinitionName = x.definition.Name,
                    CreateDate = x.definition.CreateDate,
                    ModifyDate = x.definition.ModifyDate,
                    DatabaseName = x.definition.DatabaseName,
                    SchemaName = x.definition.SchemaName,
                    IsReplicated = x.definition.IsReplicated,
                    WithCheckOption = x.definition.WithCheckOption,
                    Definition = x.definition.Definition,
                    FunctionType = x.definition.FunctionType,
                    ObjectType = x.definition.ObjectType,
                    ReturnType = x.definition.ReturnType,
                    ProviderReturnType = x.definition.ProviderReturnType,
                    Identifier = x.definition.Identifier,
                    IsSystemObject = x.definition.IsSystemObject,
                    IsDeterministic = x.definition.IsDeterministic,

                    // Parameter Specific Data (Uses safe fallbacks if function has 0 parameters)
                    Name = parameter?.Name ?? string.Empty, // Will be empty string if no parameters exist
                    OrdinalPosition = parameter?.OrdinalPosition ?? 0,
                    DataType = parameter?.DataType,
                    ProviderDataType = parameter?.ProviderDataType,
                    MaxLength = parameter?.MaxLength ?? 0,
                    Precision = parameter?.Precision ?? 0,
                    Scale = parameter?.Scale ?? 0,
                    IsOutput = parameter?.IsOutput ?? false,
                    IsNullable = parameter?.IsNullable ?? false,
                    DefaultValue = parameter?.DefaultValue,
                    DefinitionHash = x.definition?.DefinitionHash,
                    FullName = $"{x.definition.SchemaName}.{x.definition.Name}",
                    SchemaId = x.definition.SchemaId
                })
            .ToList();
        }
    }
}

