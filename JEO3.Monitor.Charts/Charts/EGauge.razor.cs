using JEO3.Core;
using JEO3.Logging;
using Microsoft.AspNetCore.Components;
using PanoramicData.ECharts;
using Microsoft.JSInterop; // Required to catch the exact JS exception type

namespace JEO3.Monitor.ECharts
{
    public partial class EGauge : MonitorChartBase
    {
        #region Properties
        [Parameter] public override string Width { get; set; } = "270px";
        [Parameter] public override string Height { get; set; } = "270px";
        [Parameter] public string Title { get; set; } = "PRESSURE";
        private static double currentWaitPressure = 0;
        protected override ChartOptions Options { get; set; } = ChartOptionsLibrary.EGauge_Options;
        #endregion

        #region Initialization
        protected override async Task RenderInitialAsync(MonitorSnapshot initial)
        {
            if (IsDisposed || initial == null || initial.History == null) return;

            UpdateChartData();
            await base.SafeUpdateChartAsync();
        }
        #endregion

        #region Updates
        protected override async Task AppendDeltasAsync(MonitorDeltaResponse delta)
        {
            if (IsDisposed) return;

            if (delta != null && delta.History != null && delta.History.Count > 0)
            {
                UpdateChartData();
                await SafeUpdateChartAsync();
            }
        }

        private void UpdateChartData()
        {
            try
            {
                if (IsDisposed) return;

                // Pull the most recent value from the globally maintained WaitHistory
                var latest = WaitHistory.OrderByDescending(v => v.Timestamp).FirstOrDefault();
                currentWaitPressure = Math.Round(latest?.TotalWaitMsPerSec ?? 0);

                if (Options.Series?.FirstOrDefault() is GaugeSeries gaugeSeries)
                {
                    gaugeSeries.Data = new List<object> { new { value = currentWaitPressure, name = "☢️" + Title } };

                    if (gaugeSeries.Progress?.ItemStyle != null)
                    {
                        gaugeSeries.Progress.ItemStyle.Color = currentWaitPressure > 200000 ? new Color(ColorHex.Red)
                            : currentWaitPressure > 100000 ? new Color(ColorHex.Yellow) : new Color(ColorHex.MatrixGlow);
                    }
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