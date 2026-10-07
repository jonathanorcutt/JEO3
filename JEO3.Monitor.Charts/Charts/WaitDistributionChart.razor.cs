using JEO3.Logging;
using Microsoft.AspNetCore.Components;
using PanoramicData.ECharts;

namespace JEO3.Monitor.ECharts
{
    public partial class WaitDistributionChart : MonitorChartBase
    {
        #region Properties
        [Parameter] public int TopN { get; set; } = 10;
        protected override ChartOptions Options { get; set; } = ChartOptionsLibrary.WaitDistributionChart_Options;
        #endregion

        #region Initialization
        protected override async Task RenderInitialAsync(MonitorSnapshot initial)
        {
            if (IsDisposed || initial == null || initial.History == null) return;

            UpdatePieChart();
            await SafeUpdateChartAsync();
        }
        #endregion

        #region Updates
        protected override async Task AppendDeltasAsync(MonitorDeltaResponse delta)
        {
            if (IsDisposed) return;

            // Only redraw if the delta actually contained new wait data
            if (delta.History != null && delta.History.Count > 0)
            {
                UpdatePieChart();
                await SafeUpdateChartAsync();
            }
        }
        private void UpdatePieChart()
        {
            try
            {
                if (IsDisposed) return;

                var latest = WaitHistory.OrderByDescending(h => h.Timestamp).FirstOrDefault();
                if (latest?.TopWaits == null) return;

                if (Options.Series?.FirstOrDefault() is PieSeries pie)
                {
                    pie.Data = latest.TopWaits
                        .Take(TopN)
                        .Select(w => new { name = w.WaitType, value = w.WaitMsPerSec })
                        .Cast<object>()
                        .ToList();
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
