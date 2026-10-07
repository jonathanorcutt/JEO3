using JEO3.Monitor.ECharts;

namespace JEO3.Monitor.ECharts
{
    public interface IMonitorBase
    {
        string Width { get;  }
        string Height { get; }
        string LabelText { get; }
        bool IsDisposed { get; }
    }
}
