using JEO3.Monitor;

namespace JEO3.Site.Services
{
    public class MonitorSnapshotService : IAsyncDisposable
    {
        #region Properties
        public MonitorSnapshot? Latest { get; private set; }
        public event Action<MonitorSnapshot>? SnapshotUpdated;
        private readonly HttpClient _http;
        private CancellationTokenSource? _cts;
        private Task? _loopTask;
        #endregion

        #region Initialization
        public MonitorSnapshotService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient();
            var section = config.GetSection("Monitoring");
            _http.BaseAddress = new Uri(section["ApiBaseUrl"] ?? throw new Exception("API base URL is not configured"));
        }
        public async ValueTask DisposeAsync()
        {
            if (_cts is not null) { await _cts.CancelAsync(); try { await _loopTask!; } catch (OperationCanceledException) { } _cts.Dispose(); }
        }
        #endregion

        #region Run
        public async Task Start()
        {
            if (_loopTask is not null) return;
            _cts = new CancellationTokenSource();

            var snapshot = await _http.GetFromJsonAsync<MonitorSnapshot>("api/monitor/snapshot", _cts.Token);
            if (snapshot is not null)
            {
                Latest = snapshot;
                SnapshotUpdated?.Invoke(snapshot);
            }
        }
        #endregion
    }
}
