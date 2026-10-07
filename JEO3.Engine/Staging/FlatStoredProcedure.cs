using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatStoredProcedure : FlatObject
    {
        // Parameters
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
        public bool IsReadOnly { get; private set; }
        [JsonInclude]
        public string? DefaultValue { get; private set; }
        [JsonInclude]
        public string? Identifier { get; private set; }

        // Definition
        [JsonInclude]
        public string DefinitionName { get; private set; }
        [JsonInclude]
        public string DatabaseName { get; private set; }
        [JsonInclude]
        public string SchemaName { get; private set; }
        [JsonInclude]
        public bool IsAutoExecuted { get; private set; }
        [JsonInclude]
        public bool IsExecutionReplicated { get; private set; }
        [JsonInclude]
        // You can join with sys.sql_modules to get the actual definition/text
        public string Definition { get; private set; }
        [JsonInclude]
        public byte[]? DefinitionHash { get; private set; }

        public static IReadOnlyList<FlatStoredProcedure> Create(
            IEnumerable<FlatStoredProcedureDefinition> definitions,
            IEnumerable<FlatStoredProcedureParameter> parameters)
        {
            return definitions.GroupJoin(
                parameters,
                d => d.ObjectId,
                p => p.ObjectId,
                (definition, paramGroup) => new { definition, paramGroup })
            .SelectMany(
                x => x.paramGroup.DefaultIfEmpty(), // Performs the Left Join behavior
                (x, parameter) => new FlatStoredProcedure
                {
                    // Parent Procedure Info (Always populated)
                    ObjectId = x.definition.ObjectId,
                    DefinitionName = x.definition.Name,
                    FullName = x.definition.FullName,
                    SchemaId = x.definition.SchemaId,
                    CreateDate = x.definition.CreateDate,
                    ModifyDate = x.definition.ModifyDate,
                    DatabaseName = x.definition.DatabaseName,
                    SchemaName = x.definition.SchemaName,
                    IsAutoExecuted = x.definition.IsAutoExecuted,
                    IsExecutionReplicated = x.definition.IsExecutionReplicated,
                    Definition = x.definition.Definition,

                    // Parameter Info (Uses null-conditional operator to handle empty parameters safely)
                    Name = parameter?.Name,
                    OrdinalPosition = parameter?.OrdinalPosition ?? 0,
                    DataType = parameter?.DataType,
                    ProviderDataType = parameter?.ProviderDataType,
                    MaxLength = parameter?.MaxLength,
                    Precision = parameter?.Precision,
                    Scale = parameter?.Scale,
                    IsOutput = parameter?.IsOutput ?? false,
                    IsNullable = parameter?.IsNullable ?? false,
                    IsReadOnly = parameter?.IsReadOnly ?? false,
                    DefaultValue = parameter?.DefaultValue,
                    DefinitionHash = x.definition.DefinitionHash,
                    // Identifier = parameter.i
                })
            .ToList();
        }
    }
}
