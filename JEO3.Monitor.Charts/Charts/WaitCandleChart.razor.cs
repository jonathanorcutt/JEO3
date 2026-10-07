using JEO3.Logging;
using PanoramicData.ECharts;

namespace JEO3.Monitor.ECharts
{
    public partial class WaitCandleChart : MonitorChartBase
    {
        #region Properties
        protected override ChartOptions Options { get; set; } = ChartOptionsLibrary.WaitCandleChart_Options;
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

            // Filter appropriately based on the chart (e.g. NewWaitHistory, NewPerformanceHistory)
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

                var history = WaitHistory.OrderBy(h => h.Timestamp).ToList();

                Options.XAxis!.Data = history
                    .Select(h => (object)h.Timestamp.ToLocalTime().ToString("HH:mm:ss"))
                    .ToList();
                var candles = new List<object>();

                for (var i = 0; i < history.Count; i++)
                {
                    var current = history[i].TotalWaitMsPerSec;
                    var previous = i > 0 ? history[i - 1].TotalWaitMsPerSec : current;

                    var open = previous;
                    var close = current;

                    // Give the candle a real body + wick based on the movement
                    var movement = Math.Abs(close - open);
                    var low = Math.Max(0, Math.Min(open, close) - movement * 0.35);
                    var high = Math.Max(open, close) + movement * 0.35;

                    candles.Add(new object[] { open, close, low, high });
                }

                ((CandlestickSeries)Options.Series![0]).Data = candles;
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex);
            }
        }
        #endregion
    }
}
