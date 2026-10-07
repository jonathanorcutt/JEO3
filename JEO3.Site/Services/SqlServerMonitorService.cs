using System.Diagnostics;
using JEO3.Logging;
using JEO3.Monitor;

namespace JEO3.Site.Services
{
    /// <summary>
    /// Polls sys.dm_os_wait_stats and supporting DMVs on a fixed interval.
    /// Computes wait-time rates from successive snapshots.
    ///
    /// Wait-rate formula (documented here and in WaitRateSample.cs):
    ///   WaitTimeMsPerSec(type) = Δwait_time_ms(type) / Δelapsed_seconds
    /// where Δ = value_at_sampleB − value_at_sampleA.
    /// A negative delta (counter reset after server restart) is clamped to zero.
    /// The primary "headline" rate is the sum across all non-benign wait types.
    ///
    /// SQL Server-specific. Not used for Oracle or Postgres.
    /// </summary>
    public sealed class SqlServerMonitorService : IServerMonitorService
    {
        #region Properties
        private readonly IMonitorApiClient _apiClient;
        private readonly IMonitorStateConsumer _consumer;
        private readonly MonitorState _workspace = new MonitorState();
        private CancellationTokenSource? _cts;
        private Task? _loopTask;
        private readonly List<WaitRateSample> _history = new(HistoryCapacity + 1);
        private DateTime _lastServerTimeUtc = DateTime.MinValue;
        private readonly List<DeadlockEvent> _deadlockLog = [];

        // Constants
        private const int DefaultIntervalSeconds = 3;
        protected const int MaxRetention = 60000;
        private const int HistoryCapacity = MaxRetention;
        private const int SparklineCapacity = MaxRetention;
        public int IntervalSeconds { get; set; } = DefaultIntervalSeconds;
        public bool IsPolling => _cts is { IsCancellationRequested: false };
        #endregion

        #region Initialization
        public SqlServerMonitorService(IMonitorApiClient apiClient, IMonitorStateConsumer consumer)
        {
            _apiClient = apiClient;
            _consumer = consumer;
        }
        public async ValueTask DisposeAsync()
        {
            await StopAsync().ConfigureAwait(false);
        }
        #endregion

        #region Execution
        public Task StartAsync()
        {
            DateTime now = DateTime.Now;
            if (IsPolling) return Task.CompletedTask;

            _cts = new CancellationTokenSource();
            _loopTask = RunLoopAsync(_cts.Token);
            _workspace.Status = MonitorPollingStatus.Polling;
            _consumer.NotifyMonitorStateChanged(_workspace);

            var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
            Trace.WriteLine($"JEO3.Site - SqlServerMonitorService - StartAsync - Elapsed {log}sec");

            return Task.CompletedTask;
        }
        public async Task StopAsync()
        {
            DateTime now = DateTime.Now;
            if (_cts == null) return;
            await _cts.CancelAsync();
            if (_loopTask != null)
            {
                try { await _loopTask.ConfigureAwait(false); }
                catch (OperationCanceledException) { }
                catch (Exception ex) { ExceptionUtility.LogException(ex); }
            }
            _cts.Dispose();
            _cts = null;
            _loopTask = null;
            _workspace.Status = MonitorPollingStatus.Stopped;
            _consumer.NotifyMonitorStateChanged(_workspace);

            var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
            Trace.WriteLine($"JEO3.Site - SqlServerMonitorService - StopAsync - Elapsed {log}sec");
        }
        private async Task RunLoopAsync(CancellationToken ct)
        {
            DateTime now = DateTime.Now;
            // 1. Initial Snapshot (Populate the UI immediately)
            var initial = await _apiClient.GetInitialSnapshotAsync(ct).ConfigureAwait(false);
            if (initial != null)
            {
                ApplyInitialSnapshot(initial);
            }

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(IntervalSeconds));

            // 2. Polling Loop (Grab deltas every 2 seconds)
            while (!ct.IsCancellationRequested && await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
            {
                if (_lastServerTimeUtc == DateTime.MinValue) continue;

                var delta = await _apiClient.GetDeltaAsync(_lastServerTimeUtc, ct).ConfigureAwait(false);

                if (delta != null)
                {
                    // ---> THIS IS WHERE APPLYDELTA EXECUTES <---
                    ApplyDelta(delta);
                }
            }

            var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
            Trace.WriteLine($"JEO3.Site - SqlServerMonitorService - RunLoopAsync - Elapsed {log}sec");
        }
        private void ApplyInitialSnapshot(MonitorSnapshot snapshot)
        {
            DateTime now = DateTime.Now;
            _lastServerTimeUtc = snapshot.ServerTimeUtc;
            _workspace.Status = snapshot.Status;
            _workspace.LastError = snapshot.LastError;

            // Load historical wait stats
            _history.Clear();
            if (snapshot.History != null)
            {
                _history.AddRange(snapshot.History);
            }
            _workspace.History = _history.AsReadOnly();

            // Setup sparklines
            var spark = _workspace.SparklinePoints;
            spark.Clear();
            foreach (var sample in _history.TakeLast(SparklineCapacity))
            {
                spark.Add(Math.Round(sample.TotalWaitMsPerSec, 0));
            }

            // Load full initial state grids
            _workspace.ActiveRequests = snapshot.ActiveRequests ?? [];
            _workspace.BlockingChain = snapshot.BlockingChain ?? [];
            _workspace.TopExpensiveQueries = snapshot.TopExpensiveQueries ?? [];

            // Prime the deadlock log
            _deadlockLog.Clear();
            if (snapshot.RecentDeadlocks != null)
            {
                _deadlockLog.AddRange(snapshot.RecentDeadlocks);
            }
            _workspace.Deadlocks = _deadlockLog;

            _consumer.NotifyMonitorStateChanged(_workspace);

            var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
            Trace.WriteLine($"JEO3.Site - SqlServerMonitorService - ApplyInitialSnapshot - Elapsed {log}sec");
        }
        private void ApplyDelta(MonitorDeltaResponse delta)
        {
            DateTime now = DateTime.Now;
            _lastServerTimeUtc = delta.ServerTimeUtc;
            _workspace.Status = delta.Status;
            _workspace.LastError = delta.LastError;

            // 1. TIME-SERIES: Append wait stats to sparklines/charts
            if (delta.History != null && delta.History.Count > 0)
            {
                foreach (var sample in delta.History)
                {
                    _history.Add(sample);
                    if (_history.Count > HistoryCapacity) _history.RemoveAt(0);

                    _workspace.SparklinePoints.Add(Math.Round(sample.TotalWaitMsPerSec, 0));
                    while (_workspace.SparklinePoints.Count > SparklineCapacity)
                        _workspace.SparklinePoints.RemoveAt(0);
                }
                _workspace.History = _history.AsReadOnly();
            }

            // 2. DEADLOCK LOG: Append new deadlock events (NEVER clear existing)
            if (delta.RecentDeadlocks != null && delta.RecentDeadlocks.Count > 0)
            {
                foreach (var dl in delta.RecentDeadlocks)
                {
                    if (!_deadlockLog.Any(d => d.Timestamp == dl.Timestamp && d.VictimSessionId == dl.VictimSessionId))
                    {
                        _deadlockLog.Add(dl);
                    }
                }
                _workspace.Deadlocks = _deadlockLog.OrderByDescending(d => d.Timestamp).ToList();
            }

            // 3. TOP EXPENSIVE QUERIES: Sticky update (Only overwrite if API provided a non-empty cache refresh)
            if (delta.TopExpensiveQueries != null && delta.TopExpensiveQueries.Count > 0)
            {
                _workspace.TopExpensiveQueries = delta.TopExpensiveQueries;
            }

            // 4. LIVE ACTIVE STATES: Always update current active requests & blocking trees
            _workspace.ActiveRequests = delta.ActiveRequests ?? [];
            _workspace.BlockingChain = delta.BlockingChain ?? [];

            _consumer.NotifyMonitorStateChanged(_workspace);


            var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
            Trace.WriteLine($"JEO3.Site - SqlServerMonitorService - ApplyDelta - Elapsed {log}sec");
        }
        #endregion
    }
}