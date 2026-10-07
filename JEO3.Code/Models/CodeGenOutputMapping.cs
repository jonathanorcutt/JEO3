namespace JEO3.Code
{
    internal class CodeGenOutputMapping
    {
        public string FilePath { get; init; }
        public string Namespace { get; init; } = "JEO3.Domain";
        public string AttributesFolderName { get; init; } = "Attributes";
        public string EntitiesFolderName { get; init; } = "Tables";
        public string ProceduresFolderName { get; init; } = "Procedures";
        public string ProcedureDefinitionsFolderName { get; init; } = "Procedures/Definitions";
        public string QueriesFolderName { get; init; } = "Queries";
        public string ViewsFolderName { get; init; } = "Views";
        public string ViewDefinitionsFolderName { get; init; } = "Views/CreateScripts";
        public string MissingIndexesFolderName { get; init; } = "MissingIndexes";
        public string IndexesFolderName { get; init; } = "Indexes";
        public string TriggersFolderName { get; init; } = "Triggers";
        public string RelationsFolderName { get; init; } = "Relations";
        public string UDTFolderName { get; init; } = "UDTs";

        public string AttributesFolderPath => Path.Combine(FilePath, AttributesFolderName);
        public string EntitiesFolderPath => Path.Combine(FilePath, EntitiesFolderName);
        public string ProceduresFolderPath => Path.Combine(FilePath, ProceduresFolderName);
        public string ProcedureDefinitionsFolderPath => Path.Combine(FilePath, ProcedureDefinitionsFolderName);
        public string QueriesFolderPath => Path.Combine(FilePath, QueriesFolderName);
        public string ViewsFolderPath => Path.Combine(FilePath, ViewsFolderName);
        public string ViewDefinitionsFolderPath => Path.Combine(FilePath, ViewDefinitionsFolderName);
        public string IndexesFolderPath => Path.Combine(FilePath, IndexesFolderName);
        public string MissingIndexesFolderPath => Path.Combine(FilePath, MissingIndexesFolderName);
        public string TriggersFolderPath => Path.Combine(FilePath, TriggersFolderName);
        public string RelationsFolderPath => Path.Combine(FilePath, RelationsFolderName);
        public string UDTFolderPath => Path.Combine(FilePath, UDTFolderName);

        public CodeGenOutputMapping(string filePath)
        {
            FilePath = filePath;
        }
    }
}
