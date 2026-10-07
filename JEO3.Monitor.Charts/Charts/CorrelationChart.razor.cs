using JEO3.Logging;
using PanoramicData.ECharts;

namespace JEO3.Monitor.ECharts
{
    public partial class CorrelationChart : MonitorChartBase
    {
        #region Properties
        protected override ChartOptions Options { get; set; } = ChartOptionsLibrary.CorrelationChart_Options;
        #endregion

        #region Initialization
        protected override async Task RenderInitialAsync(MonitorSnapshot initial)
        {
            if (IsDisposed || initial == null || initial.History == null) return;

            UpdateChartData();
            await SafeUpdateChartAsync();
        }

        #endregion

        #region Updates
        protected override async Task AppendDeltasAsync(MonitorDeltaResponse delta)
        {
            if (IsDisposed) return;

            if (delta.History != null && delta.History.Count > 0)
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

                if (Options.Series?.FirstOrDefault() is ScatterSeries scatter)
                {
                    scatter.Data = WaitHistory
                        .Select(h => (object)new object[] { h.TotalWaitingTasksPerSec, h.TotalWaitMsPerSec })
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
