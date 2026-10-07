using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models.Base;
using JEO3.Diagrams;
using JEO3.Diagrams.Export;
using JEO3.Generation;
using JEO3.Generation.ExecutionPlan;
using JEO3.Generation.Models;
using JEO3.Logging;
using JEO3.Schema;
using JEO3.Site.Components.Diagram;
using JEO3.Site.Extensions;
using JEO3.Site.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace JEO3.Site.Dashboard
{
    public partial class DiagramViewerJarvis : IDisposable
    {
        #region Properties
        [Inject] private ConnectionService ConnectionService { get; set; }
        [Inject] private WSWorkspace W { get; set; } = new();
        [Parameter] public EventCallback OnDiagramSettingChanged { get; set; }

        // Diagram Singletons
        public BlazorDiagram? ActiveDiagram { get; set; }
        private ElementReference queryResultsPaneDiv;

        private CancellationTokenSource? _layoutCancellationTokenSource;
        private bool _isLayoutComputing = false;
        private int _parameterEditorRefreshTrigger = 0;

        private bool isRendered;
        private string DiagramStyle { get; set; } = "";
        private string DiagramBackgroundColor { get; set; } = "rgba(0,0,0,.9)";

        // Cache previous state to avoid unnecessary re-renders
        private string _lastDiagramStyle = "";
        private string _lastDiagramBackgroundColor = "";

        #endregion

        #region Initialization
        protected override void OnInitialized()
        {
            W.OnDashboardStateChanged += HandleStateChanged;
            if (ActiveDiagram != null)
            {
                ActiveDiagram.SelectionChanged += OnDiagramSelectionChanged;
            }
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                isRendered = true;

                if (W.SelectedTable != null && W.SelectedTable.DatabaseName != null)
                {
                    var options = W.UI.Settings.ToGenerationOptions(W.UI.Grids.ActiveConstraints);
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
            if (ActiveDiagram == null && W.Result.DiagramResultJarvis != null && isRendered && W.UI.Appearance.IsSchemaLoaded)
            {
                var dmg = DiagramBuilder.CreateDiagramFromQueryTables(W.Result.DiagramResultJarvis.ExecutionNodes, W.UI.Appearance.DiagramTheme == SqlEditorTheme.Light);
                ActiveDiagram = dmg;
                ActiveDiagram?.RegisterComponent<TableNodeModel, NodeTableNode>();
                DiagramBuilder.SetViewport(ActiveDiagram, W.UI.Diagram.DiagramZoom, 200, 300);
                ActiveDiagram?.Refresh();

                // Ensure event is attached upon creation
                ActiveDiagram.SelectionChanged += OnDiagramSelectionChanged;
            }
        }
        public void Dispose()
        {
            if (W != null)
            {
                W.OnDashboardStateChanged -= HandleStateChanged;
            }
            if (ActiveDiagram != null)
            {
                ActiveDiagram.SelectionChanged -= OnDiagramSelectionChanged;
            }

            _layoutCancellationTokenSource?.Cancel();
            _layoutCancellationTokenSource?.Dispose();
        }
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (ActiveDiagram != null)
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
            if (isRendered == false || W.SelectedTable?.Name == null) { return; }

            try
            {
                options.Traversal.EnforceStrictness();
                W.Result.DiagramResultJarvis = await Task.Run(async () => await GoJarvis(graphTable, options));

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
            if (W.Result.DiagramResultJarvis != null)
            {
                await ProcessJarvisDiagram(token);
            }
        }
        private async Task ProcessJarvisDiagram(CancellationToken token)
        {
            var finalNodes = GetFinalExecutionNodes(W.Result.DiagramResultJarvis);
            var diagram = DiagramBuilder.CreateDiagramFromQueryTables(finalNodes, W.UI.Appearance.DiagramTheme == SqlEditorTheme.Light);

            var width =  ActiveDiagram?.Container?.Width ?? 1600;
            var height = ActiveDiagram?.Container?.Height ?? 800;

            if (W.UI.Diagram.ActiveParameters == null)
            {
                W.UI.Diagram.ApplyPreset(LayoutPresetType.JEO);
            }
            else
            {
                W.UI.Diagram.ActiveParameters.CanvasWidth = width;
                W.UI.Diagram.ActiveParameters.CanvasHeight = height;
            }

            var parameters = W.UI.Diagram.ActiveParameters;
            token.ThrowIfCancellationRequested();

            switch (W.UI.Diagram.LayoutShape)
            {
                case GraphLayoutShape.EntityRelation:
                    DiagramBuilder.ApplyForceDirectedLayout(diagram, new ERParameters() { CanvasHeight = height, CanvasWidth = width, IdealDistance = 20 });
                    break;
                case GraphLayoutShape.CircularStar:
                    await DiagramBuilder.ApplyGraphShapeLayoutCircular(diagram);
                    break;
                case GraphLayoutShape.FruchtermanReingold:
                    await DiagramBuilder.ApplyGraphShapeLayoutFruchtermanReingold(diagram, (FruchtermanReingoldParameters)parameters);
                    break;
                case GraphLayoutShape.ISOM:
                    await DiagramBuilder.ApplyGraphShapeLayoutISOM(diagram, (IsomParameters)parameters);
                    break;
                case GraphLayoutShape.KamadaKawai:
                    await DiagramBuilder.ApplyGraphShapeLayoutKamadaKawai(diagram, (KkParameters)parameters);
                    break;
                case GraphLayoutShape.Sugiyama:
                    await DiagramBuilder.ApplyGraphShapeLayoutSugiyama(diagram, (SugiyamaParameters)parameters);
                    break;
                case GraphLayoutShape.FruchtermanReingoldBounded:
                    await DiagramBuilder.ApplyGraphShapeLayoutFructermanReingoldBounded(diagram, (FruchtermanReingoldParameters)parameters);
                    break;
                case GraphLayoutShape.LinLog:
                    await DiagramBuilder.ApplyGraphShapeLayoutLinLog(diagram, (LinLogParameters)parameters);
                    break;
                case GraphLayoutShape.Random:
                    await DiagramBuilder.ApplyGraphShapeLayoutRandom(diagram, (RandomParameters)parameters);
                    break;
                case GraphLayoutShape.TreeBallon:
                    var selectedNodeVertex = diagram.Nodes.FirstOrDefault(v => ((TableNodeModel)v).IsRootNode);
                    if (selectedNodeVertex != null)
                    {
                        await DiagramBuilder.ApplyGraphShapeLayoutTreeBalloon(diagram, selectedNodeVertex, (TreeBalloonParameters)parameters);
                    }
                    break;
                case GraphLayoutShape.TreeDouble:
                    await DiagramBuilder.ApplyGraphShapeLayoutTreeDouble(diagram, (TreeDoubleParameters)parameters);
                    break;
                case GraphLayoutShape.TreeSimple:
                    await DiagramBuilder.ApplyGraphShapeLayoutTreeSimple(diagram, (TreeSimpleParameters)parameters);
                    break;
                case GraphLayoutShape.None:
                    break;
            }

            token.ThrowIfCancellationRequested();

            await InvokeAsync(() =>
            {
                if (ActiveDiagram != null)
                {
                    ActiveDiagram.Links.Clear();
                    ActiveDiagram.Nodes.Clear();

                    foreach (var node in diagram.Nodes) ActiveDiagram.Nodes.Add(node);
                    foreach (var link in diagram.Links) ActiveDiagram.Links.Add(link);

                    DiagramBuilder.SetViewport(ActiveDiagram, W.UI.Diagram.DiagramZoom, 300, 200);
                    ActiveDiagram.Refresh();
                }
                else
                {
                    ActiveDiagram = diagram;
                    ActiveDiagram.RegisterComponent<TableNodeModel, NodeTableNode>();
                    ActiveDiagram.SelectionChanged += OnDiagramSelectionChanged;
                    DiagramBuilder.SetViewport(ActiveDiagram, W.UI.Diagram.DiagramZoom, 300, 200);
                    ActiveDiagram.Refresh();
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
        private void OnDiagramSelectionChanged(SelectableModel model)
        {
            if (model is TableNodeModel clickedTable)
            {
                DiagramBuilder.HandleClusterSelection(ActiveDiagram, clickedTable);
            }
        }
        private async Task OnPresetChanged(object selectedPreset)
        {
            if (selectedPreset is LayoutPresetType preset)
            {
                W.UI.Diagram.ApplyPreset(preset);
                _parameterEditorRefreshTrigger++;
                await DebouncedReRunLayout();
            }
        }
        private async Task ExportDiagram()
        {
            if (ActiveDiagram == null) { return; }
            byte[] fileBytes = VisioDiagramExporter.Export(ActiveDiagram);

            // 2. Convert to a Base64 string directly in C# to safely feed into JS 
            // (This protects against SignalR byte-array fragmentation)
            var base64Data = Convert.ToBase64String(fileBytes);

            await JS.InvokeVoidAsync("downloadBase64File", "application/vnd.ms-visio.drawing", base64Data, "Diagram.vsdx");
        }

        private async Task OnDiagramZoomChanged(double value)
        {
            if (ActiveDiagram == null) { return; }
            DiagramBuilder.SetViewport(ActiveDiagram, Math.Abs(value));
            await InvokeAsync(StateHasChanged);
        }
        #endregion

        #region Toggle
        private void ToggleDiagramSidebar()
        {
            W.UI.Appearance.IsSidebarExpandedDiagram = !W.UI.Appearance.IsSidebarExpandedDiagram;
        }
        private async Task TogglePaneExpansionHalfScreen()
        {
            W.UI.Appearance.IsPaneExpandedResults = !W.UI.Appearance.IsPaneExpandedResults;
            await JS.InvokeVoidAsync("toggleElementHalfscreen", queryResultsPaneDiv, W.UI.Appearance.IsPaneExpandedResults);
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

        public async Task<QueryGenerationResult> GoJarvis(ITable graphTable, QueryGenerationOptions options)
        {
            options.ExecutionPlan = DiscoveryMode.WideNetExtended;
            options.Strategy = GenerationMethod.PlanDrivenExecution;
            options.ExecutionPlanStrategy = new ExecutionPlanStrategy()
            {
                StrategyName = "Jarvis Execution Plan Discovery",
                DiscoveryMode = DiscoveryMode.WideNetExtended,
                EdgePolicy = new EdgeSelectionPolicy()
                {
                    SortByIndexWeight = true,
                    // TEST 1: Flip this to false temporarily.
                    // If your node count jumps to 15+, your database schema simply lacks indexes on those FKs!
                    FilterUnindexedHazards = options.Traversal.StrictIndexMatchingOnly,
                },

                PostTraversalCompression = (tracker) =>
                {
                    // PHASE 2: Trim the fat using our strategy compression pass!
                    // Group all discovered nodes by their physical table ID
                    var tableGroups = tracker.Nodes
                        .Where(n => n.Direction != TraversalDirection.Root)
                        .GroupBy(n => n.TableObjectId)
                        .ToList();

                    foreach (var group in tableGroups)
                    {
                        if (group.Count() > 1)
                        {
                            // Calculate total path optimization penalty back to the root chain using local ValueTuples
                            var optimalNode = group
                                .Select(node =>
                                {
                                    int totalPathPenalty = 0;
                                    var current = node;
                                    while (current?.Relationship != null)
                                    {
                                        var evaluation = TraversalOptimizer.EvaluateJoinQuality(
                                            current.Relationship,
                                            current.Direction == TraversalDirection.Down
                                        );
                                        totalPathPenalty += evaluation.WeightPenalty;
                                        current = current.Parent;
                                    }
                                    return (Node: node, Depth: node.Depth, PathPenalty: totalPathPenalty);
                                })
                                // Pull the absolute winner based on index metrics, then depth
                                .OrderBy(tuple => tuple.PathPenalty)
                                .ThenBy(tuple => tuple.Depth)
                                .Select(tuple => tuple.Node)
                                .First();

                            // Cleanly remove any redundant duplicate nodes
                            foreach (var redundantNode in group)
                            {
                                if (redundantNode != optimalNode)
                                {
                                    tracker.Nodes.Remove(redundantNode);
                                }
                            }
                        }
                    }
                }
            };

            var jarvis = new DiagramDrivenGraphGenerator();
            // Run Johnny-5 / Jarvis
            QueryGenerationResult jarvisResult = jarvis.Generate(graphTable, options);
            return jarvisResult;
        }

        public List<QueryTable> GetFinalExecutionNodes(QueryGenerationResult result)
        {
            var allNodes = result.ExecutionNodes;
            if (allNodes == null || !allNodes.Any())
                return [];

            var activeGraph = DiagramBuilder.BuildActiveGraph(result.Tracker);

            var sourceNodeIdsWithChildren = activeGraph.Edges
                .Select(edge => edge.SourceNodeId)
                .ToHashSet();

            var terminalLeafNodes = activeGraph.Nodes
                .Where(node => !sourceNodeIdsWithChildren.Contains(node.NodeId))
                .ToList();

            var activePathNodeIds = new HashSet<Guid>();

            foreach (var leaf in terminalLeafNodes)
            {
                var current = leaf;
                activePathNodeIds.Add(current.NodeId);

                while (true)
                {
                    var incomingEdge = activeGraph.Edges
                        .FirstOrDefault(edge => edge.TargetNodeId == current.NodeId);

                    if (incomingEdge == null)
                        break;

                    var parentNode = activeGraph.Nodes
                        .FirstOrDefault(node => node.NodeId == incomingEdge.SourceNodeId);

                    if (parentNode == null)
                        break;

                    activePathNodeIds.Add(parentNode.NodeId);
                    current = parentNode;
                }
            }

            return allNodes
                .Where(node => activePathNodeIds.Contains(node.Id))
                .ToList();
        }
    }
}