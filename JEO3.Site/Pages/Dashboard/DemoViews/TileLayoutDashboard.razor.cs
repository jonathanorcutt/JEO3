using JEO3.Schema;
using JEO3.Site.Components.Charting;
using JEO3.Site.Dashboard;
using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Components.TileLayout
{
    public partial class TileLayoutDashboard
    {
        #region Properties
        [Inject] private WSWorkspace W { get; set; } = new();
        private bool editMode = true;
        private bool showGrid = true;
        public string StorageKey { get; set; } = "user_homepage_grid";
        private List<ITable> treeTables = new();

        #endregion

        #region RenderFragments
        private RenderFragment GetHeatmapTile() => __builder =>
        {
            __builder.OpenComponent<Heatmap<ITable>>(0);
            __builder.AddAttribute(1, "Items", W.Context?.Tables);
            __builder.AddAttribute(2, "Title", "Schema Heat Map");
            __builder.AddAttribute(3, "LabelSelector", (Func<ITable, string>)(t => t.TablePath));
            __builder.AddAttribute(4, "ValueSelector", GetTableMetric);
            __builder.AddAttribute(5, "CellWidth", 228);
            __builder.AddAttribute(6, "CellHeight", 30);
            __builder.CloseComponent();
        };
        private RenderFragment GetTreemapTile() => __builder =>
        {
            __builder.OpenComponent<GenericTreeMap<ITable>>(0);
            __builder.AddAttribute(1, "Data", treeTables);
            __builder.AddAttribute(3, "LabelSelector", (Func<ITable, string>)(t => t.TablePath));
            __builder.AddAttribute(4, "ValueSelector", (Func<ITable, double>)(t => (double)(t.Rows ?? 0)));
            __builder.AddAttribute(5, "SubLabelSelector", (Func<ITable, string>)(t => (t.Rows ?? 0).ToString("N0")));
            __builder.AddAttribute(6, "GroupKeySelector", (Func<ITable, string>)(t => t.TablePath.Split('.')[0]));
            __builder.CloseComponent();
        };
        private RenderFragment GetJarvisDiagram() => __builder =>
        {
            __builder.OpenComponent<DiagramViewerJarvis>(0);
            __builder.CloseComponent();
        };
        private RenderFragment GetERDiagram() => __builder =>
        {
            __builder.OpenComponent<DiagramViewerDatabase>(0);
            __builder.CloseComponent();
        };
        private RenderFragment GetQueryEditor() => __builder =>
        {
            __builder.OpenComponent<QueryEditor>(0);
            __builder.AddAttribute(1, "Id", $"txtQueryEditor-{Guid.NewGuid()}");
            __builder.CloseComponent();
        };
        private RenderFragment GetJEOGrid() => __builder =>
        {
            __builder.OpenComponent<JEOGrid>(0);
            __builder.AddAttribute(1, "DataTable", W.Result.Databars.Tables);
            __builder.AddAttribute(2, "PrimaryKeyColumn", "PrimaryKeyColumn");
            __builder.AddAttribute(3, "OnRowClick",
                Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this,
                    async ((object? a, string? b) row) => { await HandleTableGridDrillDown((row.a, row.b)); })
                );
            __builder.AddAttribute(4, "AllowVirtualization", true);
            __builder.CloseComponent();
        };
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
        }
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


        #region Contour
        private static IReadOnlyList<string> MetricNames => GetTableCaps().Keys.ToList();
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




        #region Commented

        // new GenericTileItem
        // {
        // 	Id = "analytics_widget9",
        // 	Title = "Insights",
        // 	Icon = "analytics",
        // 	Col = 1, Row = 1, ColSpan = 33, RowSpan = 19,
        //    ChildContent = @<TaskRateChart/>
        // },
        // new GenericTileItem
        // {
        // 	Id = "analytics_widget1",
        // 	Title = "Insights",
        // 	Icon = "analytics",
        // 	Col = 1, Row = 58, ColSpan = 64, RowSpan = 5,
        //    ChildContent = GetJEOGrid()
        // },
        // new GenericTileItem
        // {
        // 	Id = "analytics_widget2",
        // 	Title = "Insights",
        // 	Icon = "analytics",
        // 	Col = 13, Row = 1, ColSpan = 12, RowSpan = 4,
        //    ChildContent = GetQueryEditor()
        // },
        // new GenericTileItem
        // {
        // 	Id = "analytics_widget6",
        // 	Title = "Insights",
        // 	Icon = "analytics",
        // 	Col = 1, Row = 1, ColSpan = 12, RowSpan = 4,
        //    ChildContent = @<LoadingCubeMonitor/>
        // },
        // new GenericTileItem
        // {
        // 	Id = "analytics_widget2",
        // 	Title = "Insights",
        // 	Icon = "analytics",
        // 	Col = 34, Row = 1, ColSpan = 33, RowSpan = 19,
        //    ChildContent = @<ActiveRequestsChart/>
        // },
        // new GenericTileItem
        // {
        // 	Id = "analytics_widget8",
        // 	Title = "Insights",
        // 	Icon = "analytics",
        // 	Col = 1, Row = 1, ColSpan = 12, RowSpan = 4,
        //    ChildContent = @<ActiveRequestsGrid/>
        // },
        // new GenericTileItem
        // {
        // 	Id = "analytics_widget7",
        // 	Title = "Insights",
        // 	Icon = "analytics",
        // 	Col = 1, Row = 1, ColSpan = 33, RowSpan = 19,
        //    ChildContent = @<ActiveRequestsGrid/>
        // },
        #endregion
    }
}
