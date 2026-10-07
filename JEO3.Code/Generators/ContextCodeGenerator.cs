using System.Text;
using System.Text.RegularExpressions;
using JEO3.Core;
using JEO3.Engine;
using JEO3.Extensions;
using JEO3.Generation;
using JEO3.Generation.Models;
using JEO3.IO;
using JEO3.IO.Directory;
using JEO3.IO.File;
using JEO3.Providers;
using JEO3.Schema;

namespace JEO3.Code;

public sealed class ContextCodeGenerator
{
    #region Properties

    #region Attibutes
    private string TABLE_ATTR => @"
namespace " + Output.Namespace + @"
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class JeoTable : Attribute
    {
        public string Name { get; }
        public string Schema { get; set; } = ""dbo"";
        public JeoTable(string name) => this.Name = name;
    }
}";
    private string KEY_ATTR => @"
namespace " + Output.Namespace + @"
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class JeoKey : Attribute
    {
        public string Name { get; set; } = string.Empty;
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsDbGenerated { get; set; } // e.g., IDENTITY or Default values
    }
}";
    #endregion

    #region Generation Constants
    private const string NewLine = "\r\n";
    private const string Using = "using";
    private const string Namespace = "namespace";
    private const string PublicClass = "public class ";
    private const string Navigation = "public virtual";
    private const string PropertyAccessor = "{ get; set; }";
    #endregion

    #region State
    private readonly IDatabaseProvider provider;
    private readonly CodeGenOutputMapping Output;
    private DatabaseContext context { get; set; } = null!;
    private Dictionary<(string Schema, string Name), string> tableAliases { get; set; } = [];
    #endregion

    #endregion

    #region Initialization
    public ContextCodeGenerator(IDatabaseProvider provider, string outputPath)
    {
        this.provider = provider;
        Output = new CodeGenOutputMapping(outputPath);
    }
    public async Task LoadContext()
    {
        var rawData = await StagingContextFactory.GetFlatContext(provider, false);
        context = DatabaseContextFactory.GetContext(rawData, provider);
    }
    #endregion

    #region Entry Point
    public async Task GenerateCode()
    {
        ArgumentNullException.ThrowIfNull(context);

        CreateDirectories();
        BuildTableAliases();
        ExportMetadata();

        foreach (var table in context.Tables.OrderBy(t => t.SchemaName).ThenBy(t => t.Name))
        {
            GenerateQuery(table);
            {
                var generator = new DiagramDrivenGraphGenerator();
                var result = generator.Generate(table, QueryGenerationOptions.GetDefaultGenerationOptions());
                GenerateEntity(table, result);
            }
        }
    }
    private void BuildTableAliases()
    {
        tableAliases = context.Tables.ToDictionary(
            table => (table.SchemaName, table.Name),
            table => table.Columns.Any(column => column.Name == table.Name)
                ? $"{SanitizeIdentifier(table.Name)}Entity"
                : SanitizeIdentifier(table.Name));
    }
    private void ExportMetadata()
    {
        WriteCsv(
            Output.IndexesFolderPath,
            "Indexes.csv",
            context.Indexes
                .OrderBy(index => index.SchemaName)
                .ThenBy(index => index.Name)
                .ThenBy(index => index.IndexId));

        WriteCsv(
            Output.MissingIndexesFolderPath,
            "MissingIndexes.csv",
            context.MissingIndexes
                .OrderBy(index => index.SchemaName)
                .ThenBy(index => index.TableName));

        WriteCsv(
            Output.TriggersFolderPath,
            "Triggers.csv",
            context.Triggers.OrderBy(trigger => trigger.Name));

        WriteCsv(
            Output.ViewsFolderPath,
            "Views.csv",
            context.Views
                .OrderBy(view => view.SchemaName)
                .ThenBy(view => view.Name));

        WriteCsv(
            Output.ProceduresFolderPath,
            "Procedures.csv",
            context.Procedures
                .OrderBy(procedure => procedure.SchemaName)
                .ThenBy(procedure => procedure.Name));

        WriteCsv(
            Output.UDTFolderPath,
            "UserDefinedTypes.csv",
            context.UserDefinedTypes
                .OrderBy(type => type.SchemaName)
                .ThenBy(type => type.Name));
    }
    private static void WriteCsv<T>(string folderPath, string fileName, IEnumerable<T> values)
    {
        var output = CsvUtility.ToCsv(values);
        new FileObject(Path.Combine(folderPath, fileName)).Create(output);
    }
    private void GenerateQuery(ITable table)
    {
        var generator = new DiagramDrivenGraphGenerator();
        var result = generator.Generate(
            table,
            QueryGenerationOptions.GetDefaultGenerationOptions());

        var queryFolder = Path.Combine(Output.QueriesFolderPath, table.SchemaName);
        EnsureDirectory(queryFolder);

        var events = result.Tracker.Events.Select(eventInfo =>
            $"-- FromTableId: {eventInfo.FromTableId} " +
            $"ToTableId: {eventInfo.ToTableId} " +
            $"RelationId: {eventInfo.RelationId} " +
            $"Direction: {eventInfo.Direction} " +
            $"Depth: {eventInfo.Depth} " +
            $"Timestamp: {eventInfo.Timestamp} " +
            $"Message: {eventInfo.Reason}");

        var query = result.GeneratedQuery;

        if (result.Tracker.Events.Any())
        {
            query += NewLine + "-- EVENTS:" + NewLine + string.Join(NewLine, events);
        }

        var fileName = SanitizeFileName(table.Name) + ".sql";
        new FileObject(Path.Combine(queryFolder, fileName)).Create(query);

        GenerateEntity(table, result);
    }
    private void GenerateEntity(ITable table, QueryGenerationResult result)
    {
        var schemaPath = Path.Combine(Output.EntitiesFolderPath, table.SchemaName);
        EnsureDirectory(schemaPath);

        var fileName = SanitizeFileName(table.Name) + ".cs";
        var filePath = Path.Combine(schemaPath, fileName);
        var code = GenerateTableCode(table, result);

        new FileObject(filePath).Create(code);
    }
    #endregion

    #region Generation
    // Usings
    private string GetUsings(ITable table)
    {
        var childNamespaces = table.ChildRelations
            .Select(child => child.ColumnPairs.First().ReferencedColumn?.SchemaName)
            .Where(schema => !string.IsNullOrWhiteSpace(schema))
            .Select(schema => $"{Using} {Output.Namespace}.{schema};");

        var parentNamespaces = table.ParentRelations
            .Select(parent => parent.ColumnPairs.First().ParentColumn?.SchemaName)
            .Where(schema => !string.IsNullOrWhiteSpace(schema))
            .Select(schema => $"{Using} {Output.Namespace}.{schema};");

        var currentNamespace = $"{Using} {Output.Namespace}.{table.SchemaName};";

        return childNamespaces
            .Concat(parentNamespaces)
            .Where(value => value != currentNamespace)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value)
            .Aggregate(
                new StringBuilder(),
                (builder, value) => builder.Append(builder.Length == 0 ? value : NewLine + value))
            .ToString();
    }
    // Namespace
    private string GetNamespaceProperty(ITable table)
    {
        return $"{Namespace} {Output.Namespace}.{table.SchemaName}";
    }
    // Table
    private string GenerateTableCode(ITable table, QueryGenerationResult result)
    {
        try
        {
            var usings = GetUsings(table);
            var ns = usings + NewLine + NewLine + GetNamespaceProperty(table);
            var cls = GetTableProperty(table, result);
            var columns = table.Columns.Select(GetColumnProperty).ToList();

            var code =
                $"{Using} {Output.Namespace};{NewLine}{NewLine}" +
                $"{ns}{NewLine}{{{NewLine}" +
                $"{Indent(cls, 1, true)}" +
                $"{Indent(string.Join(NewLine, columns), 2, true)}{NewLine}{NewLine}";

            // Navigation properties must not collide with scalar properties.
            var takenNames = new HashSet<string>(
                table.Columns.Select(column => SanitizeIdentifier(column.Name)),
                StringComparer.OrdinalIgnoreCase);

            var parentNavigation = table.ForeignKeys.Select(foreignKey =>
            {
                var typeName = tableAliases[
                    (foreignKey.ParentTable.SchemaName, foreignKey.ParentTable.Name)];

                var propertyName = typeName;

                if (table.ForeignKeys.Count(x =>
                        x.ParentTable.Name == foreignKey.ParentTable.Name) > 1)
                {
                    propertyName = SanitizeIdentifier(
                        foreignKey.ColumnPairs.First().ReferencedColumnName
                            .Replace("ID", "", StringComparison.Ordinal)
                            .Replace("Id", "", StringComparison.Ordinal));
                }

                propertyName = MakeUniqueName(propertyName, takenNames);

                return $"{Navigation} " +
                       $"{Output.Namespace}.{foreignKey.ParentTable.SchemaName}.{typeName} " +
                       $"{propertyName} {PropertyAccessor}";
            });

            code += Indent(
                string.Join(NewLine, parentNavigation),
                2,
                true) + NewLine;

            var childNavigation = table.ReferencedByForeignKeys
                .SelectMany(value => value.ColumnPairs)
                .Select(pair =>
                {
                    var typeName = tableAliases[
                        (pair.ReferencedColumn.Table.SchemaName,
                         pair.ReferencedColumn.Table.Name)];

                    var propertyName = SanitizeIdentifier(pair.ReferencedTableName) + "s";

                    if (table.ReferencedByForeignKeys.Count(x =>
                            x.ReferencedTable?.Name == pair.ReferencedTableName) > 1)
                    {
                        propertyName =
                            SanitizeIdentifier(
                                pair.ReferencedColumnName
                                    .Replace("ID", "", StringComparison.Ordinal)
                                    .Replace("Id", "", StringComparison.Ordinal))
                            + SanitizeIdentifier(pair.ReferencedTableName)
                            + "s";
                    }

                    propertyName = MakeUniqueName(propertyName, takenNames);

                    return $"{Navigation} " +
                           $"ICollection<{Output.Namespace}.{pair.ReferencedColumn?.SchemaName}.{typeName}> " +
                           $"{propertyName} {PropertyAccessor}";
                });

            code += Indent(
                string.Join(NewLine, childNavigation),
                2,
                true);

            code += NewLine + Indent("}", 1, true) + NewLine + "}";

            return NormalizeGeneratedCode(code);
        }
        catch (Exception ex)
        {
            return $"// GENERATION ERROR: {table.SchemaName}.{table.Name}{NewLine}" +
                   $"// {ex}{NewLine}";
        }
    }
    // Table Property
    private string GetTableProperty(ITable tbl, QueryGenerationResult result)
    {
        var coverage = CoverageCalculator.CalculateCoverage(tbl, context.Tables, context.Relations);
        const int padding = 20;
        var properTableName = tbl.Columns.Any(v => v.Name == tbl.Name) ? tbl.Name + "Entity" : tbl.Name;
        var cls = $"// {tbl.SchemaName}.{tbl.Name}" + "      (object_id = " + tbl.ObjectId + ")" + NewLine;
        cls += "// Columns: " + tbl.Columns.Count + "    -     Rows:" + tbl.Rows + "  -  Created Date: " + tbl.CreatedDate.ToString() + "  -  Last Modified: " + tbl.DateLastModified.ToString() + NewLine;
        cls += tbl.ParentRelations.Count == 0 ? "" : "// Parents:".PadRight(padding, ' ') + string.Join(", ", tbl.ParentRelations.Select(v => "(" + string.Join(", ", v.ColumnPairs.Select(x => (x.ParentTableName + "." + x.ParentColumnName) ?? string.Empty)) + ")")) + NewLine;
        cls += tbl.ChildRelations.Count == 0 ? "" : "// Children:".PadRight(padding, ' ') + string.Join(", ", tbl.ChildRelations.Select(v => "(" + string.Join(", ", v.ColumnPairs.Select(x => (x.ReferencedTableName + "." + x.ReferencedColumnName) ?? string.Empty)) + ")")) + NewLine;
        cls += tbl.ForeignKeys.Count == 0 ? "" : "// Foreign Keys:".PadRight(padding, ' ') + string.Join(", ", tbl.ForeignKeys.Select(v => "(" + string.Join(", ", v.ColumnPairs.Select(x => x.ReferencedColumnName)) + ")")) + NewLine;
        cls += tbl.Indexes.Count == 0 ? "" : "// Indexes:".PadRight(padding, ' ') + string.Join(", ", tbl.Indexes.Select(v => "(" + string.Join(", ", v.KeyColumns.Select(x => x.Name)) + ")")) + NewLine;
        cls += tbl.MissingIndexes.Count == 0 ? "" : "// Missing Indexes:".PadRight(padding, ' ') + string.Join(", ", tbl.MissingIndexes.Select(v => v.SuggestedKeyColumns + ", " + v.IncludedColumns) + ")") + NewLine;
        cls += result.Metrics.AverageTraversalDepth <= 1 ? "" : "// Average Depth:      " + Math.Round(result.Metrics.AverageTraversalDepth, 5) + NewLine;
        cls += result.ExecutionNodes?.Count < 2 ? "" : "// Linear Unique Table Join Count: " + result.ExecutionNodes?.Count + "      - Total Reach: " + coverage.ReachableTableCount + "  (" + coverage.ReachablePercentage + "%" + ")" + NewLine;
        cls += "// Connected Tables: " + coverage.ReachableTables.Count + "  -   Disconnected Tables: " + coverage.DisconnectedTables.Count + NewLine; // string.Join(", ", coverage.ReachableTables.OrderBy(v => v)) + NewLine;
        cls += $"[JTable(\"{tbl.Name}\", Schema = \"{tbl.SchemaName}\")]{NewLine}" + PublicClass + properTableName.Replace(".", "_") + NewLine + "{" + NewLine;
        return cls;
    }
    // Column Property
    private string GetColumnProperty(IColumn column)
    {
        var isUDT = column.SystemTypeId != column.UserTypeId || column.SystemTypeId == 240;
        var isUnmappable = column.SystemDataType?.Length == 0 && isUDT;

        if (isUnmappable)
        {
            return
                $"// SKIPPED [UserDefined/Unmapped]: {column.Name} - " +
                $"UserTypeName={column.UserTypeName}, " +
                $"UserTypeId={column.UserTypeId}, " +
                $"SystemTypeId={column.SystemTypeId}, " +
                $"SystemDataType={column.SystemDataType}";
        }

        var property = string.Empty;
        var isKey = column.IsPrimaryKey || column.IsForeignKey;

        if (isKey)
        {
            var attributes = new List<string>();

            if (column.IsPrimaryKey)
                attributes.Add("IsPrimaryKey=true");

            if (column.IsForeignKey)
                attributes.Add("IsForeignKey=true");

            if (column.IsIdentity)
                attributes.Add("IsDbGenerated=true");

            property += $"[JKey({string.Join(", ", attributes)})]{NewLine}";
        }

        var determinedCType = SQLTypeMapper.SqlTypeToClr(column.DataType, column.IsNullable);
        determinedCType = determinedCType == "object" ? SQLTypeMapper.SqlTypeToClr(column.SystemDataType, column.IsNullable) : determinedCType;

        property +=
            $"public {SQLTypeMapper.SqlTypeToClr(determinedCType, column.IsNullable)} " +
            $"{SanitizeIdentifier(column.Name)} {PropertyAccessor} {(isUDT ? "// UDT Type: " + column.DataType : "")}";

        return property;
    }
    #endregion

    #region Filesystem
    private void CreateDirectories()
    {
        foreach (var path in GetTopLevelFolderPaths())
            EnsureDirectory(path);

        EnsureDirectory(Output.ProcedureDefinitionsFolderPath);
        EnsureDirectory(Output.ViewDefinitionsFolderPath);

        new FileObject(Path.Combine(Output.AttributesFolderPath, "JeoTable.cs"))
            .Create(TABLE_ATTR);

        new FileObject(Path.Combine(Output.AttributesFolderPath, "JeoKey.cs"))
            .Create(KEY_ATTR);

        foreach (var schema in context.Tables
                     .Select(table => table.SchemaName)
                     .Distinct(StringComparer.Ordinal))
        {
            EnsureDirectory(Path.Combine(Output.EntitiesFolderPath, schema));
        }
    }
    private static void EnsureDirectory(string path)
    {
        var directory = new DirectoryObject(path);

        if (!directory.Exists)
            directory.Create(System.Security.AccessControl.FileSystemRights.FullControl);
    }
    private List<string> GetTopLevelFolderPaths() =>
    [
        Output.AttributesFolderPath,
        Output.EntitiesFolderPath,
        Output.ProceduresFolderPath,
        Output.IndexesFolderPath,
        Output.QueriesFolderPath,
        Output.ViewsFolderPath,
        Output.MissingIndexesFolderPath,
        Output.TriggersFolderPath,
        Output.RelationsFolderPath,
        Output.UDTFolderPath
    ];
    #endregion

    #region Helpers
    private static string MakeUniqueName(string name, ISet<string> takenNames)
    {
        name = string.IsNullOrWhiteSpace(name)
            ? "Navigation"
            : name;

        var candidate = name;
        var suffix = 2;

        while (!takenNames.Add(candidate))
            candidate = $"{name}{suffix++}";

        return candidate;
    }
    private static string SanitizeFileName(string name) =>
        name.Replace(new[] { ".", "-" }, "_");
    private static string NormalizeGeneratedCode(string code)
    {
        var lines = code
            .Split('\n')
            .Select(line => line.TrimEnd('\r', ' ', '\t'))
            .Where(line => !string.IsNullOrWhiteSpace(line));

        return string.Join(NewLine, lines) + NewLine + NewLine;
    }
    private static string Indent(string code, int depth, bool trimEnd)
    {
        var prefix = new string('\t', depth);

        var lines = code
            .Split('\n')
            .Select(line => line.Replace("\r", string.Empty))
            .Select(line => prefix + line);

        var value = string.Join(NewLine, lines);

        return trimEnd
            ? value.Trim(' ', '\r', '\n')
            : value;
    }
    private static string SanitizeIdentifier(string name)
    {
        var clean = Regex.Replace(name ?? string.Empty, @"[^A-Za-z0-9_]", "_");

        if (string.IsNullOrEmpty(clean))
            return "_";

        if (char.IsDigit(clean[0]))
            clean = "_" + clean;

        if (CSharpReservedKeywords.CSharpKeywords.Contains(clean))
            clean = "@" + clean;

        return clean;
    }
    #endregion
}

