using JEO3.Generation.Models;
using JEO3.Schema;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JEO3.Site.Dashboard
{
    public partial class SchemaToolbox : IDisposable
    {
        #region Properties

        [Inject]
        private WSWorkspace W { get; set; }

        [Parameter]
        public EventCallback<IProcedure> OnStoredProcedureDefinitionSelected { get; set; }
        [Parameter]
        public EventCallback<IView> OnViewDefinitionSelected { get; set; }
        [Parameter]
        public EventCallback<object> OnColumnSearchFilterChange { get; set; }

        public RadzenDataGrid<Table> gridTables = new();
        public RadzenDataGrid<Column> gridColumns = new();
        public RadzenDataGrid<Column> gridAllObjects = new();
        public RadzenDataGrid<IRelationColumnPair> gridTableRelations = new();
        public RadzenDataGrid<IProcedure> gridStoredProcedureDefinitions = new();
        public RadzenDataGrid<IView> gridViewDefinitions = new();
        private bool _isDisposed;
        #endregion

        #region Initialization

        protected override void OnInitialized()
        {
            W.OnDashboardStateChanged += HandleWorkspaceUpdated;
            W.OnTableChanged += HandleTableChanged;
        }

        public void Dispose()
        {
            if (W != null)
            {
                W.OnDashboardStateChanged -= HandleWorkspaceUpdated;
                W.OnTableChanged -= HandleTableChanged;
            }
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
            await InvokeAsync(StateHasChanged);
        }

        #endregion

        #region Selection

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

            StateHasChanged();
        }

        #endregion

        #region Toggle Constraints

        private async Task ToggleConstraint(GraphRelationConstraint constraint, bool isEnabled)
        {
            constraint.IsDisabled = !isEnabled;
            W.NotifyDashboardStateChanged();

            // FIX 5: Force refresh after updating constraints
            StateHasChanged();
        }

        #endregion

        #region Auto Filter Events

        private async Task AutoFilterAllObjectsChanged(bool value)
        {
            if (W.UI.Grids.AutoFilterAllObjects == false)
            {
                W.UI.Grids.SearchColumnName = string.Empty;
                W.UI.Grids.Core.AllObjects = W.Context.Columns.ToList();
                await gridAllObjects?.Reload();
            }
            else if (W.UI.Grids.Selected.Columns.Any())
            {
                W.UI.Grids.SearchColumnName = W.UI.Grids.Selected.Columns.First().Name;
            }
        }

        private async Task AutoFilterRelationsChanged(bool value)
        {
            W.UI.Grids.AutoFilterRelations = value;

            if (W.UI.Grids.AutoFilterRelations && W.SelectedTable != null)
            {
                W.UI.Grids.Core.Relations = W.Context.Relations
                    .Where(v => v.ParentTable.Name == W.SelectedTable.Name || v.ReferencedTable.Name == W.SelectedTable.Name).SelectMany(v => v.ColumnPairs).ToList();

                await gridTableRelations.Reload();
            }
            else if (W.UI.Grids.AutoFilterRelations == false)
            {
                W.UI.Grids.Core.Relations = W.Context.Relations.SelectMany(v => v.ColumnPairs).ToList();
                await gridTableRelations.Reload();
            }
        }

        #endregion
    }
}
