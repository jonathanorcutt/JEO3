namespace JEO3.Site.Components.Charting;

/// <summary>
/// A single laid-out node in the treemap. Generic over <typeparamref name="TItem"/> so the
/// component never needs to know whether it's wrapping an ITable, IColumn, or anything else -
/// it just carries the original item alongside the numbers the layout needs.
/// </summary>
public sealed class TreeMapItem<TItem>
{
    public required string Label { get; init; }
    public required double Value { get; init; }
    public string? GroupKey { get; init; }
    public string? SubLabel { get; init; }
    public TItem? Data { get; init; }

    // Area in layout units after normalizing Value to the container's pixel area.
    internal double NormalizedValue { get; set; }

    public double X { get; internal set; }
    public double Y { get; internal set; }
    public double W { get; internal set; }
    public double H { get; internal set; }
}
