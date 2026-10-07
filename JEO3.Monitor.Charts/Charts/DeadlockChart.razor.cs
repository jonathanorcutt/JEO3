using JEO3.Logging;
using Microsoft.AspNetCore.Components;
using PanoramicData.ECharts;

namespace JEO3.Monitor.ECharts
{
    public partial class DeadlockChart : MonitorChartBase
    {
        #region Properties
        [Parameter] public int RollingMinutes { get; set; } = 30;
        protected override ChartOptions Options { get; set; } = ChartOptionsLibrary.DeadlockChart_Options;
        #endregion

        #region Initialization
        protected override async Task RenderInitialAsync(MonitorSnapshot initial)
        {
            if (IsDisposed || initial == null || initial.RecentDeadlocks == null) return;

            UpdateChartData();
            await base.SafeUpdateChartAsync();
        }
        #endregion

        #region Updates
        protected override async Task AppendDeltasAsync(MonitorDeltaResponse delta)
        {
            if (IsDisposed) return;

            if (delta.RecentDeadlocks != null && delta.RecentDeadlocks.Count > 0)
            {
                UpdateChartData();
                await base.SafeUpdateChartAsync();
            }
        }
        private void UpdateChartData()
        {
            try
            {
                if (IsDisposed) return;

                var deadlocksByMinute = RecentDeadlocks
                    .GroupBy(d => new DateTime(d.Timestamp.Year, d.Timestamp.Month, d.Timestamp.Day,
                                                d.Timestamp.Hour, d.Timestamp.Minute, 0))
                    .OrderBy(g => g.Key)
                    .TakeLast(RollingMinutes)
                    .ToList();

                Options.XAxis!.Data = deadlocksByMinute.Select(g => (object)g.Key.ToString("HH:mm")).ToList();

                if (Options.Series?.FirstOrDefault() is BarSeries bar)
                {
                    bar.Data = deadlocksByMinute.Select(g => (object)g.Count()).ToList();
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
