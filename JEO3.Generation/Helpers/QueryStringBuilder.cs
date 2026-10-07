using System.Text;
using JEO3.Core.Extensions;
using JEO3.Generation.Models;

namespace JEO3.Generation
{
    internal static class QueryStringBuilder
    {
        internal static string BuildJoinClause(QueryTable table, bool isDownward, string joinType)
        {
            // Filter?
            var relations = table.Relationship?.ColumnPairs.Where(v => v.ParentColumn != null && v.ReferencedColumn != null).ToList();

            // Validation
            if (relations == null || relations.Count == 0) return string.Empty;

            // Check Direction
            return isDownward
                ? $"\t{joinType} {table.Relationship?.ReferencedTable?.SchemaName.Sql()}.{table.Relationship?.ReferencedTable?.Name.Sql()} {table.Alias.Sql()} ON {string.Join(" AND ", relations.Select(v => $"{table.Alias.Sql()}.{v.ReferencedColumn.Name.Sql()} = {table.Parent?.Alias.Sql()}.{v.ParentColumn.Name.Sql()}"))}"
                : $"\t{joinType} {table.Relationship?.ParentTable?.SchemaName.Sql()}.{table.Relationship?.ParentTable?.Name.Sql()} {table.Alias.Sql()} ON {string.Join(" AND ", relations.Select(v => $"{table.Alias.Sql()}.{v.ParentColumn.Name.Sql()} = {table.Parent?.Alias.Sql()}.{v.ReferencedColumn.Name.Sql()}"))}";
        }

        internal static string BuildProjectionClause(QueryTable node, QueryGenerationOptions options, bool isLastNode)
        {
            var settings = options.Retrieval;
            var format = options.Formatting;

            // Determine which policy applies to this specific node
            bool isRoot = node.Parent == null;
            TableSelectPolicy activePolicy = isRoot ? settings.RootSelectPolicy : settings.JoinSelectPolicy;

            // Direct exit if this node type shouldn't project columns
            if (activePolicy == TableSelectPolicy.None)
            {
                return string.Empty;
            }

            // Resolve structural breadcrumbs context
            string breadcrumb = format.AdvancedAnnotations.HasFlag(AnnotationVerbosity.PathBreadcrumbs)
                ? BuildBreadcrumb(node, format.AdvancedAnnotations)
                : string.Empty;

            string trailingComma = isLastNode ? "" : ",";

            // BRANCH A: Handle Compact Star Projection (.*)
            if (activePolicy == TableSelectPolicy.Star)
            {
                if (format.SingleLinePerTable)
                {
                    string comment = !string.IsNullOrEmpty(breadcrumb) ? $"  -- ^^ {node.Table.Name.PadRight(options.Formatting.BreadcrumbPaddingRight.GetValueOrDefault(), ' ')}{breadcrumb}" : "";
                    return $"    {node.Alias.Sql()}.*{trailingComma}{comment}{Environment.NewLine}";
                }
                else
                {
                    var starBuilder = new StringBuilder();
                    starBuilder.AppendLine($"    {node.Alias.Sql()}.*{trailingComma}");
                    if (!string.IsNullOrEmpty(breadcrumb))
                    {
                        starBuilder.AppendLine($"    -- ^^ {node.Table.Name.PadRight(options.Formatting.BreadcrumbPaddingRight.GetValueOrDefault(), ' ')}{breadcrumb}");
                    }
                    return starBuilder.ToString();
                }
            }

            // BRANCH B: Handle Primary Key Only Projection Strategy
            if (activePolicy == TableSelectPolicy.PkOnly)
            {
                if (isRoot)
                {
                    var pkId = node.Table.Columns.Where(v => v.IsPrimaryKey).FirstOrDefault()?.Name.Sql() ?? string.Empty;

                    string comment = !string.IsNullOrEmpty(breadcrumb) ? $"  -- ^^ {node.Table.Name.PadRight(options.Formatting.BreadcrumbPaddingRight.GetValueOrDefault(), ' ')}{breadcrumb}" : "";
                    return $"    {node.Alias.Sql()}.{pkId.Sql()} AS {node.Alias}_{pkId}{trailingComma}{comment}{Environment.NewLine}";
                }
                else if (node.Relationship?.ParentTable == node.Table)
                {
                    var pkId = node.Relationship?.ParentTable.Columns.Where(v => v.IsPrimaryKey).FirstOrDefault()?.Name.Sql() ?? string.Empty;

                    string comment = !string.IsNullOrEmpty(breadcrumb) ? $"  -- ^^ {node.Table.Name.PadRight(options.Formatting.BreadcrumbPaddingRight.GetValueOrDefault(), ' ')}{breadcrumb}" : "";
                    return $"    {node.Alias.Sql()}.{pkId.Sql()} AS {node.Alias}_{pkId}{trailingComma}{comment}{Environment.NewLine}";
                }
                else if (node.Relationship?.ReferencedTable == node.Table)
                {
                    var fkId = node.Relationship?.ReferencedTable.Columns.Where(v => v.IsPrimaryKey).FirstOrDefault()?.Name.Sql() ?? string.Empty;

                    string comment = !string.IsNullOrEmpty(breadcrumb) ? $"  -- ^^ {node.Table.Name.PadRight(options.Formatting.BreadcrumbPaddingRight.GetValueOrDefault(), ' ')}{breadcrumb}" : "";
                    return $"    {node.Alias.Sql()}.{fkId.Sql()} AS {node.Alias}_{fkId}{trailingComma}{comment}{Environment.NewLine}";
                }
            }

            // BRANCH C: Render Individual Explicit Columns (AllColumns Strategy)
            var columns = node.Table.Columns
                .Where(c => !options.Traversal.OmitFromSelectsList.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            int totalAvailable = columns.Count;
            bool wasCapped = false;

            if (settings.ColumnsPerTableLimit.HasValue && totalAvailable > settings.ColumnsPerTableLimit.Value)
            {
                columns = columns.Take(settings.ColumnsPerTableLimit.Value).ToList();
                wasCapped = true;
            }

            if (!columns.Any()) return string.Empty;

            var sb = new StringBuilder();
            string label = !string.IsNullOrEmpty(breadcrumb)
                ? GetCappedLabel(node.Table.Name, wasCapped, settings.ColumnsPerTableLimit, totalAvailable)
                : string.Empty;

            if (format.SingleLinePerTable)
            {
                var renderedColumns = columns.Select((c, idx) =>
                {
                    bool isAbsoluteLastColumn = isLastNode && (idx == columns.Count - 1);
                    return $"{node.Alias.Sql()}.{c.Name.Sql()}{(isAbsoluteLastColumn ? "" : ",")}";
                });

                sb.Append("    ");
                sb.AppendLine(string.Join(" ", renderedColumns));

                if (!string.IsNullOrEmpty(label))
                {
                    sb.AppendLine($"    -- ^^ {label.PadRight(options.Formatting.BreadcrumbPaddingRight.GetValueOrDefault(), ' ')}{breadcrumb}");
                }
            }
            else
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    bool isAbsoluteLastColumn = isLastNode && (i == columns.Count - 1);
                    sb.Append($"    {node.Alias.Sql()}.{columns[i].Name.Sql()}{(isAbsoluteLastColumn ? "" : HastTrailingComma(i, columns.Count, isLastNode))}");

                    if (i == 0 && !string.IsNullOrEmpty(label))
                    {
                        sb.Append($"  -- ^^ {label.PadRight(options.Formatting.BreadcrumbPaddingRight.GetValueOrDefault(), ' ')}{breadcrumb}");
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        internal static string IndentLines(string value, int spaces)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var indent = new string(' ', spaces);
            var lines = value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            return string.Join(Environment.NewLine, lines.Select(line =>
                string.IsNullOrWhiteSpace(line) ? line : indent + line));
        }

        private static string HastTrailingComma(int currentColumnIndex, int totalColumns, bool isLastTableNode)
        {
            return (currentColumnIndex == totalColumns - 1 && isLastTableNode) ? "" : ",";
        }

        private static string GetCappedLabel(string tableName, bool wasCapped, int? max, int total)
        {
            return wasCapped
                ? $"{tableName} [{max}/{total}]"
                : tableName;
        }

        private static string BuildBreadcrumb(QueryTable node, AnnotationVerbosity verbosity)
        {
            if (node.Parent == null) return " [ROOT]";
            if (verbosity.HasFlag(AnnotationVerbosity.PathBreadcrumbs) == false) return string.Empty;

            var path = new List<string> { node.Table.Name };
            var current = node.Parent;
            while (current != null)
            {
                path.Insert(0, current.Table.Name);
                current = current.Parent;
            }

            return $" ({string.Join(" -> ", path)})";
        }
    }
}