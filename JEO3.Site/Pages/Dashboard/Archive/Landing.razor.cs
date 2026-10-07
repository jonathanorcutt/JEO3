using JEO3.Code;
using JEO3.Extensions;
using JEO3.Generation;
using JEO3.Generation.Models;
using JEO3.Logging;
using JEO3.Providers;
using JEO3.Schema;
using JEO3.Site.Dashboard;
using JEO3.Site.Extensions;
using JEO3.Site.Infrastructure;
using JEO3.Site.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.JSInterop;
using JEO3.Site.Services;
using Radzen;

namespace JEO3.Site.Pages
{
    public partial class Landing : ComponentBase
    {
        #region Properties

        [Inject] private IJSRuntime JS { get; set; }
        [Inject] private DialogService DialogService { get; set; }
        [Inject] private HybridCache Cache { get; set; }
        // Machine
        [Inject] private WSWorkspace W { get; set; } = new();
        [Inject] private ConnectionService ConnectionService { get; set; }
        [Inject] private ISchemaCacheService SchemaService { get; set; }

        // Components
        private SchemaToolbox toolbox;
        private QueryEditor queryEditor;
        private QueryEditor queryEditor2;
        private QueryEditor queryEditor3;
        private QueryEditor queryEditor4;
        private DiagramViewerDatabase diagramViewer;
        private DiagramViewerJarvis diagramViewer2;
        //private DashboardTopBar dashboardTopBar;
        private bool _isDisposed;

        // Databars
        protected DataBarViewModel databars { get; set; }

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
                    // GET & LOAD CONTEXT
                    var context = await SchemaService.GetDatabaseSchemaAsync(prov);
                    W.LoadSchemaContext(context, prov);
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex);
            }
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                W.UI.Settings.LevelsUp = 12;
                W.UI.Settings.LevelsDown = 12;
                W.UI.Settings.StopOnCycles = true;
                W.UI.Settings.IgnoreNullableForeignKeys = false;
                W.UI.Settings.JoinSelectPolicy = TableSelectPolicy.AllColumns;
                //W.UI.Settings.AdvancedAnnotations = AnnotationVerbosity.PathBreadcrumbs | AnnotationVerbosity.ExecutionSummary | AnnotationVerbosity.TraversalSummary | AnnotationVerbosity.DoNotRemoveAscii | AnnotationVerbosity.TableFooter;
                W.UI.Diagram.LayoutShape = JEO3.Diagrams.GraphLayoutShape.KamadaKawai;

                //ITable tbl = (this.ConnectionService.ConnectionSettings.First().ConnectionString.ToLower().Contains("adventure") && W.UI.Appearance.IsSchemaLoaded)
                //    ? W.Context.Tables.FirstOrDefault(v => v.Name.ToLower().Contains("salesorderdetail"))
                //    : W.Context.Tables.FirstOrDefault();

                //// AW Only
                //if (tbl != null)
                //{
                //    await SelectedTableChanged(tbl);
                //    StateHasChanged();
                //}



                // Populate tables
                await PopulateDatabarsGrids();
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

        #region Databars

        private async Task PopulateDatabarsGrids()
        {
            var options = W.UI.Settings.ToGenerationOptions(W.UI.Grids.ActiveConstraints);
            var results = await W.Context.GetAllTablesResult(options);

            databars = new DataBarViewModel(W.Context, results.ToList());
            await databars.Populate();

            // Treemap
            treeTables = W.Context.Tables.ToList();


            var connSetting = W.Context.Provider.Connection.ConnectionString.ToLower().Contains("adventure");
            var loaded = (connSetting != null && W.UI.Appearance.IsSchemaLoaded);
            if (loaded)
            {
                var tbls = W.Context.Tables;
                var r1 = results.FirstOrDefault(v => v.Tracker.Root.Table.Name.ToLower().Contains("salesorderdetail"));
                var r2 = results.FirstOrDefault(v => v.Tracker.Root.Table.Name.ToLower().EndsWith("person"));
                var r3 = results.FirstOrDefault(v => v.Tracker.Root.Table.Name.ToLower().EndsWith("product"));
                var r4 = results.FirstOrDefault(v => v.Tracker.Root.Table.Name.ToLower().StartsWith("location"));

                W.Result.DiagramResultDatabase = r1;
                W.Result.DiagramResultJarvis = r1;
                W.Result.QueryEditorResult = r1;

                ITable tbl1 = loaded ? W.Context.GetDefaultSiteSearchTable() : tbls.FirstOrDefault();
                ITable tbl2 = loaded ? W.Context.GetSiteSearchTable("salesorderheader") : tbls.FirstOrDefault();
                ITable tbl3 = loaded ? W.Context.GetSiteSearchTable("person") : tbls.FirstOrDefault();
                ITable tbl4 = loaded ? W.Context.GetSiteSearchTable("product") : tbls.FirstOrDefault();
                ITable tbl5 = loaded ? W.Context.GetSiteSearchTable("billofmaterials") : tbls.FirstOrDefault();

                diagramViewerDatabase?.ReloadDiagram(tbl1, options);
                diagramViewerDatabase2?.ReloadDiagram(tbl1, options);
                diagramViewerJarvis?.ReloadDiagram(tbl1, options);
                diagramViewerJarvis2?.ReloadDiagram(tbl1, options);

                await Task.Delay(2000);

                if (W.UI.Appearance.IsMonacoEditorReady == true)
                {
                    var codeGen1 = new Code.TableCodeGenerator(W.Context, tbl1, "JEO3", true);
                    var code1 = codeGen1.GenerateTableCode(r1);
                    await queryEditor.UpdateEditorQuery(code1);

                    var codeGen2 = new Code.TableCodeGenerator(W.Context, tbl2, "JEO3", true);
                    var code2 = codeGen2.GenerateTableCode(r2);
                    await queryEditor2.UpdateEditorQuery(code2);

                    var codeGen3 = new Code.TableCodeGenerator(W.Context, tbl3, "JEO3", true);
                    var code3 = codeGen3.GenerateTableCode(r3);
                    await queryEditor3.UpdateEditorQuery(code3);

                    var codeGen4 = new Code.TableCodeGenerator(W.Context, tbl4, "JEO3", true);
                    var code4 = codeGen4.GenerateTableCode(r4);
                    await queryEditor4.UpdateEditorQuery(code4);
                }

                StateHasChanged();
            }

        }

        protected async Task HandleTableGridDrillDown((object? Id, string? Name) args)
        {
            if (args.Id == null || typeof(int?).IsAssignableFrom(args.Id.GetType()) == false) { return; }
            await HandleDrillDown((int?)args.Id);
        }
        protected async Task HandleTableHealthGridDrillDown((object? Id, string? Name) args)
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
        private async Task GenerateMissingDatabaseIndexesToEditor(ITable table)
        {
            await GenerateDatabaseIndexesToEditor(table, true);
        }

        private async Task GenerateAllDatabaseIndexesToEditor(ITable table)
        {
            await GenerateDatabaseIndexesToEditor(table, false);
        }

        private async Task GenerateDatabaseIndexesToEditor(ITable table, bool missingOnly)
        {
            var options = W.UI.Settings.ToGenerationOptions(W.UI.Grids.ActiveConstraints);
            options.IncludeCTEs = false;
            options.Traversal.PreventRootTypeRecursion = true;
            options.Retrieval.JoinSelectPolicy = TableSelectPolicy.None;
            options.Retrieval.RootSelectPolicy = TableSelectPolicy.AllColumns;
            options.Formatting.AdvancedAnnotations = AnnotationVerbosity.TableFooter | AnnotationVerbosity.PathBreadcrumbs | AnnotationVerbosity.ExecutionSummary | AnnotationVerbosity.TraversalSummary;
            var gen = new PlanDrivenQueryGenerator();
            QueryGenerationResult jarvisResult = gen.Generate(table, options);
            if (jarvisResult == null) return;

            jarvisResult.Coverage = CoverageCalculator.CalculateCoverage(table, W.Context.Tables, W.Context.Relations);
            W.Result.QueryEditorResult = jarvisResult;
            var content = IndexCodeStringGenerator.GetDatabaseMissingIndexesSql(W.Context, jarvisResult, missingOnly);
            await queryEditor.UpdateEditorQuery(content);
        }
        private async Task GenerateSQLProceduresToEditor(ITable table)
        {
            var options = W.UI.Settings.ToGenerationOptions(W.UI.Grids.ActiveConstraints);
            options.Traversal.PreventRootTypeRecursion = true;
            var gen = new DiagramDrivenGraphGenerator();
            QueryGenerationResult jarvisResult = gen.Generate(table, options);
            if (jarvisResult == null) return;

            jarvisResult.Coverage = CoverageCalculator.CalculateCoverage(table, W.Context.Tables, W.Context.Relations);
            W.Result.QueryEditorResult = jarvisResult;

            var sql = ProcedureCodeGenerator.GenerateProcedures(table) + Environment.NewLine;
            var clsGenerator = new TableCodeGenerator(W.Context, table, "JEO3", false);
            var clsDef = clsGenerator.GenerateTableCode(jarvisResult) + Environment.NewLine;
            var idxGen = IndexCodeStringGenerator.GetDatabaseMissingIndexesSql(W.Context, jarvisResult);

            string content = "-- Generated Procedures:" + Environment.NewLine + sql + Environment.NewLine +
                "-- Generated Query:" + Environment.NewLine + jarvisResult.GeneratedQuery + Environment.NewLine +
                "-- " + Environment.NewLine + idxGen + Environment.NewLine +
                "-- C# Class" + Environment.NewLine + clsDef + Environment.NewLine;
            await queryEditor.UpdateEditorQuery(content);
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

        public async Task SelectedViewDefinitionChanged(IView view)
        {
            if (view != null)
            {
                await this.queryEditor.UpdateEditorQuery(view.Definition);
            }
        }
        public async Task SelectedStoredProcedureDefinitionChanged(IProcedure procedure)
        {
            if (procedure != null)
            {
                await this.queryEditor.UpdateEditorQuery(procedure.Definition);
            }
        }

        // Tables Grid - Selected Table Change
        public async Task SelectedTableChanged(ITable selectedTable)
        {
            if (W.UI.Settings == null || selectedTable == null || !W.UI.Settings.FlagAutoGenerateQueries) return;
            W.UI.Appearance.IsShowingPageLoadAsciiArt = W.UI.Appearance.IsShowingPageLoadAsciiArt && W.UI.Appearance.IsMonacoEditorReady ? false : W.UI.Appearance.IsShowingPageLoadAsciiArt;
            W.UI.Appearance.IsQueryBusy = true;

            try
            {
                if (W.UI.Appearance.IsSchemaLoaded && W.UI.Appearance.IsShowingPageLoadAsciiArt == false)
                {
                    // bob?.Play(500);
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
                ExceptionUtility.LogException(ex);
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
                await InvokeAsync(StateHasChanged); // Essential fix: force render chain execution
            }
        }

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


        protected List<ITable> treeTables = new();

        private int selectedIndex = 0;
        private void UpdateTabChanged(int index)
        {
            selectedIndex = index;
        }
    }
}
