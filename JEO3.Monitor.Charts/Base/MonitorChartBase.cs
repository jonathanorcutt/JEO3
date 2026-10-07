using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PanoramicData.ECharts;
using System;
using System.Threading.Tasks;

namespace JEO3.Monitor.ECharts
{
    /// <summary>
    /// Shared lifecycle for every live monitoring chart.
    /// Inherits the delta-polling engine from MonitorBase and provides the static ECharts Options shell with adaptive resizing.
    /// Concrete charts must supply the initial ChartOptions shell and map delta data into it.
    /// </summary>
    public abstract class MonitorChartBase : MonitorBase, IAsyncDisposable
    {
        #region Properties

        [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
        [CascadingParameter] public ChartThemeType ActiveTheme { get; set; } = ChartThemeType.Default;
        protected EChart? Chart;
        protected abstract ChartOptions Options { get; set; }
        private ChartThemeType _lastTheme;

        // ResizeObserver state module handles
        private IJSObjectReference? _resizeModule;
        private DotNetObjectReference<MonitorChartBase>? _selfReference;
        #endregion

        #region Initialization

        protected override void OnParametersSet()
        {
            // Only rebuild options if the theme actually changed
            if (ActiveTheme != _lastTheme || Options == null)
            {
                _lastTheme = ActiveTheme;
                var chartType = this.GetType().Name;
                var chartTheme = ChartTheme.FromEnum(ActiveTheme);
                switch (chartType)
                {
                    case nameof(ActiveRequestsChart):
                        this.Options = ChartOptionsLibrary.BuildActiveRequestsChartOptions(chartTheme);
                        break;
                    case nameof(BlockingChart):
                        this.Options = ChartOptionsLibrary.BuildBlockingChartOptions(chartTheme);
                        break;
                    case nameof(CorrelationChart):
                        this.Options = ChartOptionsLibrary.BuildCorrelationChartOptions(chartTheme);
                        break;
                    case nameof(DeadlockChart):
                        this.Options = ChartOptionsLibrary.BuildDeadlockChartOptions(chartTheme);
                        break;
                    case nameof(EGauge):
                        this.Options = ChartOptionsLibrary.BuildEGaugeOptions(chartTheme);
                        break;
                    case nameof(TaskRateChart):
                        this.Options = ChartOptionsLibrary.BuildTaskRateChartOptions(chartTheme);
                        break;
                    case nameof(TopWaitsChart):
                        this.Options = ChartOptionsLibrary.BuildTopWaitsChartOptions(chartTheme);
                        break;
                    case nameof(WaitCandleChart):
                        this.Options = ChartOptionsLibrary.BuildWaitCandleChartOptions(chartTheme);
                        break;
                    case nameof(WaitRateChart):
                        this.Options = ChartOptionsLibrary.BuildWaitRateChartOptions(chartTheme);
                        break;
                    case nameof(WaitDistributionChart):
                        this.Options = ChartOptionsLibrary.BuildWaitDistributionChartOptions(chartTheme);
                        break;
                    case nameof(WaitRateSparklineChart):
                        this.Options = ChartOptionsLibrary.BuildWaitRateSparklineChartOptions(chartTheme);
                        break;
                    case nameof(WaitRateStackedAreaChart):
                        this.Options = ChartOptionsLibrary.BuildWaitRateStackedAreaChartOptions(chartTheme);
                        break;
                }
                StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // CRITICAL SAFETY CHECK: Wait until the wrapped chart ref binds completely to avoid null references
                if (Chart == null) return;

                _selfReference = DotNetObjectReference.Create(this);

                // Zero-dependency ResizeObserver runtime initialization script pattern
                _resizeModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import",
                    "data:text/javascript,export function observe(id,ref){const el=document.getElementById(id);if(!el)return;const ro=new ResizeObserver(()=>{ref.invokeMethodAsync('TriggerChartResize');});ro.observe(el);return{dispose:()=>ro.disconnect()};}");

                // FIX 1: Access the public .Id parameter hanging directly off your referenced Chart field instantiation!
                await _resizeModule.InvokeVoidAsync("observe", Chart.Id, _selfReference);
            }
        }

        public override async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            if (_resizeModule != null && (_selfReference != null && ((IMonitorBase)_selfReference.Value).IsDisposed == false))
            {
                try
                {
                    await _resizeModule.InvokeVoidAsync("dispose");
                    await _resizeModule.DisposeAsync();
                }
                catch { }
            }
            _selfReference?.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Update
        /// <summary>
        /// Wraps UI updates and JS Interop calls inside an isolated, safe execution envelope.
        /// </summary>
        protected async Task SafeUpdateChartAsync()
        {
            if (IsDisposed) return;

            try
            {
                await InvokeAsync(StateHasChanged);

                // Final confirmation that the browser context hasn't died during state rendering
                if (!IsDisposed && Chart != null)
                {
                    await Chart.UpdateAsync();
                }
            }
            catch (JSDisconnectedException)
            {
                // Silently ignore: The browser circuit dropped right mid-render execution.
            }
            catch (Exception ex) when (ex.Message.Contains("JavaScript interop"))
            {
                // Fallback catch-all boundary for general interop disconnect errors
            }
        }
        #endregion

        #region Resize
        [JSInvokable]
        public async Task TriggerChartResize()
        {
            // FIX 2: Check against the component's internal property value to find the correct canvas block
            if (Chart != null && !string.IsNullOrEmpty(Chart.Id))
            {
                try
                {
                    // Target the actual underlying HTML unique DOM element node generated by the wrapper
                    await JSRuntime.InvokeVoidAsync("eval", $@"
                        (function() {{
                            var el = document.getElementById('{Chart.Id}');
                            if (el) {{
                                var chartDiv = el.querySelector('div') || el;
                                var instance = echarts.getInstanceByDom(chartDiv);
                                if (instance) {{
                                    instance.resize();
                                }}
                            }}
                        }})();
                    ");
                }
                catch (JSException)
                {
                    // Shield pipeline gracefully from quick context switches
                }
            }
        }
        #endregion
    }
}