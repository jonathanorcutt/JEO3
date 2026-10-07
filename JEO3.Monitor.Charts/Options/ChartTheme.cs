using JEO3.Core;

namespace JEO3.Monitor.ECharts
{
    public sealed class ChartTheme
    {
        public string Name { get; set; } = "Matrix Green";
        public string BackgroundColor { get; set; } = "#0a0c10";
        public string TextColor { get; set; } = ColorHex.White;
        public string TextDimColor { get; set; } = "#a0a0a0";
        public string AxisLineColor { get; set; } = "#00ff41";
        public string GridLineColor { get; set; } = "rgba(0, 255, 65, 0.12)";
        public string TooltipBg { get; set; } = "rgba(10, 12, 16, 0.92)";
        public string TooltipBorder { get; set; } = "#00ff41";

        public string FontFamily { get; set; } = "Consolas, 'Courier New', ui-monospace, monospace";
        public int AxisFontSize { get; set; } = 11;
        public int LegendFontSize { get; set; } = 11;

        // Series Color Palette
        public string SeriesPrimary { get; set; } = ColorHex.MatrixGreen;
        public string SeriesSecondary { get; set; } = ColorHex.NeonBlue;
        public string SeriesTertiary { get; set; } = ColorHex.Purple;
        public string SeriesQuaternary { get; set; } = ColorHex.Orange;
        public string CriticalRed { get; set; } = ColorHex.DarkRed;

        #region Presets
        public static ChartTheme MatrixGreen => new()
        {
            Name = "Matrix Green",
            BackgroundColor = "#0a0c10",
            TextColor = ColorHex.MatrixGreen,
            AxisLineColor = "#00ff41",
            GridLineColor = "rgba(0, 255, 65, 0.12)",
            TooltipBorder = "#00ff41",
            SeriesPrimary = ColorHex.MatrixGreen,
            SeriesSecondary = ColorHex.NeonBlue,
            SeriesTertiary = ColorHex.Purple,
            SeriesQuaternary = ColorHex.Orange
        };
        public static ChartTheme CyberBlue => new()
        {
            Name = "Cyber Blue",
            BackgroundColor = "#080e18",
            TextColor = ColorHex.ElectricDodger,
            AxisLineColor = "#0084FF",
            GridLineColor = "rgba(0, 132, 255, 0.15)",
            TooltipBorder = "#0084FF",
            SeriesPrimary = ColorHex.ElectricDodger,
            SeriesSecondary = ColorHex.NeonLime,
            SeriesTertiary = ColorHex.BrightPurple,
            SeriesQuaternary = ColorHex.AmberOrange
        };
        public static ChartTheme VaporwavePink => new()
        {
            Name = "Vaporwave Pink",
            BackgroundColor = "#120814",
            TextColor = ColorHex.WarmPink,
            AxisLineColor = "#FF69B4",
            GridLineColor = "rgba(255, 105, 180, 0.15)",
            TooltipBorder = "#FF69B4",
            SeriesPrimary = ColorHex.WarmPink,
            SeriesSecondary = ColorHex.AlienLime,
            SeriesTertiary = ColorHex.ElectricPurple,
            SeriesQuaternary = ColorHex.Yellow
        };
        public static ChartTheme HighContrastDark => new()
        {
            Name = "High Contrast",
            BackgroundColor = "#000000",
            TextColor = ColorHex.PureWhite,
            AxisLineColor = "#FFFFFF",
            GridLineColor = "rgba(255, 255, 255, 0.2)",
            TooltipBorder = "#FFFFFF",
            SeriesPrimary = ColorHex.VividLime,
            SeriesSecondary = ColorHex.DodgerBlue,
            SeriesTertiary = ColorHex.WarmPinky,
            SeriesQuaternary = ColorHex.Yellow
        };
        public static List<ChartTheme> AllThemes => new()
        {
            HighContrastDark, MatrixGreen, CyberBlue, VaporwavePink
        };
        #endregion

        #region Helpers
        public static ChartTheme FromEnum(ChartThemeType themeType) => themeType switch
        {
            ChartThemeType.Default => HighContrastDark,
            ChartThemeType.Green => MatrixGreen,
            ChartThemeType.Blue => CyberBlue,
            ChartThemeType.Pink => VaporwavePink,
            _ => HighContrastDark
        };
        #endregion
    }
}
