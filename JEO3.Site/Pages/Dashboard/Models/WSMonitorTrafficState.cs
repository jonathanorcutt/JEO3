namespace JEO3.Site.Dashboard
{
    /// <summary>
    /// Holds UI presentation state values to separate layout overhead from metadata engines.
    /// </summary>
    public sealed class WSMonitorTrafficState
    {
        public string CurrentTable { get; set; } = string.Empty;
        public string PercentCompleted { get; set; } = string.Empty;
        public bool IsRunning { get; set; } = false;
    }
}
