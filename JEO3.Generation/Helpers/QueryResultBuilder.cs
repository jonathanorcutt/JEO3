using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using JEO3.Core;
using JEO3.Core.Extensions;
using JEO3.Generation.Formatting;
using JEO3.Generation.Models;
using JEO3.Generation.Models.Extensions;
using JEO3.Schema;

namespace JEO3.Generation
{
    internal static class QueryResultBuilder
    {

        #region Builder

        /// <summary>
        ///  Compiles SQL comments that provide insights into the query generation process, including metadata about the graph structure, cardinality estimations, and 
        ///  any relevant warnings or informational messages. The content and verbosity of the comments should be tailored based on the specified QueryGenerationOptions, 
        ///  allowing for different levels of detail to be included in the generated SQL query for debugging or informational purposes.
        /// </summary>
        /// <param name="root">The root node of the graph structure.</param>
        /// <param name="metrics">The metrics related to the query generation process.</param>
        /// <param name="options">The options for query generation.</param>
        /// <returns>A string containing the compiled SQL comments.</returns>
        internal static QueryGenerationResult GenerateQueryResult(IQueryTracker tracker, QueryGenerationOptions _options, GenerationMethod method)
        {
            var sb = new StringBuilder();

            //tracker.ReclaimAliases();
            sb.AppendLine("---");
            sb.AppendLine(new string('-', FMT.CategoryPrefixLength) + $" TABLE: {tracker.Root.Table.Name}");
            sb.AppendLine("---");

            // Calculate Metrics
            var metrics = AnalyzeGraphMetrics(tracker.Root.Table, tracker.Nodes, _options);

            // Prepend Info
            sb.Append(CompileGenerationInfo(tracker.Root.Table, _options));

            // Append Graph Analytical Metadata
            sb.Append(CompileGraphMetadata(tracker.Nodes, metrics, _options));

            // Append Graph Traversal Metadata
            sb.Append(CompileTraversalMetadata(metrics, _options));

            // Append Graph Cardinality Estimates
            sb.Append(CompileCardinalityEstimation(tracker.Root.Table, tracker.Nodes, _options));

            // Append SELECT
            sb.Append(CompileSelectClauses(tracker.Nodes, _options, true));

            // Append FROM TABLE
            sb.AppendLine($"FROM {tracker.Root.Table.SchemaName.Sql()}.{tracker.Root.Table.Name.Sql()} {tracker.Root.Alias.Sql()}");

            // If Auto-Generate
            if (_options.EnableGraphGeneration == true)
            {
                // Append JOINS
                sb.Append(CompileJoinClauses(tracker.Nodes, _options));
            }

            // Query
            var query = sb.ToString();

            if (_options.IncludeCTEs)
            {
                query += Environment.NewLine + Environment.NewLine + CompileCteQuery(tracker.Nodes, _options);
            }

            // Smooth
            query = query.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

            // 2. Instantiate with the fixed string token, the text query, and include the tracking flight recorder
            var result = new QueryGenerationResult(
                query,             // The final string builder output
                metrics,           // The analyzed structural metrics
                tracker.Nodes,     // The final read-only flattened execution nodes
                tracker,
                method          // The live execution tracker scratchpad for debugging
            );

            return result;
        }

        #endregion

        #region Metrics

        /// <summary>
        /// Analyzes the graph structure to compute metrics related to the query generation process, such as the number of joins, duplicate occurrences of tables, and any relevant details about these occurrences.
        /// </summary>
        /// <param name="root">The root node of the graph structure.</param>
        /// <returns>The query generation result metrics.</returns>
        private static QueryGenerationResultMetric AnalyzeGraphMetrics(ITable root, List<QueryTable> _nodes, QueryGenerationOptions _options)
        {
            // Calculate Joins
            var joinCount = _nodes.Count(x => x.Parent != null);

            // Clear out any old tracking states
            var duplicateDetails = new List<string>();

            // Identify Multiple Occurrence Tables strictly as a data operation
            var duplicates = _nodes.GroupBy(x => x.Table.TablePath)
                                   .Where(g => g.Count() > 1)
                                   .Select(g => new { TableName = g.Key, Count = g.Count() })
                                   .ToList();

            var duplicateCount = duplicates.Count;

            foreach (var dup in duplicates)
            {
                var aliases = _nodes.Where(x => x.Table.TablePath == dup.TableName).Select(x => x.Alias);
                var msg = $"--   -> {dup.TableName} occurs {dup.Count} times as aliases: {string.Join(", ", aliases)}";

                // This is now GUARANTEED to populate every single execution run
                duplicateDetails.Add(msg);
            }

            // Debug
            var msgError = $"{typeof(QueryResultBuilder)}.{nameof(AnalyzeGraphMetrics)} - Generation logic is flawed and we're not accurately tracking duplicates, which could lead to incorrect query generation and potential data integrity issues";
            Trace.Assert(duplicateDetails.Count == duplicateCount, msgError);

            var depths = _nodes.Select(x => x.Depth).ToList();
            int maxDepth = depths.Any() ? depths.Max() : 0;
            double avgDepth = depths.Any() ? depths.Average() : 0;
            bool hitUpLimit = _nodes.Any(x => x.Direction == TraversalDirection.Up && x.Depth >= _options.Traversal.LevelsUp);
            bool hitDownLimit = _nodes.Any(x => x.Direction == TraversalDirection.Down && x.Depth >= _options.Traversal.LevelsDown);
            bool hitDepthLimit = hitUpLimit || hitDownLimit;

            return new QueryGenerationResultMetric(_nodes.Count, maxDepth, avgDepth, hitDepthLimit, joinCount, duplicateDetails);
        }

        #endregion

        #region Sql Building

        /// <summary>
        /// Compiles the SELECT clauses for the SQL query based on the discovered nodes and the specified QueryGenerationOptions. This method should utilize a helper class, such as ColumnStringBuilder,
        /// </summary>
        /// <param name="options">The options for query generation.</param>
        /// <returns>A string containing the compiled SELECT clauses.</returns>
        private static string CompileSelectClauses(List<QueryTable> _nodes, QueryGenerationOptions _options, bool ascii = false)
        {
            var sql = new StringBuilder();
            if (!_nodes.Any()) { return string.Empty; }

            // Render Header/ASCII Metadata
            if (_options.Formatting.AdvancedAnnotations.HasFlag(AnnotationVerbosity.DoNotRemoveAscii) == false)
            {
                sql.AppendLine(SQLConstants.SelectTop1000.PadRight(FMT.DividerLength - 2, ' ') + "--");
            }
            else if (ascii)
            {
                string hr = new string('-', FMT.DividerLength);

                var x = new Random(Environment.TickCount).Next(0, Ascii.Payloads.Count - 1);
                if (x == 0)
                {
                    var art = A5C11.GetAsciiArt("DO NOT REMOVE");
                    var parts = art.Split("\n").ToList();
                    var line = parts.Select(v => v.TrimEnd().Replace("\r", "")).Where(v => v != null && v.Length > 0).Select(v => (new string('-', 2) + "    " + v).PadRight(FMT.DividerLength - 2, ' ').PadRight(FMT.DividerLength - 2, '-') + new string('-', 2)).ToList();

                    sql.AppendLine(string.Join("\n", line));
                }
                else
                {
                    sql.AppendLine(string.Join("\r\n", Ascii.Payloads[x].Split("\r\n").Select(v => "--".PadRight(5, ' ') + v)));
                }
                // sql.AppendLine("                                                                                                                                                                                               \r\n          --::--                     =======                                                                                              /############%\\                                      \r\n      **************            ===============             *************             *++++++++++++            %%%%%%%%%%%%%             /###############%\\                                    \r\n    ****#%%*******#%%%-       =====%%%%======%%%%-       ++***#*#**********+        ++++++=+++++++==          %%%%%%%%%%%%%%%%+        /##################%\\                                   \r\n   ***%%%%%%%***%%%%%%%       ===%%%%%%%===%%%%%%%#      ***%%%%%%%**%%%%%%%       *++%%%%%%#++%%%%%%%      +%%%%%%%%%%%%%%%%%%       /#################%%%|                                   \r\n   ***%%%%===***%%%%---       ===%%%%**+===%%%%====      ***%%%*--***%%%%+++       +++%%%---*++%%%%---      #%%%%%%---%%%%%%---=     /##########/              /##\\   /##\\   /##\\   /##\\       \r\n  *****%%%%%*****%%%%%-*    =======%%%%======%%%%===   *******%%%******%%%%**    =++++##%%%++++##%%%%+*    %%%%%%%%%%%%%%%%%%%%%%    |##########\\__________    \\__/   \\__/   \\__/   \\__/       \r\n  **********************-   ========================   **********************+   =+++++++++++++++++++++    %%%%%%%%%%%%%%%%%%%%%%     \\####################|                                   \r\n  **********************-   ========================   **********************+   =+++++++++++++++++++++    %%%%%%%%%%%%%%%%%%%%%%      \\##################/                                    \r\n  **********************+   ========================   **********************+   =+++++++++++++++++++++    %%%%%%%%%%%%%%%%%%%%%%       \\################/                                     \r\n  *** ******= +***** ****   ==== ======   =====  ===   **** ****** *****% ****   ++++ *++++   ++++= *++    %%% %%%%%%  +%%%%  %%%        \\##############/                                      \r\n  **   ****    ***    **    -=     ===-   -==    -=-    **   ***    ***     **   ++     +++   +++    ++   ++    *##*  +*#+    +*           \\###########/                                        ");
                sql.AppendLine(hr);
                sql.AppendLine(SQLConstants.SelectTop1000.PadRight(FMT.DividerLength - 2, ' ') + "--");
                sql.AppendLine(hr);
            }

            // Filter nodes based on target selection strategy to accurately calculate final comma placement
            var outputNodes = _nodes.Where(node =>
            {
                bool isRoot = node.Parent == null;
                var policy = isRoot ? _options.Retrieval.RootSelectPolicy : _options.Retrieval.JoinSelectPolicy;
                return policy != TableSelectPolicy.None;
            }).ToList();

            // Project each valid table node linearly
            for (int i = 0; i < outputNodes.Count; i++)
            {
                bool isLastNode = i == outputNodes.Count - 1;
                string selectSegment = QueryStringBuilder.BuildProjectionClause(outputNodes[i], _options, isLastNode);

                if (!string.IsNullOrWhiteSpace(selectSegment))
                {
                    sql.Append(selectSegment);
                }
            }

            return sql.ToString();
        }

        /// <summary>
        /// Compiles the JOIN clauses for the SQL query based on the discovered nodes and the specified QueryGenerationOptions. This method should utilize a helper class, such as QueryStringBuilder,
        /// </summary>
        /// <returns></returns>
        private static string CompileJoinClauses(List<QueryTable> _nodes, QueryGenerationOptions _options)
        {
            var sql = new StringBuilder();
            if (_nodes == null || _nodes.Count == 0) { return string.Empty; }

            // Loop Tables
            foreach (var node in _nodes.Where(x => x.Parent != null))
            {
                bool down = node.Relationship!.ReferencedTable == node.Table;
                string joinType = node.Relationship!.IsNullable || _options.Retrieval.LeftJoinsOnly ? "LEFT JOIN" : "INNER JOIN";

                // Build Join Clause
                var sqlJoin = QueryStringBuilder.BuildJoinClause(node, down, joinType);
                sql.AppendLine(sqlJoin);
            }

            return sql.ToString();
        }

        /// <summary>
        /// Generate SQL CTEs for each logical branch of the graph, allowing for modular query construction and improved readability. Each CTE will encapsulate a subset of the graph's nodes,
        /// </summary>
        /// <param name="_nodes"></param>
        /// <param name="_options"></param>
        /// <returns></returns>
        public static string CompileCteQuery(List<QueryTable> _nodes, QueryGenerationOptions _options)
        {
            try
            {
                var sql = new StringBuilder();
                if (!_nodes.Any()) { return string.Empty; }

                // Group nodes into logical branches (e.g., by their closest major hub)
                var cteBranches = PartitionIntoBranches(_nodes);

                sql.AppendLine("WITH ");
                var cteBlocks = new List<string>();

                foreach (var branch in cteBranches)
                {
                    // Identify the local root of this CTE branch (the node closest to the main query root)
                    var localRoot = branch.OrderBy(n => n.Depth).First();

                    // Create a scoped copy of the branch nodes where we temporarily treat 
                    // the local root as having no parent *within this CTE's compilation context*.
                    var scopedNodes = branch.Select(n => n == localRoot
                        ? n.CloneAsLocalRoot()
                        : n
                    ).ToList();

                    var cteSb = new StringBuilder();
                    cteSb.AppendLine($"  -- CTE Module for {localRoot.Table.SchemaName}.{localRoot.Table.Name} branch ({localRoot.Alias})");
                    cteSb.AppendLine($"  CTE_{localRoot.Alias} AS (" + "    " + "SELECT");

                    // REUSE: Compile the SELECT for this CTE module's columns
                    string selectClause = CompileSelectClauses(scopedNodes, _options);
                    cteSb.AppendLine(QueryStringBuilder.IndentLines(selectClause, 4));

                    // The anchor FROM for this local CTE
                    cteSb.AppendLine($"    FROM {localRoot.Table.SchemaName}.{localRoot.Table.Name} {localRoot.Alias}");

                    // REUSE: Compile the JOINs for this CTE module's downstream children
                    string joinClauses = CompileJoinClauses(scopedNodes, _options);
                    if (!string.IsNullOrWhiteSpace(joinClauses))
                    {
                        cteSb.Append(QueryStringBuilder.IndentLines(joinClauses, 4));
                    }

                    cteSb.AppendLine("  )");
                    cteBlocks.Add(cteSb.ToString());
                }

                // Join all compiled CTE declarations with commas
                sql.AppendLine(string.Join(",\n\n", cteBlocks));

                // 2. Final Orchestration Select
                var mainRoot = _nodes.First(n => n.Parent == null);
                sql.AppendLine("SELECT ");
                sql.AppendLine($"  {mainRoot.Alias}.*");

                // Explicitly alias the module selections to mitigate runtime dataset collisions
                foreach (var branch in cteBranches)
                {
                    var localRoot = branch.OrderBy(n => n.Depth).First();
                    sql.AppendLine($"  , CTE_{localRoot.Alias}.*");
                }

                sql.AppendLine($"FROM {mainRoot.Table.SchemaName}.{mainRoot.Table.Name} {mainRoot.Alias}");

                // Connect the high-level orchestrated CTE modules back to the main query structure
                foreach (var branch in cteBranches)
                {
                    var localRoot = branch.OrderBy(n => n.Depth).First();

                    string joinKeyword = (localRoot.RequiresLeftJoin || _options.Retrieval.LeftJoinsOnly)
                        ? "LEFT JOIN"
                        : "INNER JOIN";

                    var parentAlias = localRoot.Parent?.Alias ?? mainRoot.Alias;
                    var joinConditions = new List<string>();

                    if (localRoot.Relationship?.ColumnPairs != null && localRoot.Relationship.ColumnPairs.Any())
                    {
                        foreach (var col in localRoot.Relationship.ColumnPairs)
                        {
                            string cteColName;
                            string parentColName;

                            // Explicitly resolve column owners by checking physical table names
                            if (localRoot.Relationship.ParentTable.Name == localRoot.Table.Name)
                            {
                                // Local root table holds the primary key (e.g., SalesOrder -> SalesOrderId)
                                cteColName = col.ParentColumn?.Name;
                                parentColName = col.ReferencedColumn?.Name;
                            }
                            else if (localRoot.Relationship.ReferencedTable.Name == localRoot.Table.Name)
                            {
                                // Local root table holds the foreign key (e.g., Shipment -> FK_034_SalesOrderLineId)
                                cteColName = col.ReferencedColumn?.Name;
                                parentColName = col.ParentColumn?.Name;
                            }
                            else
                            {
                                // Fallback context: handle direction flag heuristics if table names match an abstract schema bridge
                                bool isDownwards = localRoot.Direction == TraversalDirection.Down;
                                cteColName = isDownwards ? col.ReferencedColumn.Name : col.ParentColumn.Name;
                                parentColName = isDownwards ? col.ParentColumn.Name : col.ReferencedColumn.Name;
                            }

                            joinConditions.Add($"CTE_{localRoot.Alias}.{cteColName} = {parentAlias}.{parentColName}");
                        }
                    }
                    else
                    {
                        joinConditions.Add($"CTE_{localRoot.Alias}.id = {parentAlias}.id");
                    }

                    string joinClause = string.Join(" AND ", joinConditions);
                    sql.AppendLine($"  {joinKeyword} CTE_{localRoot.Alias} ON {joinClause}");
                }

                return sql.ToString();
            }
            catch (Exception ex)
            {
                return "-- EXCEPTION : " + ex.ToString();
            }
        }

        /// <summary>
        /// Partitions the list of QueryTable nodes into logical branches based on their relationships and hierarchy. Each branch represents a subset of the graph that can be treated as a separate Common Table Expression (CTE) in the SQL query.
        /// </summary>
        /// <param name="_nodes"></param>
        /// <returns></returns>
        private static List<List<QueryTable>> PartitionIntoBranches(List<QueryTable> _nodes)
        {
            var branches = new List<List<QueryTable>>();
            var root = _nodes.FirstOrDefault(n => n.Parent == null);
            if (root == null) return branches;

            // Identify the "Hub" nodes. Direct children of the root are great natural boundaries.
            var hubs = _nodes.Where(n => n.Parent == root || n.IsLogicalHub(_nodes)).ToList();

            var visited = new HashSet<QueryTable> { root }; // Ensure the root stays out of individual CTE scopes

            foreach (var hub in hubs)
            {
                var branchNodes = new List<QueryTable>();
                hub.GatherDescendants(_nodes, branchNodes, visited);

                if (branchNodes.Any())
                {
                    branches.Add(branchNodes);
                }
            }

            return branches;
        }

        #endregion

        #region Annotation

        /// <summary>
        /// Compiles SQL comments related to cardinality estimation for the query. This method should utilize a hierarchy cardinality estimator to compute estimates for the size 
        /// of the result set based on the structure of the graph and the specified estimation options. The compiled comments should provide insights into the estimated number 
        /// of rows that will be returned by the query, as well as any relevant details about the estimation process or assumptions made. The verbosity of the comments should 
        /// be controlled by the QueryGenerationOptions, allowing for more detailed diagnostic information to be included when appropriate.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static string CompileCardinalityEstimation(ITable root, List<QueryTable> _nodes, QueryGenerationOptions _options)
        {
            var sql = new StringBuilder();

            // Hardcoded Limit For Demonstration Purposes - Only show estimation comments when verbosity is set to Diagnostic
            if (_options.Formatting.AdvancedAnnotations.HasFlag(AnnotationVerbosity.CardinalityEstimation) == false)
            {
                return string.Empty;
            }

            // Prepare statistics (Usually loaded from a database system catalog cache)
            var catalog = new TableStatisticsCatalog();
            catalog.Tables[root.TablePath] = new TableStatistics { TotalRowCount = 100000 };

            // Evaluate the 3 variants
            var configurations = Enum.GetValues<RootTraversalMethod>();
            var estimator = new CardinalityEstimator(catalog);

            var indentPrefix = $"--".PadRight(FMT.IndentLength1, ' ');

            sql.AppendLine((new string('-', FMT.CategoryPrefixLength) + " CARDINALITY ESTIMATION COMPILATION VARIANTS ").PadRight(FMT.DividerLength, ' '));
            foreach (var method in configurations)
            {
                var optionsMock = new EstimationOptions(method, CustomFilterSelectivity: 0.05);
                var runtimeEstimates = estimator.ComputeEstimates(_nodes, optionsMock);

                // Find the terminal node leaf depth maximums or output estimates
                double terminalEstimate = runtimeEstimates.Values.LastOrDefault();
                sql.AppendLine(indentPrefix + $"Variant Traversal [ {method,-22}]: Estimated Query Node Set Size = {terminalEstimate:F2} rows");
            }
            sql.AppendLine("--");

            return sql.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="metrics"></param>
        /// <returns></returns>
        private static string CompileTraversalMetadata(QueryGenerationResultMetric metrics, QueryGenerationOptions _options)
        {
            // If the user doesn't want annotations, bail immediately BEFORE wasting string allocation.
            // The data metrics are already completely safe because they were calculated in Step 1.
            if (!_options.Formatting.AdvancedAnnotations.HasFlag(AnnotationVerbosity.TraversalSummary))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var indentPrefix1 = $"--".PadRight(FMT.IndentLength1, ' ');

            sb.AppendLine(new string('-', FMT.CategoryPrefixLength) + " TRAVERSAL SUMMARY");
            sb.AppendLine(indentPrefix1 + $"Nodes Visited: {metrics.NodesVisited}");
            sb.AppendLine(indentPrefix1 + $"Maximum Depth: {metrics.MaxTraversalDepth}");
            sb.AppendLine(indentPrefix1 + $"Average Depth: {Math.Round(metrics.AverageTraversalDepth, 4)}");
            sb.AppendLine(indentPrefix1 + $"Hit Depth Limit: {(metrics.HitDepthLimit ? "YES" : "NO")}");

            // ... Path Lineage Breadcrumbs print code here ...

            sb.AppendLine("---");
            return sb.ToString();
        }

        /// <summary>
        /// Compiles SQL comments that provide metadata about the graph structure and any relevant warnings or informational messages related to the query generation process.
        /// </summary>
        /// <param name="root">The root node of the graph structure.</param>
        /// <param name="metrics">The metrics related to the query generation process.</param>
        /// <param name="options">The options for query generation.</param>
        /// <returns>A string containing the compiled SQL comments.</returns>
        private static string CompileGraphMetadata(List<QueryTable> _nodes, QueryGenerationResultMetric metrics, QueryGenerationOptions _options)
        {
            // If the user doesn't want annotations, bail immediately BEFORE wasting string allocation.
            // The data metrics are already completely safe because they were calculated in Step 1.
            if (!_options.Formatting.AdvancedAnnotations.HasFlag(AnnotationVerbosity.ExecutionSummary))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var indentPrefix1 = $"--".PadRight(FMT.IndentLength1, ' ');

            sb.AppendLine(new string('-', FMT.CategoryPrefixLength) + " GRAPH EXECUTION SUMMARY");
            sb.AppendLine(indentPrefix1 + $"Unique Tables in Query: {_nodes.Select(v => v.Table.TablePath).Distinct().Count()}");
            sb.AppendLine(indentPrefix1 + $"Total Tables Instances: {_nodes.Count} ({metrics.JoinCount} Joins computed)");

            if (metrics.DuplicateCount > 0)
            {
                sb.AppendLine("--");
                sb.AppendLine("-- [Warning] Multiple Occurrence Tables Detected:");
                foreach (var msg in metrics.DuplicateDetails)
                {
                    sb.AppendLine(msg); // Just print the strings we already cached
                }
            }

            // ... Path Lineage Breadcrumbs print code here ...

            sb.AppendLine("---");
            return sb.ToString();
        }

        /// <summary>
        /// Compiles SQL comments that provide insights into the query generation method used, including details about the traversal strategy, root table, levels of recursion, 
        /// and any relevant information about how the query was constructed. This method should format the comments in a clear and informative manner, allowing for easy interpretation 
        /// by developers or database administrators who may be reviewing the generated SQL query. The verbosity of the comments should be controlled by the QueryGenerationOptions, 
        /// allowing for different levels of detail to be included based on the needs of the user.
        /// </summary>
        /// <param name="root">The root node of the graph structure.</param>
        /// <param name="options">The options for query generation.</param>
        /// <returns>A string containing the compiled SQL comments.</returns>
        private static string CompileGenerationInfo(ITable root, QueryGenerationOptions _options)
        {
            if (_options.Formatting.AdvancedAnnotations.HasFlag(AnnotationVerbosity.GenerationInfo) == false) { return string.Empty; }

            var sql = new StringBuilder();

            var methodType = string.Empty;
            var methodInfo = string.Empty;

            switch (_options.Strategy)
            {
                case GenerationMethod.FullGraph:
                    methodType = "Full Graph Traversal";
                    methodInfo = $"Traversing from the root, Up and down independently from every discovered table (true graph traversal)";
                    break;
                case GenerationMethod.RootSplit:
                    methodType = "Root Split Traversal";
                    methodInfo = $"Traversing from the root, Only recurse downward from the root and recurse upward from the root separately";
                    break;
                case GenerationMethod.UpThenDown:
                    methodType = "Up Then Down Traversal";
                    methodInfo = $"Traversing from the root, Up traversal first, then down traversal from every discovered node";
                    break;
                case GenerationMethod.PlanDrivenExecution:
                    methodType = "Constrained Recursive Graph Normalization";
                    methodInfo = $"Ancestral State Tracking: Prevents infinite loops by forbidding re-entry into current lineage branches.\r\n--    Terminal Leaf Collapse: Eliminates visual noise by identifying duplicate schema types at the furthest descent.\r\n";
                    break;
                case GenerationMethod.ExtendedDiscovery:
                    methodType = "Extended Discovery";
                    methodInfo = $"I forget";
                    break;
                case GenerationMethod.Backtracking:
                    methodType = "Backtracking";
                    methodInfo = $"I forget";
                    break;
                case GenerationMethod.JEO3:
                    methodType = "jon forgot";
                    methodInfo = $"I forget";
                    break;
                default:
                    methodType = "Automatic Optimization Generation";
                    methodInfo = $"Generates queries for each of the three traversal methods and the optimizer will choose the winner and automatically update the relevant query generation options";
                    break;
            }

            var indentPrefix = $"--".PadRight(FMT.IndentLength1, ' ');
            sql.AppendLine(new string('-', FMT.DividerLength));
            sql.AppendLine((new string('-', FMT.CategoryPrefixLength) + $" GENERATION METHOD: {methodType}").PadRight(FMT.DividerLength, ' '));
            sql.AppendLine(indentPrefix + $"{methodInfo}");
            sql.AppendLine(indentPrefix + $"Root Table: {root.TablePath}");
            sql.AppendLine((indentPrefix + $"Levels Up: {_options.Traversal.LevelsUp}").PadRight(FMT.DividerHalfLength, ' ') + $"Down: {_options.Traversal.LevelsDown}");
            sql.AppendLine((indentPrefix + $"Join Table Select: {_options.Retrieval.JoinSelectPolicy}").PadRight(FMT.DividerHalfLength, ' ') + $"Single Line Per Table: {_options.Formatting.SingleLinePerTable}");
            sql.AppendLine("--");

            return sql.ToString();
        }

        #endregion
    }
}
