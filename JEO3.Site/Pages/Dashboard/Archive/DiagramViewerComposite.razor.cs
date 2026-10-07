//using System.Data;
//using Blazor.Diagrams;
//using Blazor.Diagrams.Core.Models.Base;
//using JEO3.Core;
//using JEO3.Diagrams;
//using JEO3.Generation;
//using JEO3.Generation.Models;
//using JEO3.Logging;
//using JEO3.Schema;
//using JEO3.Site.Components.Database;
//using JEO3.Site.Components.Diagram;
//using JEO3.Site.Extensions;
//using JEO3.Site.Services;
//using Microsoft.AspNetCore.Components;
//using Microsoft.JSInterop;

//namespace JEO3.Site.Dashboard
//{
//    public partial class DiagramViewerComposite : IDisposable
//    {
//        #region Properties

//        [Inject]
//        private ConnectionService ConnectionService { get; set; }

//        [Inject]
//        private WSWorkspace W { get; set; } = new();

//        [Parameter]
//        public EventCallback OnDiagramSettingChanged { get; set; }

//        // Diagram Singletons
//        private BlazorDiagram? ActiveDiagram { get; set; }
//        private BlazorDiagram? ActiveDiagram2 { get; set; }
//        private ElementReference queryResultsPaneDiv;

//        private System.Threading.CancellationTokenSource? _layoutCancellationTokenSource;
//        private bool _isLayoutComputing = false;
//        private bool _isDisposed = false;
//        private int _parameterEditorRefreshTrigger = 0;

//        private bool isRendered;
//        private string DiagramStyle { get; set; } = "";
//        private string DiagramBackgroundColor { get; set; } = "rgba(0,0,0,.9)";

//        // Cache previous state to avoid unnecessary re-renders
//        private string _lastDiagramStyle = "";
//        private string _lastDiagramBackgroundColor = "";

//        #endregion

//        #region Initialization & Lifecycle

//        protected override void OnInitialized()
//        {
//            W.OnDashboardStateChanged += HandleStateChanged;
//            if (ActiveDiagram != null)
//            {
//                ActiveDiagram.SelectionChanged += OnDiagramSelectionChanged;
//            }
//        }

//        protected override void OnAfterRender(bool firstRender)
//        {
//            if (firstRender)
//            {
//                isRendered = true;
//                StateHasChanged();
//            }
//        }

//        private async void HandleStateChanged()
//        {
//            if (_isDisposed) return;
//            await InvokeAsync(StateHasChanged);
//        }

//        protected override void OnParametersSet()
//        {
//            if (ActiveDiagram == null && W.Result.DiagramResultJarvis != null)
//            {
//                var dmg = DiagramBuilder.CreateDiagramFromQueryTables(W.Result.DiagramResultJarvis.ExecutionNodes, W.UI.Appearance.DiagramTheme == SqlEditorTheme.Light);
//                ActiveDiagram = dmg;
//                ActiveDiagram?.RegisterComponent<TableNodeModel, NodeTableNode>();
//                DiagramBuilder.SetViewport(ActiveDiagram, W.UI.Diagram.DiagramZoom);
//                ActiveDiagram?.Refresh();

//                // Ensure event is attached upon creation
//                ActiveDiagram.SelectionChanged += OnDiagramSelectionChanged;
//            }
//            if (ActiveDiagram2 == null && W.Result.DiagramResultDatabase != null)
//            {
//                var dmg = DiagramBuilder.CreateDiagramFromQueryTablesFull(W.Result.DiagramResultDatabase.ExecutionNodes, W.UI.Appearance.DiagramTheme == SqlEditorTheme.Light);
//                ActiveDiagram2 = dmg;
//                ActiveDiagram2?.RegisterComponent<TableNodeModel, JErTableNode>();
//                DiagramBuilder.SetViewport(ActiveDiagram2, W.UI.Diagram.DiagramZoom);
//                ActiveDiagram2?.Refresh();
//            }
//        }

//        public void Dispose()
//        {
//            _isDisposed = true;
//            if (W != null)
//            {
//                W.OnDashboardStateChanged -= HandleStateChanged;
//            }
//            if (ActiveDiagram != null)
//            {
//                ActiveDiagram.SelectionChanged -= OnDiagramSelectionChanged;
//            }

//            _layoutCancellationTokenSource?.Cancel();
//            _layoutCancellationTokenSource?.Dispose();
//        }

//        public async ValueTask DisposeAsync()
//        {
//            try
//            {
//                if (ActiveDiagram != null || ActiveDiagram2 != null)
//                {
//                    await Task.CompletedTask;
//                }
//            }
//            catch (Microsoft.JSInterop.JSDisconnectedException)
//            {
//                // Securely ignore: The browser session has already ended
//            }
//            catch (Exception)
//            {
//                // Catch-all for other disposal race conditions
//            }
//        }

//        #endregion

//        #region Load Diagram

//        public async Task ReloadDiagram(ITable graphTable, QueryGenerationOptions options)
//        {
//            try
//            {
//                options.Traversal.EnforceStrictness();

//                var gen = new DiagramDrivenGraphGenerator();
//                // Both diagram types use the same generation result — run once and share.
//                var result = await Task.Run(() => gen.Generate(graphTable, options));

//                W.Result.DiagramResultJarvis = result;
//                W.Result.DiagramResultDatabase = result;
//                await DebouncedReRunLayout();
//            }
//            catch (Exception ex)
//            {
//                ExceptionUtility.LogException(ex);
//            }
//        }

//        // Engine
//        private async Task DebouncedReRunLayout()
//        {
//            _layoutCancellationTokenSource?.Cancel();
//            _layoutCancellationTokenSource?.Dispose();
//            _layoutCancellationTokenSource = new System.Threading.CancellationTokenSource();

//            var token = _layoutCancellationTokenSource.Token;

//            try
//            {
//                await Task.Delay(250, token);

//                _isLayoutComputing = true;
//                await InvokeAsync(StateHasChanged);

//                await Task.Run(async () =>
//                {
//                    token.ThrowIfCancellationRequested();
//                    await ExecuteGraphLayoutEngine(token);
//                }, token);
//            }
//            catch (TaskCanceledException)
//            {
//                return;
//            }
//            finally
//            {
//                if (!token.IsCancellationRequested)
//                {
//                    await InvokeAsync(() =>
//                    {
//                        _isLayoutComputing = false;
//                        StateHasChanged();
//                    });
//                }
//            }
//        }

//        private async Task ExecuteGraphLayoutEngine(System.Threading.CancellationToken token)
//        {
//            // By decoupling these, both graphs can be generated side-by-side simultaneously
//            if (W.Result.DiagramResultJarvis != null)
//            {
//                await ProcessJarvisDiagram(token);
//            }

//            if (W.Result.DiagramResultDatabase != null)
//            {
//                await ProcessDatabaseDiagram(token);
//            }
//        }

//        private async Task ProcessJarvisDiagram(System.Threading.CancellationToken token)
//        {
//            var finalNodes = W.Result.DiagramResultJarvis.GetFinalExecutionNodes();
//            var diagram = DiagramBuilder.CreateDiagramFromQueryTables(finalNodes, W.UI.Appearance.DiagramTheme == SqlEditorTheme.Light);

//            var width = ActiveDiagram?.Container?.Width ?? 800;
//            var height = ActiveDiagram?.Container?.Height ?? 400;

//            if (W.UI.Diagram.ActiveParameters == null)
//            {
//                W.UI.Diagram.ApplyPreset(LayoutPresetType.JEO);
//            }
//            else
//            {
//                W.UI.Diagram.ActiveParameters.CanvasWidth = width;
//                W.UI.Diagram.ActiveParameters.CanvasHeight = height;
//            }

//            var parameters = W.UI.Diagram.ActiveParameters;
//            token.ThrowIfCancellationRequested();

//            switch (W.UI.Diagram.LayoutShape)
//            {
//                case GraphLayoutShape.CircularStar:
//                    await DiagramBuilder.ApplyGraphShapeLayoutCircular(diagram);
//                    break;
//                case GraphLayoutShape.FruchtermanReingold:
//                    await DiagramBuilder.ApplyGraphShapeLayoutFruchtermanReingold(diagram, (FruchtermanReingoldParameters)parameters);
//                    break;
//                case GraphLayoutShape.ISOM:
//                    await DiagramBuilder.ApplyGraphShapeLayoutISOM(diagram, (IsomParameters)parameters);
//                    break;
//                case GraphLayoutShape.KamadaKawai:
//                    await DiagramBuilder.ApplyGraphShapeLayoutKamadaKawai(diagram, (KkParameters)parameters);
//                    break;
//                case GraphLayoutShape.Sugiyama:
//                    await DiagramBuilder.ApplyGraphShapeLayoutSugiyama(diagram, (SugiyamaParameters)parameters);
//                    break;
//                case GraphLayoutShape.FruchtermanReingoldBounded:
//                    await DiagramBuilder.ApplyGraphShapeLayoutFructermanReingoldBounded(diagram, (FruchtermanReingoldParameters)parameters);
//                    break;
//                case GraphLayoutShape.LinLog:
//                    await DiagramBuilder.ApplyGraphShapeLayoutLinLog(diagram, (LinLogParameters)parameters);
//                    break;
//                case GraphLayoutShape.Random:
//                    await DiagramBuilder.ApplyGraphShapeLayoutRandom(diagram, (RandomParameters)parameters);
//                    break;
//                case GraphLayoutShape.TreeBallon:
//                    var selectedNodeVertex = diagram.Nodes.FirstOrDefault(v => ((TableNodeModel)v).IsRootNode);
//                    if (selectedNodeVertex != null)
//                    {
//                        await DiagramBuilder.ApplyGraphShapeLayoutTreeBalloon(diagram, selectedNodeVertex, (TreeBalloonParameters)parameters);
//                    }
//                    break;
//                case GraphLayoutShape.TreeDouble:
//                    await DiagramBuilder.ApplyGraphShapeLayoutTreeDouble(diagram, (TreeDoubleParameters)parameters);
//                    break;
//                case GraphLayoutShape.TreeSimple:
//                    await DiagramBuilder.ApplyGraphShapeLayoutTreeSimple(diagram, (TreeSimpleParameters)parameters);
//                    break;
//                case GraphLayoutShape.None:
//                    break;
//            }

//            token.ThrowIfCancellationRequested();

//            await InvokeAsync(() =>
//            {
//                if (ActiveDiagram != null)
//                {
//                    ActiveDiagram.Links.Clear();
//                    ActiveDiagram.Nodes.Clear();

//                    foreach (var node in diagram.Nodes) ActiveDiagram.Nodes.Add(node);
//                    foreach (var link in diagram.Links) ActiveDiagram.Links.Add(link);

//                    ActiveDiagram.Refresh();
//                }
//                else
//                {
//                    ActiveDiagram = diagram;
//                    ActiveDiagram.RegisterComponent<TableNodeModel, NodeTableNode>();
//                    ActiveDiagram.SelectionChanged += OnDiagramSelectionChanged;
//                    DiagramBuilder.SetViewport(ActiveDiagram, W.UI.Diagram.DiagramZoom);
//                    ActiveDiagram.Refresh();
//                }
//            });
//        }

//        private async Task ProcessDatabaseDiagram(System.Threading.CancellationToken token)
//        {
//            var finalNodes = W.Result.DiagramResultDatabase.GetFinalExecutionNodes();
//            var diagram = DiagramBuilder.CreateDiagramFromQueryTablesFull(finalNodes, W.UI.Appearance.DiagramTheme == SqlEditorTheme.Light);

//            token.ThrowIfCancellationRequested();

//            await InvokeAsync(() =>
//            {
//                if (ActiveDiagram2 != null)
//                {
//                    ActiveDiagram2.Links.Clear();
//                    ActiveDiagram2.Nodes.Clear();

//                    foreach (var node in diagram.Nodes) ActiveDiagram2.Nodes.Add(node);
//                    foreach (var link in diagram.Links) ActiveDiagram2.Links.Add(link);

//                    ActiveDiagram2.Refresh();
//                }
//                else
//                {
//                    ActiveDiagram2 = diagram;
//                    ActiveDiagram2.RegisterComponent<TableNodeModel, JErTableNode>();
//                    DiagramBuilder.SetViewport(ActiveDiagram2, W.UI.Diagram.DiagramZoom);
//                    ActiveDiagram2.Refresh();
//                }
//            });
//        }

//        #endregion

//        #region SQL Execution

//        private async Task SelectTop1000()
//        {
//            if (string.IsNullOrWhiteSpace(W.SelectedTable.SchemaName) || string.IsNullOrWhiteSpace(W.SelectedTable.Name)) return;
//            var query = $"{SQLConstants.SelectTop1000} X.* FROM [{W.SelectedTable.SchemaName}].[{W.SelectedTable.Name}] X";
//            await Execute(query);
//        }

//        private async Task Execute(string query)
//        {
//            try
//            {
//                if (W.SelectedTable == null) { return; }

//                W.UI.Appearance.IsQueryBusy = true;

//                var dt = await ConnectionService.Execute(query);
//                if (dt == null) { return; }

//                var tabId = this.W.UI.Tabs.NextTabId++;
//                await CreateNewQueryResultsTab(dt, tabId, W.SelectedTable);
//            }
//            catch (Exception ex)
//            {
//                ExceptionUtility.LogException(ex);
//            }
//            finally
//            {
//                W.UI.Appearance.IsQueryBusy = false;
//                await InvokeAsync(StateHasChanged);
//            }
//        }

//        #endregion

//        #region Query Results Tabs

//        private async Task CreateNewQueryResultsTab(DataTable dt, int tabId, ITable selectedTable)
//        {
//            this.W.UI.Tabs.ResultColumns.Clear();

//            foreach (DataColumn column in dt.Columns)
//            {
//                this.W.UI.Tabs.ResultColumns.Add(column.ColumnName, typeof(string));
//            }

//            var cl = new Dictionary<string, Type>();
//            foreach (DataColumn col in dt.Columns)
//            {
//                cl.Add(col.ColumnName, col.DataType);
//            }

//            this.W.UI.Tabs.Tabs.Add(new DashboardUiTabItem
//            {
//                Id = tabId,
//                Title = dt.Rows.Count + $" - " + selectedTable.Name,
//                Icon = "fa-solid fa-database",
//                Columns = cl,
//                Rows = dt.AsEnumerable().Select((rw, i) =>
//                {
//                    var row = new Dictionary<string, object>();
//                    foreach (var column in this.W.UI.Tabs.ResultColumns)
//                    {
//                        row.Add(column.Key, rw[column.Key].ToString());
//                    }
//                    return row;
//                }).ToList()
//            });
//            this.W.UI.Tabs.SelectedIndex = this.W.UI.Tabs.Tabs.Count - 1;
//        }

//        #endregion

//        #region Themes

//        public async Task ApplySqlTheme()
//        {
//            if (DiagramBackgroundColor != "white" || DiagramStyle != "")
//            {
//                DiagramBackgroundColor = "white";
//                DiagramStyle = "";
//                await InvokeAsync(StateHasChanged);
//            }
//            await Task.CompletedTask;
//        }

//        private async Task ApplyAs400Theme()
//        {
//            if (DiagramBackgroundColor != "black" || DiagramStyle != "")
//            {
//                DiagramBackgroundColor = "black";
//                DiagramStyle = "";
//                await InvokeAsync(StateHasChanged);
//            }
//            await Task.CompletedTask;
//        }

//        private async Task OnThemeChange(SqlEditorTheme sqlEditorTheme)
//        {
//            if (W.UI.Appearance.IsSchemaLoaded && sqlEditorTheme == SqlEditorTheme.AS400)
//            {
//                await ApplyAs400Theme();
//            }
//            else if (sqlEditorTheme == SqlEditorTheme.Light)
//            {
//                await ApplySqlTheme();
//            }
//            W.NotifyMainLayoutStateChanged();
//        }

//        #endregion

//        #region Events

//        private async Task OnDiagramLayoutShapeChanged(object shape)
//        {
//            await OnDiagramSettingChanged.InvokeAsync();
//        }

//        private void OnDiagramSelectionChanged(SelectableModel model)
//        {
//            if (model is TableNodeModel clickedTable)
//            {
//                DiagramBuilder.HandleClusterSelection(ActiveDiagram, clickedTable);
//            }
//        }

//        private async Task OnPresetChanged(object selectedPreset)
//        {
//            if (selectedPreset is LayoutPresetType preset)
//            {
//                W.UI.Diagram.ApplyPreset(preset);
//                _parameterEditorRefreshTrigger++;
//                await DebouncedReRunLayout();
//            }
//        }

//        #endregion

//        #region Toggle

//        private void ToggleDiagramSidebar()
//        {
//            W.UI.Appearance.IsSidebarExpandedDiagram = !W.UI.Appearance.IsSidebarExpandedDiagram;
//        }

//        private async Task TogglePaneExpansionHalfScreen()
//        {
//            W.UI.Appearance.IsPaneExpandedResults = !W.UI.Appearance.IsPaneExpandedResults;
//            await JS.InvokeVoidAsync("toggleElementHalfscreen", queryResultsPaneDiv, W.UI.Appearance.IsPaneExpandedResults);
//        }

//        private async Task TogglePaneExpansionFullScreen()
//        {
//            W.UI.Appearance.IsPaneFullScreenResults = !W.UI.Appearance.IsPaneFullScreenResults;
//            var isFullscreen = await JS.InvokeAsync<bool>("eval", "document.fullscreenElement != null");

//            if (isFullscreen)
//            {
//                await JS.InvokeVoidAsync("toggleElementFullscreen", queryResultsPaneDiv, false);
//            }
//            else
//            {
//                await JS.InvokeVoidAsync("toggleElementFullscreen", queryResultsPaneDiv, true);
//            }
//        }

//        #endregion
//    }
//}