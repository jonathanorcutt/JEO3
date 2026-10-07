using BlazorMonaco;
using JEO3.Core;
using PanoramicData.ECharts;

namespace JEO3.Monitor.ECharts
{
    public class ChartOptionsLibrary
    {
        private const int WaitRateChart_Max = 1000000;

        #region WaitRateChart (Priority #1 Top Telemetry Graph)
        public static ChartOptions BuildWaitRateChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,
                //AnimationDurationUpdate = new NumberOrFunction(500),
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
                    Top = "1%",
                    Bottom = "20%",   // was 18% — extra room so the zoom bar + legend don't crowd the axis labels
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
                    Interval = 5,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel
                    {
                        Color = new Color(theme.TextColor),
                        FontFamily = theme.FontFamily,
                        FontSize = theme.AxisFontSize
                    },
                    // Change this line right here to Show = false:
                    SplitLine = new SplitLine { Show = false }
                },

                YAxisList = new List<YAxis>
                    {
                        new YAxis { Type = AxisType.Value, Min = 0, Max = 50000, Show = false}, // I/O Load
                        new YAxis { Type = AxisType.Value, Min = 0, Max = 10000, Show = false }, // Memory (MB)
                        new YAxis { Type = AxisType.Value, Min = 0, Max = 70, Show = false }, // CPU %
                        new YAxis
                        {
                            Type = AxisType.Value,
                            Scale = true,
                            Min = 0,
                            Max = 350000,
                            AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) }  },
                            AxisLabel = new AxisLabel
                            {
                                Color = new Color(theme.TextColor),
                                FontFamily = theme.FontFamily,
                                FontSize = theme.AxisFontSize,
                                Show = true
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
                                Bottom = "11%",   // was 0 — lifts the zoom bar off the same line as the legend
                                TextStyle = new TextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = 10 }
                            }
                        },

                Series = new List<ISeries>
                    {
                        new LineSeries
                        {
                            Name = "I/O Load", YAxisIndex = 0, ShowSymbol = false, Smooth = true,
                            AreaStyle = new AreaStyle { Opacity = 0.15 },
                            ItemStyle = new ItemStyle { Color = new Color(theme.SeriesTertiary) },
                            //MarkLine = new MarkLine
                            //{
                            //    Data = new List<object> { new { yAxis = 200000, name = "⏲ IO" } },
                            //    LineStyle = new LineStyle { Color = new Color(theme.SeriesTertiary), Type = new LineType(LineTypeStyle.Dashed) },
                            //    Label = new Label { Show = true, Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = 10, Formatter = "{b}", Align = HorizontalAlign.Right }
                            //}
                        },
                        new LineSeries
                        {
                            Name = "Memory (MB)", YAxisIndex = 1, ShowSymbol = false, Smooth = true,
                            AreaStyle = new AreaStyle { Opacity = 0.15 },
                            ItemStyle = new ItemStyle { Color = new Color(theme.SeriesSecondary) },
                            //MarkLine = new MarkLine
                            //{
                            //    Data = new List<object> { new { yAxis = 200000, name = "💾 Nom-Nom" } },
                            //    LineStyle = new LineStyle { Color = new Color(ColorHex.Yellow), Type = new LineType(LineTypeStyle.Dashed) },
                            //    Label = new Label { Show = true, Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = 10, Formatter = "{b}", Align = HorizontalAlign.Right }
                            //}
                    },
                        new LineSeries
                        {
                            Name = "CPU %", YAxisIndex = 2, ShowSymbol = false, Smooth = true,
                            AreaStyle = new AreaStyle { Opacity = 0.15 },
                            ItemStyle = new ItemStyle { Color = new Color(theme.SeriesQuaternary) },
                            //MarkLine = new MarkLine
                            //{
                            //    Data = new List<object> { new { yAxis = 100, name = "🤷 CPU High" } },
                            //    LineStyle = new LineStyle { Color = new Color(theme.CriticalRed), Type = new LineType(LineTypeStyle.Dashed) },
                            //    Label = new Label { Show = true, Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = 10, Formatter = "{b}", Align = HorizontalAlign.Right }
                            //}
                        },
                        new LineSeries
                        {
                            Name = "ms wait / sec", YAxisIndex = 3, ShowSymbol = true, Smooth = true,
                            AreaStyle = new AreaStyle { Opacity = 0.45 },
                            ItemStyle = new ItemStyle { Color = new Color(theme.SeriesPrimary) },
                            MarkLine = new MarkLine
                            {
                                Data = new List<object> { new { yAxis = 300000, name = "🥠 DB Game Over" } },
                                LineStyle = new LineStyle { Color = new Color(ColorHex.MatrixGlow), Width = 2, Type = new LineType(LineTypeStyle.Dashed) },
                                Label = new Label { Show = true, Color = ColorHex.PureWhite, Offset = [0, 10], FontFamily = theme.FontFamily, FontSize = 10, Formatter = "{b}", Align = HorizontalAlign.Right }
                            }
                        }
                    }
            };
        }
        #endregion

        #region Parallel
        public static ChartOptions BuildWaitRateParallelChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,

                // 1. Configure the individual, independent vertical axes
                ParallelAxis = new List<ParallelAxis>
        {
            new ParallelAxis
            {
                Dim = 0,
                Name = "I/O Load",
                Min = 0,
                Max = 25000,
                NameTextStyle = new NameTextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily }
            },
            new ParallelAxis
            {
                Dim = 1,
                Name = "Memory (MB)",
                Min = 0,
                Max = 4000,
                NameTextStyle = new NameTextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily }
            },
            new ParallelAxis
            {
                Dim = 2,
                Name = "CPU %",
                Min = 0,
                Max = 100,
                NameTextStyle = new NameTextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily }
            },
            new ParallelAxis
            {
                Dim = 3,
                Name = "ms wait / sec",
                Min = 0,
                Max = 350000,
                NameTextStyle = new NameTextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily }
            }
        },

                // 2. Parallel coordinates configurations require a Parallel layout layout object
                Parallel = new PanoramicData.ECharts.Parallel
                {
                    Left = "5%",
                    Right = "15%",
                    Top = "15%",
                    Bottom = "10%",
                    // Changes the default layout styling to support your dark/light theme parameters
                    ParallelAxisDefault = new ParallelAxisDefault
                    {
                        Type = "value",
                        AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                        AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily }
                    }
                },

                // 3. Define the data structures inside a ParallelSeries
                Series = new List<ISeries>
        {
            new ParallelSeries
            {
                LineStyle = new LineStyle
                {
                    Width = 2,
                    Opacity = 0.4, // Keep it slightly transparent so overlapping lines don't turn into a solid block
                    Color = new Color(theme.SeriesPrimary)
                },
                Smooth = true, // Mimics the fluid flowing curves in your example image
                
                // DATA FORMAT REQUIREMENT:
                // Data expects a multi-dimensional array mapping matching the order of your dimensions:
                // [ [IO_1, Mem_1, CPU_1, Wait_1], [IO_2, Mem_2, CPU_2, Wait_2] ]
                Data = new List<object>
                {
                    new List<double> { 12000, 2100, 45, 120000 },
                    new List<double> { 22000, 3800, 95, 310000 }, // High outlier path
                    new List<double> { 5000, 1800, 20, 15000 }
                }
            }
        }
            };
        }
        #endregion

        #region StackedArea
        public static ChartOptions BuildWaitRateStackedAreaChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,

                Tooltip = new Tooltip
                {
                    Trigger = TooltipTrigger.Axis,
                    // Axis pointer as "shadow" looks much better behind vertical bars
                    AxisPointer = new AxisPointer { Type = AxisPointerType.Shadow },
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
                    Top = "1%",
                    Bottom = "20%",
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
                    Interval = 5,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel
                    {
                        Color = new Color(theme.TextColor),
                        FontFamily = theme.FontFamily,
                        FontSize = theme.AxisFontSize
                    },
                    SplitLine = new SplitLine { Show = false }
                },

                YAxisList = new List<YAxis>
        {
            new YAxis { Type = AxisType.Value, Min = 0, Max = 25000, Show = false}, // I/O Load
            new YAxis { Type = AxisType.Value, Min = 0, Max = 4000, Show = false }, // Memory (MB)
            new YAxis { Type = AxisType.Value, Min = 0, Max = 100, Show = false }, // CPU %
            new YAxis
            {
                Type = AxisType.Value,
                Scale = true,
                Min = 0,
                Max = 350000,
                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) }  },
                AxisLabel = new AxisLabel
                {
                    Color = new Color(theme.TextColor),
                    FontFamily = theme.FontFamily,
                    FontSize = theme.AxisFontSize,
                    Show = true
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
                Bottom = "11%",
                TextStyle = new TextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = 10 }
            }
        },

                Series = new List<ISeries>
        {
            // CHANGED: Shifted background resource layout to physical columns (BarSeries)
            new BarSeries
            {
                Name = "I/O Load", YAxisIndex = 0,
                ItemStyle = new ItemStyle { Color = new Color(theme.SeriesTertiary) }
            },
            new BarSeries
            {
                Name = "Memory (MB)", YAxisIndex = 1,
                ItemStyle = new ItemStyle { Color = new Color(theme.SeriesSecondary) }
            },
            new BarSeries
            {
                Name = "CPU %", YAxisIndex = 2,
                ItemStyle = new ItemStyle { Color = new Color(theme.SeriesQuaternary) }
            },
            
            // KEEP: Primary metric remains a distinct, prominent line slicing through the bars
            new LineSeries
            {
                Name = "ms wait / sec", YAxisIndex = 3, ShowSymbol = true, Smooth = true,
                LineStyle = new LineStyle { Width = 3 },
                ItemStyle = new ItemStyle { Color = new Color(theme.SeriesPrimary) },
                MarkLine = new MarkLine
                {
                    Data = new List<object> { new { yAxis = 300000, name = "🥠 Game Over" } },
                    LineStyle = new LineStyle { Color = new Color(theme.CriticalRed), Width = 2, Type = new LineType(LineTypeStyle.Dashed) },
                    Label = new Label { Show = true, Color = ColorHex.PureWhite, FontFamily = theme.FontFamily, FontSize = 10, Formatter = "{b}", Align = HorizontalAlign.Right }
                }
            }
        }
            };
        }
        #endregion

        #region WaitRateChartSparkline
        public static ChartOptions BuildWaitRateSparklineChartOptions(ChartTheme theme)
        {
            return new ChartOptions()
            {
                // 1. Hide the X Axis completely
                XAxis = new() { Show = false },

                // 2. Hide the Y Axis completely
                YAxis = new() { Show = false },

                // 3. Remove all margins/padding around the chart so it fits tightly
                Grid = new()
                {
                    Left = "0",
                    Right = "0",
                    Top = "0",
                    Bottom = "0"
                },

                // 4. Configure the line data
                Series = new()
                {
                    new LineSeries()
                    {
                        Data = new List<object> { 10, 15, 8, 22, 18, 25 },
                        Symbol = "none", // Removes the individual data point dots
                        Smooth = true,   // Optional: Makes the sparkline a smooth curved path
                        LineStyle = new()
                        {
                            Width = 2,
                            Color = "#007acc"
                        }
                    }
                }
            };
        }
        #endregion

        #region ActiveRequestsChart
        public static ChartOptions BuildActiveRequestsChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,
                Tooltip = new Tooltip
                {
                    Trigger = TooltipTrigger.Axis,
                    BackgroundColor = theme.TooltipBg,
                    BorderColor = theme.TooltipBorder,
                    BorderWidth = 1,
                    TextStyle = new TextStyle { Color = new Color("#ffffff"), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                Grid = new Grid { Left = "1%", Right = "1%", Bottom = "2%", Top = "2%", ContainLabel = true },
                XAxis = new XAxis
                {
                    Type = AxisType.Category,
                    Data = new List<object>(),
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                YAxis = new YAxis
                {
                    Type = AxisType.Value,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
                },
                Series = new List<ISeries>
                {
                    new BarSeries
                    {
                        Name = "wait (s)",
                        Data = new List<object>(),
                        ItemStyle = new ItemStyle
                        {
                            Color = new Color(theme.SeriesSecondary),
                            BorderColor = new Color(theme.SeriesPrimary),
                            BorderWidth = 1
                        },
                        MarkLine = new MarkLine
                        {
                            Data = new List<object> { new { yAxis = 300000, name = "game over" } },
                            LineStyle = new LineStyle { Color = new Color(theme.CriticalRed), Width = 2, Type = new LineType(LineTypeStyle.Dashed) },
                            Label = new Label { Show = true, Color = new Color(theme.CriticalRed), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize, Position = LabelPosition.Outside }
                        }
                    }
                }
            };
        }
        #endregion

        #region BlockingChart
        public static ChartOptions BuildBlockingChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,
                Tooltip = new Tooltip
                {
                    Trigger = TooltipTrigger.Axis,
                    BackgroundColor = theme.TooltipBg,
                    BorderColor = theme.TooltipBorder,
                    TextStyle = new TextStyle { Color = new Color("#ffffff"), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
                XAxis = new XAxis
                {
                    Type = AxisType.Category,
                    Data = new List<object>(),
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                YAxis = new YAxis
                {
                    Type = AxisType.Value,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
                },
                Series = new List<ISeries>
                {
                    new BarSeries
                    {
                        Name = "wait (s)",
                        Data = new List<object>(),
                        ItemStyle = new ItemStyle
                        {
                            Color = theme.SeriesPrimary,
                            BorderColor = theme.AxisLineColor
                        },
                        MarkLine = new MarkLine
                        {
                            Data = new List<object> { new { yAxis = 300000, name = "game over" } },
                            LineStyle = new LineStyle { Color = theme.SeriesQuaternary, Type = new LineType(LineTypeStyle.Dashed) },
                            Label = new Label { Show = true, Color = new Color(theme.SeriesQuaternary), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                        }
                    }
                }
            };
        }
        #endregion

        #region CorrelationChart
        public static ChartOptions BuildCorrelationChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,
                Tooltip = new Tooltip
                {
                    Trigger = TooltipTrigger.Item,
                    Formatter = "{c}",
                    BackgroundColor = theme.TooltipBg,
                    BorderColor = theme.TooltipBorder,
                    TextStyle = new TextStyle { Color = new Color("#ffffff"), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                Grid = new Grid { Top = "10%", Left = "3%", Right = "3%", Bottom = "5%", ContainLabel = true },
                XAxis = new XAxis
                {
                    Type = AxisType.Value,
                    Name = "Tasks/sec",
                    NameTextStyle = new NameTextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    SplitLine = new SplitLine { Show = false }
                },
                YAxis = new YAxis
                {
                    Type = AxisType.Value,
                    Name = "Wait (ms/sec)",
                    NameTextStyle = new NameTextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
                },
                Series = new List<ISeries>
                {
                    new ScatterSeries
                    {
                        Name = "Contention Profile",
                        SymbolSize = 8,
                        ItemStyle = new ItemStyle { Color = theme.SeriesPrimary },
                        Data = new List<object>()
                    }
                }
            };
        }
        #endregion

        #region DeadlockChart
        public static ChartOptions BuildDeadlockChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,
                Tooltip = new Tooltip
                {
                    Trigger = TooltipTrigger.Axis,
                    BackgroundColor = theme.TooltipBg,
                    BorderColor = theme.CriticalRed,
                    TextStyle = new TextStyle { Color = new Color("#ffffff"), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
                XAxis = new XAxis
                {
                    Type = AxisType.Category,
                    Data = new List<object>(),
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.CriticalRed) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                YAxis = new YAxis
                {
                    Type = AxisType.Value,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.CriticalRed) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
                },
                Series = new List<ISeries>
                {
                    new BarSeries
                    {
                        Name = "deadlocks",
                        Data = new List<object>(),
                        ItemStyle = new ItemStyle
                        {
                            Color = theme.CriticalRed,
                            BorderColor = ColorHex.Red
                        }
                    }
                }
            };
        }
        #endregion

        #region EGauge
        public static ChartOptions BuildEGaugeOptions(ChartTheme theme)
        {
            return new ChartOptions
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
                            ItemStyle = new ItemStyle { Color = theme.SeriesPrimary, BorderColor = "#003300", BorderWidth = 2, BorderJoin = LineJoin.Round, BorderRadius = 2 }
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

                    AxisLabel = new()
                    {
                        Color = new Color(ColorHex.White),
                        FontFamily = "NaziTypewriterRegular, consolas",
                        FontSize = 12,
                        FontWeight = FontWeight.Normal,
                        TextBorderColor = new Color(ColorHex.MatrixGlow),
                        TextBorderWidth = .3,
                        TextBorderType = new LineType(LineTypeStyle.Solid),
                        ShadowColor = new Color(ColorHex.MatrixGlow),
                        ShadowBlur = 1,
                        ShadowOffsetX = 1,
                        ShadowOffsetY = 1,
                        TextShadowColor = new Color(ColorHex.MatrixGlow),
                        TextShadowOffsetX = 1,
                        TextShadowOffsetY = 1,
                        Distance = -0.8,
                        Formatter = new StringOrFunction(new JavascriptFunction(@"function (value) { return (value / 1000) + ''; } ")),
                    },
                        //AxisLabel = new AxisLabel
                        //{
                        //    Color = new Color("#FFFFFF"),
                        //    FontFamily = theme.FontFamily,
                        //    FontSize = theme.AxisFontSize + 1,
                        //    FontWeight = FontWeight.Normal,
                        //    TextBorderColor = new Color(theme.SeriesPrimary),
                        //    TextBorderWidth = 0.5,
                        //    TextBorderType = new LineType(LineTypeStyle.Solid),
                        //    ShadowColor = new Color(theme.SeriesPrimary),
                        //    ShadowBlur = 1,
                        //    Distance = -0.85,
                        //    Formatter = new StringOrFunction(new JavascriptFunction(@"function (value) { return (value / 1000) + ''; } ")),
                        //},
                        SplitLine = new SplitLine
                        {
                            Interval = new NumberOrFunction(50000),
                            Length = 15,
                            LineStyle = new LineStyle { Type = new LineType(LineTypeStyle.Dashed), Color = new Color("#FFFFFF") }
                        },
                        Title = new Title
                        {
                            Text = "{value}",
                            Color = new Color(theme.SeriesPrimary),
                            FontFamily = theme.FontFamily,
                            FontSize = theme.LegendFontSize + 1,
                            FontWeight = FontWeight.Bold,
                            OffsetCenter = new double[] { 0, 20 },
                            BackgroundColor = new Color(ColorHex.Transparent),
                            BorderColor = new Color(ColorHex.Transparent),
                            ShadowColor = new Color(theme.SeriesPrimary),
                            ValueAnimation = true,
                            ShadowBlur = 12,
                        },
                        AxisTick = new AxisTick
                        {
                            Show = true,
                            Inside = false,
                            LineStyle = new LineStyle { Color = new Color(theme.SeriesPrimary), Width = 2 },
                            Interval = new NumberOrFunction(50000)
                        },
                        AxisLine = new AxisLine
                        {
                            LineStyle = new LineStyle { Width = 2, Color = new Color(theme.SeriesPrimary) }
                        },
                        Detail = new Detail
                        {
                            ValueAnimation = true,
                            FontFamily = theme.FontFamily,
                            FontSize = 20,
                            FontStyle = FontStyle.Normal,
                            FontWeight = FontWeight.Bold,
                            Formatter = new StringOrFunction(new JavascriptFunction(@"function (value) { return (value / 1000).toFixed(2) + 's/s'; } ")),
                            Color = new Color(theme.SeriesPrimary),
                            TextBorderColor = new Color(ColorHex.Black),
                            TextBorderWidth = 1,
                            TextShadowColor = new Color(theme.SeriesPrimary),
                            TextShadowBlur = 12,
                            ShadowColor = theme.SeriesPrimary,
                            ShadowBlur = 12,
                            BorderRadius = 0
                        },
                        ItemStyle = new ItemStyle
                        {
                            Color = new Color(theme.SeriesPrimary),
                            AreaColor = new Color(theme.SeriesPrimary),
                            BorderColor = new Color(ColorHex.Black),
                            BorderWidth = 1.75,
                            Opacity = 1,
                            ShadowColor = new Color(theme.SeriesPrimary),
                            ShadowBlur = 10
                        },
                        Progress = new Progress
                        {
                            Show = true,
                            Width = 10,
                            ItemStyle = new ItemStyle
                            {
                                Color = new Color(theme.SeriesPrimary),
                                Color0 = new Color(theme.SeriesPrimary),
                                AreaColor = new Color(theme.SeriesPrimary),
                                Opacity = 1,
                                BorderColor = new Color(theme.SeriesPrimary),
                                BorderColor0 = new Color(theme.SeriesPrimary),
                                BorderWidth = 1.75,
                                ShadowColor = new Color(theme.SeriesPrimary),
                                ShadowBlur = 12,
                                BorderCap = LineCap.Round
                            }
                        },
                        AnimationDuration = new NumberOrFunction(1000)
                    }
                }
            };
        }
        #endregion

        #region TaskRateChart
        public static ChartOptions BuildTaskRateChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,
                Tooltip = new Tooltip
                {
                    Trigger = TooltipTrigger.Axis,
                    BackgroundColor = theme.TooltipBg,
                    BorderColor = theme.TooltipBorder,
                    TextStyle = new TextStyle { Color = new Color("#ffffff"), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
                XAxis = new XAxis
                {
                    Type = AxisType.Time,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                YAxis = new YAxis
                {
                    Type = AxisType.Value,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
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
                            Color = theme.SeriesPrimary,
                            BorderColor = theme.SeriesSecondary
                        },
                        Animation = true
                    }
                }
            };
        }
        #endregion

        #region TopWaitsChart
        public static ChartOptions BuildTopWaitsChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,
                Tooltip = new Tooltip
                {
                    Trigger = TooltipTrigger.Axis,
                    BackgroundColor = theme.TooltipBg,
                    BorderColor = theme.TooltipBorder,
                    TextStyle = new TextStyle { Color = new Color("#ffffff"), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
                XAxis = new XAxis
                {
                    Type = AxisType.Category,
                    Data = new List<object>(),
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                YAxis = new YAxis
                {
                    Type = AxisType.Value,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
                },
                Series = new List<ISeries>
                {
                    new BarSeries
                    {
                        Name = "ms/sec",
                        Data = new List<object>(),
                        ItemStyle = new ItemStyle
                        {
                            Color = theme.SeriesTertiary,
                            BorderColor = theme.SeriesQuaternary
                        },
                        MarkLine = new MarkLine
                        {
                            Data = new List<object> { new { yAxis = 300000, name = "game over" } },
                            LineStyle = new LineStyle { Color = theme.SeriesQuaternary, Type = new LineType(LineTypeStyle.Dashed) },
                            Label = new Label { Show = true, Color = new Color(theme.SeriesQuaternary), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                        }
                    }
                }
            };
        }
        #endregion

        #region WaitCandleChart
        public static ChartOptions BuildWaitCandleChartOptions(ChartTheme theme)
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
                    TextStyle = new TextStyle { Color = new Color("#ffffff"), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                Grid = new Grid { Left = "1%", Right = "1%", Top = "8%", Bottom = "18%", ContainLabel = true },
                XAxis = new XAxis
                {
                    Type = AxisType.Category,
                    BoundaryGap = true,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                YAxis = new YAxis
                {
                    Type = AxisType.Value,
                    Scale = true,
                    AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color(theme.AxisLineColor) } },
                    AxisLabel = new AxisLabel { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize },
                    SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color(theme.GridLineColor) } }
                },
                DataZoom = new List<IDataZoom>
                {
                    new InsideDataZoom(),
                    new SliderDataZoom
                    {
                        Height = 16, Bottom = 5,
                        TextStyle = new TextStyle { Color = new Color(theme.TextColor), FontFamily = theme.FontFamily, FontSize = 10 }
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
                            Color = theme.SeriesPrimary,
                            Color0 = theme.CriticalRed,
                            BorderColor = theme.SeriesPrimary,
                            BorderColor0 = theme.CriticalRed
                        }
                    }
                }
            };
        }
        #endregion

        #region 1WaitDistributionChart
        public static ChartOptions BuildWaitDistributionChartOptions(ChartTheme theme)
        {
            return new ChartOptions
            {
                BackgroundColor = theme.BackgroundColor,
                Tooltip = new Tooltip
                {
                    Trigger = TooltipTrigger.Item,
                    BackgroundColor = theme.TooltipBg,
                    BorderColor = theme.TooltipBorder,
                    TextStyle = new TextStyle { Color = new Color("#ffffff"), FontFamily = theme.FontFamily, FontSize = theme.AxisFontSize }
                },
                Legend = new Legend
                {
                    Orient = Orient.Vertical,
                    Top = "5%",
                    Left = new NumberOrString("left"),
                    TextStyle = new TextStyle
                    {
                        Color = new Color(theme.TextColor),
                        FontFamily = theme.FontFamily,
                        FontSize = theme.LegendFontSize,
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
                            BorderColor = theme.BackgroundColor,
                            BorderWidth = 2
                        },
                        Data = new List<object>()
                    }
                }
            };
        }
        #endregion

        #region Backward Compatibility Static Instances
        public static ChartOptions WaitRateChart_Options => BuildWaitRateChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions ActiveRequestsChart_Options => BuildActiveRequestsChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions BlockingChart_Options => BuildBlockingChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions CorrelationChart_Options => BuildCorrelationChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions DeadlockChart_Options => BuildDeadlockChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions EGauge_Options => BuildEGaugeOptions(ChartTheme.MatrixGreen);
        public static ChartOptions TaskRateChart_Options => BuildTaskRateChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions TopWaitsChart_Options => BuildTopWaitsChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions WaitCandleChart_Options => BuildWaitCandleChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions WaitDistributionChart_Options => BuildWaitDistributionChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions WaitRateChartSparkline_Options => BuildWaitRateSparklineChartOptions(ChartTheme.HighContrastDark);
        public static ChartOptions WaitRateChartStackedAreaChart_Options => BuildWaitRateStackedAreaChartOptions(ChartTheme.HighContrastDark);
        #endregion
    }
}

#region Commented
//public sealed class ChartOptionsLibrary2
//{

//    #region Properties
//    private const int WaitRateChart_Max = 1000000;
//    #endregion

//    #region ActiveRequestsChart
//    public static ChartOptions ActiveRequestsChart_Options { get; set; } = new()
//    {
//        // Deep cyberpunk dark background
//        BackgroundColor = "#0a0c10",

//        Tooltip = new Tooltip
//        {
//            Trigger = TooltipTrigger.Axis,
//            BackgroundColor = "rgba(10, 12, 16, 0.85)", // Semi-transparent dark
//            BorderColor = "#1e90ff",                    // Dodger blue border
//            BorderWidth = 1,
//            TextStyle = new TextStyle { Color = new Color("#ffffff") }
//        },

//        Grid = new Grid { Left = "1%", Right = "1%", Bottom = "2%", Top = "2%", ContainLabel = true },

//        XAxis = new XAxis
//        {
//            Type = AxisType.Category,
//            Data = new List<object>(),
//            AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color("#1e90ff") } }, // Neon blue axis line
//            AxisLabel = new AxisLabel { Color = new Color("#6495ed") } // Cornflower blue text
//        },

//        YAxis = new YAxis
//        {
//            Type = AxisType.Value,
//            AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color("#1e90ff") } },
//            AxisLabel = new AxisLabel { Color = new Color("#6495ed") },
//            SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(30, 144, 255, 0.1") } } // Subtle blue grid matrix
//        },

//        Series = new List<ISeries>
//    {
//        new BarSeries
//        {
//            Name = "wait (s)",
//            Data = new List<object>(),

//// Cyberpunk Neon Blue Main Bars
//            ItemStyle = new ItemStyle
//            {
//                Color =  new Color("#1e90ff"),       // Intense Dodger Blue for active data
//                Color0 =  new Color("#6495ed"),      // Cornflower Blue variant
//                BorderColor =  new Color("#00ffff"), // Cyber cyan sharp edge highlight
//                BorderWidth = 1
//            },

//// High-voltage Neon Magenta warning line
//            MarkLine = new MarkLine
//            {
//                Data = new List<object>
//                {
//                    new { yAxis = 300000, name = "game over" }
//                },
//                LineStyle = new LineStyle
//                {
//                    Color =  new Color("#ff007f"),   // Neon pink / magenta game over threshold
//                    Width = 2,
//                    Type = new LineType(LineTypeStyle.Dashed)
//                },
//                Label = new Label
//                {
//                    Show = true,
//                    Color = new Color("#ff007f"),
//                    Position = LabelPosition.Outside
//                }
//            }
//        }
//    }
//    };
//    #endregion

//    #region BlockingChart
//    public static ChartOptions BlockingChart_Options { get; set; } = new()
//    {
//        Tooltip = new Tooltip { Trigger = TooltipTrigger.Axis },
//        Grid = new Grid { Left = "1%", Right = "1%", Bottom = "25%", Top = "8%", ContainLabel = true },
//        XAxis = new XAxis { Type = AxisType.Category, Data = new List<object>() },
//        YAxis = new YAxis { Type = AxisType.Value },
//        Series = new List<ISeries>
//        {
//            new BarSeries
//            {
//                Name = "wait (s)",
//                Data = new List<object>(),
//                ItemStyle = new ItemStyle
//                {
//                    Color = "#008000",
//                    Color0 = "#006400",
//                    BorderColor = "#32CD32",
//                    BorderColor0 = "#2CFF05"
//                },
//                MarkLine = new MarkLine
//                {
//                    Data = new List<object>
//                    {
//                        new { yAxis = 300000, name = "game over" }
//                    },
//                    LineStyle = new LineStyle
//                    {
//                        Color = "#FFAE00",
//                        Type = new LineType(LineTypeStyle.Dashed)
//                    }
//                }
//            }
//        }
//    };
//    #endregion

//    #region CorrelationChart
//    public static ChartOptions CorrelationChart_Options = new()
//    {
//        Tooltip = new Tooltip { Trigger = TooltipTrigger.Item, Formatter = "{c}" },
//        Grid = new Grid
//        {
//            Top = "5%",
//            ContainLabel = true
//        },
//        XAxis = new XAxis { Type = AxisType.Value, Name = "Tasks/sec", SplitLine = new SplitLine { Show = false } },
//        YAxis = new YAxis { Type = AxisType.Value, Name = "Wait (ms/sec)" },
//        Series = new List<ISeries>
//            {
//                new ScatterSeries
//                {
//                    Name = "Contention Profile",
//                    SymbolSize = 8,
//                    ItemStyle = new ItemStyle { Color = "#00FF00" },
//                    Data = new List<object>()
//                }
//            }
//    };
//    #endregion

//    #region DeadlockChart
//    public static ChartOptions DeadlockChart_Options { get; set; } = new()
//    {
//        Tooltip = new Tooltip { Trigger = TooltipTrigger.Axis },
//        Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
//        XAxis = new XAxis { Type = AxisType.Category, Data = new List<object>() },
//        YAxis = new YAxis { Type = AxisType.Value },
//        Series = new List<ISeries>
//            {
//                new BarSeries
//                {
//                    Name = "deadlocks",
//                    Data = new List<object>(),
//                    ItemStyle = new ItemStyle
//                    {
//                        Color = "#008000",
//                        Color0 = "#006400",
//                        BorderColor = "#32CD32",
//                        BorderColor0 = "#2CFF05"
//                    }
//                }
//            }
//    };
//    #endregion

//    #region EGuage
//    public static ChartOptions EGauge_Options { get; set; } = new()
//    {
//        BackgroundColor = new Color(ColorHex.Transparent),

//        Series = new List<ISeries>
//            {
//                new GaugeSeries
//                {
//                    Name = "Total Wait Pressure",
//                    Pointer = new Pointer()
//                    { 
//                        //Icon =  new Icon("arrow"),
//                        Length = "80%",
//                        Width = 6,
//                        Show = true,
//                        ItemStyle = new ItemStyle() { Color = "#00ff00", BorderColor = "#003300", BorderWidth = 2, BorderJoin = LineJoin.Round, BorderRadius = 2 }
//                    },
//                    Min = 0,
//                    Max = 300000,
//                    ColorBy = ColorBy.Series,
//                    AnimationEasing = AnimationEasing.Linear,
//                    Animation = true,
//                    Tooltip = new Tooltip()
//                    {
//                        ValueFormatter = new StringOrFunction("function (value) { return (value / 1000) + 'sec/sec'; } "),
//                        Trigger = TooltipTrigger.Axis
//                    },
//                    StartAngle = 225,
//                    EndAngle = -45,
//                    AxisLabel = new()
//                    {
//                        Color = new Color(ColorHex.White),
//                        FontFamily = "NaziTypewriterRegular, consolas",
//                        FontSize = 13,
//                        FontWeight = FontWeight.Normal,
//                        TextBorderColor = new Color(ColorHex.MatrixGlow),
//                        TextBorderWidth = .5,
//                        TextBorderType = new LineType(LineTypeStyle.Solid),
//                        ShadowColor = new Color(ColorHex.MatrixGlow),
//                        ShadowBlur = 1,
//                        ShadowOffsetX = 1,
//                        ShadowOffsetY = 1,
//                        TextShadowColor = new Color(ColorHex.MatrixGlow),
//                        TextShadowOffsetX = 1,
//                        TextShadowOffsetY = 1,
//                        Distance = -0.85,
//                        Formatter = new StringOrFunction(new JavascriptFunction(@"function (value) { return (value / 1000) + ''; } ")),
//                    },
//                    SplitLine = new SplitLine()
//                    {
//                        Interval = new NumberOrFunction(50000),
//                        Length = 15,
//                        LineStyle = new LineStyle() { Type = new LineType(LineTypeStyle.Dashed), Color = new Color("#FFFFFF") }
//                    },

//				    // --- MATRIX READOUT ---
//				    Title = new Title
//                    {
//                        Text = "{value}",
//                        Color = new Color(ColorHex.MatrixGreen),
//                        FontFamily = "Consolas",
//                        FontSize = 12,
//                        FontWeight = FontWeight.Bold,
//                        OffsetCenter = new double[] { 0, 20 },
//                        BackgroundColor = new Color(ColorHex.Transparent),
//                        BorderColor = new Color(ColorHex.Transparent),
//                        BorderWidth = 1,
//                        ShadowColor = new Color(ColorHex.MatrixGlow),
//                        ValueAnimation = true,
//                        ShadowBlur = 12,
//                    },
//                    AxisTick = new()
//                    {
//                        Show = true,
//                        Inside = false,
//                        LineStyle = new()
//                        {
//                            Color = new Color(ColorHex.MatrixGlow),
//                            Width = 2
//                        },
//                        Interval = new NumberOrFunction(50000)
//                    },

//				    // --- OUTER SCALE ---
//				    AxisLine = new AxisLine
//                    {
//                        LineStyle = new LineStyle
//                        {
//                            Width = 2,
//                            Color = new Color(ColorHex.MatrixGlow)
//                        }
//                    },

//				    // --- DIGITAL VALUE ---
//				    Detail = new Detail
//                    {
//                        ValueAnimation = true,
//                        FontFamily = "Consolas",
//                        FontSize = 22,
//                        FontStyle = FontStyle.Normal,
//                        FontWeight = FontWeight.Bold,
//                        Formatter = new StringOrFunction(new JavascriptFunction(@"function (value) { return (value / 1000).toFixed(2) + 's/s'; } ")),
//                        Color = new Color(ColorHex.MatrixGreen),
//                        TextBorderColor = new Color(ColorHex.Black),
//                        TextBorderWidth = 1,
//                        TextShadowColor = new Color(ColorHex.MatrixGlow),
//                        TextShadowBlur = 12,
//                        TextShadowOffsetX = 0,
//                        TextShadowOffsetY = 0,
//                        ShadowColor = ColorHex.MatrixGlow,
//                        ShadowBlur = 12,
//                        BorderRadius = 0
//                    },

//				    // --- THE GREEN PRESSURE ARC ---
//				    ItemStyle = new ItemStyle
//                    {
//                        Color = new Color(ColorHex.MatrixGreen),
//                        AreaColor = new Color(ColorHex.MatrixGreen),
//                        BorderColor = new Color(ColorHex.Black),
//                        BorderWidth = 1.75,
//                        Opacity = 1,
//                        ShadowColor = new Color(ColorHex.MatrixGlow),
//                        ShadowBlur = 10
//                    },

//				    // --- PROGRESS ARC ---
//				    Progress = new Progress
//                    {
//                        Show = true,
//                        Width = 10,
//                        ItemStyle = new ItemStyle
//                        {
//                            Color = new Color(ColorHex.MatrixGlow), // Dynamically overwritten in UpdateChartData
//						    Color0 = new Color(ColorHex.MatrixGreen),
//                            AreaColor = new Color(ColorHex.MatrixGreen),
//                            Opacity = 1,
//                            BorderColor = new Color(ColorHex.MatrixGreen),
//                            BorderColor0 = new Color(ColorHex.MatrixGreen),
//                            BorderWidth = 1.75,
//                            ShadowColor = new Color(ColorHex.MatrixGlow),
//                            ShadowBlur = 12,
//                            BorderCap = LineCap.Round
//                        }
//                    },
//				    // --- MATRIX, NOT CARTOON GAUGE ---
//				    AnimationDuration = new NumberOrFunction(1000)
//                }
//            }
//    };
//    #endregion

//    #region TaskRateChart
//    public static ChartOptions TaskRateChart_Options { get; set; } = new()
//    {
//        Tooltip = new Tooltip { Trigger = TooltipTrigger.Axis },
//        Grid = new Grid { Left = "1%", Right = "1%", Bottom = "10%", Top = "8%", ContainLabel = true },
//        XAxis = new XAxis { Type = AxisType.Time },
//        YAxis = new YAxis { Type = AxisType.Value },
//        Series = new List<ISeries>
//            {
//                new LineSeries
//                {
//                    Name = "tasks/sec",
//                    ShowSymbol = false,
//                    Data = new List<object>(),
//                    ItemStyle = new ItemStyle
//                    {
//                        Color = "#26FF00",
//                        Color0 = "#006400",
//                        BorderColor = "#00FFFB",
//                        BorderColor0 = "#2CFF05"
//                    },
//				    // NOTE: the original "game over" MarkLine at yAxis=300000 was copy-pasted from
//				    // the ms-based wait charts and doesn't apply to a tasks/sec rate. Removed rather
//				    // than guessing a real threshold — add one back if you have an actual rate alert
//				    // level in mind.
//				    Animation = true
//                }
//            }
//    };
//    #endregion

//    #region TopWaitsChart
//    public static ChartOptions TopWaitsChart_Options { get; set; } = new()
//    {
//        Tooltip = new Tooltip
//        {
//            Trigger = TooltipTrigger.Axis
//        },
//        Grid = new Grid
//        {
//            Left = "1%",
//            Right = "1%",
//            Bottom = "10%",
//            Top = "8%",
//            ContainLabel = true
//        },
//        XAxis = new XAxis
//        {
//            Type = AxisType.Category,
//            Data = new List<object>(),
//            AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }
//        },
//        YAxis = new YAxis
//        {
//            Type = AxisType.Value,
//            AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }
//        },
//        Series = new List<ISeries>
//            {
//                new BarSeries
//                {
//                    Name = "ms/sec",
//                    Data = new List<object>(),
//                    ItemStyle = new ItemStyle
//                    {
//                        Color = "#C800FF",
//                        Color0 = "#FF0062",
//                        BorderColor = "#FF0062",
//                        BorderColor0 = "#2CFF05"
//                    },
//                    MarkLine = new MarkLine
//                    {
//                        Data = new List<object>
//                        {
//                            new { yAxis = 300000, name = "game over" }
//                        },
//                        LineStyle = new LineStyle
//                        {
//                            Color = "#FFAE00",
//                            Type = new LineType(LineTypeStyle.Dashed)
//                        }
//                    }
//                }
//            }
//    };
//    #endregion

//    #region WaitCandleChart
//    public static ChartOptions WaitCandleChart_Options { get; set; } = new()
//    {
//        BackgroundColor = "transparent",

//        Tooltip = new Tooltip
//        {
//            Trigger = TooltipTrigger.Axis,
//            AxisPointer = new() { Type = AxisPointerType.Cross }
//        },
//        Grid = new Grid
//        {
//            Left = "1%",
//            Right = "1%",
//            Top = "8%",
//            Bottom = "18%",
//            ContainLabel = true
//        },
//        XAxis = new XAxis
//        {
//            Type = AxisType.Category,
//            BoundaryGap = true,
//            AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }
//        },
//        YAxis = new YAxis
//        {
//            Type = AxisType.Value,
//            Scale = true,
//            AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }
//        },
//        DataZoom = new List<IDataZoom>
//            {
//                new InsideDataZoom(),
//                new SliderDataZoom { Height = 20, Bottom = 10 }
//            },

//        Series = new List<ISeries>
//            {
//                new CandlestickSeries
//                {
//                    Name = "WAIT",
//                    Data = new List<object>(),
//                    ItemStyle = new ItemStyle
//                    {
//                        Color = "#32CD32",
//                        Color0 = "#ff1744",
//                        BorderColor = "#32CD32",
//                        BorderColor0 = "#ff5252"
//                    }
//                }
//            }
//    };
//    #endregion

//    #region WaitDistributionChart
//    public static ChartOptions WaitDistributionChart_Options { get; set; } = new()
//    {
//        Tooltip = new Tooltip
//        {
//            Trigger = TooltipTrigger.Item
//        },
//        Legend = new Legend
//        {
//            Orient = Orient.Vertical,
//            Top = "5%",
//            Left = new NumberOrString("left"),
//            TextStyle = new TextStyle { Color = ColorHex.NeonLime, Align = HorizontalAlign.Left }
//        },
//        Grid = new Grid()
//        {
//            Left = "right"
//        },
//        Series = new List<ISeries>
//            {
//                new PieSeries
//                {
//                    Name = "Wait Types",
//                    Radius = new CircleRadius(new NumberOrString("40%"), new NumberOrString("70%")),
//                    AvoidLabelOverlap = true,
//                    ItemStyle = new ItemStyle
//                    {
//                        BorderRadius = 10,
//                        BorderColor = "#11151c",
//                        BorderWidth = 2
//                    },
//                    Data = new List<object>()
//                }
//            }
//    };
//    #endregion

//    #region WaitRateChart
//    public static ChartOptions WaitRateChart_Options { get; set; } = new()
//    {
//        Tooltip = new Tooltip
//        {
//            Trigger = TooltipTrigger.Axis,
//            AxisPointer = new() { Type = AxisPointerType.Shadow }
//        },
//        Grid = new Grid
//        {
//            Left = "1%",
//            Right = "0%",
//            Bottom = "12%",
//            Top = "3%",
//            ContainLabel = true,
//            ShadowBlur = 12,
//            ShadowColor = ColorHex.MatrixGlow
//        },
//        XAxis = new XAxis { Type = AxisType.Time, AxisLabel = new AxisLabel() { Color = ColorHex.White } },
//        // Four independent axes instead of one shared axis. Each series now carries its *true* value
//        // (real %, real MB) plotted against its own scale, instead of being pre-multiplied into a
//        // shared 0-CHART_MAX range just so the lines visually fit together — that pre-scaling is what
//        // made the tooltip show "100,000%" for a real 10% CPU reading. Only the wait-rate axis (index 0)
//        // is visible; the other three are Show = false so the panel still reads as one chart, but each
//        // series' tooltip now reports its correct raw value via YAxisIndex below.
//        YAxisList = new List<YAxis>
//            {
//                new YAxis { Type = AxisType.Value },               // 1: CPU %
//			    new YAxis { Type = AxisType.Value },
//                new YAxis { Type = AxisType.Value },                                     // 3: I/O Load
//                new YAxis { Type = AxisType.Value, Scale = true, AxisLabel = new AxisLabel { Color = ColorHex.NeonLime } }, // 0: ms wait / sec                                // 2: Memory (MB)
//		    },
//        DataZoom = new List<IDataZoom>
//            {
//                new InsideDataZoom(),
//                new SliderDataZoom { Height = 20, Bottom = 0 }
//            },
//        Legend = new Legend
//        {

//            Data = new List<string> { "I/O Load", "Memory (MB)", "CPU %", "ms wait / sec", },
//            //TextStyle = new() { Color = "#ccc" },
//            Orient = Orient.Horizontal
//        },
//        Series = new List<ISeries>
//            {
//                new LineSeries
//                {
//                    Name = "I/O Load",
//                    YAxisIndex = 0,
//                    ShowSymbol = false,
//                    AreaStyle = new AreaStyle(),
//                    Data = new List<object>(),
//                    ItemStyle = new ItemStyle() { Color = ColorHex.Purple },
//                    Smooth = true,
//                    MarkLine = new MarkLine
//                    {
//                        Data = new List<object> { new { yAxis = 200000, name = "⏲ IO" } },
//                        LineStyle = new LineStyle
//                        {
//                            Color = ColorHex.Blurple,
//                            Type = new LineType(LineTypeStyle.Dashed)
//                        },
//                        Label = new Label
//                        {
//                            Show = true,
//                            Color = ColorHex.White,
//                            Formatter = "{b}"
//                        }
//                    },
//                    ColorBy = ColorBy.Data
//                },
//                new LineSeries
//                {
//                    Name = "Memory (MB)",
//                    YAxisIndex = 1,
//                    ShowSymbol = false,
//                    AreaStyle = new AreaStyle(),
//                    Data = new List<object>(),
//                    ItemStyle = new ItemStyle() {  Color = ColorHex.NeonBlue },
//                    Smooth = true,
//                    MarkLine = new MarkLine
//                    {
//                        Data = new List<object> { new { yAxis = 200000, name = "💾 Nom-Nom" } },
//                        LineStyle = new LineStyle
//                        {
//                            Color = ColorHex.Yellow,
//                            Type = new LineType(LineTypeStyle.Dashed)
//                        },
//                        Label = new Label
//                        {
//                            Show = true,
//                            Color = ColorHex.White,
//                            Formatter = "{b}"
//                        }
//                    },
//                    ColorBy = ColorBy.Data
//                },
//                new LineSeries
//                {
//                    Name = "CPU %",
//                    YAxisIndex = 2,
//                    ShowSymbol = false,
//                    AreaStyle = new AreaStyle(),
//                    Data = new List<object>(),
//                    Smooth = true,
//                    ItemStyle = new ItemStyle() { Color = ColorHex.Orange },
//                    MarkLine = new MarkLine
//                    {
//                        Data = new List<object> { new { yAxis = WaitRateChart_Max, name = "🤷‍ CPU High" } },
//                        LineStyle = new LineStyle
//                        {
//                            Color = ColorHex.DarkRed,
//                            Type = new LineType(LineTypeStyle.Dashed)
//                        },
//                        Label = new Label
//                        {
//                            Show = true,
//                            Color = ColorHex.White,
//                            Formatter = "{b}"
//                        }
//                    } ,
//                    ColorBy = ColorBy.Data
//                },
//                new LineSeries
//                {
//                    Name = "ms wait / sec",
//                    YAxisIndex = 3,
//                    ShowSymbol = false,
//                    AreaStyle = new AreaStyle(),
//                    Data = new List<object>(),
//                    Smooth = true,
//                    ItemStyle = new ItemStyle() { Color = ColorHex.BrightLime },
//                    MarkLine = new MarkLine
//                    {
//                        Data = new List<object> { new { yAxis = WaitRateChart_Max * .3, name = "🥠 Game Over " } },
//                        LineStyle = new LineStyle
//                        {
//                            Color = ColorHex.MatrixDark,
//                            Type = new LineType(LineTypeStyle.Dashed),
//                        },
//                        Label = new Label
//                        {
//                            Show = true,
//                            Color = ColorHex.White,
//                            Formatter = "{b}"
//                        }
//                    },
//                    ColorBy = ColorBy.Data
//                },
//            }
//    };
//    #endregion
//}

//  return new()
//  {
//      Tooltip = new PanoramicData.ECharts.Tooltip
//      {
//          Trigger = TooltipTrigger.Axis,
//          AxisPointer = new() { Type = AxisPointerType.Shadow }
//      },
//      Grid = new Grid { Left = "1%", Right = "1%", Bottom = "1%", Top = "1%", ContainLabel = true, ShadowBlur = 12, ShadowColor = ColorHex.MatrixGlow },
//      XAxis = new XAxis { Type = AxisType.Time, AxisLabel = new AxisLabel() { Color = ColorHex.White } },
//      // Four independent axes instead of one shared axis. Each series now carries its *true* value
//      // (real %, real MB) plotted against its own scale, instead of being pre-multiplied into a
//      // shared 0-CHART_MAX range just so the lines visually fit together — that pre-scaling is what
//      // made the tooltip show "100,000%" for a real 10% CPU reading. Only the wait-rate axis (index 0)
//      // is visible; the other three are Show = false so the panel still reads as one chart, but each
//      // series' tooltip now reports its correct raw value via YAxisIndex below.
//      YAxisList = new List<YAxis>
//      {
//          new YAxis { Type = AxisType.Value, Show = false,
//          AxisLabel = new AxisLabel { Color = ColorHex.NeonLime } },                                     // 3: I/O Load
// new YAxis { Type = AxisType.Value, Show = false,
//          AxisLabel = new AxisLabel { Color = ColorHex.NeonLime } },                                    // 2: Memory (MB)
// new YAxis { Type = AxisType.Value, Show = false, Min = 0, Max = 100,
//          AxisLabel = new AxisLabel { Color = ColorHex.NeonLime } },               // 1: CPU %
//          new YAxis { Type = AxisType.Value, AxisLabel = new AxisLabel() { Color = ColorHex.White } }, // 0: ms wait / sec
//},
//      DataZoom = new List<IDataZoom>
//      {
//          new InsideDataZoom(),
//          new SliderDataZoom { Height = 20, Bottom = 10 }
//      },
//      Legend = new Legend
//      {
//          Data = new List<string> { "I/O Load", "Memory (MB)", "CPU %", "ms wait / sec", },
//          TextStyle = new() { Color = "#ccc" },
//      },
//      Series = new List<ISeries>
//      {
//          new LineSeries
//          {
//              Name = "I/O Load",
//              YAxisIndex = 3,
//              ShowSymbol = false,
//              AreaStyle = new AreaStyle(),
//              Data = new List<object>(),
//              ItemStyle = new PanoramicData.ECharts.ItemStyle() { Color = JEO3.Core.ColorHex.CharcoalGray },
//              Smooth = true,
//              MarkLine = new MarkLine
//              {
//                  Data = new List<object> { new { yAxis = 200000, name = "game over" } },
//                  LineStyle = new LineStyle
//                  {
//                      Color = JEO3.Core.ColorHex.Blurple,
//                      Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed)
//                  }
//              },
//              ColorBy = ColorBy.Data
//          },
//          new LineSeries
//          {
//              Name = "Memory (MB)",
//              YAxisIndex = 2,
//              ShowSymbol = false,
//              AreaStyle = new AreaStyle(),
//              Data = new List<object>(),
//              ItemStyle = new PanoramicData.ECharts.ItemStyle() { Color = ColorHex.Purple },
//              Smooth = true,
//              MarkLine = new MarkLine
//              {
//                  Data = new List<object> { new { yAxis = 200000, name = "game over" } },
//                  LineStyle = new LineStyle
//                  {
//                      Color = ColorHex.Yellow,
//                      Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed)
//                  }
//              },
//              ColorBy = ColorBy.Data
//          },
//          new LineSeries
//          {
//              Name = "CPU %",
//              YAxisIndex = 1,
//              ShowSymbol = false,
//              AreaStyle = new AreaStyle(),
//              Data = new List<object>(),
//              Smooth = true,
//              ItemStyle = new PanoramicData.ECharts.ItemStyle() { Color = ColorHex.Orange },
//              MarkLine = new MarkLine
//              {
//                  Data = new List<object> { new { yAxis = 100, name = "game over" } },
//                  LineStyle = new LineStyle
//                  {
//                      Color = ColorHex.DarkRed,
//                      Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed)
//                  }
//              },
//              ColorBy = ColorBy.Data
//          },
//          new LineSeries
//          {
//              Name = "ms wait / sec",
//              YAxisIndex = 0,
//              ShowSymbol = false,
//              AreaStyle = new AreaStyle(),
//              Data = new List<object>(),
//              Smooth = true,
//              ItemStyle = new PanoramicData.ECharts.ItemStyle() { Color = JEO3.Core.ColorHex.BrightLime },
//              MarkLine = new MarkLine
//              {
//                  Data = new List<object> { new { yAxis = 10000000 * .3, name = "game over" } },
//                  LineStyle = new LineStyle
//                  {
//                      Color = ColorHex.MatrixGreen,
//                      Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed),
//                  },
//                  Label = new Label() { Color = ColorHex.MatrixGlow }
//              },
//              ColorBy = ColorBy.Data
//          }
//      }
//  };


//    return new ChartOptions()
//    {
//        Tooltip = new Tooltip
//        {
//            Trigger = TooltipTrigger.Axis,
//            AxisPointer = new() { Type = AxisPointerType.Shadow }
//        },
//        Grid = new Grid { Left = "1%", Right = "1%", Bottom = "1%", Top = "1%", ContainLabel = true, ShadowBlur = 12, ShadowColor = ColorHex.MatrixGlow },
//        XAxis = new XAxis { Type = AxisType.Time, AxisLabel = new AxisLabel() { Color = ColorHex.White } },
//        YAxisList = new List<YAxis>
//        {
//            new YAxis { Type = AxisType.Value, Show = false, AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }, Min = 0, Max = 10000 },                                     // 3: I/O Load
//   new YAxis { Type = AxisType.Value, Show = false, AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }, Min = 0, Max = 16000 },                                    // 2: Memory (MB)
//   new YAxis { Type = AxisType.Value, Show = false,  Position = LeftOrRight.Left, AxisLabel = new AxisLabel { Color = ColorHex.NeonLime }, Min = 0, Max = 100,  },               // 1: CPU %
//            new YAxis { Type = AxisType.Value, AxisLabel = new AxisLabel() { Color = ColorHex.White }, Min = 0, Max = 350000 }, // 0: ms wait / sec
//  },
//        DataZoom = new List<IDataZoom>
//                {
//                    new InsideDataZoom(),
//                    new SliderDataZoom { Height = 20, Bottom = 0 }
//                },
//        Legend = new Legend
//        {
//            Data = new List<string> { "I/O Load", "Memory (MB)", "CPU %", "ms wait / sec" },
//            TextStyle = new() { Color = ColorHex.White },
//            Orient = Orient.Horizontal
//        },
//        Series = new List<ISeries>
//                {
//                    new LineSeries
//                    {
//                        Name = "I/O Load",
//                        YAxisIndex = 0,
//                        ShowSymbol = false,
//                        AreaStyle = new AreaStyle(),
//                        Data = new List<object>(),
//                        ItemStyle = new ItemStyle() { Color = ColorHex.Purple },
//                        Smooth = true,
//                        //MarkLine = new MarkLine
//                        //{
//                        //    Data = new List<object> { new { yAxis = 200000, name = "game over" } },
//                        //    LineStyle = new LineStyle
//                        //    {
//                        //        Color = ColorHex.Blurple,
//                        //        Type = new LineType(LineTypeStyle.Dashed)
//                        //    }
//                        //},
//                        ColorBy = ColorBy.Data
//                    },
//                    new LineSeries
//                    {
//                        Name = "Memory (MB)",
//                        YAxisIndex = 1,
//                        ShowSymbol = false,
//                        AreaStyle = new AreaStyle(),
//                        Data = new List<object>(),
//                        ItemStyle = new ItemStyle() { Color = ColorHex.NeonBlue },
//                        Smooth = true,
//                        //MarkLine = new MarkLine
//                        //{
//                        //    Data = new List<object> { new { yAxis = 200000, name = "game over" } },
//                        //    LineStyle = new LineStyle
//                        //    {
//                        //        Color = ColorHex.Yellow,
//                        //        Type = new LineType(LineTypeStyle.Dashed)
//                        //    }
//                        //},
//                        ColorBy = ColorBy.Data
//                    },
//                    new LineSeries
//                    {
//                        Name = "CPU %",
//                        YAxisIndex = 2,
//                        ShowSymbol = false,
//                        AreaStyle = new AreaStyle(),
//                        Data = new List<object>(),
//                        Smooth = true,
//                        ItemStyle = new ItemStyle() { Color = ColorHex.Orange },
//                        //MarkLine = new MarkLine
//                        //{
//                        //    Data = new List<object> { new { yAxis = 100, name = "game over" } },
//                        //    LineStyle = new LineStyle
//                        //    {
//                        //        Color = ColorHex.DarkRed,
//                        //        Type = new LineType(LineTypeStyle.Dashed)
//                        //    }
//                        //},
//                        ColorBy = ColorBy.Data
//                    },
//                    new LineSeries
//                    {
//                        Name = "ms wait / sec",
//                        YAxisIndex = 3,
//                        ShowSymbol = false,
//                        AreaStyle = new AreaStyle(),
//                        Data = new List<object>(),
//                        Smooth = true,
//                        ItemStyle = new ItemStyle() { Color = ColorHex.BrightLime },
//                        MarkLine = new MarkLine
//                        {
//                            Data = new List<object> { new { yAxis = 300000, name = "game over" } },
//                            LineStyle = new LineStyle
//                            {
//                                Color = ColorHex.MatrixDark,
//                                Type = new LineType(LineTypeStyle.Dashed),
//                            },
//                            Label = new Label() { Color = ColorHex.MatrixGlow }
//                        },
//                        ColorBy = ColorBy.Data
//                    }

//                }
//    };
//}

#endregion