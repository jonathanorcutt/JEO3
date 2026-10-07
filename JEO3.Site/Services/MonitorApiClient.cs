using System.Diagnostics;
using JEO3.Monitor;

namespace JEO3.Site.Services
{
    public sealed class MonitorApiClient : IMonitorApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<MonitorApiClient> _logger;

        public MonitorApiClient(HttpClient http, ILogger<MonitorApiClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<MonitorSnapshot?> GetInitialSnapshotAsync(CancellationToken ct = default)
        {
            try
            {
                DateTime now = DateTime.Now;

                MonitorSnapshot result = null;
                while (result == null)
                {
                    result = await _http.GetFromJsonAsync<MonitorSnapshot>("/api/monitor/initial", ct).ConfigureAwait(false);
                }

                var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
                Trace.WriteLine($"JEO3.Site - MonitorApiClient - GetInitialSnapshotAsync - Elapsed {log}sec");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch initial monitor snapshot");
                return null;
            }
            finally
            {
                bool bp = true;
            }
        }

        public async Task<MonitorDeltaResponse?> GetDeltaAsync(DateTime sinceUtc, CancellationToken ct = default)
        {
            try
            {
                DateTime now = DateTime.Now;

                var url = $"/api/monitor/delta?sinceUtc={Uri.EscapeDataString(sinceUtc.ToString("o"))}";
                var result =  await _http.GetFromJsonAsync<MonitorDeltaResponse>(url, ct).ConfigureAwait(false);

                var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
                Trace.WriteLine($"JEO3.Site - MonitorApiClient - GetDeltaAsync - Elapsed {log}sec");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch monitor delta");
                return null;
            }
        }
    }
}