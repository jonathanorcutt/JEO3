using JEO3.Schema;
using JEO3.Site.Components.Charting;
using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Dashboard
{
    public partial class ContourTemplateDisplay : ComponentBase, IDisposable
    {
        #region Propeties
        private int selectedEntitiesNavTabIndex = 0;
        private static IReadOnlyList<string> MetricNames => GetTableCaps().Keys.ToList();
        private bool _rendered { get; set; }
        private bool _isDisposed { get; set; }
        #endregion

        #region Initiillization
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _rendered = true;
                await InvokeAsync(StateHasChanged);
            }
        }
        protected override void OnInitialized()
        {
            W.OnDashboardStateChanged += HandleStateChanged;
        }

        private async void HandleStateChanged()
        {
            if (_isDisposed) return;
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            if (W != null)
            {
                W.OnDashboardStateChanged -= HandleStateChanged;
            }
            _isDisposed = true;
        }

        #endregion

        #region Charts
        // --- VIEW 2: COLUMN METRIC VECTOR ---
        public static readonly IReadOnlyList<string> ColumnMetrics = new[]
        {
            "IsPrimaryKey", "IsForeignKey", "IsNullable", "IsIndexed"
        };

        private double GetContourNormalizedMetric(ITable t, string metric)
        {
            double raw = metric switch
            {
                "Columns" => t.Columns.Count,
                "Indexes" => t.Indexes.Count,
                "Foreign Keys" => t.ForeignKeys.Count,
                "Referenced By FKs" => t.ReferencedByForeignKeys.Count,
                "Triggers" => t.Triggers.Count,
                "Check Constraints" => t.CheckConstraints.Count,
                "Parents" => t.ParentRelations.Count,
                "Children" => t.ChildRelations.Count,
                "Statistics" => t.Statistics.Count,
                "Missing Indexes" => t.MissingIndexes.Count,
                _ => 0
            };

            return Math.Min(1.0, raw / GetTableCaps()[metric]);
        }

        private void UpdateEntitiesNavTabChanged(int index)
        {
            selectedEntitiesNavTabIndex = index;
        }

        private static Dictionary<string, double> GetTableCaps()
        {
            return new()
            {
                ["Columns"] = 50,
                ["Indexes"] = 15,
                ["Foreign Keys"] = 15,
                ["Referenced By FKs"] = 15,
                ["Triggers"] = 10,
                ["Check Constraints"] = 10,
                ["Parents"] = 15,
                ["Children"] = 15,
                ["Statistics"] = 15,
                ["Missing Indexes"] = 10
            };
        }
        public static double GetColumnMetric(IColumn col, string metric)
        {
            if (col == null) return 0;
            return metric switch
            {
                "IsPrimaryKey" => col.IsPrimaryKey ? 1.0 : 0.0,
                "IsForeignKey" => col.IsForeignKey ? 1.0 : 0.0,
                "IsNullable" => col.IsNullable ? 1.0 : 0.0,
                "IsIndexed" => col.IsIndexed ? 1.0 : 0.0,
                _ => 0.0
            };
        }

        // --- VIEW 3: MISSING INDEX METRIC VECTOR ---
        public static readonly IReadOnlyList<string> MissingIndexMetrics = new[]
        {
            "UserSeeks", "UserScans", "AvgUserImpact", "Score"
        };
        public static double GetMissingIndexMetric(IMissingIndex idx, string metric)
        {
            if (idx == null) return 0;
            return metric switch
            {
                "UserSeeks" => Math.Clamp((double)(idx.UserSeeks ?? 0) / 100_000.0, 0.0, 1.0),
                "UserScans" => Math.Clamp((double)(idx.UserScans ?? 0) / 10_000.0, 0.0, 1.0),
                "AvgUserImpact" => Math.Clamp((double)(idx.AvgUserImpact ?? 0) / 100.0, 0.0, 1.0),
                "Score" => Math.Clamp((double)idx.Score / 1_000_000.0, 0.0, 1.0),
                _ => 0.0
            };
        }

        #endregion
    }
}
