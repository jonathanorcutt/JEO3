using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Components.Charting;

/// <summary>
/// Fully generic treemap. Doesn't know or care what the metric means - you decide that by
/// what you pass into ValueSelector. Sizing, coloring by group, sub-labels, and click-through
/// are all delegate-driven so this drops into any list of any shape.
/// </summary>
public partial class GenericTreeMap<TItem>
{
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Data { get; set; } = default!;

    [Parameter, EditorRequired]
    public Func<TItem, string> LabelSelector { get; set; } = default!;

    /// <summary>The metric. Whatever you put here is what sizes the rectangles - rows, columns, indexes, anything.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, double> ValueSelector { get; set; } = default!;

    /// <summary>Optional second line inside the cell (e.g. a formatted version of the metric).</summary>
    [Parameter]
    public Func<TItem, string>? SubLabelSelector { get; set; }

    /// <summary>Optional grouping key used purely for color assignment (e.g. schema name).</summary>
    [Parameter]
    public Func<TItem, string>? GroupKeySelector { get; set; }

    [Parameter]
    public EventCallback<TItem> OnCellClick { get; set; }

    [Parameter]
    public string Title { get; set; } = "TREEMAP";

    [Parameter]
    public double AspectWidth { get; set; } = 1480;

    [Parameter]
    public double AspectHeight { get; set; } = 850;

    /// <summary>Override the default palette if you want the cells to match a different accent set.</summary>
    [Parameter]
    public string[]? Palette { get; set; }

    private static readonly string[] DefaultPalette =
    {
        "#39ff14", "#00d9ff", "#ff3b6b", "#b388ff", "#ffd54f", "#ff8a3d", "#4dffdf", "#7c8cff"
    };

    private List<TreeMapItem<TItem>> _items = [];
    private TreeMapItem<TItem>? _hovered;
    private readonly Dictionary<string, string> _groupColors = [];

    protected override void OnParametersSet()
    {
        var raw = Data?.ToList() ?? [];

        _items = raw
            .Select(d => new TreeMapItem<TItem>
            {
                Label = LabelSelector(d),
                Value = Math.Max(0, ValueSelector(d)),
                SubLabel = SubLabelSelector?.Invoke(d),
                GroupKey = GroupKeySelector?.Invoke(d),
                Data = d
            })
            .Where(i => i.Value > 0)
            .ToList();

        AssignColors();
        Squarify.Layout(_items, 0, 0, AspectWidth, AspectHeight);
    }

    private double TotalValue => _items.Sum(i => i.Value);
    private double ViewWidth => AspectWidth;
    private double ViewHeight => AspectHeight;
    private string ContainerStyle => $"aspect-ratio: {AspectWidth.ToString(System.Globalization.CultureInfo.InvariantCulture)} / {AspectHeight.ToString(System.Globalization.CultureInfo.InvariantCulture)};";

    private void AssignColors()
    {
        _groupColors.Clear();
        var palette = Palette is { Length: > 0 } ? Palette : DefaultPalette;
        var groups = _items.Select(i => i.GroupKey ?? i.Label).Distinct().ToList();
        for (var i = 0; i < groups.Count; i++)
            _groupColors[groups[i]] = palette[i % palette.Length];
    }

    private string ColorFor(TreeMapItem<TItem> item)
    {
        return _groupColors.GetValueOrDefault(item.GroupKey ?? item.Label, DefaultPalette[0]);
    }

    private static string Truncate(string s, double widthPx)
    {
        var maxChars = Math.Max(1, (int)(widthPx / 7.2));
        return s.Length <= maxChars ? s : s[..Math.Max(0, maxChars - 1)] + "\u2026";
    }

    private async Task HandleClick(TreeMapItem<TItem> item)
    {
        if (OnCellClick.HasDelegate)
            await OnCellClick.InvokeAsync(item.Data);
    }
}
