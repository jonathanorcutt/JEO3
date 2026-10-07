using JEO3.Logging;
using Microsoft.AspNetCore.Components;
using PanoramicData.ECharts;

namespace JEO3.Monitor.ECharts
{
    public partial class BlockingChart : MonitorChartBase
    {
        #region Properties
        [Parameter] public int TopN { get; set; } = 10;
        protected override ChartOptions Options { get; set; } = ChartOptionsLibrary.BlockingChart_Options;
        #endregion

        #region Initialization
        protected override async Task RenderInitialAsync(MonitorSnapshot initial)
        {
            if (IsDisposed || initial == null || initial.BlockingChain == null) return;

            if (initial.BlockingChain != null)
            {
                await UpdateChartData(initial.BlockingChain);
                await SafeUpdateChartAsync();
            }
        }
        #endregion

        #region Updates	
        protected override async Task AppendDeltasAsync(MonitorDeltaResponse delta)
        {
            if (IsDisposed) return;

            if (delta.BlockingChain != null)
            {
                await UpdateChartData(delta.BlockingChain);
                await SafeUpdateChartAsync();
            }
        }
        private async Task UpdateChartData(IEnumerable<BlockingRow> activeRequests)
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
                if (Chart != null) await Chart.UpdateAsync();
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex);
            }
        }
        #endregion
    }
}
