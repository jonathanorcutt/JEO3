namespace JEO3.Site.Services
{
    /// <summary>
    /// Polls SQL Server DMVs on a fixed interval and updates DashboardWorkspace.Monitor.
    /// MSSQL-specific — not applicable to Oracle or Postgres.
    /// </summary>
    public interface IServerMonitorService : IAsyncDisposable
    {
        /// <summary>Start polling. Safe to call multiple times; noop if already running.</summary>
        Task StartAsync();

        /// <summary>Stop polling and reset state to Stopped.</summary>
        Task StopAsync();

        /// <summary>True while the polling loop is active.</summary>
        bool IsPolling { get; }

        /// <summary>Polling interval in seconds (default 5).</summary>
        int IntervalSeconds { get; set; }
    }
}
