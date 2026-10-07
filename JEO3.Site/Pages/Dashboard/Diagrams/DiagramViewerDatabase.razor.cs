using Blazor.Diagrams;
using JEO3.Diagrams;
using JEO3.Diagrams.Export;
using JEO3.Generation;
using JEO3.Generation.Models;
using JEO3.Logging;
using JEO3.Schema;
using JEO3.Site.Components.Database;
using JEO3.Site.Extensions;
using JEO3.Site.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JEO3.Site.Dashboard
{
    public partial class DiagramViewerDatabase : IDisposable
    {
        #region Properties
        [Inject] private ConnectionService ConnectionService { get; set; }
        [Inject] private WSWorkspace W { get; set; } = new();
        [Parameter] public EventCallback OnDiagramSettingChanged { get; set; }

        // Diagram Singletons
        public BlazorDiagram? ActiveDiagram2 { get; set; }
        private ElementReference queryResultsPaneDiv;

        private CancellationTokenSource? _layoutCancellationTokenSource;
        private bool _isLayoutComputing = false;
        private int _parameterEditorRefreshTrigger = 0;
        private bool isRendered;
        private string DiagramStyle { get; set; } = "";
        private string DiagramBackgroundColor { get; set; } = "rgba(0,0,0,.9)";

        #endregion

        #region Initialization
        protected override void OnInitialized()
        {
            W.OnDashboardStateChanged += HandleStateChanged;
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                isRendered = true;

                if (W.SelectedTable != null && W.SelectedTable.DatabaseName != null)
                {
                    var options = W.UI.ERSettings.ToGenerationOptions(W.UI.Grids.ActiveConstraints);
                    await ReloadDiagram(W.SelectedTable, options);
                }
                StateHasChanged();
            }
        }
        private async void HandleStateChanged()
        {
            await InvokeAsync(StateHasChanged);
        }
        protected override void OnParametersSet()
        {
            if (ActiveDiagram2 == null && W.Result.DiagramResultDatabase != null && isRendered && W.UI.Appearance.IsSchemaLoaded)
            {
                var dmg = DiagramBuilder.CreateDiagramFromQueryTablesFull(W.Result.DiagramResultDatabase.ExecutionNodes, W.UI.Appearance.ERDiagramTheme == SqlEditorTheme.Light);
                ActiveDiagram2 = dmg;
                ActiveDiagram2?.RegisterComponent<TableNodeModel, JErTableNode>();
                DiagramBuilder.SetViewport(ActiveDiagram2, W.UI.ERDiagram.DiagramZoom);
                ActiveDiagram2?.Refresh();
            }
        }
        public void Dispose()
        {
            if (W != null)
            {
                W.OnDashboardStateChanged -= HandleStateChanged;
            }

            _layoutCancellationTokenSource?.Cancel();
            _layoutCancellationTokenSource?.Dispose();
        }
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (ActiveDiagram2 != null)
                {
                    await Task.CompletedTask;
                }
            }
            catch (Microsoft.JSInterop.JSDisconnectedException)
            {
                // Securely ignore: The browser session has already ended
            }
            catch (Exception)
            {
                // Catch-all for other disposal race conditions
            }
        }
        #endregion

        #region Load Diagram
        public async Task ReloadDiagram(ITable graphTable, QueryGenerationOptions options)
        {
            if (isRendered == false) { return; }

            try
            {
                options.Traversal.EnforceStrictness();

                var gen = new DiagramDrivenGraphGenerator();
                // Offload diagram generation to background thread to prevent UI blocking
                W.Result.DiagramResultDatabase = await Task.Run(() => gen.Generate(graphTable, options));
                await DebouncedReRunLayout();
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
            }
        }
        private async Task DebouncedReRunLayout()
        {
            _layoutCancellationTokenSource?.Cancel();
            _layoutCancellationTokenSource?.Dispose();
            _layoutCancellationTokenSource = new System.Threading.CancellationTokenSource();

            var token = _layoutCancellationTokenSource.Token;

            try
            {
                await Task.Delay(1, token);

                _isLayoutComputing = true;
                await InvokeAsync(StateHasChanged);

                await Task.Run(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    await ExecuteGraphLayoutEngine(token);
                }, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    await InvokeAsync(() =>
                    {
                        _isLayoutComputing = false;
                        StateHasChanged();
                    });
                }
            }
        }
        private async Task ExecuteGraphLayoutEngine(CancellationToken token)
        {
            if (W.Result.DiagramResultDatabase != null)
            {
                await ProcessDatabaseDiagram(token);
            }
        }
        private async Task ProcessDatabaseDiagram(CancellationToken token)
        {
            var finalNodes = W.Result.DiagramResultDatabase.GetFinalExecutionNodes();
            var diagram = DiagramBuilder.CreateDiagramFromQueryTablesFull(finalNodes, W.UI.Appearance.ERDiagramTheme == SqlEditorTheme.Light);

            var width = ActiveDiagram2?.Container?.Width ?? 800;
            var height = ActiveDiagram2?.Container?.Height ?? 400;

            if (W.UI.ERDiagram.ActiveParameters == null)
            {
                W.UI.ERDiagram.ApplyPreset(LayoutPresetType.JEO);
            }
            else
            {
                W.UI.ERDiagram.ActiveParameters.CanvasWidth = width;
                W.UI.ERDiagram.ActiveParameters.CanvasHeight = height;
            }

            W.UI.ERDiagram.ActiveParameters.UseEntityRelationMode = true;
            var parameters = W.UI.ERDiagram.ActiveParameters;
            token.ThrowIfCancellationRequested();


            await DiagramBuilder.ApplyGraphShapeLayoutER2(diagram, new ERParameters() { CanvasHeight = width, CanvasWidth = height, UseEntityRelationMode = true, IdealDistance = 220, SiblingSpacing = 20 });
            //await DiagramBuilder.ApplyGraphShapeLayoutER(diagram, new ERParameters() { CanvasHeight = width, CanvasWidth = height, UseEntityRelationMode = true });


            //switch (W.UI.ERDiagram.LayoutShape)
            //{
            //    case GraphLayoutShape.CircularStar:
            //        await DiagramBuilder.ApplyGraphShapeLayoutCircular(diagram);
            //        break;
            //    case GraphLayoutShape.FruchtermanReingold:
            //        await DiagramBuilder.ApplyGraphShapeLayoutFruchtermanReingold(diagram, (FruchtermanReingoldParameters)parameters);
            //        break;
            //    case GraphLayoutShape.ISOM:
            //        await DiagramBuilder.ApplyGraphShapeLayoutISOM(diagram, (IsomParameters)parameters);
            //        break;
            //    case GraphLayoutShape.KamadaKawai:
            //        await DiagramBuilder.ApplyGraphShapeLayoutKamadaKawai(diagram, (KkParameters)parameters);
            //        break;
            //    case GraphLayoutShape.Sugiyama:
            //        await DiagramBuilder.ApplyGraphShapeLayoutSugiyama(diagram, (SugiyamaParameters)parameters);
            //        break;
            //    case GraphLayoutShape.FruchtermanReingoldBounded:
            //        await DiagramBuilder.ApplyGraphShapeLayoutFructermanReingoldBounded(diagram, (FruchtermanReingoldParameters)parameters);
            //        break;
            //    case GraphLayoutShape.LinLog:
            //        await DiagramBuilder.ApplyGraphShapeLayoutLinLog(diagram, (LinLogParameters)parameters);
            //        break;
            //    case GraphLayoutShape.Random:
            //        await DiagramBuilder.ApplyGraphShapeLayoutRandom(diagram, (RandomParameters)parameters);
            //        break;
            //    case GraphLayoutShape.TreeBallon:
            //        var selectedNodeVertex = diagram.Nodes.FirstOrDefault(v => ((TableNodeModel)v).IsRootNode);
            //        if (selectedNodeVertex != null)
            //        {
            //            await DiagramBuilder.ApplyGraphShapeLayoutTreeBalloon(diagram, selectedNodeVertex, (TreeBalloonParameters)parameters);
            //        }
            //        break;
            //    case GraphLayoutShape.TreeDouble:
            //        await DiagramBuilder.ApplyGraphShapeLayoutTreeDouble(diagram, (TreeDoubleParameters)parameters);
            //        break;
            //    case GraphLayoutShape.TreeSimple:
            //        await DiagramBuilder.ApplyGraphShapeLayoutTreeSimple(diagram, (TreeSimpleParameters)parameters);
            //        break;
            //    case GraphLayoutShape.None:
            //        break;
            //}

            token.ThrowIfCancellationRequested();

            await InvokeAsync(() =>
            {
                if (ActiveDiagram2 != null)
                {
                    ActiveDiagram2.Links.Clear();
                    ActiveDiagram2.Nodes.Clear();

                    foreach (var node in diagram.Nodes) ActiveDiagram2.Nodes.Add(node);
                    foreach (var link in diagram.Links) ActiveDiagram2.Links.Add(link);

                    DiagramBuilder.SetViewport(ActiveDiagram2, W.UI.ERDiagram.DiagramZoom, 0, 0);
                    ActiveDiagram2.Refresh();
                }
                else
                {
                    ActiveDiagram2 = diagram;
                    ActiveDiagram2.RegisterComponent<TableNodeModel, JErTableNode>();
                    DiagramBuilder.SetViewport(ActiveDiagram2, W.UI.ERDiagram.DiagramZoom, 0, 0);
                    ActiveDiagram2.Refresh();
                }
            });
        }
        #endregion

        #region Themes
        public async Task ApplySqlTheme()
        {
            if (DiagramBackgroundColor != "#E2E2E2" || DiagramStyle != "")
            {
                DiagramBackgroundColor = "#E2E2E2";
                DiagramStyle = "";
                await InvokeAsync(StateHasChanged);
            }
            await Task.CompletedTask;
        }
        private async Task ApplyAs400Theme()
        {
            if (DiagramBackgroundColor != "black" || DiagramStyle != "")
            {
                DiagramBackgroundColor = "black";
                DiagramStyle = "";
                await InvokeAsync(StateHasChanged);
            }
            await Task.CompletedTask;
        }
        private async Task OnThemeChange(SqlEditorTheme sqlEditorTheme)
        {
            if (W.UI.Appearance.IsSchemaLoaded && sqlEditorTheme == SqlEditorTheme.AS400)
            {
                await ApplyAs400Theme();
            }
            else if (sqlEditorTheme == SqlEditorTheme.Light)
            {
                await ApplySqlTheme();
            }
            W.NotifyMainLayoutStateChanged();
        }
        #endregion

        #region Events
        private async Task OnDiagramLayoutShapeChanged(object shape)
        {
            await OnDiagramSettingChanged.InvokeAsync();
        }
        private async Task OnPresetChanged(object selectedPreset)
        {
            if (selectedPreset is LayoutPresetType preset)
            {
                W.UI.ERDiagram.ApplyPreset(preset);
                _parameterEditorRefreshTrigger++;
                await DebouncedReRunLayout();
            }
        }
        private async Task ExportDiagram()
        {
            if (ActiveDiagram2 == null) { return; }
            byte[] fileBytes = VisioDiagramExporter.Export(ActiveDiagram2);

            // 2. Convert to a Base64 string directly in C# to safely feed into JS 
            // (This protects against SignalR byte-array fragmentation)
            var base64Data = Convert.ToBase64String(fileBytes);

            await JS.InvokeVoidAsync("downloadBase64File", "application/vnd.ms-visio.drawing", base64Data, "Diagram.vsdx");
        }
        private async Task OnDiagramZoomChanged(double value)
        {
            if (ActiveDiagram2 == null) { return; }
            DiagramBuilder.SetViewport(ActiveDiagram2, Math.Abs(value));
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Toggle
        private void ToggleDiagramSidebar()
        {
            W.UI.Appearance.IsSidebarExpandedERDiagram = !W.UI.Appearance.IsSidebarExpandedERDiagram;
        }
        private async Task TogglePaneExpansionFullScreen()
        {
            W.UI.Appearance.IsPaneFullScreenResults = !W.UI.Appearance.IsPaneFullScreenResults;
            var isFullscreen = await JS.InvokeAsync<bool>("eval", "document.fullscreenElement != null");

            if (isFullscreen)
            {
                await JS.InvokeVoidAsync("toggleElementFullscreen", queryResultsPaneDiv, false);
            }
            else
            {
                await JS.InvokeVoidAsync("toggleElementFullscreen", queryResultsPaneDiv, true);
            }
        }
        #endregion
    }
}