using JEO3.Monitor;

namespace JEO3.Site.Dashboard
{
    public sealed class WSMonitorState
    {
        public MonitorState Monitor { get; } = new();

        // For The "Generate Traffic" Button In UI
        public WSMonitorTrafficState Traffic { get; set; } = new();
    }
}
