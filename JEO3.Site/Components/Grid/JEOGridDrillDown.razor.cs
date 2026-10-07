// JEOGridDrillDown.razor.cs
using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JEO3.Site.Dashboard
{
    public partial class JEOGridDrillDown : IDisposable
    {
        private const string KeyCol = "PrimaryKeyColumn";

        [Inject] private WSWorkspace W { get; set; } = new();

        [Parameter] public DataTable? DataTable { get; set; }        // Tables
        [Parameter] public DataTable? ChildDataTable { get; set; }   // Columns
        [Parameter] public int PrimaryColumnWidth { get; set; } = 300;
        [Parameter] public bool AllowVirtualization { get; set; }
        [Parameter] public EventCallback<(object? Id, string? Name)> OnRowClick { get; set; }
        [Parameter] public EventCallback<object?> OnTableSelectionChanged { get; set; }

        private ILookup<string, DataRow> _childByKey = Enumerable.Empty<DataRow>().ToLookup(r => "");
        private IEnumerable<IDictionary<string, object>>? _gridData;
        private Dictionary<string, (double min, double max)> _columnMinMaxCache = new(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, IEnumerable<IDictionary<string, object>>> _mappedChildDataCache = new();
        public IEnumerable<IDictionary<string, object>> GetCachedMappedChildData(IDictionary<string, object> parentRow)
        {
            var keyStr = Key(GetParentKeyValue(parentRow));
            if (string.IsNullOrEmpty(keyStr))
            {
                return Enumerable.Empty<IDictionary<string, object>>();
            }

            if (!_mappedChildDataCache.TryGetValue(keyStr, out var cachedData))
            {
                var childTable = GetChildDataTable(parentRow);
                cachedData = MapDataTable(childTable);
                _mappedChildDataCache[keyStr] = cachedData;
            }

            return cachedData;
        }
        protected override void OnParametersSet()
        {
            //_gridData = MapDataTable(DataTable);
            //BuildColumnMinMaxCache();

            //_childByKey = ChildDataTable != null && ChildDataTable.Columns.Contains(KeyCol)
            //    ? ChildDataTable.AsEnumerable().ToLookup(r => Key(r[KeyCol]))
            //    : Enumerable.Empty<DataRow>().ToLookup(r => "");


            _gridData = MapDataTable(DataTable);
            BuildColumnMinMaxCache();
            _mappedChildDataCache.Clear(); // <-- Add this line

            _childByKey = ChildDataTable != null && ChildDataTable.Columns.Contains(KeyCol)
                ? ChildDataTable.AsEnumerable().ToLookup(r => Key(r[KeyCol]))
                : Enumerable.Empty<DataRow>().ToLookup(r => "");
        }

        private static string Key(object? v) => (v == null || v == DBNull.Value) ? "" : v.ToString()!;

        private bool HasChildRows(IDictionary<string, object> parentRow) =>
            parentRow.TryGetValue(KeyCol, out var v) && _childByKey[Key(v)].Any();

        private DataTable? GetChildDataTable(IDictionary<string, object> parentRow)
        {
            if (ChildDataTable == null || !parentRow.TryGetValue(KeyCol, out var v)) return null;

            var rows = _childByKey[Key(v)].ToList();
            if (rows.Count == 0) return null;

            var segment = ChildDataTable.Clone();
            foreach (var row in rows) segment.ImportRow(row);
            return segment;
        }

        private void RenderParentRow(RowRenderEventArgs<IDictionary<string, object>> args)
        {
            args.Expandable = HasChildRows(args.Data);
        }

        private Task ParentRowExpanded(IDictionary<string, object> row)
        {
            if (row == null)
            {
                return Task.CompletedTask;
            }
            object? id = GetParentKeyValue(row);

            if (id != null && OnTableSelectionChanged.HasDelegate)
            {
                //return OnTableSelectionChanged.InvokeAsync(id);
            }

            return Task.CompletedTask;
        }

        private async Task OpenDrillDown(object? id, string? name)
        {
            if (OnRowClick.HasDelegate)
            {
                await OnRowClick.InvokeAsync((id, name));
            }

            if (id != null && OnTableSelectionChanged.HasDelegate)
            {
                await OnTableSelectionChanged.InvokeAsync(id);
            }
        }

        private object? GetParentKeyValue(IDictionary<string, object> row)
        {
            if (row.ContainsKey(KeyCol))
            {
                return row[KeyCol];
            }
            return null;
        }

        private IEnumerable<IDictionary<string, object>> MapDataTable(DataTable? dt)
        {
            var list = new List<IDictionary<string, object>>();
            if (dt == null) return list;

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }
                list.Add(dict);
            }
            return list;
        }

        private string GetKeyText(object val) { return val?.ToString()?.Trim() ?? ""; }

        private bool TryGetDouble(object value, out double val)
        {
            val = 0; if (value == null || value == DBNull.Value) return false;
            return double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out val);
        }

        private void BuildColumnMinMaxCache()
        {
            _columnMinMaxCache.Clear();
            if (DataTable == null) return;

            foreach (DataColumn col in DataTable.Columns)
            {
                _columnMinMaxCache[col.ColumnName] = GetColumnMinMax(DataTable, col.ColumnName);
            }
        }

        private (double min, double max) GetColumnMinMax(DataTable? dt, string colName)
        {
            if (dt == null || dt.Rows.Count == 0 || !dt.Columns.Contains(colName))
            {
                return (0, 0);
            }
            double min = double.MaxValue;
            double max = double.MinValue;
            bool hasValues = false;
            foreach (DataRow row in dt.Rows)
            {
                var valObj = row[colName];
                if (TryGetDouble(valObj, out double val))
                {
                    if (val < min) min = val;
                    if (val > max) max = val;
                    hasValues = true;
                }
            }
            return hasValues ? (min, max) : (0, 0);
        }

        private (double min, double max) GetCachedColumnMinMax(string colName)
        {
            return _columnMinMaxCache.TryGetValue(colName, out var cached) ? cached : (0, 0);
        }

        private Dictionary<string, Dictionary<string, (double min, double max)>> _childColumnMinMaxCache = new(StringComparer.OrdinalIgnoreCase);

        private (double min, double max) GetChildColumnMinMax(DataTable? childTable, string colName)
        {
            try
            {
                if (childTable == null) return (0, 0);

                string cacheKey = $"{childTable.Rows.Count}_{colName}";

                if (!_childColumnMinMaxCache.ContainsKey(cacheKey))
                {
                    _childColumnMinMaxCache[cacheKey] = new Dictionary<string, (double, double)>();
                    foreach (DataColumn col in childTable.Columns)
                    {
                        _childColumnMinMaxCache[cacheKey][col.ColumnName] = GetColumnMinMax(childTable, col.ColumnName);
                    }
                }

                return _childColumnMinMaxCache[cacheKey].TryGetValue(colName, out var cached) ? cached : (0, 0);
            }
            catch (Exception ex)
            {
                var gptSucks = $"Error in GetChildColumnMinMax: {ex.Message}";
                return (0, 0);
            }
        }

        private string GetDynamicRainbowColor(int index, int total)
        {
            if (total <= 0) return "#0078d4";
            double hue = (double)index / total * 360;
            return $"hsl({hue}, 70%, 45%)";
        }

        private static string GetColumnPropertyExpression(string name, Type type)
        {
            var expression = $@"it[""{name}""].ToString()";

            if (type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(short) ||
                type == typeof(byte))
            {
                return $"int.Parse({expression})";
            }

            if (type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(float))
            {
                return $"double.Parse({expression})";
            }

            if (type == typeof(DateTime))
            {
                return $"DateTime.Parse({expression})";
            }

            return expression;
        }
        private void OnChildSort(DataGridColumnSortEventArgs<IDictionary<string, object>> args, IDictionary<string, object> parentRow)
        {
            var keyStr = Key(GetParentKeyValue(parentRow));
            if (!_mappedChildDataCache.TryGetValue(keyStr, out var currentData)) return;

            if (args.SortOrder == null) return;

            // Use the actual property name without the "it[]" wrapper for own C# dictionary sorting
            var propertyName = args.Column.Property.Replace("it[\"", "").Replace("\"]", "");

            object GetValue(IDictionary<string, object> row) => row.TryGetValue(propertyName, out var val) ? val : null;

            int CompareSafe(object x, object y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return Comparer<object>.Create(CompareValues).Compare(x, y);
            }

            var sortedData = args.SortOrder == SortOrder.Ascending
                ? currentData.OrderBy(row => GetValue(row), Comparer<object>.Create(CompareSafe))
                : currentData.OrderByDescending(row => GetValue(row), Comparer<object>.Create(CompareSafe));

            _mappedChildDataCache[keyStr] = sortedData.ToList();
        }

        private int CompareValues(object? x, object? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            if (x is IComparable cx && y is IComparable cy)
            {
                try { return cx.CompareTo(cy); } catch { }
            }
            return string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        public void Dispose() { }
    }
}