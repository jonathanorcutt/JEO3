using System.Text;

namespace JEO3.Generation.Diagnostics
{
    public sealed class SchemaDiffReport
    {
        public List<SchemaDrift> Drifts { get; init; } = [];
        public IEnumerable<SchemaDrift> DeploymentsRequired => Drifts.Where(d => d.Type == DriftType.MissingInTarget);
        public IEnumerable<SchemaDrift> PotentialDataLoss => Drifts.Where(d => d.Type == DriftType.MissingInSource);
        public IEnumerable<SchemaDrift> Alterations => Drifts.Where(d => d.Type == DriftType.Modified);
        public bool IsInSync => !Drifts.Any();

        public string ToText()
        {
            var sb = new StringBuilder();

            AppendSection(sb, "DEPLOYMENTS REQUIRED", DeploymentsRequired);
            AppendSection(sb, "POTENTIAL DATA LOSS", PotentialDataLoss);
            AppendSection(sb, "ALTERATIONS", Alterations);

            return sb.ToString();
        }
        private static void AppendSection(
        StringBuilder sb,
        string title,
        IEnumerable<SchemaDrift> items)
        {
            sb.AppendLine(title);
            sb.AppendLine(new string('-', title.Length));

            foreach (var d in items)
            {
                sb.AppendLine($"{d.ObjectType}: {d.ObjectPath}");
                sb.AppendLine($"{d.Description}");
                sb.AppendLine();
            }

            sb.AppendLine();
        }
    }
}
