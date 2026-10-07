using System.Text.Json.Serialization;
using JEO3.Monitor;
using JEO3.Providers;
using JEO3.Schema;

namespace JEO3.Site.Dashboard
{
    public sealed class WSWorkspace : IMonitorStateConsumer
    {
        #region Properties

        [JsonIgnore]
        public DatabaseContext Context { get; private set; }
        public WSUiState UI { get; } = new();
        [JsonIgnore]
        public WSGenerationResult Result { get; set; } = new();
        public WSMonitorState Observation { get; set; } = new();

        [JsonIgnore]
        public ITable SelectedTable => UI.Grids.Selected.Table;

        public bool IsPreviewMode { get; set; }

        #endregion

        #region Selected Table

        public void SelectTable(ITable table)
        {
            UI.Grids.Selected.Table = table;
            UI.Grids.Selected.Tables = [table];
            OnTableChanged?.Invoke();
        }

        #endregion

        #region Monitor


        #endregion

        #region Events

        public event Action? OnTableChanged;
        public event Action? OnMainLayoutStateChanged;
        public event Action? OnDashboardStateChanged;
        /// <summary>Fired by SqlServerMonitorService after each poll cycle.</summary>
        public event Action<MonitorState>? OnMonitorStateChanged;
        public void NotifyMainLayoutStateChanged() => OnMainLayoutStateChanged?.Invoke();
        public void NotifyDashboardStateChanged() => OnDashboardStateChanged?.Invoke();
        public void NotifyMonitorStateChanged(MonitorState state) => OnMonitorStateChanged(state);//.Invoke();

        #endregion

        #region Set Property Methods

        public void LoadSchemaContext(DatabaseContext context, IDatabaseProvider provider)
        {
            this.Context = context;
            this.UI.Grids.Core.Tables = context.Tables;
            this.UI.Grids.Core.Relations = context.Relations.SelectMany(v => v.ColumnPairs).ToList();
            this.UI.Grids.Core.StoredProcedureDefinitions = context.Procedures;
            this.UI.Grids.Core.ViewDefinitions = context.Views;
            this.UI.Grids.Core.AllObjects = context.Columns
                    .GroupBy(v => new { v.TableName, v.Name }).Select(v => v.First()).Distinct()
                    .OrderBy(v => v.TableName).ThenBy(v => v.Name).ToList();
            this.UI.Appearance.IsSchemaLoaded = true;
        }

        #endregion
    }
}