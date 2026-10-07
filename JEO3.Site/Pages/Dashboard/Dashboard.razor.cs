using System.Data;
using System.Diagnostics;
using System.Text;
using JEO3.Code;
using JEO3.Core;
using JEO3.Extensions;
using JEO3.Generation;
using JEO3.Generation.Models;
using JEO3.Logging;
using JEO3.Monitor.ECharts;
using JEO3.Providers;
using JEO3.Schema;
using JEO3.Site.Components;
using JEO3.Site.Extensions;
using JEO3.Site.Infrastructure;
using JEO3.Site.Models;
using JEO3.Site.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace JEO3.Site.Dashboard
{
    public partial class Dashboard : IDisposable
    {
        #region Properties

        [Inject] private IJSRuntime JS { get; set; }
        [Inject] private DialogService DialogService { get; set; }
        [Inject] private WSWorkspace W { get; set; } = new();
        [Inject] private ConnectionService ConnectionService { get; set; }
        [Inject] private ISchemaCacheService SchemaService { get; set; }

        // Splitter Refs - Didnt Force Expansion Correction
        private RadzenSplitter outerSplitter;
        private RadzenSplitter innerSplitter;

        // Components
        private DashboardTopBar dashboardTopBar;
        private QueryEditor queryEditor;
        private DiagramViewerDatabase diagramViewer;
        private DiagramViewerJarvis diagramViewer2;

        // Tab Indexes
        private int selectedEntitiesNavTabIndex = 0;

        // Flags
        private bool _isDisposed { get; set; }
        private bool _rendered { get; set; }

        // Charts
        private List<ITable> treeTables = new();
        [Inject] IMonitorChartDataService DataService { get; set; } = default!;
        private bool _started;
        #endregion

        #region Initialization

        protected override async Task OnInitializedAsync()
        {
            W.OnMainLayoutStateChanged += OnGlobalWorkspaceUpdated;

            try
            {
                // Primary Connection String
                if (System.Configuration.ConfigurationManager.ConnectionStrings.Count == 0) throw new Exception("Exception: No Connection Strings.");
                var conns = ConnectionService.ConnectionSettings.ToDatabaseProviderList();
                var prov = conns.FirstOrDefault();

                // Validation
                if (prov != null)
                {
                    // Load Context :)
                    var context = await SchemaService.GetDatabaseSchemaAsync(prov);
                    W.LoadSchemaContext(context, prov);

                    // Populate tables
                    await PopulateDatabarsGrids();
                }
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
            }
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    var defaultTbl = W.UI.Grids.Core.Tables.FirstOrDefault();
                    // Send Some Data To The Data Display Tab
                    ITable tbl = W.UI.Grids.Core.Tables.FirstOrDefault(v => v.Name.ToLower() == "product") ?? defaultTbl;
                    await SelectTop1000(tbl);

                    // AW Only
                    if (tbl != null)
                    {
                        W.SelectTable(tbl);
                        await InvokeAsync(StateHasChanged);
                    }
                }
                catch (Exception ex)
                {
                    await ExceptionUtility.LogExceptionAsync(ex);
                }
                finally
                {
                    Trace.WriteLine($"DASHBOARD LOADED!");
                    await DataService.EnsureStartedAsync();
                    Trace.WriteLine($"DataService LOADED!");
                    _started = true;

                    _rendered = true;
                    StateHasChanged();
                }
            }
        }

        public void Dispose()
        {
            _bobTimer?.Dispose();
            if (W != null)
            {
                W.OnMainLayoutStateChanged -= OnGlobalWorkspaceUpdated;
            }
        }

        #endregion

        #region Global Events
        private async void OnGlobalWorkspaceUpdated()
        {
            if (_isDisposed) return;

            await SelectedTableChanged(W.SelectedTable);
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Query Editor
        private async Task GenerationSettingChanged()
        {
            await this.SelectedTableChanged(W.SelectedTable);
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Grid Events
        // ************************************************************
        public async Task SelectedTableChanged(ITable selectedTable)
        {
            if (W.UI.Settings == null || selectedTable == null || !W.UI.Settings.FlagAutoGenerateQueries) return;
            W.UI.Appearance.IsShowingPageLoadAsciiArt = W.UI.Appearance.IsShowingPageLoadAsciiArt && W.UI.Appearance.IsMonacoEditorReady ? false : W.UI.Appearance.IsShowingPageLoadAsciiArt;
            W.UI.Appearance.IsQueryBusy = true;
            W.SelectTable(selectedTable);

            try
            {
                if (W.UI.Appearance.IsSchemaLoaded && W.UI.Appearance.IsShowingPageLoadAsciiArt == false)
                {
                    await JS.InvokeVoidAsync("bob.play", "Rock");
                    await JS.InvokeVoidAsync("bob.play", "PanicLeft");
                    await JS.InvokeVoidAsync("bob.play", "GlitchRight");
                    await JS.InvokeVoidAsync("bob.play", "Lift");
                    await JS.InvokeVoidAsync("bob.play", "Fall");
                    await JS.InvokeVoidAsync("bob.play", "FloorIt");
                    await JS.InvokeVoidAsync("bob.play", "PanicLeft");
                    await JS.InvokeVoidAsync("bob.play", "PanicLeft");
                    await JS.InvokeVoidAsync("bob.play", "JumpEscalation");
                    await JS.InvokeVoidAsync("bob.play", "NavigatorClimb");
                }

                // If Selected Table Has Changed, Reset the options so we dont go berserk if this is a highly connected table
                var selectedTableChanged = W.SelectedTable == null || selectedTable != W.SelectedTable;
                if (selectedTableChanged)
                {
                    // W.UI.Grids.ActiveConstraints?.Clear();
                }

                // Run Engine
                var options = W.UI.Settings.ToGenerationOptions(W.UI.Grids.ActiveConstraints);
                //QueryGenerationResult jarvisResult = await GenerationService.GoJarvis(selectedTable, options);

                var gen = new DiagramDrivenGraphGenerator();
                QueryGenerationResult jarvisResult = gen.Generate(selectedTable, options);
                if (jarvisResult == null) return;

                jarvisResult.Coverage = CoverageCalculator.CalculateCoverage(selectedTable, W.Context.Tables, W.Context.Relations);
                W.Result.QueryEditorResult = jarvisResult;
                W.UI.Grids.ActiveConstraints = jarvisResult.ExecutionNodes.ToGraphRelationConstraintList();

                if (W.UI.Appearance.IsMonacoEditorReady && W.UI.Appearance.IsSchemaLoaded)
                {
                    await diagramViewer.ReloadDiagram(selectedTable, options);
                    await diagramViewer2.ReloadDiagram(selectedTable, options);
                    await queryEditor.UpdateEditorQuery(jarvisResult.GeneratedQuery);
                }
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
            }
            finally
            {
                if (W?.UI?.Appearance != null)
                {
                    W.UI.Appearance.IsQueryBusy = false;
                    var nodeCount = W.Result.QueryEditorResult?.ExecutionNodes?.Count;
                    const int limit = 300;
                    if (nodeCount > limit)
                    {
                        await DialogService.Confirm($"You are getting close to the pre-defined traversal limits ({nodeCount} / {limit} nodes). Proceed with caution", "WARNING!");
                    }
                }


                await SelectTop1000(selectedTable);

                // await InvokeAsync(StateHasChanged); // Essential fix: force render chain execution
                W.NotifyDashboardStateChanged();
                StateHasChanged();
            }
        }
        private async Task SelectTop1000(ITable selectedTable)
        {
            try
            {
                if (W.UI.Tabs.Tabs.Any(v => v.TableName == selectedTable.Name)) { return; }
                if (string.IsNullOrWhiteSpace(selectedTable.SchemaName) || string.IsNullOrWhiteSpace(selectedTable.Name)) return;
                var query = $"{SQLConstants.SelectTop1000} X.* FROM [{selectedTable.SchemaName}].[{selectedTable.Name}] X";
                if (W.SelectedTable == null) { return; }

                W.UI.Appearance.IsQueryBusy = true;

                var dt = await W.Context.Provider.GetDataTable(query);
                if (dt == null) { return; }

                var tabId = this.W.UI.Tabs.NextTabId++;
                await CreateNewQueryResultsTab(dt, tabId, selectedTable);
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
            }
            finally
            {
                W.UI.Appearance.IsQueryBusy = false;
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion

        #region Traffic Stress
        private async Task GenerateTraffic()
        {
            if (W.Observation.Traffic.IsRunning) { return; }
            W.Observation.Traffic.IsRunning = true;

            try
            {
                const int upLimit = 3;
                const int downLimit = 3;
                int idx = 0;
                Parallel.ForEach(W.Context.Tables, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, async (tbl) =>
                {
                    idx++;
                    int pctCompleted = (int)(((double)(idx) / (double)(W.Context.Tables.Count)) * 100);
                    W.Observation.Traffic.CurrentTable = tbl.SchemaName + "." + tbl.Name;
                    W.Observation.Traffic.PercentCompleted = pctCompleted + "%";
                    W.NotifyDashboardStateChanged();

                    for (var up = 1; up < upLimit; up++)
                    {
                        for (var down = 1; down < downLimit; down++)
                        {
                            var options = W.UI.Settings.ToGenerationOptions(new List<GraphRelationConstraint>(), GenerationMethod.PlanDrivenExecution);
                            options.Traversal.LevelsUp = up;
                            options.Traversal.LevelsDown = down;
                            options.Retrieval.RootSelectPolicy = TableSelectPolicy.AllColumns;
                            options.Retrieval.JoinSelectPolicy = TableSelectPolicy.AllColumns;
                            options.Traversal.PreventRootTypeRecursion = true;
                            pctCompleted = (int)(((double)(up * down) / (double)(upLimit * downLimit)) * 100);
                            W.Observation.Traffic.PercentCompleted = pctCompleted + "%";
                            await InvokeAsync(StateHasChanged);

                            Parallel.ForEach(tbl.Indexes, new ParallelOptions() { MaxDegreeOfParallelism = 2 }, (index) =>
                            {
                                string script = $"ALTER INDEX {index.Name} ON {tbl.SchemaName}.{tbl.Name} REBUILD WITH (ONLINE = ON);";
                                W.Context.Provider.ExecuteNonQuery(script);
                            });

                            var gen = new PlanDrivenQueryGenerator();
                            var result = gen.Generate(tbl, options);
                            var dt = await W.Context.Provider.GetDataTable(result.GeneratedQuery);
                            if (dt == null)
                            {
                                await Task.Delay(1250);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
            }
            finally
            {
                W.Observation.Traffic.IsRunning = false;
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion

        #region Databars
        private async Task PopulateDatabarsGrids()
        {
            var options = W.UI.Settings.ToGenerationOptions(W.UI.Grids.ActiveConstraints);
            var results = await W.Context.GetAllTablesResult(options, new());

            W.Result.Databars = new DataBarViewModel(W.Context, results.ToList());
            await W.Result.Databars.Populate();

            // Treemap
            treeTables = W.Context.Tables.ToList();
        }
        private async Task OnDrillDownTableSelected(object x)
        {
            if (typeof(int).IsAssignableFrom(x?.GetType()) == false) { return; }
            var table = W.UI.Grids.Core.Tables.FirstOrDefault(v => v.ObjectId == (int)x);
            await SelectedTableChanged(table);
        }
        private async Task HandleTableGridDrillDown((object? Id, string? Name) args)
        {
            if (args.Id == null || typeof(int?).IsAssignableFrom(args.Id.GetType()) == false) { return; }
            await HandleDrillDown((int?)args.Id);
        }
        private async Task HandleDrillDown(int? id)
        {
            if (id == null) { return; }
            var table = W.UI.Grids.Core.Tables.FirstOrDefault(v => v.ObjectId == id);
            if (table == null) { return; }
            W.SelectTable(table);
            await this.SelectedTableChanged(table);
        }
        #endregion

        #region Context Menu
        private async Task GenerateMissingDatabaseIndexesToEditor()
        {
            await GenerateDatabaseIndexesToEditor(true);
        }
        private async Task GenerateAllDatabaseIndexesToEditor()
        {
            await GenerateDatabaseIndexesToEditor(false);
        }
        private async Task GenerateDatabaseIndexesToEditor(bool missingOnly)
        {
            var content = IndexCodeStringGenerator.GetDatabaseMissingIndexesSql(W.Context, W.Result.QueryEditorResult, missingOnly);
            await queryEditor.UpdateEditorQuery(content);
        }
        private async Task GenerateSQLProceduresToEditor()
        {
            var sql = ProcedureCodeGenerator.GenerateProcedures(W.SelectedTable) + Environment.NewLine;
            var idxGen = IndexCodeStringGenerator.GetDatabaseMissingIndexesSql(W.Context, W.Result.QueryEditorResult);
            string content = "-- Generated Procedures:" + Environment.NewLine + sql + Environment.NewLine +
                "-- Generated Query:" + Environment.NewLine + W.Result.QueryEditorResult.GeneratedQuery + Environment.NewLine +
                "-- " + Environment.NewLine + idxGen + Environment.NewLine;
            await queryEditor.UpdateEditorQuery(content);
        }
        private async Task GenerateAllTableClassesToEditor()
        {
            var classesString = string.Empty;
            foreach (var table in W.Context.Tables)
            {
                var result = W.Result.Databars.Results.FirstOrDefault(v => v.Tracker.Root.Table.ObjectId == table.ObjectId);
                if (result == null) { continue; }
                var clsGenerator = new TableCodeGenerator(W.Context, table, "JEO3", false);
                classesString += clsGenerator.GenerateTableCode(result) + Environment.NewLine + Environment.NewLine;
            }
            await queryEditor.UpdateEditorQuery(classesString);
        }
        private async Task GenerateDbContextToEditor()
        {
            var ctxString = string.Empty;
            var ctxGen = new ContextCodeGenerator(W.Context.Provider, "");
            await ctxGen.GenerateCode();
            await queryEditor.UpdateEditorQuery(ctxString);
        }
        private async Task GenerateTableClassToEditor()
        {
            if (W.Result.QueryEditorResult.Metrics == null)
            {
                var options = W.UI.Settings.ToGenerationOptions();
                var gen = new DiagramDrivenGraphGenerator();
                W.Result.QueryEditorResult = gen.Generate(W.SelectedTable, options);
            }

            var clsGenerator = new TableCodeGenerator(W.Context, W.SelectedTable, "JEO3", false);
            var clsDef = clsGenerator.GenerateTableCode(W.Result.QueryEditorResult) + Environment.NewLine;
            await queryEditor.UpdateEditorQuery(clsDef);
        }
        private async Task ArchiveGeneratorSelected(GenerationMethod method)
        {
            var selectedTable = W.SelectedTable;

            // Run Engine
            var options = W.UI.Settings.ToGenerationOptions(W.UI.Grids.ActiveConstraints);
            QueryGenerationResult jarvisResult = null;// = await GenerationService.GoJarvis(selectedTable, options);

            switch (method)
            {
                case GenerationMethod.FullGraph:
                    jarvisResult = new FullGraphGenerator().Generate(selectedTable, options);
                    break;
                case GenerationMethod.RootSplit:
                    jarvisResult = new RootSplitGenerator().Generate(selectedTable, options);
                    break;
                case GenerationMethod.UpThenDown:
                    jarvisResult = new UpThenDownGenerator().Generate(selectedTable, options);
                    break;
                case GenerationMethod.ExtendedDiscovery:
                    jarvisResult = new ExtendedDiscoveryCompressionGenerator().Generate(selectedTable, options);
                    break;
                case GenerationMethod.Backtracking:
                    jarvisResult = new BacktrackingSymmetricGenerator().Generate(selectedTable, options);
                    break;
                case GenerationMethod.JEO3:
                    //jarvisResult = new JEO3Generator().Generate(selectedTable, options);
                    break;
                default:
                    break;
            }
            if (jarvisResult == null) return;
            jarvisResult.Coverage = CoverageCalculator.CalculateCoverage(selectedTable, W.Context.Tables, W.Context.Relations);
            W.Result.QueryEditorResult = jarvisResult;
            //W.UI.Grids.ActiveConstraints = jarvisResult.ExecutionNodes?.ToGraphRelationConstraintList() ?? [];

            //if (W.UI.Settings.AutoGenerateDiagrams)
            await diagramViewer.ReloadDiagram(selectedTable, options);
            await diagramViewer2.ReloadDiagram(selectedTable, options);

            await queryEditor.UpdateEditorQuery(jarvisResult.GeneratedQuery);
        }
        private async Task GenerateProcedureModels()
        {
            var allCSharpClasses = string.Empty;

            foreach (var procedure in W.Context.Procedures)
            {
                if (procedure != null && new string[] { "insert", "update", "delete" }.Any(v => (procedure.Name ?? string.Empty).Trim().ToLower().Contains(v)) == false)
                {
                    var query = $"EXEC sys.sp_describe_first_result_set "
                        + $"N'EXEC {procedure.SchemaName}.{procedure.Name} {(string.Join(", ", procedure.Parameters.Select(v => $"{v.Name} = NULL")))}'";
                    procedure.OutputColumns = (await W.Context.Provider.GetInstances<DescribeFirstResultSet>(query)).ToList();
                    procedure.OutputCSharpDefinition = GenerateDescriptor(procedure.Name, procedure.OutputColumns);
                    allCSharpClasses += $"// Name: {procedure.SchemaName}.{procedure.Name}" + "\r\n"
                        + "// Parameters: " + string.Join(", ", procedure.Parameters.Select(v => v.Name + "(" + v.DataType + ")")) + "\r\n"
                        + "// Output Columns: " + string.Join(", ", procedure.OutputColumns.Select(v => v.name + "(" + v.system_type_name + ")")) + "\r\n"
                        + procedure.OutputCSharpDefinition + "\r\n";
                }
                else if (procedure != null && new string[] { "insert", "update" }.Any(v => procedure.Name.ToLower().Contains(v)) && procedure.Parameters.Any())
                {
                    var parameters = procedure.Parameters.Select(v => new DescribeFirstResultSet() { name = v.Name.TrimStart('@'), system_type_name = v.DataType, column_ordinal = v.Ordinal, is_nullable = v.IsNullable }).ToList();
                    procedure.OutputCSharpDefinition = GenerateDescriptor(procedure.Name, parameters);
                    allCSharpClasses += $"// Name: {procedure.SchemaName}.{procedure.Name}" + "\r\n"
                        + "// Parameters: " + string.Join(", ", procedure.Parameters.Select(v => v.Name + "(" + v.DataType + ")" + (v.IsNullable ? " NULL" : string.Empty))) + "\r\n"
                        + "// Output Columns: " + string.Join(", ", parameters.Select(v => v.name + "(" + v.system_type_name + ")")) + "\r\n"
                        + procedure.OutputCSharpDefinition + "\r\n";
                }
            }
            //System.IO.File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProcedureCodeDefinitions.txt"), allCSharpClasses);
        }
        private static string GenerateDescriptor(string className, IEnumerable<DescribeFirstResultSet> columns)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"public sealed class {className}");
            sb.AppendLine("{");

            foreach (var c in columns.OrderBy(x => x.column_ordinal))
            {
                var type = SQLTypeMapper.SqlTypeToClr(c.system_type_name, c.is_nullable.GetValueOrDefault());

                sb.AppendLine($"    public {type} {c.name} {{ get; init; }}");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }
        #endregion

        #region Query Results Tabs
        private async Task CreateNewQueryResultsTab(DataTable dt, int tabId, ITable selectedTable)
        {
            this.W.UI.Tabs.ResultColumns.Clear();

            foreach (DataColumn column in dt.Columns)
            {
                this.W.UI.Tabs.ResultColumns.Add(column.ColumnName, typeof(string));
            }

            var cl = new Dictionary<string, Type>();
            foreach (DataColumn col in dt.Columns)
            {
                cl.Add(col.ColumnName, col.DataType);
            }

            this.W.UI.Tabs.Tabs.Add(new DashboardUiTabItem
            {
                Id = tabId,
                TableName = selectedTable.Name,
                Title = dt.Rows.Count + $" - " + selectedTable.Name,
                Icon = "fa-solid fa-database",
                Columns = cl,
                Rows = dt.AsEnumerable().Select((rw, i) =>
                {
                    var row = new Dictionary<string, object>();
                    foreach (var column in this.W.UI.Tabs.ResultColumns)
                    {
                        row.Add(column.Key, rw[column.Key].ToString());
                    }
                    return row;
                }).ToList()
            });
            this.W.UI.Tabs.SelectedIndex = this.W.UI.Tabs.Tabs.Count - 1;
        }
        #endregion

        #region Heatmap
        private double GetTableMetric(ITable table, Heatmap<ITable>.HeatMapMetric metric)
        {
            return metric switch
            {
                Heatmap<ITable>.HeatMapMetric.Rows
                    => table.Rows ?? 0,
                Heatmap<ITable>.HeatMapMetric.CheckConstraints
                    => table.CheckConstraints.Count,
                Heatmap<ITable>.HeatMapMetric.Triggers
                    => table.Triggers.Count,
                Heatmap<ITable>.HeatMapMetric.Indexes
                    => table.Indexes.Count,
                Heatmap<ITable>.HeatMapMetric.MissingIndexes
                    => table.MissingIndexes.Count,
                Heatmap<ITable>.HeatMapMetric.Relations
                    => table.Relations.Count,
                Heatmap<ITable>.HeatMapMetric.ExtendedProperties
                    => table.ExtendedProperties.Count,
                Heatmap<ITable>.HeatMapMetric.Columns
                    => table.Columns.Count,
                Heatmap<ITable>.HeatMapMetric.ReferencedBy
                    => table.ReferencedByForeignKeys.Count,
                _ => 0
            };
        }
        #endregion

        #region Tab Groups
        private async Task OnNavTabIndexChanged()
        {
            W.UI.Nav.ExpandedGroupIndices.Add(WSNavGroupMap.GroupFor(W.UI.Nav.SelectedTabIndex));

            try
            {
                await Task.Delay(50);
                await JS.InvokeVoidAsync("resizeCharts");
            }
            catch (Exception ex)            
            {

            }
        }
        private void UpdateEntitiesNavTabChanged(int index)
        {
            selectedEntitiesNavTabIndex = index;
        }

        #endregion

        #region Tree
        private IObject? SelectedObject { get; set; }
        private async Task OnTreeNodeClick(IObject? obj)
        {
            SelectedObject = obj;
            if (obj is ITable)
            {
                var tbl = (ITable)obj;
                await SelectedTableChanged(tbl);
            }
        }
        private async Task<IEnumerable<object>> GetChildrenAsync(ITable table, object node) =>
           await Task.FromResult(node is ITable t ? t.Columns.Cast<object>() : Enumerable.Empty<object>());

        #endregion

        #region Pac-Man Bob
        // Pac-Man Bob
        private System.Threading.Timer _bobTimer;
        private string _bobStyle = "background-image: url('../img/pacman/pacman_ghost_gif.gif'); transition: all 0.5s ease;";
        private void MakeBobWeird(object state)
        {
            // Goes in OnInitialized() - Start the timer to periodically trigger Bob's behaviors
            //_bobTimer = new System.Threading.Timer(MakeBobWeird, null, 5000, 2000);

            // Guard against calling state updates if the component is already disposing
            if (_isDisposed) return;

            var rand = new Random().Next(1, 8);

            _bobStyle = rand switch
            {
                1 => "background-image: url('../img/pacman/pacman.gif'); transform: rotate(180deg);",
                2 => "background-image: url('../img/pacman/pacman.gif'); transform: scaleX(3.0);",
                3 => "background-image: none;",
                4 => "background-image: url('../im/pacman/pacman.gif'); transform: translateX(120px); filter: hue-rotate(90deg);",
                5 => "background-image: url('../im/pacman/pacman.gif'); transform: rotate(-180deg);filter: hue-rotate(90deg);translateX(120px);",
                6 => "background-image: url('../im/pacman/pacman.gif'); transform: rotate(-100deg);filter: hue-rotate(-450deg);translateX(160px);",
                7 => "background-image: url('../im/pacman/pacman.gif'); transform: rotate(30deg);filter: hue-rotate(70deg);translateX(-500px);",
                _ => "background-image: url('../img/pacman/pacman.gif');"
            };

            InvokeAsync(() =>
            {
                if (!_isDisposed)
                {
                    StateHasChanged();
                }
            });
        }
        #endregion
    }
}
