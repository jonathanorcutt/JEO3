using System.Text;
using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Code
{
    public class CodeContextGenerator
    {
        #region Fields
        private static readonly HashSet<string> UninflectedWords = new(StringComparer.OrdinalIgnoreCase) { "Data", "Series", "Species", "Equipment", "Information" };

        private readonly IDatabaseContext _ctx;
        private readonly string _nameSpace;
        private readonly List<QueryGenerationResult> _results;
        #endregion

        #region Constructor
        public CodeContextGenerator(IDatabaseContext ctx, string nameSpace, List<QueryGenerationResult> results)
        {
            _ctx = ctx;
            _nameSpace = nameSpace;
            _results = results;
        }
        #endregion

        #region Generate
        public string Generate()
        {
            var tables = _ctx.Tables.ToList();
            var tableAliases = BuildTableAliases(tables);
            var dbSetEntries = new List<(string PropertyName, string TypeName, string SchemaName)>();

            var allUsings = new List<string>();
            var allClasses = new List<string>();

            for (int i = 0; i < tables.Count; i++)
            {
                var table = tables[i];
                var result = _results[i];
                var generator = new TableCodeGenerator(_ctx, table, _nameSpace, false);
                var classMods = generator.GenerateTableModule(result);
                allUsings.AddRange(classMods.UsingNamespaces);
                var typeName = tableAliases[(table.SchemaName, table.Name)];

                allClasses.AddRange(classMods.Code);
                dbSetEntries.Add((Pluralize(typeName), typeName, table.SchemaName));
            }

            allUsings = allUsings.Distinct().OrderBy(v => v).ToList();

            if (tables.Any(v => v.SchemaName == string.Empty || v.SchemaName == "dbo"))
            {
                allUsings = allUsings.Concat(new string[] { $"{_nameSpace}.dbo" }).OrderBy(v => v).ToList();
            }
            var sb = new StringBuilder();
            sb.AppendLine(string.Join("\r\n", allUsings.Select(v => "using " + v + ";")));
            sb.AppendLine(GenerateContextClass(dbSetEntries));
            foreach (var item in allClasses)
            {
                sb.AppendLine(item);
            }

            return sb.ToString();
        }
        private string GenerateContextClass(List<(string PropertyName, string TypeName, string SchemaName)> entries)
        {
            var sb = new StringBuilder();
            sb.Append($"namespace {_nameSpace}\r\n{{\r\n");
            sb.Append("\tpublic class GeneratedDbContext\r\n\t{\r\n");

            foreach (var entry in entries)
            {
                sb.Append($"\t\tpublic virtual DbSet<{_nameSpace}.{entry.SchemaName}.{entry.TypeName}> {entry.PropertyName} {{ get; set; }}\r\n");
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