using System.Text;
using System.Text.RegularExpressions;
using JEO3.Core;
using JEO3.Extensions;
using JEO3.Generation;
using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Code
{
    public sealed class TableCodeGeneratorOld
    {
        #region Properties
        private Dictionary<(string Schema, string Name), string> tableAliases { get; set; } = [];
        private string _nameSpace;
        private ITable _table;
        private bool _useAttributes;
        private IDatabaseContext _ctx;
        #endregion

        #region Initialization
        public TableCodeGeneratorOld(IDatabaseContext context, ITable table, string nameSpace, bool useAttributes = false)
        {
            _ctx = context;
            _nameSpace = nameSpace;
            _table = table;
            _useAttributes = useAttributes;
        }
        #endregion

        #region Generate - TODO: Consolidate Overlap Between This And ContectCodeGenerator.cs
        public string GenerateTableCode(QueryGenerationResult result)
        {
            try
            {
                tableAliases = _ctx.Tables.ToDictionary(
                    t => (t.SchemaName, t.Name),
                    t => t.Columns.Any(c => c.Name == t.Name) ? t.Name.Replace(".", "_") + "Entity" : t.Name.Replace(".", "_"));

                string replaceBad(string s)
                {
                    return s.Replace(new string[] { ".", "-" }, "_");
                }

                var usings = GetUsings(_table);
                var ns = $"{usings}\r\n\r\n{GetNamespaceProperty(_table)}";
                var cls = GetTableProperty(_table, result);
                var columns = _table.Columns.Select(v => GetColumnProperty(v)).ToList();

                var sb = new StringBuilder();
                sb.Append($"using {_nameSpace};\r\n\r\n{ns}\r\n{{\r\n{Indent(cls, 1, true).TrimEnd('\t')}{Indent(string.Join("\r\n", columns), 2, true)}\r\n\r\n");

                // HERE WE NEED TO SUFFIX # FOR MULTIPLE SAME TYPE RELATIONS
                var takenNames = new HashSet<string>(_table.Columns.Select(c => c.Name));

                var parentNavLines = _table.ForeignKeys.Select(v =>
                {
                    var typeName = tableAliases[(v.ParentTable.SchemaName, v.ParentTable.Name)];
                    var propName = typeName;

                    if (_table.ForeignKeys.Count(x => x.ParentTable.Name == v.ParentTable.Name) > 1)
                        propName = replaceBad(v.ColumnPairs.First().ReferencedColumnName.Replace("ID", "").Replace("Id", ""));

                    if (takenNames.Contains(propName)) propName += "Nav";
                    takenNames.Add(propName);

                    return $"public virtual {_nameSpace}.{v.ParentTable.SchemaName}.{typeName} {propName} {{ get; set; }}";
                });
                sb.Append(Indent(string.Join("\r\n", parentNavLines), 2, true)).Append("\r\n");

                var childNavLines = _table.ReferencedByForeignKeys.SelectMany(v => v.ColumnPairs).Select(v =>
                {
                    try
                    {
                        var typeName = tableAliases[(v.ReferencedColumn.Table.SchemaName, v.ReferencedColumn.Table.Name)];
                        var propName = replaceBad(v.ReferencedTableName) + "s";

                        if (_table.ReferencedByForeignKeys.Count(x => x.ReferencedTable?.Name == v.ReferencedTableName) > 1)
                            propName = replaceBad(v.ReferencedColumnName.Replace("ID", "").Replace("Id", "")) + replaceBad(v.ReferencedTableName) + "s";

                        if (takenNames.Contains(propName)) propName += "Nav";
                        takenNames.Add(propName);
                        return $"public virtual ICollection<{_nameSpace}.{v.ReferencedColumn?.SchemaName}.{typeName}> {propName} {{ get; set; }}";
                    }
                    catch (Exception ex)
                    {
                        return "// SKIPPING: Likely UDT Issue: " + ex.Message;
                    }

                });

                sb.Append(Indent(string.Join("\r\n", childNavLines), 2, true))
                  .Append(Indent("\r\n}", 1, true).TrimEnd('\n'))
                  .Append("\r\n}".TrimEnd('\n'));

                var code = string.Join("\r\n", sb.ToString().Split('\n').Select(v => v.TrimEnd()).Where(v => v != string.Empty)) + "\r\n\r\n";
                return code;
            }
            catch (Exception ex)
            {
                return $" EXCEPTION: {_table.Name}: {ex.ToString()}";
            }
        }
        public string GenerateTableAnnotationOnly(QueryGenerationResult result, string prefix = "// ")
        {
            try
            {
                tableAliases = _ctx.Tables.ToDictionary(
                    t => (t.SchemaName, t.Name),
                    t => t.Columns.Any(c => c.Name == t.Name) ? t.Name.Replace(".", "_") + "Entity" : t.Name.Replace(".", "_"));

                var usings = GetUsings(_table, prefix);
                return GetTableAnnotations(_table, result, prefix);
            }
            catch (Exception ex)
            {
                return "Error Generating Table Annotation";
            }
        }
        private string GenerateTableHeaderCode(QueryGenerationResult result, bool includeUsings = true, string prefix = "// ")
        {
            try
            {
                tableAliases = _ctx.Tables.ToDictionary(
                    t => (t.SchemaName, t.Name),
                    t => t.Columns.Any(c => c.Name == t.Name) ? t.Name.Replace(".", "_") + "Entity" : t.Name.Replace(".", "_"));

                var usings = includeUsings ? GetUsings(_table) : string.Empty;
                var ns = $"{usings}\r\n\r\n{GetNamespaceProperty(_table)}";
                return $"{ns}\r\n{GetTableProperty(_table, result)}";
            }
            catch (Exception ex)
            {
                return "Error Generating Table Annotation";
            }
        }
        private string GetUsings(ITable tbl, string prefix = "")
        {
            var u1 = tbl.ChildRelations.Select(v => $"{prefix}using {_nameSpace}.{v.ColumnPairs.First().ReferencedColumn?.SchemaName};").Distinct().Where(v => v != $"using {_nameSpace}.{tbl.SchemaName};\r\n");
            var u2 = tbl.ParentRelations.Select(v => $"{prefix}using {_nameSpace}.{v.ColumnPairs.First().ParentColumn?.SchemaName};").Distinct().Where(v => v != $"using {_nameSpace}.{tbl.SchemaName};\r\n");

            return $"{string.Join("\r\n", u1)}\r\n{string.Join("\r\n", u2)}";
        }
        private string GetNamespaceProperty(ITable tbl)
        {
            return $"namespace {_nameSpace}.{tbl.SchemaName}";
        }
        public string GetTableProperty(ITable tbl, QueryGenerationResult result, bool isCode = true)
        {
            var properTableName = tbl.Columns.Any(v => v.Name == tbl.Name) ? tbl.Name + "Entity" : tbl.Name;
            var cls = $"{GetTableAnnotations(tbl, result)}\r\n";
            if (_useAttributes)
            {
                cls += $"[JeoTable(\"{tbl.Name}\", Schema = \"{tbl.SchemaName}\")]\r\npublic class {properTableName.Replace(".", "_")}\r\n{{\r\n";
            }
            else if (isCode)
            {
                cls += $"public class {properTableName.Replace(".", "_")}\r\n{{\r\n";
            }
            return cls;
        }
        public string GetTableAnnotations(ITable tbl, QueryGenerationResult result, string prefix = "// ")
        {
            var coverage = CoverageCalculator.CalculateCoverage(tbl, _ctx.Tables, _ctx.Relations);
            const int padding = 20;
            var properTableName = tbl.Columns.Any(v => v.Name == tbl.Name) ? tbl.Name + "Entity" : tbl.Name;

            var sb = new StringBuilder();
            sb.Append($"{prefix}{tbl.SchemaName}.{tbl.Name}      (object_id = {tbl.ObjectId})\r\n");
            sb.Append($"{prefix}Columns: {tbl.Columns.Count}  -   Rows:{tbl.Rows}  -  Created Date: {tbl.CreatedDate.ToString()}  -  Last Modified: {tbl.DateLastModified.ToString()}\r\n");

            if (tbl.ParentRelations.Count > 0)
                sb.Append($"{prefix}{"Parents:".PadRight(padding, ' ')}{string.Join(", ", tbl.ParentRelations.Select(v => "(" + string.Join(", ", v.ColumnPairs.Select(x => (x.ParentTableName + "." + x.ParentColumnName) ?? string.Empty)) + ")"))}\r\n");

            if (tbl.ChildRelations.Count > 0)
                sb.Append($"{prefix}{"Children:".PadRight(padding, ' ')}{string.Join(", ", tbl.ChildRelations.Select(v => "(" + string.Join(", ", v.ColumnPairs.Select(x => (x.ReferencedTableName + "." + x.ReferencedColumnName) ?? string.Empty)) + ")"))}\r\n");

            if (tbl.ForeignKeys.Count > 0)
                sb.Append($"{prefix}{"Foreign Keys:".PadRight(padding, ' ')}{string.Join(", ", tbl.ForeignKeys.Select(v => "(" + string.Join(", ", v.ColumnPairs.Select(x => x.ReferencedColumnName)) + ")"))}\r\n");

            if (tbl.Indexes.Count > 0)
                sb.Append($"{prefix}{"Indexes:".PadRight(padding, ' ')}{string.Join(", ", tbl.Indexes.Select(v => "(" + string.Join(", ", v.KeyColumns.Select(x => x.Name)) + ")"))}\r\n");

            if (tbl.MissingIndexes.Count > 0)
                sb.Append($"{prefix}{"Missing Indexes:".PadRight(padding, ' ')}{string.Join(", ", tbl.MissingIndexes.Select(v => v.SuggestedKeyColumns + ", " + v.IncludedColumns) + ")")}\r\n");

            if (result.Metrics.AverageTraversalDepth > 1)
                sb.Append($"{prefix}Average Depth:   {Math.Round(result.Metrics.AverageTraversalDepth, 5)}\r\n");

            if (result.ExecutionNodes?.Count >= 2)
                sb.Append($"{prefix}Linear Unique Table Join Count: {result.ExecutionNodes?.Count}      - Total Reach: {coverage.ReachableTableCount}  ({coverage.ReachablePercentage}%)\r\n");

            sb.Append($"{prefix}Connected Tables: {coverage.ReachableTables.Count}  -   Disconnected Tables: {coverage.DisconnectedTables.Count}");
            return sb.ToString();
        }
        private string GetColumnProperty(IColumn col)
        {
            var isUDT = col.SystemTypeId != col.UserTypeId || col.SystemTypeId == 240;
            if (col.SystemDataType?.Length == 0 && (isUDT))
            {
                return $"// SKIPPED [UserDefined/Unmapped]: {col.Name} - UserTypeName={col.UserTypeName}, UserTypeId={col.UserTypeId}, SystemTypeId={col.SystemTypeId}, SystemDataType={col.SystemDataType}";
            }
            var determinedCType = SQLTypeMapper.SqlTypeToClr(col.DataType, col.IsNullable);
            determinedCType = determinedCType == "object" ? SQLTypeMapper.SqlTypeToClr(col.SystemDataType, col.IsNullable) : determinedCType;

            var prop = new StringBuilder(); if (_useAttributes) { var isKey = col.IsPrimaryKey || col.IsForeignKey; if (isKey) { prop.Append("[JeoKey(").Append(col.IsPrimaryKey ? "IsPrimaryKey=true" : "IsForeignKey=true").Append(col.IsIdentity ? ", IsDbGenerated=true" : "").Append(")]\r\n"); } }
            prop.Append($"public {determinedCType} {SanitizeIdentifier(col.Name)} {{ get; set; }} {(isUDT ? "// UDT Type: " + col.DataType : "")}"); return prop.ToString();
        }
        #endregion

        #region Helpers
        private string Indent(string code, int depth, bool trimEnd) { var lines = code.Split('\n').Select(v => v.Replace("\r", "")).ToList(); for (var i = 0; i < depth; i++) { for (var x = 0; x < lines.Count; x++) { lines[x] = "\t" + lines[x]; } } var val = string.Join("\r\n", lines); if (trimEnd) { val = val.Trim(' ', '\r', '\n'); } return val; }
        private string SanitizeIdentifier(string name) { var clean = Regex.Replace(name, @"[^A-Za-z0-9_]", ""); if (clean.Length > 0 && char.IsDigit(clean[0])) clean = "" + clean; if (CSharpReservedKeywords.CSharpKeywords.Contains(clean)) clean = "@" + clean; return clean; }
        #endregion
    }
}