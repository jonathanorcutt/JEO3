using JEO3.Monitor;
using Microsoft.AspNetCore.Components;
using PanoramicData.ECharts;

namespace JEO3.Site.Components.ECharts
{
    /// <summary>
    /// Shared lifecycle for every live monitoring chart.
    /// Inherits the delta-polling engine from MonitorBase and provides the static ECharts Options shell.
    /// Concrete charts must supply the initial ChartOptions shell and map delta data into it.
    /// </summary>
    public abstract class MonitorChartBase : MonitorBase
    {
        protected PanoramicData.ECharts.EChart? Chart;

        protected abstract ChartOptions Options { get; set; }
    }
}