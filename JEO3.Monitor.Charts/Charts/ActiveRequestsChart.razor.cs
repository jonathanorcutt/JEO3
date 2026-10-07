using JEO3.Logging;
using Microsoft.AspNetCore.Components;
using PanoramicData.ECharts;

namespace JEO3.Monitor.ECharts
{
    public partial class ActiveRequestsChart : MonitorChartBase
    {
        #region Properties
        [Parameter] public int TopN { get; set; } = 10;
        protected override ChartOptions Options { get; set; } = ChartOptionsLibrary.ActiveRequestsChart_Options;

        //            = new ()
        //        {
        //            // Deep cyberpunk dark background
        //            BackgroundColor = "#0a0c10",

        //            Tooltip = new PanoramicData.ECharts.Tooltip
        //            {
        //                Trigger = TooltipTrigger.Axis,
        //                BackgroundColor = "rgba(10, 12, 16, 0.85)", // Semi-transparent dark
        //                BorderColor = "#1e90ff",                    // Dodger blue border
        //                BorderWidth = 1,
        //                TextStyle = new PanoramicData.ECharts.TextStyle { Color = new Color("#ffffff") }
        //            },

        //            Grid = new Grid { Left = "3%", Right = "4%", Bottom = "10%", Top = "8%", ContainLabel = true },

        //            XAxis = new XAxis
        //            {
        //                Type = AxisType.Category,
        //                Data = new List<object>(),
        //                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color("#1e90ff") } }, // Neon blue axis line
        //                AxisLabel = new AxisLabel { Color = new Color("#6495ed") } // Cornflower blue text
        //            },

        //            YAxis = new YAxis
        //            {
        //                Type = AxisType.Value,
        //                AxisLine = new AxisLine { LineStyle = new LineStyle { Color = new Color("#1e90ff") } },
        //                AxisLabel = new AxisLabel { Color = new Color("#6495ed") },
        //                SplitLine = new SplitLine { LineStyle = new LineStyle { Color = new Color("rgba(30, 144, 255, 0.1") } } // Subtle blue grid matrix
        //            },

        //            Series = new List<ISeries>
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
        //                    Type = new PanoramicData.ECharts.LineType(LineTypeStyle.Dashed)
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
        //        };
        #endregion

        #region Initialization
        protected override async Task RenderInitialAsync(MonitorSnapshot initial)
        {
            if (IsDisposed || initial == null || initial.ActiveRequests == null) return;

            if (initial.ActiveRequests != null)
            {
                UpdateChartData(initial.ActiveRequests);
                await SafeUpdateChartAsync();
            }
        }
        #endregion

        #region Updates
        protected override async Task AppendDeltasAsync(MonitorDeltaResponse delta)
        {
            if (IsDisposed) return;

            if (delta.ActiveRequests != null)
            {
                UpdateChartData(delta.ActiveRequests);
                await SafeUpdateChartAsync();
            }
        }
        private void UpdateChartData(IEnumerable<ActiveRequestRow> activeRequests)
        {
            try
            {
                if (IsDisposed) return;

                var topActive = activeRequests
                    .OrderByDescending(r => r.WaitTimeSec)
                    .Take(TopN)
                    .ToList();

                Options.XAxis!.Data = topActive.Select(r => (object)$"SPID {r.SessionId}").ToList();

                if (Options.Series?.FirstOrDefault() is BarSeries bar)
                {
                    bar.Data = topActive.Select(r => (object)r.WaitTimeSec).ToList();
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex);
            }
        }
        #endregion
    }
}
