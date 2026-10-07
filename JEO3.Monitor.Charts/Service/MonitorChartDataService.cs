namespace JEO3.Monitor.ECharts
{
    public sealed class MonitorChartDataService : IMonitorChartDataService, IAsyncDisposable
    {
        public static bool Started = false;
        private readonly IMonitorApiClient _apiClient;
        private readonly SemaphoreSlim _startLock = new(1, 1);
        private Task? _pollingTask;
        private CancellationTokenSource? _cts;
        private DateTime _lastServerTimeUtc;

        public MonitorSnapshot? CurrentSnapshot { get; private set; }
        public event Action<MonitorDeltaResponse>? OnDelta;

        public MonitorChartDataService(IMonitorApiClient apiClient) => _apiClient = apiClient;

        public async Task EnsureStartedAsync()
        {
            if (_pollingTask != null || Started) return;
            await _startLock.WaitAsync();
            try
            {
                if (_pollingTask != null) return;
                Started = true;
                CurrentSnapshot = await _apiClient.GetInitialSnapshotAsync();
                if (CurrentSnapshot != null) _lastServerTimeUtc = CurrentSnapshot.ServerTimeUtc;
                _cts = new CancellationTokenSource();
                _pollingTask = PollAsync(_cts.Token);
            }
            finally { _startLock.Release(); }
        }

        private async Task PollAsync(CancellationToken token)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));
            while (!token.IsCancellationRequested && await timer.WaitForNextTickAsync(token))
            {
                var delta = await _apiClient.GetDeltaAsync(_lastServerTimeUtc);
                if (delta == null) continue;
                _lastServerTimeUtc = delta.ServerTimeUtc;
                OnDelta?.Invoke(delta);
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            if (_pollingTask != null) await _pollingTask;
        }
    }
}
