using System.Net;
using System.Text;
using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Generation.Diagnostics
{
    public sealed class SchemaDiagnostic
    {
        public ITable Table { get; init; } = default!;
        public List<SchemaFinding> Findings { get; init; } = [];
        public int Score { get; init; }
        public SchemaFindingSeverity Severity =>
            Findings.Any(x => x.Severity == SchemaFindingSeverity.Critical)
            ? SchemaFindingSeverity.Critical
            : Findings.Any(x => x.Severity == SchemaFindingSeverity.Warning)
                ? SchemaFindingSeverity.Warning
                : Findings.Any(x => x.Severity == SchemaFindingSeverity.Info)
                    ? SchemaFindingSeverity.Info
                    : SchemaFindingSeverity.Good;
        public int RiskScore => Findings.Sum(x => x.Score);
        public SchemaFindingSeverity OverallSeverity =>
            RiskScore switch
            {
                >= 15 => SchemaFindingSeverity.Critical,
                >= 8 => SchemaFindingSeverity.Warning,
                >= 1 => SchemaFindingSeverity.Info,
                _ => SchemaFindingSeverity.Good
            };
        public int CriticalCount => Findings.Count(x => x.Severity == SchemaFindingSeverity.Critical);
        public int WarningCount => Findings.Count(x => x.Severity == SchemaFindingSeverity.Warning);
        public int InfoCount => Findings.Count(x => x.Severity == SchemaFindingSeverity.Info);
        public int GoodCount => Findings.Count(x => x.Severity == SchemaFindingSeverity.Good);

        /// <summary>
        /// Generates a comprehensive, human-readable text report of the diagnostic findings.
        /// </summary>
        public string ToText()
        {
            var sb = new StringBuilder();

            // HEADER
            sb.AppendLine("============================================================");
            sb.AppendLine("                  SCHEMA DIAGNOSTIC REPORT                  ");
            sb.AppendLine("============================================================");

            if (Table != null)
            {
                sb.AppendLine($"Target Object : {Table.TablePath ?? Table.Name}");
            }
            else
            {
                sb.AppendLine("Target Object : Global Context");
            }

            sb.AppendLine($"Risk Score    : {RiskScore}");
            sb.AppendLine($"Overall Health: {OverallSeverity.ToString().ToUpperInvariant()}");
            sb.AppendLine("------------------------------------------------------------");
            sb.AppendLine($"Summary       : {CriticalCount} Critical | {WarningCount} Warning | {InfoCount} Info | {GoodCount} Good");
            sb.AppendLine("============================================================");
            sb.AppendLine();

            if (Findings.Count == 0)
            {
                sb.AppendLine("No diagnostic findings to report. Schema looks clean.");
                return sb.ToString();
            }

            // Group findings by severity (highest to lowest)
            var groupedFindings = Findings
                .GroupBy(f => f.Severity)
                .OrderByDescending(g => (int)g.Key);

            foreach (var group in groupedFindings)
            {
                string severityHeader = $"{group.Key.ToString().ToUpperInvariant()} FINDINGS ({group.Count()})";
                sb.AppendLine(severityHeader);
                sb.AppendLine(new string('-', severityHeader.Length));

                foreach (var finding in group.OrderByDescending(f => f.Score))
                {
                    sb.AppendLine($"[{finding.Category}] {finding.Title} (Impact: {(finding.Score > 0 ? "+" : "")}{finding.Score})");
                    sb.AppendLine($"    Description : {finding.Description}");

                    // Optional Metadata Appendages
                    if (!string.IsNullOrWhiteSpace(finding.ObjectPath) && (Table == null || finding.ObjectPath != Table.TablePath))
                        sb.AppendLine($"    Object      : {finding.ObjectPath}");

                    if (!string.IsNullOrWhiteSpace(finding.ColumnName))
                        sb.AppendLine($"    Column      : {finding.ColumnName}");

                    if (!string.IsNullOrWhiteSpace(finding.ForeignKeyName))
                        sb.AppendLine($"    Foreign Key : {finding.ForeignKeyName}");

                    if (!string.IsNullOrWhiteSpace(finding.IndexName))
                        sb.AppendLine($"    Index       : {finding.IndexName}");

                    if (!string.IsNullOrWhiteSpace(finding.Recommendation))
                    {
                        sb.AppendLine($"    Action      : {finding.Recommendation}");
                    }

                    sb.AppendLine();
                }
            }

            sb.AppendLine("============================================================");
            sb.AppendLine("                        END OF REPORT                       ");
            sb.AppendLine("============================================================");

            return sb.ToString();
        }

        public string ToHtml()
        {
            var sb = new StringBuilder();

            // 1. HTML Header & Embedded Fluent Styles
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset=\"UTF-8\">");
            sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine($"    <title>Schema Diagnostic Report - {(Table != null ? WebUtility.HtmlEncode(Table.TablePath ?? Table.Name) : "Global")}</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        :root {");
            sb.AppendLine("            --bg: #f8fafc; --surface: #ffffff; --text: #1e293b; --text-muted: #64748b; --border: #e2e8f0;");
            sb.AppendLine("            --critical: #ef4444; --critical-bg: #fef2f2;");
            sb.AppendLine("            --warning: #f59e0b; --warning-bg: #fffbeb;");
            sb.AppendLine("            --info: #3b82f6; --info-bg: #eff6ff;");
            sb.AppendLine("            --good: #10b981; --good-bg: #ecfdf5;");
            sb.AppendLine("        }");
            sb.AppendLine("        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: var(--bg); color: var(--text); line-height: 1.5; margin: 0; padding: 2rem; }");
            sb.AppendLine("        .container { max-width: 1000px; margin: 0 auto; }");
            sb.AppendLine("        .card { background: var(--surface); border: 1px solid var(--border); border-radius: 8px; padding: 1.5rem; margin-bottom: 1.5rem; box-shadow: 0 1px 3px rgba(0,0,0,0.05); }");
            sb.AppendLine("        .header-grid { display: grid; grid-template-columns: 2fr 1fr; gap: 1rem; align-items: center; border-bottom: 2px solid var(--border); padding-bottom: 1rem; margin-bottom: 1.5rem; }");
            sb.AppendLine("        h1, h2, h3 { margin: 0 0 0.5rem 0; font-weight: 600; }");
            sb.AppendLine("        h1 { font-size: 1.75rem; color: #0f172a; }");
            sb.AppendLine("        h2 { font-size: 1.25rem; margin-top: 2rem; padding-bottom: 0.5rem; border-bottom: 1px solid var(--border); }");
            sb.AppendLine("        .meta-text { font-family: monospace; font-size: 0.9rem; color: var(--text-muted); }");
            sb.AppendLine("        .badge { display: inline-block; padding: 0.25rem 0.75rem; border-radius: 9999px; font-size: 0.85rem; font-weight: 700; text-transform: uppercase; }");
            sb.AppendLine("        .badge-critical { background: var(--critical-bg); color: var(--critical); }");
            sb.AppendLine("        .badge-warning { background: var(--warning-bg); color: var(--warning); }");
            sb.AppendLine("        .badge-info { background: var(--info-bg); color: var(--info); }");
            sb.AppendLine("        .badge-good { background: var(--good-bg); color: var(--good); }");
            sb.AppendLine("        .metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(140px, 1fr)); gap: 1rem; margin-bottom: 2rem; }");
            sb.AppendLine("        .metric-card { background: var(--surface); border: 1px solid var(--border); border-radius: 6px; padding: 1rem; text-align: center; }");
            sb.AppendLine("        .metric-val { font-size: 1.75rem; font-weight: 700; margin-bottom: 0.25rem; }");
            sb.AppendLine("        .metric-lbl { font-size: 0.75rem; text-transform: uppercase; color: var(--text-muted); font-weight: 600; }");
            sb.AppendLine("        .finding { padding: 1rem; border-left: 4px solid var(--border); margin-bottom: 1rem; background: var(--surface); border-radius: 0 6px 6px 0; border: 1px solid var(--border); border-left-width: 4px; }");
            sb.AppendLine("        .finding-CRITICAL { border-left-color: var(--critical); }");
            sb.AppendLine("        .finding-WARNING { border-left-color: var(--warning); }");
            sb.AppendLine("        .finding-INFO { border-left-color: var(--info); }");
            sb.AppendLine("        .finding-GOOD { border-left-color: var(--good); }");
            sb.AppendLine("        .finding-meta { display: flex; gap: 1rem; font-size: 0.85rem; color: var(--text-muted); margin: 0.5rem 0; font-family: monospace; }");
            sb.AppendLine("        .action-box { background: #f8fafc; border: 1px dashed #cbd5e1; padding: 0.75rem; border-radius: 4px; margin-top: 0.75rem; font-size: 0.9rem; }");
            sb.AppendLine("        .action-box strong { color: #334155; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"container\">");

            // 2. Dashboard Header Card
            sb.AppendLine("    <div class=\"card\">");
            sb.AppendLine("        <div class=\"header-grid\">");
            sb.AppendLine("            <div>");
            sb.AppendLine("                <h1>Schema Diagnostic Report</h1>");
            string targetName = Table != null ? (Table.TablePath ?? Table.Name) : "Global Context";
            sb.AppendLine($"                <div class=\"meta-text\">Target Object: <strong>{WebUtility.HtmlEncode(targetName)}</strong></div>");
            sb.AppendLine("            </div>");
            sb.AppendLine("            <div style=\"text-align: right;\">");
            sb.AppendLine($"                <span class=\"badge badge-{OverallSeverity.ToString().ToLowerInvariant()}\">Health: {OverallSeverity}</span>");
            sb.AppendLine("            </div>");
            sb.AppendLine("        </div>");

            // 3. Mini Summary Metrics Widgets
            sb.AppendLine("        <div class=\"metrics-grid\">");
            sb.AppendLine($"            <div class=\"metric-card\"><div class=\"metric-val\" style=\"color: var(--text);\">{RiskScore}</div><div class=\"metric-lbl\">Risk Score</div></div>");
            sb.AppendLine($"            <div class=\"metric-card\"><div class=\"metric-val\" style=\"color: var(--critical);\">{CriticalCount}</div><div class=\"metric-lbl\">Critical</div></div>");
            sb.AppendLine($"            <div class=\"metric-card\"><div class=\"metric-val\" style=\"color: var(--warning);\">{WarningCount}</div><div class=\"metric-lbl\">Warning</div></div>");
            sb.AppendLine($"            <div class=\"metric-card\"><div class=\"metric-val\" style=\"color: var(--info);\">{InfoCount}</div><div class=\"metric-lbl\">Info</div></div>");
            sb.AppendLine("        </div>");
            sb.AppendLine("    </div>");

            // 4. Clean Findings Rendering Loop
            if (Findings.Count == 0)
            {
                sb.AppendLine("    <div class=\"card\" style=\"text-align: center; color: var(--good);\">");
                sb.AppendLine("        <h3>✓ No diagnostic issues found. Schema looks exceptionally clean!</h3>");
                sb.AppendLine("    </div>");
            }
            else
            {
                var sortedGroups = Findings
                    .GroupBy(f => f.Severity)
                    .OrderByDescending(g => (int)g.Key);

                foreach (var group in sortedGroups)
                {
                    sb.AppendLine($"    <h2>{group.Key.ToString().ToUpperInvariant()} ISSUES ({group.Count()})</h2>");

                    foreach (var finding in group.OrderByDescending(f => f.Score))
                    {
                        sb.AppendLine($"    <div class=\"finding finding-{finding.Severity.ToString().ToUpperInvariant()}\">");

                        string issueTypeTag = finding.IssueType.HasValue ? $" <small style='color:var(--text-muted); font-weight:normal;'>({finding.IssueType})</small>" : "";
                        sb.AppendLine($"        <h3 style=\"margin-bottom: 0.25rem;\">[{WebUtility.HtmlEncode(finding.Category)}]{issueTypeTag} {WebUtility.HtmlEncode(finding.Title)}</h3>");
                        sb.AppendLine($"        <p style=\"margin: 0.5rem 0; color: #334155;\">{WebUtility.HtmlEncode(finding.Description)}</p>");

                        // Metadata Details Bar
                        if (!string.IsNullOrWhiteSpace(finding.ColumnName) ||
                            !string.IsNullOrWhiteSpace(finding.ForeignKeyName) ||
                            !string.IsNullOrWhiteSpace(finding.IndexName))
                        {
                            sb.AppendLine("        <div class=\"finding-meta\">");
                            if (!string.IsNullOrWhiteSpace(finding.ColumnName))
                                sb.AppendLine($"            <div><strong>Col:</strong> {WebUtility.HtmlEncode(finding.ColumnName)}</div>");
                            if (!string.IsNullOrWhiteSpace(finding.ForeignKeyName))
                                sb.AppendLine($"            <div><strong>FK:</strong> {WebUtility.HtmlEncode(finding.ForeignKeyName)}</div>");
                            if (!string.IsNullOrWhiteSpace(finding.IndexName))
                                sb.AppendLine($"            <div><strong>Index:</strong> {WebUtility.HtmlEncode(finding.IndexName)}</div>");
                            sb.AppendLine("        </div>");
                        }

                        // Remediation Block
                        if (!string.IsNullOrWhiteSpace(finding.Recommendation))
                        {
                            sb.AppendLine("        <div class=\"action-box\">");
                            sb.AppendLine($"            <strong>Recommendation:</strong> {WebUtility.HtmlEncode(finding.Recommendation)}");
                            sb.AppendLine("        </div>");
                        }

                        sb.AppendLine("    </div>");
                    }
                }
            }

            // 5. Document Close
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
