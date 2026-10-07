using JEO3.Generation;
using JEO3.Schema;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace JEO3.Site.Dashboard
{
    public partial class TableNav : IDisposable
    {
        #region Properties
        [Inject] private WSWorkspace W { get; set; }
        [Inject] private IJSRuntime JS { get; set; }
        [Inject] private ContextMenuService ContextMenuService { get; set; } = default!;
        [Parameter] public EventCallback<ITable> OnGenerateSQLProceduresToEditor { get; set; }
        [Parameter] public EventCallback<ITable> OnGenerateAllDatabaseIndexesToEditor { get; set; }
        [Parameter] public EventCallback<ITable> OnGenerateMissingDatabaseIndexesToEditor { get; set; }
        [Parameter] public EventCallback<GenerationMethod> OnArchiveGeneratorSelected { get; set; }

        // Elements
        public RadzenDataGrid<ITable> gridTables = new();
        public RadzenDataGrid<IColumn> gridColumns = new();
        public RadzenDataGrid<IColumn> gridAllObjects = new();
        private ElementReference tablesTreePaneDiv;
        private ElementReference tablesPaneDiv;
        private ElementReference columnsPaneDiv;
        private ElementReference allColumnsPaneDiv;

        // Other
        private FilterCaseSensitivity FilterCaseSensitivity { get; set; } = FilterCaseSensitivity.CaseInsensitive;
        private CancellationTokenSource? _filterCts;
        private int selectedTableTabIndex;
        private bool _isDisposed;

        // Freeze Flags
        bool frozen = true;
        bool frozen2 = true;
        bool frozen3 = true;
        bool frozen4 = true;

        private const string TableGridId = "table-grid";

        #endregion

        #region Initialization

        protected override void OnInitialized()
        {
            // Subscribe to workspace events
            W.OnDashboardStateChanged += HandleWorkspaceUpdated;
            W.OnTableChanged += HandleTableChanged;
        }

        //private List<IObject> _treeItems;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (W.SelectedTable != null)
                {
                    var table = W.SelectedTable;
                    if (table != null && table.Columns.Any())
                    {
                        W.UI.Grids.Selected.Tables = [table];
                        W.UI.Grids.Core.Columns = table.Columns;

                        var firstColumn = table.Columns.First();
                        W.UI.Grids.Selected.Columns = [firstColumn];

                        // do the state work directly instead of calling OnColumnSelected,
                        // which triggers its own StateHasChanged
                        if (W.UI.Grids.AutoFilterRelations)
                        {
                            W.UI.Grids.SearchColumnName = firstColumn.Name;

                            if (gridAllObjects != null)
                            {
                                await gridAllObjects.Reload();
                            }
                        }
                    }
                }
                StateHasChanged();
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _filterCts?.Cancel();
            _filterCts?.Dispose();

            W.OnDashboardStateChanged -= HandleWorkspaceUpdated;
            W.OnTableChanged -= HandleTableChanged;
        }

        #endregion

        #region Global Events

        private async void HandleWorkspaceUpdated()
        {
            if (_isDisposed) return;
            await InvokeAsync(StateHasChanged);
        }

        #endregion

        #region Selected Table Changed

        private async void HandleTableChanged()
        {
            if (_isDisposed) return;
            // Optionally trigger grid reloads here if they don't auto-bind
            // gridTables?.Reload();
            await InvokeAsync(StateHasChanged);
        }

        #endregion

        #region Context Menu

        // On Ctx Open
        private async Task OnTableContextMenu(DataGridCellMouseEventArgs<ITable> args)
        {
            // args.Data gives you the exact table model for the row right-clicked
            ITable clickedTable = args.Data;
            await OpenTableContextMenu(args);
        }
        private async Task GenerateAllDatabaseIndexes(MenuItemEventArgs args, ITable table, bool missingOnly)
        {
            await OnGenerateAllDatabaseIndexesToEditor.InvokeAsync(table);
        }
        private async Task GenerateMissingDatabaseIndexes(MenuItemEventArgs args, ITable table, bool missingOnly)
        {
            await OnGenerateMissingDatabaseIndexesToEditor.InvokeAsync(table);
        }
        private async Task GenerateSQLProcedures(MenuItemEventArgs args, ITable table)
        {
            await OnGenerateSQLProceduresToEditor.InvokeAsync(table);
        }

        #endregion

        #region Selection

        private async Task OnTableRowSelected(ITable table)
        {
            W.SelectTable(table);


            if (table != null && table.Columns.Any())
            {
                W.UI.Grids.Selected.Tables = [table];
                W.UI.Grids.Core.Columns = table.Columns;

                var firstColumn = table.Columns.First();
                W.UI.Grids.Selected.Columns = [firstColumn];

                // do the state work directly instead of calling OnColumnSelected,
                // which triggers its own StateHasChanged
                if (W.UI.Grids.AutoFilterRelations)
                {
                    W.UI.Grids.SearchColumnName = firstColumn.Name;

                    if (gridAllObjects != null)
                    {
                        await gridAllObjects.Reload();
                    }
                }
            }

            W.NotifyMainLayoutStateChanged();
            StateHasChanged();
        }

        private async Task OnColumnSelected(IColumn column)
        {
            W.UI.Grids.Selected.Columns = [column];

            if (W.UI.Grids.AutoFilterRelations)
            {
                W.UI.Grids.SearchColumnName = column.Name;

                if (gridAllObjects != null)
                {
                    await gridAllObjects.Reload();
                }
            }

            //W.NotifyMainLayoutStateChanged();
            StateHasChanged();
        }


        private async Task OnColumnSearchFilterChange(object value)
        {
            _filterCts?.Cancel();
            _filterCts = new CancellationTokenSource();
            var token = _filterCts.Token;

            try
            {
                await Task.Delay(200, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            W.UI.Grids.SearchColumnName = value?.ToString();
            W.NotifyMainLayoutStateChanged();
            StateHasChanged();
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
                await OnTableRowSelected(tbl);
            }
        }
        private void OnTablesTabIndexChanged()
        {
            //
            bool stop = true;
        }
        #endregion

        #region Toggle Fullscreen
        private async Task TogglePaneExpansionFullScreen(ElementReference element)
        {
            W.UI.Appearance.IsPaneFullScreenResults = !W.UI.Appearance.IsPaneFullScreenResults;
            var isFullscreen = await JS.InvokeAsync<bool>("eval", "document.fullscreenElement != null");

            if (isFullscreen)
            {
                await JS.InvokeVoidAsync("toggleElementFullscreen", element, false);
            }
            else
            {
                await JS.InvokeVoidAsync("toggleElementFullscreen", element, true);
            }

            await InvokeAsync(StateHasChanged);
        }
        #endregion
    }
}
