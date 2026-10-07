using System.Collections.Concurrent;
using JEO3.Core;
using JEO3.Generation;
using JEO3.Generation.Models;
using JEO3.Schema;
using JEO3.Site.Dashboard;

namespace JEO3.Site.Extensions
{
    internal static class GeneralExtensions
    {
        internal static QueryGenerationOptions Clone(this QueryGenerationOptions options, GenerationMethod newMethod)
        {
            var newOptions = new QueryGenerationOptions(newMethod, options.EnableGraphGeneration, options.IncludeCTEs, options.RoutingMode, new ProjectionSettings(options.Retrieval.RootSelectPolicy, options.Retrieval.JoinSelectPolicy, options.Retrieval.LeftJoinsOnly, options.Retrieval.ColumnsPerTableLimit),
                new FormattingSettings(options.Formatting.AdvancedAnnotations, options.Formatting.SingleLinePerTable, options.Formatting.BreadcrumbPaddingRight),
                new TraversalSettings(options.Traversal.LevelsUp, options.Traversal.LevelsDown, options.Traversal.StopOnCycles, options.Traversal.IgnoreNullableForeignKeys, options.Traversal.IgnoreTablesWithZeroRows, options.Traversal.PreventRootTypeRecursion, options.Traversal.StrictIndexMatchingOnly, options.Traversal.BiasSmallerTableScans, options.Traversal.OmitFromSelectsList, options.Traversal.RelationConstraints));

            return newOptions;
        }
        internal static QueryGenerationOptions ToGenerationOptions(this WSDiagramSettingsState settings)
            => settings.ToGenerationOptions(new List<GraphRelationConstraint>(), GenerationMethod.PlanDrivenExecution);
        internal static QueryGenerationOptions ToGenerationOptions(this WSDiagramSettingsState settings, List<GraphRelationConstraint> activeConstraints, GenerationMethod method = GenerationMethod.PlanDrivenExecution)
        {
            var options = new QueryGenerationOptions(
                strategy: method,
             enableGraphGeneration: settings.FlagAutoGenerateQueries,
             routingMode: ExecutionRoutingMode.EnforceSelectedUiSettings, // Every engine call is direct now
             retrieval: new ProjectionSettings(settings.RootSelectPolicy, settings.JoinSelectPolicy, settings.LeftJoinsOnly, settings.ColumnsPerTableLimit),
             formatting: new FormattingSettings(settings.AdvancedAnnotations, settings.PutTableSelectsSingleLinePerTable, settings.BreadcrumbPaddingRight),
             traversal: new TraversalSettings(
                 levelsUp: settings.LevelsUp,
                 levelsDown: settings.LevelsDown,
                 stopOnCycles: settings.StopOnCycles,
                 ignoreNullableForeignKeys: settings.IgnoreNullableForeignKeys,
                 ignoreTablesWithZeroRows: settings.IgnoreTablesWithZeroRows,
                 preventRootTypeRecursion: settings.PreventRootTypeRecursion,
                 strictIndexMatchingOnly: settings.StrictIndexMatchingOnly,
                 biasSmallerTableScans: settings.BiasSmallerTableScans,
                 enforceStrictCyclePath: settings.EnforceStrictCyclePath,
                 enforceStrictLeafCollapse: settings.EnforceStrictLeafCollapse,
                 enforceStrictDepthLimit: settings.EnforceStrictDepthLimit,
                 omitFromSelectsList: SQLFormattingHelper.GetSqlColumnNameListFromText(settings.ColumnsToOmitFromSelectText),
                 relationConstraints: activeConstraints ?? [] //.Concat(remaining) //.Concat(blindInferredConstraints) INNFERRED RELATIONS INJECTION EXISTS HERE!
                                                              // Commented For Now. Debug Only. Do Not Delete!
             )
         );
            return options;
        }

        // Nearly Dead At This Point. Inferred Isnt Even A Thing Anymore
        internal static List<GraphRelationConstraint> ToGraphRelationConstraintList(this IEnumerable<QueryTable> nodes)
        {
            try
            {
                if (nodes == null || nodes.Count() == 0) { return []; }
                var list = new List<GraphRelationConstraint>();
                var relNodes = nodes.Where(x => x.Parent != null && x.Relationship != null).ToList();
                // Grab every node where a join relationship was actually utilized
                foreach (var node in relNodes)
                {
                    try
                    {
                        var rel = node.Relationship!;
                        if (rel == null || rel.ColumnPairs == null || rel.ColumnPairs.Count == 0)
                        {

                        }
                        else
                        {
                            // Handle composite columns gracefully by joining them into a string
                            string primaryCols = string.Join(", ", rel.ColumnPairs.Where(v => v.ParentColumn != null).Select(c => c.ParentColumn.Name));
                            string foreignCols = string.Join(", ", rel.ColumnPairs.Where(v => v.ReferencedColumn != null).Select(c => c.ReferencedColumn.Name));

                            list.Add(new GraphRelationConstraint()
                            {
                                KeyName = rel.KeyName,
                                IsDisabled = false, // It ran successfully, so it defaults to NOT disabled (Enabled)
                                PrimaryTable = rel.ParentTable.TablePath,
                                PrimaryColumns = primaryCols,
                                ForeignTable = rel.ReferencedTable.TablePath,
                                ForeignColumns = foreignCols,
                                Signature = rel.KeyName,
                                IsUserDefined = false
                            });
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                return [];
            }
        }
    }
}
