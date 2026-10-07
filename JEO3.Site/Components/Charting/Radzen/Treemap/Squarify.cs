namespace JEO3.Site.Components.Charting;

/// <summary>
/// Squarified treemap layout (Bruls, Huizing, van Wijk 1999). Pure math, no UI dependency,
/// so it's reusable outside Blazor too if you ever want it in a console tool or a test.
/// </summary>
public static class Squarify
{
    private readonly record struct Rect(double X, double Y, double W, double H);

    public static void Layout<TItem>(IReadOnlyList<TreeMapItem<TItem>> items, double x, double y, double w, double h)
    {
        if (items.Count == 0 || w <= 0 || h <= 0) return;

        var sorted = items.OrderByDescending(i => i.Value).ToList();
        var total = sorted.Sum(i => i.Value);
        if (total <= 0) total = 1;

        var area = w * h;
        foreach (var item in sorted)
            item.NormalizedValue = item.Value / total * area;

        SquarifyRows(sorted, new Rect(x, y, w, h));
    }

    private static void SquarifyRows<TItem>(List<TreeMapItem<TItem>> children, Rect rect)
    {
        var remaining = new List<TreeMapItem<TItem>>(children);
        var currentRect = rect;
        var row = new List<TreeMapItem<TItem>>();

        while (remaining.Count > 0)
        {
            var shortSide = Math.Min(currentRect.W, currentRect.H);
            var next = remaining[0];

            var rowValues = row.Select(i => i.NormalizedValue).ToList();
            var testValues = new List<double>(rowValues) { next.NormalizedValue };

            if (row.Count == 0 || Worst(rowValues, shortSide) >= Worst(testValues, shortSide))
            {
                row.Add(next);
                remaining.RemoveAt(0);
            }
            else
            {
                currentRect = LayoutRow(row, currentRect);
                row.Clear();
            }
        }

        if (row.Count > 0)
            LayoutRow(row, currentRect);
    }

    /// <summary>Worst (highest) aspect ratio that would result from laying out this row.</summary>
    private static double Worst(List<double> row, double shortSide)
    {
        if (row.Count == 0) return double.MaxValue;

        var sum = row.Sum();
        var max = row.Max();
        var min = row.Min();
        var s2 = sum * sum;
        var w2 = shortSide * shortSide;

        return Math.Max(w2 * max / s2, s2 / (w2 * min));
    }

    /// <summary>Places one row of items along the shorter side of the remaining rect, returns the leftover rect.</summary>
    private static Rect LayoutRow<TItem>(List<TreeMapItem<TItem>> row, Rect rect)
    {
        var rowArea = row.Sum(i => i.NormalizedValue);

        if (rect.W < rect.H)
        {
            // Lay the row out horizontally across the top of the rect.
            var rowHeight = rowArea / rect.W;
            var cursor = rect.X;
            foreach (var item in row)
            {
                var itemWidth = item.NormalizedValue / rowHeight;
                item.X = cursor;
                item.Y = rect.Y;
                item.W = itemWidth;
                item.H = rowHeight;
                cursor += itemWidth;
            }
            return rect with { Y = rect.Y + rowHeight, H = rect.H - rowHeight };
        }
        else
        {
            // Lay the row out vertically down the left of the rect.
            var rowWidth = rowArea / rect.H;
            var cursor = rect.Y;
            foreach (var item in row)
            {
                var itemHeight = item.NormalizedValue / rowWidth;
                item.X = rect.X;
                item.Y = cursor;
                item.W = rowWidth;
                item.H = itemHeight;
                cursor += itemHeight;
            }
            return rect with { X = rect.X + rowWidth, W = rect.W - rowWidth };
        }
    }
}
