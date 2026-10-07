using System.Text;
using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Code
{
    public static class CodeContextGeneratorOld
    {
        #region Properties
        private static readonly HashSet<string> UninflectedWords = new(StringComparer.OrdinalIgnoreCase) { "Data", "Series", "Species", "Equipment", "Information" };
        #endregion

        #region Generate
        public static List<GeneratedFile> GenerateAllTableFiles(DatabaseContext ctx, string nameSpace, QueryGenerationResult result, bool useAttributes = false)
        {
            var tableAliases = BuildTableAliases(ctx.Tables);
            return GenerateAllTableFiles(ctx, ctx.Tables, nameSpace, result, tableAliases, useAttributes);
        }
        public static List<GeneratedFile> GenerateAllTableFiles(DatabaseContext ctx, IEnumerable<ITable> tables, string nameSpace, QueryGenerationResult result, bool useAttributes = false)
        {
            var tableAliases = BuildTableAliases(tables);
            return GenerateAllTableFiles(ctx, tables, nameSpace, result, tableAliases, useAttributes);
        }
        public static List<GeneratedFile> GenerateAllTableFiles(DatabaseContext ctx, IEnumerable<ITable> tables, string nameSpace, QueryGenerationResult result, Dictionary<(string Schema, string Name), string> tableAliases, bool useAttributes = false)
        {
            var files = new List<GeneratedFile>();
            var dbSetEntries = new List<(string PropertyName, string TypeName, string SchemaName)>();

            foreach (var table in tables)
            {
                var generator = new TableCodeGenerator(ctx, table, nameSpace, useAttributes);
                var classCode = generator.GenerateTableCode(result);
                var typeName = tableAliases[(table.SchemaName, table.Name)];

                files.Add(new GeneratedFile(typeName, table.SchemaName, classCode));
                dbSetEntries.Add((Pluralize(typeName), typeName, table.SchemaName));
            }

            files.Insert(0, new GeneratedFile("GeneratedDbContext", nameSpace, GenerateContextClass(nameSpace, dbSetEntries)));

            return files;
        }
        private static string GenerateContextClass(string nameSpace, List<(string PropertyName, string TypeName, string SchemaName)> entries)
        {
            var sb = new StringBuilder();
            sb.Append($"namespace {nameSpace}\r\n{{\r\n");
            sb.Append("\tpublic class GeneratedDbContext\r\n\t{\r\n");

            foreach (var entry in entries)
            {
                sb.Append($"\t\tpublic virtual DbSet<{nameSpace}.{entry.SchemaName}.{entry.TypeName}> {entry.PropertyName} {{ get; set; }}\r\n");
            }

            sb.Append("\t}\r\n}\r\n");
            return sb.ToString();
        }
        #endregion

        #region Helpers
        public static Dictionary<(string Schema, string Name), string> BuildTableAliases(IEnumerable<ITable> tables)
        {
            return tables.ToDictionary(
                t => (t.SchemaName, t.Name),
                t => t.Columns.Any(c => c.Name == t.Name) ? t.Name.Replace(".", "_") + "Entity" : t.Name.Replace(".", "_"));
        }
        public static string Pluralize(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (UninflectedWords.Contains(name)) return name;

            if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && name.Length > 1 && !IsVowel(name[^2]))
                return name[..^1] + "ies";

            if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
                return name + "es";

            if (name.EndsWith("f", StringComparison.OrdinalIgnoreCase))
                return name[..^1] + "ves";
            if (name.EndsWith("fe", StringComparison.OrdinalIgnoreCase))
                return name[..^2] + "ves";

            return name + "s";
        }
        private static bool IsVowel(char c) => "aeiouAEIOU".IndexOf(c) >= 0;
        #endregion
    }
}
