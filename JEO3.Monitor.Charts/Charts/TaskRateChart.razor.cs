using JEO3.Logging;
using PanoramicData.ECharts;

namespace JEO3.Monitor.ECharts
{
    public partial class TaskRateChart : MonitorChartBase
    {
        #region Properties
        protected override ChartOptions Options { get; set; } = ChartOptionsLibrary.TaskRateChart_Options;
        #endregion

        #region Initialization
        protected override async Task RenderInitialAsync(MonitorSnapshot initial)
        {
            if (IsDisposed || initial == null || initial.History == null) return;

            await UpdateChartData();
            await SafeUpdateChartAsync();
        }
        #endregion

        #region Updates
        protected override async Task AppendDeltasAsync(MonitorDeltaResponse delta)
        {
            if (IsDisposed) return;

            if (delta.History != null && delta.History.Count > 0)
            {
                await UpdateChartData();
                await SafeUpdateChartAsync();
            }
        }
        private async Task UpdateChartData()
        {
            try
            {
                if (IsDisposed) return;

                if (Options.Series?.FirstOrDefault() is LineSeries line)
                {
                    // Pull directly from the shared WaitHistory list managed by MonitorBase
                    line.Data = WaitHistory
                        .Select(h => (object)new object[] { h.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), h.TotalWaitingTasksPerSec })
                        .ToList();
                    if (Chart != null) await Chart.UpdateAsync();
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
