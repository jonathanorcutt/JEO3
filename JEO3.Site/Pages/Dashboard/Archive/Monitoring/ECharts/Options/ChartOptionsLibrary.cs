using JEO3.Core;
using PanoramicData.ECharts;

namespace JEO3.Site.Components.ECharts
{
    public class ChartOptionsLibrary
    {
        #region ActiveRequestsChart
        public static ChartOptions ActiveRequestsChart_Options { get; set; } = new()
        {
            // Deep cyberpunk dark background
            BackgroundColor = "#0a0c10",

            Tooltip = new PanoramicData.ECharts.Tooltip
            {
                Trigger = TooltipTrigger.Axis,
                BackgroundColor = "rgba(10, 12, 16, 0.85)", // Semi-transparent dark
                BorderColor = "#1e90ff",                    // Dodger blue border
                BorderWidth = 1,
                TextStyle = new PanoramicData.ECharts.TextStyle { Color = new Color("#ffffff") }
            },

            Grid = new Grid { Left = "3%", Right = "4%", Bottom = "10%", Top = "8%", ContainLabel = true },

            XAxis = new XAxis
            {
                Type = AxisType.Category,
                Data = new List<object>(),
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color("#1e90ff") } }, // Neon blue axis line
                AxisLabel = new AxisLabel { Color = new Color("#6495ed") } // Cornflower blue text
            },

            YAxis = new YAxis
            {
                Type = AxisType.Value,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color("#1e90ff") } },
                AxisLabel = new AxisLabel { Color = new Color("#6495ed") },
                SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(30, 144, 255, 0.1") } } // Subtle blue grid matrix
            },

            Series = new List<ISeries>
    {
        new BarSeries
        {
            Name = "wait (s)",
            Data = new List<object>(),

// Cyberpunk Neon Blue Main Bars
            ItemStyle = new ItemStyle
            {
                Color =  new Color("#1e90ff"),       // Intense Dodger Blue for active data
                Color0 =  new Color("#6495ed"),      // Cornflower Blue variant
                BorderColor =  new Color("#00ffff"), // Cyber cyan sharp edge highlight
                BorderWidth = 1
            },

// High-voltage Neon Magenta warning line
            MarkLine = new MarkLine
            {
                Data = new List<object>
                {
                    new { yAxis = 300000, name = "game over" }
                },
                LineStyle = new LineStyle
                {
                    Color =  new Color("#ff007f"),   // Neon pink / magenta game over threshold
                    Width = 2,
                    Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed)
                },
                Label = new Label
                {
                    Show = true,
                    Color = new Color("#ff007f"),
                    Position = LabelPosition.Outside
                }
            }
        }
    }
        };
        #endregion

        #region BlockingChart
        public static ChartOptions BlockingChart_Options { get; set; } = new()
        {
            Tooltip = new PanoramicData.ECharts.Tooltip { Trigger = TooltipTrigger.Axis },
            Grid = new Grid { Left = "1%", Right = "1%", Bottom = "25%", Top = "8%", ContainLabel = true },
            XAxis = new XAxis { Type = AxisType.Category, Data = new List<object>() },
            YAxis = new YAxis { Type = AxisType.Value },
            Series = new List<ISeries>
        {
            new BarSeries
            {
                Name = "wait (s)",
                Data = new List<object>(),
                ItemStyle = new ItemStyle
                {
                    Color = "#008000",
                    Color0 = "#006400",
                    BorderColor = "#32CD32",
                    BorderColor0 = "#2CFF05"
                },
                MarkLine = new MarkLine
                {
                    Data = new List<object>
                    {
                        new { yAxis = 300000, name = "game over" }
                    },
                    LineStyle = new LineStyle
                    {
                        Color = "#FFAE00",
                        Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed)
                    }
                }
            }
        }
        };
        #endregion

        #region CorrelationChart
        public static ChartOptions CorrelationChart_Options = new()
        {
            Tooltip = new PanoramicData.ECharts.Tooltip { Trigger = TooltipTrigger.Item, Formatter = "{c}" },
            Grid = new Grid
            {
                Top = "5%",
                ContainLabel = true
            },
            XAxis = new XAxis { Type = AxisType.Value, Name = "Tasks/sec", SplitLine = new SplitLine { Show = false } },
            YAxis = new YAxis { Type = AxisType.Value, Name = "Wait (ms/sec)" },
            Series = new List<ISeries>
            {
                new ScatterSeries
                {
                    Name = "Contention Profile",
                    SymbolSize = 8,
                    ItemStyle = new ItemStyle { Color = "#00FF00" },
                    Data = new List<object>()
                }
            }
        };
        #endregion

        #region DeadlockChart
        public static ChartOptions DeadlockChart_Options { get; set; } = new()
        {
            Tooltip = new PanoramicData.ECharts.Tooltip { Trigger = TooltipTrigger.Axis },
            Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
            XAxis = new XAxis { Type = AxisType.Category, Data = new List<object>() },
            YAxis = new YAxis { Type = AxisType.Value },
            Series = new List<ISeries>
            {
                new BarSeries
                {
                    Name = "deadlocks",
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = "#008000",
                        Color0 = "#006400",
                        BorderColor = "#32CD32",
                        BorderColor0 = "#2CFF05"
                    }
                }
            }
        };
        #endregion

        #region EGuage
        public static ChartOptions EGauge_Options { get; set; } = new()
        {
            BackgroundColor = new Color(ColorHex.Transparent),

            Series = new List<ISeries>
            {
                new GaugeSeries
                {
                    Name = "Total Wait Pressure",
                    Pointer = new Pointer()
                    { 
                        //Icon =  new Icon("arrow"),
                        Length = "80%",
                        Width = 6,
                        Show = true,
                        ItemStyle = new ItemStyle() { Color = "#00ff00", BorderColor = "#003300", BorderWidth = 2, BorderJoin = LineJoin.Round, BorderRadius = 2 }
                    },
                    Min = 0,
                    Max = 300000,
                    ColorBy = ColorBy.Series,
                    AnimationEasing = AnimationEasing.Linear,
                    Animation = true,
                    Tooltip = new Tooltip()
                    {
                        ValueFormatter = new StringOrFunction("function (value) { return (value / 1000) + 'sec/sec'; } "),
                        Trigger = TooltipTrigger.Axis
                    },
                    StartAngle = 225,
                    EndAngle = -45,
                    AxisLabel = new()
                    {
                        Color = new Color(ColorHex.White),
                        FontFamily = "NaziTypewriterRegular, consolas",
                        FontSize = 11,
                        FontWeight = FontWeight.Normal,
                        TextBorderColor = new Color(ColorHex.MatrixGlow),
                        TextBorderWidth = .5,
                        TextBorderType = new LineType(LineTypeStyle.Solid),
                        ShadowColor = new Color(ColorHex.MatrixGlow),
                        ShadowBlur = 1,
                        ShadowOffsetX = 1,
                        ShadowOffsetY = 1,
                        TextShadowColor = new Color(ColorHex.MatrixGlow),
                        TextShadowOffsetX = 1,
                        TextShadowOffsetY = 1,
                        Distance = -0.85,
                        Formatter = new StringOrFunction(new JavascriptFunction(@"function (value) { return (value / 1000) + 'k'; } ")),
                    },
                    SplitLine = new SplitLine()
                    {
                        Interval = new NumberOrFunction(50000),
                        Length = 15,
                        LineStyle = new LineStyle() { Type = new LineType(LineTypeStyle.Dashed), Color = new Color("#FFFFFF") }
                    },

				    // --- MATRIX READOUT ---
				    Title = new Title
                    {
                        Text = "{value}",
                        Color = new Color(ColorHex.MatrixGreen),
                        FontFamily = "Consolas",
                        FontSize = 12,
                        FontWeight = FontWeight.Bold,
                        OffsetCenter = new double[] { 0, 20 },
                        BackgroundColor = new Color(ColorHex.Transparent),
                        BorderColor = new Color(ColorHex.Transparent),
                        BorderWidth = 1,
                        ShadowColor = new Color(ColorHex.MatrixGlow),
                        ValueAnimation = true,
                        ShadowBlur = 12,
                    },
                    AxisTick = new()
                    {
                        Show = true,
                        Inside = false,
                        LineStyle = new()
                        {
                            Color = new Color(ColorHex.MatrixGlow),
                            Width = 2
                        },
                        Interval = new NumberOrFunction(50000)
                    },

				    // --- OUTER SCALE ---
				    AxisLine = new AxisLine
                    {
                        LineStyle = new LineStyle
                        {
                            Width = 2,
                            Color = new Color(ColorHex.MatrixGlow)
                        }
                    },

				    // --- DIGITAL VALUE ---
				    Detail = new Detail
                    {
                        ValueAnimation = true,
                        FontFamily = "Consolas",
                        FontSize = 22,
                        FontStyle = FontStyle.Normal,
                        FontWeight = FontWeight.Bold,
                        Formatter = new StringOrFunction(new JavascriptFunction(@"function (value) { return (value / 1000).toFixed(2) + 's/s'; } ")),
                        Color = new Color(ColorHex.MatrixGreen),
                        TextBorderColor = new Color(ColorHex.Black),
                        TextBorderWidth = 1,
                        TextShadowColor = new Color(ColorHex.MatrixGlow),
                        TextShadowBlur = 12,
                        TextShadowOffsetX = 0,
                        TextShadowOffsetY = 0,
                        ShadowColor = ColorHex.MatrixGlow,
                        ShadowBlur = 12,
                        BorderRadius = 0
                    },

				    // --- THE GREEN PRESSURE ARC ---
				    ItemStyle = new ItemStyle
                    {
                        Color = new Color(ColorHex.MatrixGreen),
                        AreaColor = new Color(ColorHex.MatrixGreen),
                        BorderColor = new Color(ColorHex.Black),
                        BorderWidth = 1.75,
                        Opacity = 1,
                        ShadowColor = new Color(ColorHex.MatrixGlow),
                        ShadowBlur = 10
                    },

				    // --- PROGRESS ARC ---
				    Progress = new Progress
                    {
                        Show = true,
                        Width = 10,
                        ItemStyle = new ItemStyle
                        {
                            Color = new Color(ColorHex.MatrixGlow), // Dynamically overwritten in UpdateChartData
						    Color0 = new Color(ColorHex.MatrixGreen),
                            AreaColor = new Color(ColorHex.MatrixGreen),
                            Opacity = 1,
                            BorderColor = new Color(ColorHex.MatrixGreen),
                            BorderColor0 = new Color(ColorHex.MatrixGreen),
                            BorderWidth = 1.75,
                            ShadowColor = new Color(ColorHex.MatrixGlow),
                            ShadowBlur = 12,
                            BorderCap = LineCap.Round
                        }
                    },
				    // --- MATRIX, NOT CARTOON GAUGE ---
				    AnimationDuration = new NumberOrFunction(1000)
                }
            }
        };
        #endregion

        #region TaskRateChart
        public static ChartOptions TaskRateChart_Options { get; set; } = new()
        {
            Tooltip = new Tooltip { Trigger = TooltipTrigger.Axis },
            Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
            XAxis = new XAxis { Type = AxisType.Time },
            YAxis = new YAxis { Type = AxisType.Value },
            Series = new List<ISeries>
            {
                new LineSeries
                {
                    Name = "tasks/sec",
                    ShowSymbol = false,
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = "#26FF00",
                        Color0 = "#006400",
                        BorderColor = "#00FFFB",
                        BorderColor0 = "#2CFF05"
                    },
				    // NOTE: the original "game over" MarkLine at yAxis=300000 was copy-pasted from
				    // the ms-based wait charts and doesn't apply to a tasks/sec rate. Removed rather
				    // than guessing a real threshold — add one back if you have an actual rate alert
				    // level in mind.
				    Animation = true
                }
            }
        };
        #endregion

        #region TopWaitsChart
        public static ChartOptions TopWaitsChart_Options { get; set; } = new()
        {
            Tooltip = new Tooltip { Trigger = TooltipTrigger.Axis },
            Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
            XAxis = new XAxis
            {
                Type = AxisType.Category,
                Data = new List<object>(),
                AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }
            },
            YAxis = new YAxis
            {
                Type = AxisType.Value,
                AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }
            },
            Series = new List<ISeries>
            {
                new BarSeries
                {
                    Name = "ms/sec",
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = "#C800FF",
                        Color0 = "#FF0062",
                        BorderColor = "#FF0062",
                        BorderColor0 = "#2CFF05"
                    },
                    MarkLine = new MarkLine
                    {
                        Data = new List<object>
                        {
                            new { yAxis = 300000, name = "game over" }
                        },
                        LineStyle = new LineStyle
                        {
                            Color = "#FFAE00",
                            Type = new LineType(LineTypeStyle.Dashed)
                        }
                    }
                }
            }
        };
        #endregion

        #region WaitCandleChart
        public static ChartOptions WaitCandleChart_Options { get; set; } = new()
        {
            BackgroundColor = "transparent",

            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Axis,
                AxisPointer = new() { Type = AxisPointerType.Cross }
            },
            Grid = new Grid
            {
                Left = "1%",
                Right = "1%",
                Top = "8%",
                Bottom = "18%",
                ContainLabel = true
            },
            XAxis = new XAxis
            {
                Type = AxisType.Category,
                BoundaryGap = true,
                AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }
            },
            YAxis = new YAxis
            {
                Type = AxisType.Value,
                Scale = true,
                AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }
            },
            DataZoom = new List<IDataZoom>
            {
                new InsideDataZoom(),
                new SliderDataZoom { Height = 20, Bottom = 10 }
            },

            Series = new List<ISeries>
            {
                new CandlestickSeries
                {
                    Name = "WAIT",
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = "#32CD32",
                        Color0 = "#ff1744",
                        BorderColor = "#32CD32",
                        BorderColor0 = "#ff5252"
                    }
                }
            }
        };
        #endregion

        #region WaitDistributionChart
        public static ChartOptions WaitDistributionChart_Options { get; set; } = new()
        {
            Tooltip = new Tooltip { Trigger = TooltipTrigger.Item },
            Legend = new Legend
            {
                Orient = Orient.Vertical,
                Top = "5%",
                Left = new NumberOrString("left"),
                TextStyle = new TextStyle { Color = ColorHex.NeonLime, Align = HorizontalAlign.Left }
            },
            Grid = new Grid() { Left = new NumberOrString("55%") },
            Series = new List<ISeries>
            {
                new PieSeries
                {
                    Name = "Wait Types",
                    Radius = new CircleRadius(new NumberOrString("40%"), new NumberOrString("70%")),
                    AvoidLabelOverlap = true,
                    ItemStyle = new ItemStyle
                    {
                        BorderRadius = 10,
                        BorderColor = "#11151c",
                        BorderWidth = 2
                    },
                    Data = new List<object>()
                }
            }
        };
        #endregion

        #region WaitRateChart
        private const int WaitRateChart_Max = 1000000;
        public static ChartOptions WaitRateChart_Options { get; set; } = new()
        {
            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Axis,
                AxisPointer = new() { Type = AxisPointerType.Shadow }
            },
            Grid = new Grid { Left = "1%", Right = "0%", Bottom = "7%", Top = "2%", ContainLabel = true, ShadowBlur = 12, ShadowColor = ColorHex.MatrixGlow },
            XAxis = new XAxis { Type = AxisType.Time, AxisLabel = new AxisLabel() { Color = ColorHex.White } },
            // Four independent axes instead of one shared axis. Each series now carries its *true* value
            // (real %, real MB) plotted against its own scale, instead of being pre-multiplied into a
            // shared 0-CHART_MAX range just so the lines visually fit together — that pre-scaling is what
            // made the tooltip show "100,000%" for a real 10% CPU reading. Only the wait-rate axis (index 0)
            // is visible; the other three are Show = false so the panel still reads as one chart, but each
            // series' tooltip now reports its correct raw value via YAxisIndex below.
            YAxisList = new List<YAxis>
            {
               new YAxis { Type = AxisType.Value, Show = false, Min = 0, Max = 100,
                AxisLabel = new AxisLabel { Color = ColorHex.NeonLime } },               // 1: CPU %
			    new YAxis { Type = AxisType.Value, Show = false,
                AxisLabel = new AxisLabel { Color = ColorHex.NeonLime } },                                    // 2: Memory (MB)
			    new YAxis { Type = AxisType.Value, Show = false,
                AxisLabel = new AxisLabel { Color = ColorHex.NeonLime } },                                     // 3: I/O Load
                 new YAxis { Type = AxisType.Value, AxisLabel = new AxisLabel() { Color = ColorHex.White } }, // 0: ms wait / sec
			
		    },
            DataZoom = new List<IDataZoom>
            {
                new InsideDataZoom(),
                new SliderDataZoom { Height = 20, Bottom = 10 }
            },
            Legend = new Legend
            {
                Data = new List<string> { "CPU %", "Memory (MB)", "I/O Load", "ms wait / sec" },
                TextStyle = new() { Color = "#ccc" },
            },
            DataZoom = new List<IDataZoom>
            {
                new InsideDataZoom(),
                new SliderDataZoom { Height = 20, Bottom = 0 }
            },
            Series = new List<ISeries>
        {
            new LineSeries
            {
            new LineSeries
            {
                Name = "CPU %",
                YAxisIndex = 0,
                ShowSymbol = false,
                AreaStyle = new AreaStyle(),
                Data = new List<object>(),
                Smooth = true,
                ItemStyle = new PanoramicData.ECharts.ItemStyle() { Color = ColorHex.Orange },
                MarkLine = new MarkLine
                {
                    Data = new List<object> { new { yAxis = 100, name = "game over" } },
                    LineStyle = new LineStyle
                    {
                        Color = ColorHex.DarkRed,
                        Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed)
                    }
                } ,
                ColorBy = ColorBy.Data
            },
            new LineSeries
            {
                Name = "Memory (MB)",
                YAxisIndex = 1,
                ShowSymbol = false,
                AreaStyle = new AreaStyle(),
                Data = new List<object>(),
                ItemStyle = new PanoramicData.ECharts.ItemStyle() { Color = ColorHex.Purple },
                Smooth = true,
                MarkLine = new MarkLine
                {
                    Data = new List<object> { new { yAxis = 200000, name = "game over" } },
                    LineStyle = new LineStyle
                    {
                        Color = YELLOW,
                        Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed)
                    }
                },
                ColorBy = ColorBy.Data
            },
            new LineSeries
            {
                Name = "I/O Load",
                YAxisIndex = 2,
                ShowSymbol = false,
                AreaStyle = new AreaStyle(),
                Data = new List<object>(),
                ItemStyle = new PanoramicData.ECharts.ItemStyle() { Color = ColorHex.CharcoalGray },
                Smooth = true,
                MarkLine = new MarkLine
                {
                    Data = new List<object> { new { yAxis = 200000, name = "game over" } },
                    LineStyle = new LineStyle
                    {
                        Color = ColorHex.Blurple,
                        Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed)
                    }
                },
                ColorBy = ColorBy.Data
            },
                Name = "ms wait / sec",
                YAxisIndex = 3,
                ShowSymbol = false,
                AreaStyle = new AreaStyle(),
                Data = new List<object>(),
                Smooth = true,
                ItemStyle = new ItemStyle() { Color = ColorHex.BrightLime },
                MarkLine = new MarkLine
                {
                    Data = new List<object> { new { yAxis = WaitRateChart_Max * .3, name = "game over" } },
                    LineStyle = new LineStyle
                    {
                        Color = ColorHex.MatrixDark,
                        Type = new LineType(LineTypeStyle.Dashed),
                    },
                 Label = new Label(){ Color = ColorHex.MatrixGlow }
                },
                ColorBy = ColorBy.Data
            },
            }
        };
        #endregion
    }

    public class ChartTheme
    {
        public string Name { get; set; } = "Matrix Green";
        public string BackgroundColor { get; set; } = "#0a0c10";
        public string TextColor { get; set; } = ColorHex.MatrixGreen;
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
            MatrixGreen, CyberBlue, VaporwavePink, HighContrastDark
        };
        #endregion
    }

    public class ChartOptionsLibrary2
    {
        private const int WaitRateChart_Max = 1000000;
        public static string StandardFontFamily = "Consolas, 'Courier New', ui-monospace, monospace";
        public const int StandardAxisFontSize = 11;
        public const int StandardLegendFontSize = 11;

        #region Theme Builders / Generators (For Theme Dropdown Drivers)

        /// <summary>
        /// Priority #1: Top Main Telemetry Chart (WaitRateChart) generated dynamically by theme.
        /// </summary>
        public static ChartOptions BuildWaitRateChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,

                Tooltip = new Tooltip
                {
                    Trigger = TooltipTrigger.Axis,
                    AxisPointer = new AxisPointer { Type = AxisPointerType.Cross },
                    BackgroundColor = theme.TooltipBg,
                    BorderColor = theme.TooltipBorder,
                    BorderWidth = 1,
                    TextStyle = new TextStyle
                    {
                        Color = new Color("#ffffff"),
                        FontFamily = theme.FontFamily,
                        FontSize = theme.AxisFontSize
                    }
                },

                Grid = new Grid
                {
                    Left = "1%",
                    Right = "1%",
                    Bottom = "12%",
                    Top = "6%",
                    ContainLabel = true,
                    ShadowBlur = 12,
                    ShadowColor = theme.TextColor
                },

                Legend = new Legend
                {
                    Data = new List<string> { "I/O Load", "Memory (MB)", "CPU %", "ms wait / sec" },
                    Orient = Orient.Horizontal,
                    Bottom = "0%",
                    TextStyle = new TextStyle
                    {
                        Color = new Color(theme.TextColor),
                        FontFamily = theme.FontFamily,
                        FontSize = theme.LegendFontSize,
                        FontWeight = FontWeight.Bold
                    }
                },

                XAxis = new XAxis
                {
                    Type = AxisType.Time,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel
                    {
                        Color = new Color(theme.TextColor),
                        FontFamily = theme.FontFamily,
                        FontSize = theme.AxisFontSize
                    },
                    SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
                },

                YAxisList = new List<YAxis>
                {
                    // 0: I/O Load
                    new YAxis
                    {
                        Type = AxisType.Value, Show = false,
                        SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
                    },
                    // 1: Memory (MB)
                    new YAxis { Type = AxisType.Value, Show = false },
                    // 2: CPU %
                    new YAxis { Type = AxisType.Value, Show = false },
                    // 3: ms wait / sec (Primary Visible Axis)
                    new YAxis
                    {
                        Type = AxisType.Value,
                        Scale = true,
                        AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                        AxisLabel = new AxisLabel
                        {
                            Color = new Color(theme.TextColor),
                            FontFamily = theme.FontFamily,
                            FontSize = theme.AxisFontSize
                        },
                        SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
                    }
                },

                DataZoom = new List<IDataZoom>
                {
                    new InsideDataZoom(),
                    new SliderDataZoom
                    {
                        Height = 16,
                        Bottom = 22,
                        TextStyle = new TextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = 10 }
                    }
                },

                Series = new List<ISeries>
                {
                    new LineSeries
                    {
                        Name = "I/O Load",
                        YAxisIndex = 0,
                        ShowSymbol = false,
                        Smooth = true,
                        AreaStyle = new AreaStyle { Opacity = 0.15 },
                        ItemStyle = new ItemStyle { Color = new Color(theme.SeriesTertiary) },
                        MarkLine = new MarkLine
                        {
                            Data = new List<object> { new { yAxis = 200000, name = "⏲ IO" } },
                            LineStyle = new LineStyle { Color = new Color(theme.SeriesTertiary), Type = new LineType(LineTypeStyle.Dashed) },
                            Label = new Label
                            {
                                Show = true, Color = new Color(theme.TextColor),
                                FontFamily = theme.FontFamily, FontSize = 10, Formatter = "{b}"
                            }
                        }
                    },
                    new LineSeries
                    {
                        Name = "Memory (MB)",
                        YAxisIndex = 1,
                        ShowSymbol = false,
                        Smooth = true,
                        AreaStyle = new AreaStyle { Opacity = 0.25 },
                        ItemStyle = new ItemStyle { Color = new Color(theme.SeriesSecondary) },
                        MarkLine = new MarkLine
                        {
                            Data = new List<object> { new { yAxis = 200000, name = "💾 Nom-Nom" } },
                            LineStyle = new LineStyle { Color = new Color(ColorHex.Yellow), Type = new LineType(LineTypeStyle.Dashed) },
                            Label = new Label
                            {
                                Show = true, Color = new Color(theme.TextColor),
                                FontFamily = theme.FontFamily, FontSize = 10, Formatter = "{b}"
                            }
                        }
                    },
                    new LineSeries
                    {
                        Name = "CPU %",
                        YAxisIndex = 2,
                        ShowSymbol = false,
                        Smooth = true,
                        ItemStyle = new ItemStyle { Color = new Color(theme.SeriesQuaternary) },
                        MarkLine = new MarkLine
                        {
                            Data = new List<object> { new { yAxis = WaitRateChart_Max, name = "🤷 CPU High" } },
                            LineStyle = new LineStyle { Color = new Color(theme.CriticalRed), Type = new LineType(LineTypeStyle.Dashed) },
                            Label = new Label
                            {
                                Show = true, Color = new Color(theme.TextColor),
                                FontFamily = theme.FontFamily, FontSize = 10, Formatter = "{b}"
                            }
                        }
                    },
                    new LineSeries
                    {
                        Name = "ms wait / sec",
                        YAxisIndex = 3,
                        ShowSymbol = false,
                        Smooth = true,
                        AreaStyle = new AreaStyle { Opacity = 0.35 },
                        ItemStyle = new ItemStyle { Color = new Color(theme.SeriesPrimary) },
                        MarkLine = new MarkLine
                        {
                            Data = new List<object> { new { yAxis = WaitRateChart_Max * 0.3, name = "🥠 Game Over" } },
                            LineStyle = new LineStyle { Color = new Color(theme.CriticalRed), Width = 2, Type = new LineType(LineTypeStyle.Dashed) },
                            Label = new Label
                            {
                                Show = true, Color = new Color(theme.CriticalRed),
                                FontFamily = theme.FontFamily, FontSize = 10, Formatter = "{b}"
                            }
                        }
                    }
                }
            };
        }

        #endregion

        #region Default Options (Backward Compatible Singletons)

        // Priority #1: Top Chart standard default instance
        public static ChartOptions WaitRateChart_Options { get; set; } = BuildWaitRateChartOptions(ChartTheme.MatrixGreen);

        public static ChartOptions ActiveRequestsChart_Options { get; set; } = new()
        {
            BackgroundColor = "#0a0c10",
            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Axis,
                BackgroundColor = "rgba(10, 12, 16, 0.85)",
                BorderColor = ColorHex.NeonBlue,
                BorderWidth = 1,
                TextStyle = new TextStyle { Color = new Color(ColorHex.White), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            Grid = new Grid { Left = "1%", Right = "1%", Bottom = "2%", Top = "2%", ContainLabel = true },
            XAxis = new XAxis
            {
                Type = AxisType.Category,
                Data = new List<object>(),
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.NeonBlue) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.CornflowerBlue), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            YAxis = new YAxis
            {
                Type = AxisType.Value,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.NeonBlue) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.CornflowerBlue), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(30, 144, 255, 0.1)") } }
            },
            Series = new List<ISeries>
            {
                new BarSeries
                {
                    Name = "wait (s)",
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = new Color(ColorHex.DodgerBlue),
                        Color0 = new Color(ColorHex.CornflowerBlue),
                        BorderColor = new Color(ColorHex.MatrixGlow),
                        BorderWidth = 1
                    },
                    MarkLine = new MarkLine
                    {
                        Data = new List<object> { new { yAxis = 300000, name = "game over" } },
                        LineStyle = new LineStyle { Color = new Color(ColorHex.WarmPinky), Width = 2, Type = new LineType(LineTypeStyle.Dashed) },
                        Label = new Label { Show = true, Color = new Color(ColorHex.WarmPinky), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize, Position = LabelPosition.Outside }
                    }
                }
            }
        };

        public static ChartOptions BlockingChart_Options { get; set; } = new()
        {
            BackgroundColor = "#0a0c10",
            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Axis,
                BackgroundColor = "rgba(10, 12, 16, 0.85)",
                BorderColor = ColorHex.MatrixGreen,
                TextStyle = new TextStyle { Color = new Color(ColorHex.White), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
            XAxis = new XAxis
            {
                Type = AxisType.Category,
                Data = new List<object>(),
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.MatrixGreen) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.MatrixGreen), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            YAxis = new YAxis
            {
                Type = AxisType.Value,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.MatrixGreen) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.MatrixGreen), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(0, 255, 65, 0.1)") } }
            },
            Series = new List<ISeries>
            {
                new BarSeries
                {
                    Name = "wait (s)",
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = ColorHex.MatrixGreen,
                        Color0 = ColorHex.MatrixDark,
                        BorderColor = ColorHex.MatrixGlow
                    },
                    MarkLine = new MarkLine
                    {
                        Data = new List<object> { new { yAxis = 300000, name = "game over" } },
                        LineStyle = new LineStyle { Color = ColorHex.Orange, Type = new LineType(LineTypeStyle.Dashed) },
                        Label = new Label { Show = true, Color = new Color(ColorHex.Orange), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
                    }
                }
            }
        };

        public static ChartOptions CorrelationChart_Options { get; set; } = new()
        {
            BackgroundColor = "#0a0c10",
            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Item,
                Formatter = "{c}",
                BackgroundColor = "rgba(10, 12, 16, 0.85)",
                BorderColor = ColorHex.MatrixGreen,
                TextStyle = new TextStyle { Color = new Color(ColorHex.White), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            Grid = new Grid { Top = "10%", Left = "3%", Right = "3%", Bottom = "5%", ContainLabel = true },
            XAxis = new XAxis
            {
                Type = AxisType.Value,
                Name = "Tasks/sec",
                NameTextStyle = new TextStyle { Color = new Color(ColorHex.MatrixGreen), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.MatrixGreen) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.MatrixGreen), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                SplitLine = new SplitLine { Show = false }
            },
            YAxis = new YAxis
            {
                Type = AxisType.Value,
                Name = "Wait (ms/sec)",
                NameTextStyle = new TextStyle { Color = new Color(ColorHex.MatrixGreen), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.MatrixGreen) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.MatrixGreen), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(0, 255, 65, 0.1)") } }
            },
            Series = new List<ISeries>
            {
                new ScatterSeries
                {
                    Name = "Contention Profile",
                    SymbolSize = 8,
                    ItemStyle = new ItemStyle { Color = ColorHex.MatrixGreen },
                    Data = new List<object>()
                }
            }
        };

        public static ChartOptions DeadlockChart_Options { get; set; } = new()
        {
            BackgroundColor = "#0a0c10",
            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Axis,
                BackgroundColor = "rgba(10, 12, 16, 0.85)",
                BorderColor = ColorHex.DarkRed,
                TextStyle = new TextStyle { Color = new Color(ColorHex.White), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
            XAxis = new XAxis
            {
                Type = AxisType.Category,
                Data = new List<object>(),
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.DarkRed) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.WarmPink), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            YAxis = new YAxis
            {
                Type = AxisType.Value,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.DarkRed) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.WarmPink), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(224, 0, 18, 0.1)") } }
            },
            Series = new List<ISeries>
            {
                new BarSeries
                {
                    Name = "deadlocks",
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = ColorHex.DarkRed,
                        Color0 = ColorHex.WarmPinky,
                        BorderColor = ColorHex.Red
                    }
                }
            }
        };

        public static ChartOptions EGauge_Options { get; set; } = new()
        {
            BackgroundColor = new Color(ColorHex.Transparent),
            Series = new List<ISeries>
            {
                new GaugeSeries
                {
                    Name = "Total Wait Pressure",
                    Pointer = new Pointer
                    {
                        Length = "80%",
                        Width = 6,
                        Show = true,
                        ItemStyle = new ItemStyle { Color = "#00ff00", BorderColor = "#003300", BorderWidth = 2, BorderJoin = LineJoin.Round, BorderRadius = 2 }
                    },
                    Min = 0,
                    Max = 300000,
                    ColorBy = ColorBy.Series,
                    AnimationEasing = AnimationEasing.Linear,
                    Animation = true,
                    Tooltip = new Tooltip
                    {
                        ValueFormatter = new StringOrFunction("function (value) { return (value / 1000) + 'sec/sec'; } "),
                        Trigger = TooltipTrigger.Axis
                    },
                    StartAngle = 225,
                    EndAngle = -45,
                    AxisLabel = new AxisLabel
                    {
                        Color = new Color(ColorHex.White),
                        FontFamily = StandardFontFamily,
                        FontSize = 12,
                        FontWeight = FontWeight.Normal,
                        TextBorderColor = new Color(ColorHex.MatrixGlow),
                        TextBorderWidth = 0.5,
                        TextBorderType = new LineType(LineTypeStyle.Solid),
                        ShadowColor = new Color(ColorHex.MatrixGlow),
                        ShadowBlur = 1,
                        Distance = -0.85,
                        Formatter = new StringOrFunction(new JavascriptFunction(@"function (value) { return (value / 1000) + ''; } ")),
                    },
                    SplitLine = new SplitLine
                    {
                        Interval = new NumberOrFunction(50000),
                        Length = 15,
                        LineStyle = new LineStyle { Type = new LineType(LineTypeStyle.Dashed), Color = new Color("#FFFFFF") }
                    },
                    Title = new Title
                    {
                        Text = "{value}",
                        Color = new Color(ColorHex.MatrixGreen),
                        FontFamily = StandardFontFamily,
                        FontSize = 12,
                        FontWeight = FontWeight.Bold,
                        OffsetCenter = new double[] { 0, 20 },
                        BackgroundColor = new Color(ColorHex.Transparent),
                        BorderColor = new Color(ColorHex.Transparent),
                        ShadowColor = new Color(ColorHex.MatrixGlow),
                        ValueAnimation = true,
                        ShadowBlur = 12,
                    },
                    AxisTick = new AxisTick
                    {
                        Show = true,
                        Inside = false,
                        LineStyle = new LineStyle { Color = new Color(ColorHex.MatrixGlow), Width = 2 },
                        Interval = new NumberOrFunction(50000)
                    },
                    AxisLine = new AxisLine
                    {
                        LineStyle = new LineStyle { Width = 2, Color = new Color(ColorHex.MatrixGlow) }
                    },
                    Detail = new Detail
                    {
                        ValueAnimation = true,
                        FontFamily = StandardFontFamily,
                        FontSize = 20,
                        FontStyle = FontStyle.Normal,
                        FontWeight = FontWeight.Bold,
                        Formatter = new StringOrFunction(new JavascriptFunction(@"function (value) { return (value / 1000).toFixed(2) + 's/s'; } ")),
                        Color = new Color(ColorHex.MatrixGreen),
                        TextBorderColor = new Color(ColorHex.Black),
                        TextBorderWidth = 1,
                        TextShadowColor = new Color(ColorHex.MatrixGlow),
                        TextShadowBlur = 12,
                        ShadowColor = ColorHex.MatrixGlow,
                        ShadowBlur = 12,
                        BorderRadius = 0
                    },
                    ItemStyle = new ItemStyle
                    {
                        Color = new Color(ColorHex.MatrixGreen),
                        AreaColor = new Color(ColorHex.MatrixGreen),
                        BorderColor = new Color(ColorHex.Black),
                        BorderWidth = 1.75,
                        Opacity = 1,
                        ShadowColor = new Color(ColorHex.MatrixGlow),
                        ShadowBlur = 10
                    },
                    Progress = new Progress
                    {
                        Show = true,
                        Width = 10,
                        ItemStyle = new ItemStyle
                        {
                            Color = new Color(ColorHex.MatrixGlow),
                            Color0 = new Color(ColorHex.MatrixGreen),
                            AreaColor = new Color(ColorHex.MatrixGreen),
                            Opacity = 1,
                            BorderColor = new Color(ColorHex.MatrixGreen),
                            BorderColor0 = new Color(ColorHex.MatrixGreen),
                            BorderWidth = 1.75,
                            ShadowColor = new Color(ColorHex.MatrixGlow),
                            ShadowBlur = 12,
                            BorderCap = LineCap.Round
                        }
                    },
                    AnimationDuration = new NumberOrFunction(1000)
                }
            }
        };

        public static ChartOptions TaskRateChart_Options { get; set; } = new()
        {
            BackgroundColor = "#0a0c10",
            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Axis,
                BackgroundColor = "rgba(10, 12, 16, 0.85)",
                BorderColor = ColorHex.MatrixGreen,
                TextStyle = new TextStyle { Color = new Color(ColorHex.White), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
            XAxis = new XAxis
            {
                Type = AxisType.Time,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.MatrixGreen) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.MatrixGreen), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            YAxis = new YAxis
            {
                Type = AxisType.Value,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.MatrixGreen) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.MatrixGreen), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(0, 255, 65, 0.1)") } }
            },
            Series = new List<ISeries>
            {
                new LineSeries
                {
                    Name = "tasks/sec",
                    ShowSymbol = false,
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = ColorHex.MatrixGreen,
                        Color0 = ColorHex.MatrixDark,
                        BorderColor = ColorHex.MatrixGlow
                    },
                    Animation = true
                }
            }
        };

        public static ChartOptions TopWaitsChart_Options { get; set; } = new()
        {
            BackgroundColor = "#0a0c10",
            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Axis,
                BackgroundColor = "rgba(10, 12, 16, 0.85)",
                BorderColor = ColorHex.NeonLime,
                TextStyle = new TextStyle { Color = new Color(ColorHex.White), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
            XAxis = new XAxis
            {
                Type = AxisType.Category,
                Data = new List<object>(),
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.NeonLime) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.NeonLime), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            YAxis = new YAxis
            {
                Type = AxisType.Value,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.NeonLime) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.NeonLime), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(0, 255, 0, 0.1)") } }
            },
            Series = new List<ISeries>
            {
                new BarSeries
                {
                    Name = "ms/sec",
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = "#C800FF",
                        Color0 = "#FF0062",
                        BorderColor = "#FF0062"
                    },
                    MarkLine = new MarkLine
                    {
                        Data = new List<object> { new { yAxis = 300000, name = "game over" } },
                        LineStyle = new LineStyle { Color = "#FFAE00", Type = new LineType(LineTypeStyle.Dashed) },
                        Label = new Label { Show = true, Color = new Color("#FFAE00"), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
                    }
                }
            }
        };

        public static ChartOptions WaitCandleChart_Options { get; set; } = new()
        {
            BackgroundColor = "transparent",
            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Axis,
                AxisPointer = new AxisPointer { Type = AxisPointerType.Cross },
                BackgroundColor = "rgba(10, 12, 16, 0.85)",
                BorderColor = ColorHex.NeonLime,
                TextStyle = new TextStyle { Color = new Color(ColorHex.White), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            Grid = new Grid { Left = "1%", Right = "1%", Top = "8%", Bottom = "18%", ContainLabel = true },
            XAxis = new XAxis
            {
                Type = AxisType.Category,
                BoundaryGap = true,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.NeonLime) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.NeonLime), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            YAxis = new YAxis
            {
                Type = AxisType.Value,
                Scale = true,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(ColorHex.NeonLime) } },
                AxisLabel = new AxisLabel { Color = new Color(ColorHex.NeonLime), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize },
                SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(0, 255, 0, 0.1)") } }
            },
            DataZoom = new List<IDataZoom>
            {
                new InsideDataZoom(),
                new SliderDataZoom
                {
                    Height = 16, Bottom = 5,
                    TextStyle = new TextStyle { Color = new Color(ColorHex.NeonLime), FontFamily = StandardFontFamily, FontSize = 10 }
                }
            },
            Series = new List<ISeries>
            {
                new CandlestickSeries
                {
                    Name = "WAIT",
                    Data = new List<object>(),
                    ItemStyle = new ItemStyle
                    {
                        Color = "#32CD32",
                        Color0 = "#ff1744",
                        BorderColor = "#32CD32",
                        BorderColor0 = "#ff5252"
                    }
                }
            }
        };

        public static ChartOptions WaitDistributionChart_Options { get; set; } = new()
        {
            BackgroundColor = "#0a0c10",
            Tooltip = new Tooltip
            {
                Trigger = TooltipTrigger.Item,
                BackgroundColor = "rgba(10, 12, 16, 0.85)",
                BorderColor = ColorHex.NeonLime,
                TextStyle = new TextStyle { Color = new Color(ColorHex.White), FontFamily = StandardFontFamily, FontSize = StandardAxisFontSize }
            },
            Legend = new Legend
            {
                Orient = Orient.Vertical,
                Top = "5%",
                Left = new NumberOrString("left"),
                TextStyle = new TextStyle
                {
                    Color = new Color(ColorHex.NeonLime),
                    FontFamily = StandardFontFamily,
                    FontSize = StandardLegendFontSize,
                    Align = HorizontalAlign.Left
                }
            },
            Grid = new Grid { Left = "right" },
            Series = new List<ISeries>
            {
                new PieSeries
                {
                    Name = "Wait Types",
                    Radius = new CircleRadius(new NumberOrString("40%"), new NumberOrString("70%")),
                    AvoidLabelOverlap = true,
                    ItemStyle = new ItemStyle
                    {
                        BorderRadius = 10,
                        BorderColor = "#11151c",
                        BorderWidth = 2
                    },
                    Data = new List<object>()
                }
            }
        };

        #endregion
    }
}