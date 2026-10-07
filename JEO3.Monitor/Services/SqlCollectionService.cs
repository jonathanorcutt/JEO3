using System.Data;
using System.Diagnostics;
using JEO3.Core;
using JEO3.Extensions;
using JEO3.Logging;
using JEO3.Monitor.Models;
using JEO3.Monitor.Services.API;
using JEO3.Providers;
using JEO3.Providers.Extensions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JEO3.Monitor
{
    public sealed class SqlCollectionService : BackgroundService
    {
        #region Properties
        private MonitorStateStore _store;
        private MonitorSettings _options;
        private HybridCache _cache;
        private ILogger<SqlCollectionService> _logger;
        private readonly LinkedList<WaitRateSample> _history = new();
        private readonly List<DeadlockEvent> _deadlockHistory = [];
        private readonly LinkedList<MonitorHistorySample> _performanceHistory = [];
        private Dictionary<string, WaitStatRow> _prevWaits = new();
        private DateTime _prevSampleUtc;
        private int _cycleCount = 0;
        private string _schemaName = string.Empty;
        private bool _hasLoadedInitial = false;
        private int _Last_X_Minutes_Initial = 5;

        #endregion

        #region Initialization

        public SqlCollectionService(MonitorStateStore store, IOptionsMonitor<MonitorSettings> options, ILogger<SqlCollectionService> logger, HybridCache cache)
        {
            _store = store;
            _options = options.CurrentValue;
            _logger = logger;
            _cache = cache;
        }

        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _schemaName = EnvironmentResolver.ResolveTablePrefix(_options.EnvironmentType);
            var builder = new SqlInfrastructureBuilder(_options);
            await builder.Scaffold();

            //await PollInitial(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollOnce(stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    var isPermissionError = IsPermissionError(ex);
                    var status = isPermissionError ? MonitorPollingStatus.InsufficientPermissions : MonitorPollingStatus.Error;
                    var errorMsg = isPermissionError ? "VIEW SERVER STATE required to query SQL Server DMVs." : ex.Message;
                    _logger.LogWarning(ex, "Monitor poll failed: {Error}", errorMsg);

                    _store.Replace(new MonitorSnapshot
                    {
                        Status = status,
                        LastError = errorMsg,
                        History = _history.ToList(),
                        RecentDeadlocks = [.. _deadlockHistory]
                    });

                    if (isPermissionError) await Task.Delay(30000, stoppingToken).ConfigureAwait(false);
                }

                var intervalSeconds = Math.Max(_options.PollIntervalSeconds, 1);
                try { await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken).ConfigureAwait(false); }
                catch (TaskCanceledException) { break; }
            }
        }

        private async Task PollOnce(CancellationToken ct)
        {
            try
            {
                DateTime now = DateTime.Now;
                var sw = Stopwatch.StartNew();

                var provider = DatabaseProvider.From(_options.ProviderName, _options.ConnectionString);
                _cycleCount++;
                bool runExpensiveQueries = _cycleCount % 7 == 0;
                bool runDeadlockQueries = _cycleCount % 50 == 0;
                bool updateInstanceTimestamp = _cycleCount % 50 == 0;

                // These four calls are independent of each other, so kick them all off before awaiting any of
                // them. Each gets its own provider instance so concurrent calls don't share a connection.
                var r1Task = provider
                    .GetDataset("EXEC " + string.Format(SqlScaffoldingQueries.ProcessProcedureHighName, _schemaName) + " @IsInitialLoad = 0", JEO3.Providers.CommandType.Text);

                Task<DataSet> r2Task = runExpensiveQueries
                    ? provider
                        .GetDataset("EXEC " + string.Format(SqlScaffoldingQueries.ProcessProcedureMediumName, _schemaName) + " @IsInitialLoad = 0", JEO3.Providers.CommandType.Text)
                    : null;

                Task<DataSet> r3Task = runDeadlockQueries
                    ? provider
                        .GetDataset("EXEC " + string.Format(SqlScaffoldingQueries.ProcessProcedureLowName, _schemaName) + " @IsInitialLoad = 0", JEO3.Providers.CommandType.Text)
                    : null;

                Task updateTask = updateInstanceTimestamp
                    ? provider
                        .ExecuteNonQuery(string.Format(SqlScaffoldingQueries.InsertUpdateMonitorInstanceQuery, _options.UserName, _options.MachineName, _options.ApiBaseUrl, _options.TimestampLastRegistered, _options.TimestampLastActive))
                    : Task.CompletedTask;

                var pending = new List<Task>(4) { r1Task, updateTask };
                if (r2Task != null) pending.Add(r2Task);
                if (r3Task != null) pending.Add(r3Task);
                await Task.WhenAll(pending).ConfigureAwait(false);

                _logger?.LogInformation("PollOnce: DB fetch phase completed in {ElapsedMs}ms", sw.Elapsed.TotalMilliseconds);

                DataSet r1 = await r1Task.ConfigureAwait(false);
                DataSet r2 = r2Task != null ? await r2Task.ConfigureAwait(false) : null;
                DataSet r3 = r3Task != null ? await r3Task.ConfigureAwait(false) : null;

                var sampledAtUtc = DateTime.UtcNow;
                var minTime = DateTime.UtcNow.AddHours(-8);
                var prevTime = _prevSampleUtc > minTime ? _prevSampleUtc : minTime;

                // These are all synchronous, in-memory conversions over data we already fetched above —
                // no I/O involved, so no benefit to wrapping them in Task/awaiting them.
                var rawWaits = r1.Tables[0].ToList<WaitStatRow>();
                var performanceMetrics = r1.Tables[1].ToList<PerformanceMetricRow>();

                var activeRequests = runExpensiveQueries
                    ? r2.Tables[0].ToList<ActiveRequestRow>()
                    : _store.Current.ActiveRequests.ToList();

                var blocking = runExpensiveQueries
                    ? r2.Tables[1].ToList<BlockingRow>()
                    : _store.Current.BlockingChain.ToList();

                var topQueries = runExpensiveQueries
                    ? r2.Tables[2].ToList<TopExpensiveQueryRow>()
                    : _store.Current.TopExpensiveQueries.ToList();

                var newDeadlocks = runDeadlockQueries
                    ? r3.Tables[0].ToList<DeadlockEvent>()
                    : _store.Current.RecentDeadlocks.ToList();

                // FIX: If the delta batch is empty, preserve the existing queries in the dashboard store
                if (runExpensiveQueries && topQueries.Count == 0)
                {
                    topQueries = _store.Current.TopExpensiveQueries.ToList();
                }

                // Safe in-place value normalization adjustments without rebuilding containers
                foreach (var item in rawWaits)
                {
                    item.WaitTimeMs = (long)Math.Round((double)item.WaitTimeMs, 0);
                    item.MaxWaitTimeMs = (long)Math.Round((double)item.MaxWaitTimeMs, 0);
                    item.SignalWaitTimeMs = (long)Math.Round((double)item.SignalWaitTimeMs, 0);
                }

                foreach (var item in performanceMetrics)
                {
                    item.IoStallWriteMs = (long)Math.Round((double)item.IoStallWriteMs, 0);
                    item.IoStallReadMs = (long)Math.Round((double)item.IoStallReadMs, 0);
                    item.TotalServerMemoryMb = (long)Math.Round((double)item.TotalServerMemoryMb, 0);
                    item.SqlMemoryUsageMb = (long)Math.Round((double)item.SqlMemoryUsageMb, 0);
                    item.TargetServerMemoryMb = (long)Math.Round((double)item.TargetServerMemoryMb, 0);
                    item.BatchRequestsPerSec = (long)Math.Round((double)item.BatchRequestsPerSec, 0);
                    item.CpuPercent = (long)Math.Round((double)item.CpuPercent, 0);
                    item.PageLifeExpectancy = (long)Math.Round((double)item.PageLifeExpectancy, 0);
                    item.MemoryUsagePercent = (long)Math.Round((double)item.MemoryUsagePercent, 0);
                }
                rawWaits = rawWaits.Where(v => v.Timestamp > prevTime).ToList();


                // 3. Process Wait Rates and Hardware Snapshot Collections
                var waitSample = ComputeRateSample(rawWaits);
                if (waitSample != null && waitSample.Timestamp != default)
                {
                    _history.AddLast(waitSample);
                }

                var performance = performanceMetrics.FirstOrDefault();
                if (performance != null)
                {
                    _performanceHistory.AddLast(new MonitorHistorySample
                    {
                        Timestamp = sampledAtUtc,
                        Performance = performance,
                        TopExpensiveQueries = topQueries
                    });
                }

                // 🌟 STAGE A: PRUNE THE MASTER ROOTS (Solves the GC Heap & LOH Leak)
                // Hard limits the in-memory master linked list boundaries back down to your 
                // configured appsettings parameters before any sorting allocations take place.
                while (_history.Count > _options.HistoryCapacity)
                {
                    _history.RemoveFirst();
                }
                while (_performanceHistory.Count > _options.HistoryCapacity)
                {
                    _performanceHistory.RemoveFirst();
                }

                // FIX: Only process deadlock history updates when the database procedure actually runs
                if (runDeadlockQueries && newDeadlocks.Count > 0)
                {
                    // HydrateFromXml returns a new instance rather than mutating in place, so the result
                    // has to be written back. Deadlock batches are small and infrequent (only every 10th
                    // cycle), so a plain loop avoids Parallel's thread-pool scheduling overhead for no gain.
                    for (int i = 0; i < newDeadlocks.Count; i++)
                    {
                        newDeadlocks[i] = HydrateFromXml(newDeadlocks[i]);
                    }

                    foreach (var dl in newDeadlocks.OrderBy(d => d.Timestamp))
                    {
                        if (!_deadlockHistory.Any(h => h.Timestamp == dl.Timestamp && h.VictimSessionId == dl.VictimSessionId))
                            _deadlockHistory.Add(dl);
                    }
                    if (_deadlockHistory.Count > _options.DeadlockCapacity)
                        _deadlockHistory.RemoveRange(0, _deadlockHistory.Count - _options.DeadlockCapacity);
                }
                var historyList = _history.OrderByDescending(v => v.Timestamp).Take(_history.Count > 10000 ? 10000 : _history.Count).ToList();
                var performanceHistoryList = _performanceHistory.OrderByDescending(v => v.Timestamp).Take(_performanceHistory.Count > 10000 ? 10000 : _performanceHistory.Count).ToList();
                // Construct Snapshot using optimized underlying cast definitions to avoid .ToList() allocation spikes
                var snapshot = new MonitorSnapshot
                {
                    ServerTimeUtc = DateTime.UtcNow,
                    Status = MonitorPollingStatus.Polling,
                    LastError = null,
                    History = historyList,
                    ActiveRequests = activeRequests,
                    BlockingChain = blocking,
                    RecentDeadlocks = [.. _deadlockHistory],
                    TopExpensiveQueries = topQueries,
                    PerformanceMetrics = performanceMetrics,
                    PerformanceHistory = performanceHistoryList
                };
                // 1. Commit the computed wait sample instantly to the persistent history table
                if (waitSample != null && waitSample.TopWaits.Count > 0)
                {
                    // Use your existing provider extension layer to run the batch engine asynchronously
                    await provider.UpsertRange<WaitRateSample>(new List<WaitRateSample> { waitSample });
                }

                // 2. Commit hydrated deadlock participants if a deadlock query executed
                if (runDeadlockQueries && newDeadlocks.Count > 0)
                {
                    var computedParticipants = new List<DeadlockParticipant>();

                    foreach (var dl in newDeadlocks)
                    {
                        if (dl.Participants != null)
                        {
                            foreach (var p in dl.Participants)
                            {
                                computedParticipants.Add(new DeadlockParticipant
                                {
                                    Timestamp = dl.Timestamp, // Inherited from parent event timestamp context
                                    SessionId = p.SessionId,
                                    LoginName = p.LoginName,
                                    HostName = p.HostName,
                                    ProgramName = p.ProgramName,
                                    LastStatement = p.LastStatement,
                                    IsVictim = p.IsVictim
                                });
                            }
                        }
                    }

                    if (computedParticipants.Count > 0)
                    {
                        await provider.UpsertRange<DeadlockParticipant>(computedParticipants);
                    }
                }
                _store.Replace(snapshot);


                // 🌟 EVICT OLD TIMELINES: Invalidate the cache key instantly so the next 
                // frontend endpoint hit receives the fresh background updates down the wire
                try
                {
                    // Resolve HybridCache context from your constructor DI container references
                    await _cache.RemoveAsync($"monitor:initial:{_schemaName}", ct);
                }
                catch { }

                var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
                Trace.WriteLine($"PollOnce - Elapsed {log}sec");
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
            }
        }

        public async Task PollInitial(CancellationToken ct)
        {
            try
            {
                DateTime now = DateTime.Now;
                var provider = DatabaseProvider.From(_options.ProviderName, _options.ConnectionString);
                var initialLoadParam = 1;

                // 1. Establish the clean 1-hour initial startup window boundary
                var initialWindowStartUtc = DateTime.UtcNow.AddMinutes(-_Last_X_Minutes_Initial);
                string minTimeParamStr = initialWindowStartUtc.ToString("yyyy-MM-dd HH:mm:ss.fff");

                // 🌟 FULL ASYNC PARALLEL EXECUTION: 
                // Kick off all 3 procedures simultaneously. Each call grabs its own connection string 
                // footprint, forcing SQL Server to process the tasks concurrently with zero contention.
                var r1Task = provider.GetDataset(
                    "EXEC " + string.Format(SqlScaffoldingQueries.ProcessProcedureHighName, _schemaName) +
                    " @IsInitialLoad = " + initialLoadParam + ", @MinTimestamp = '" + minTimeParamStr + "'",
                    JEO3.Providers.CommandType.Text);

                var r2Task = provider.GetDataset(
                    "EXEC " + string.Format(SqlScaffoldingQueries.ProcessProcedureMediumName, _schemaName) +
                    " @IsInitialLoad = " + initialLoadParam + ", @MinTimestamp = '" + minTimeParamStr + "'",
                    JEO3.Providers.CommandType.Text);

                var r3Task = provider.GetDataset(
                    "EXEC " + string.Format(SqlScaffoldingQueries.ProcessProcedureLowName, _schemaName) +
                    " @IsInitialLoad = " + initialLoadParam + ", @MinTimestamp = '" + minTimeParamStr + "'",
                    JEO3.Providers.CommandType.Text);

                // Await all tasks concurrently on the background thread pool
                await Task.WhenAll(r1Task, r2Task, r3Task).ConfigureAwait(false);

                DataSet r1 = await r1Task.ConfigureAwait(false);
                DataSet r2 = await r2Task.ConfigureAwait(false);
                DataSet r3 = await r3Task.ConfigureAwait(false);

                // 2. Map structural database results to internal memory models (Verbatim Aliasing)
                var rawWaits = r1.Tables[0].ToList<WaitStatRow>();
                var performanceMetrics = r1.Tables[1].ToList<PerformanceMetricRow>();
                var activeRequests = r2.Tables[0].ToList<ActiveRequestRow>();
                var blocking = r2.Tables[1].ToList<BlockingRow>();
                var topQueries = r2.Tables[2].ToList<TopExpensiveQueryRow>();
                var newDeadlocks = r3.Tables[0].ToList<DeadlockEvent>();

                // Safe in-place numeric rounding adjustments
                foreach (var item in rawWaits)
                {
                    item.WaitTimeMs = (long)Math.Round((double)item.WaitTimeMs, 0);
                    item.MaxWaitTimeMs = (long)Math.Round((double)item.MaxWaitTimeMs, 0);
                    item.SignalWaitTimeMs = (long)Math.Round((double)item.SignalWaitTimeMs, 0);
                }

                foreach (var item in performanceMetrics)
                {
                    item.IoStallWriteMs = (long)Math.Round((double)item.IoStallWriteMs, 0);
                    item.IoStallReadMs = (long)Math.Round((double)item.IoStallReadMs, 0);
                    item.TotalServerMemoryMb = (long)Math.Round((double)item.TotalServerMemoryMb, 0);
                    item.SqlMemoryUsageMb = (long)Math.Round((double)item.SqlMemoryUsageMb, 0);
                    item.TargetServerMemoryMb = (long)Math.Round((double)item.TargetServerMemoryMb, 0);
                    item.BatchRequestsPerSec = (long)Math.Round((double)item.BatchRequestsPerSec, 0);
                    item.CpuPercent = (long)Math.Round((double)item.CpuPercent, 0);
                    item.PageLifeExpectancy = (long)Math.Round((double)item.PageLifeExpectancy, 0);
                    item.MemoryUsagePercent = (long)Math.Round((double)item.MemoryUsagePercent, 0);
                }

                // Clip incoming boundaries to ensure absolute synchronization matches the initial start limit
                rawWaits = rawWaits.Where(v => v.Timestamp >= initialWindowStartUtc).ToList();
                performanceMetrics = performanceMetrics.Where(p => p.Timestamp >= initialWindowStartUtc).ToList();

                // Flush active collection buffers before reconstruct sequence runs
                _history.Clear();
                _performanceHistory.Clear();
                _prevWaits.Clear();

                // 🌟 3. CHRONOLOGICAL TIMELINE RECONSTRUCTION LOOPS
                // Group raw metrics into distinct slices sorted sequentially from oldest to newest
                var sortedWaitIntervals = rawWaits
                    .GroupBy(w => w.Timestamp)
                    .OrderBy(g => g.Key)
                    .ToList();

                DateTime? loopPrevSampleUtc = null;

                foreach (var interval in sortedWaitIntervals)
                {
                    var currentTimestamp = interval.Key;

                    // Handle step-by-step timeline delta calculation math mechanics
                    if (loopPrevSampleUtc.HasValue)
                    {
                        double elapsed = Math.Max((currentTimestamp - loopPrevSampleUtc.Value).TotalSeconds, 0.001);
                        var rates = new List<WaitTypeRate>();
                        double totalWaitMs = 0;
                        double totalTasks = 0;

                        foreach (var c in interval)
                        {
                            // Check our persistent historical dictionary for matching metrics
                            if (_prevWaits.TryGetValue(c.WaitType, out var prev))
                            {
                                // True baseline relative delta calculations
                                var dWait = Math.Max(c.WaitTimeMs - prev.WaitTimeMs, 0);
                                var dTasks = Math.Max(c.WaitingTasksCount - prev.WaitingTasksCount, 0);
                                var dSignal = Math.Max(c.SignalWaitTimeMs - prev.SignalWaitTimeMs, 0);

                                var waitPerSec = dWait / elapsed;
                                var tasksPerSec = dTasks / elapsed;
                                var signalPerSec = dSignal / elapsed;

                                if (waitPerSec > 0 || tasksPerSec > 0)
                                {
                                    rates.Add(new WaitTypeRate
                                    {
                                        WaitType = c.WaitType,
                                        WaitMsPerSec = Math.Round(waitPerSec, 1),
                                        WaitingTasksPerSec = Math.Round(tasksPerSec, 1),
                                        SignalWaitMsPerSec = Math.Round(signalPerSec, 1)
                                    });
                                }

                                totalWaitMs += waitPerSec;
                                totalTasks += tasksPerSec;
                            }
                        }

                        // 🌟 HISTORICAL CREATESAMPLE: Append computed rate objects sequentially into 
                        // your historical trend cache to feed your main line graph timeline immediately.
                        if (rates.Count > 0)
                        {
                            _history.AddLast(new WaitRateSample
                            {
                                Timestamp = currentTimestamp,
                                TotalWaitMsPerSec = Math.Round(totalWaitMs, 1),
                                TotalWaitingTasksPerSec = Math.Round(totalTasks, 1),
                                ElapsedSeconds = Math.Round(elapsed, 2),
                                TopWaits = rates.OrderByDescending(r => r.WaitMsPerSec).Take(10).ToList()
                            });
                        }
                    }

                    // Keep a rolling tracking dictionary alive across all loop iterations.
                    foreach (var r in interval)
                    {
                        _prevWaits[r.WaitType] = r;
                    }
                    loopPrevSampleUtc = currentTimestamp;
                }

                // 🌟 4. SEQUENTIAL HEALTH METRICS INGESTION
                var sortedPerfMetrics = performanceMetrics.OrderBy(p => p.Timestamp).ToList();
                foreach (var perf in sortedPerfMetrics)
                {
                    _performanceHistory.AddLast(new MonitorHistorySample
                    {
                        Timestamp = perf.Timestamp,
                        Performance = perf,
                        // Top Expensive queries attach cleanly to their respective timeline anchor points
                        TopExpensiveQueries = topQueries
                    });
                }

                // Apply capacity thresholds to avoid memory leaks
                while (_history.Count > _options.HistoryCapacity) _history.RemoveFirst();
                while (_performanceHistory.Count > _options.HistoryCapacity) _performanceHistory.RemoveFirst();

                // 5. Hydrate Deadlock Events
                for (int i = 0; i < newDeadlocks.Count; i++)
                {
                    newDeadlocks[i] = HydrateFromXml(newDeadlocks[i]);
                }

                foreach (var dl in newDeadlocks.OrderBy(d => d.Timestamp))
                {
                    if (!_deadlockHistory.Any(h => h.Timestamp == dl.Timestamp && h.VictimSessionId == dl.VictimSessionId))
                        _deadlockHistory.Add(dl);
                }
                if (_deadlockHistory.Count > _options.DeadlockCapacity)
                    _deadlockHistory.RemoveRange(0, _deadlockHistory.Count - _options.DeadlockCapacity);

                // Hand over the final chronological state cursor to subsequent PollOnce delta tracking cycles
                _prevSampleUtc = loopPrevSampleUtc ?? DateTime.UtcNow;

                // 6. Push the finalized historical timeline to the frontend store
                var snapshot = new MonitorSnapshot
                {
                    ServerTimeUtc = DateTime.UtcNow,
                    Status = MonitorPollingStatus.Polling,
                    LastError = null,
                    History = _history.ToList(),
                    ActiveRequests = activeRequests,
                    BlockingChain = blocking,
                    RecentDeadlocks = [.. _deadlockHistory],
                    TopExpensiveQueries = topQueries,
                    PerformanceMetrics = performanceMetrics,
                    PerformanceHistory = _performanceHistory.ToList()
                };


                _store.Replace(snapshot);
                _hasLoadedInitial = true;


                var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
                Trace.WriteLine($"PollOnce - Elapsed {log}sec");
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
            }
        }
        private WaitRateSample ComputeRateSample(IReadOnlyList<WaitStatRow> raw)
        {
            DateTime now = DateTime.Now;
            var lastRaw = raw.GroupBy(v => v.Timestamp).OrderByDescending(v => v.Key).FirstOrDefault();
            if (lastRaw == null) { return new(); }

            // 🌟 THE CORRECTION: Extract the actual database row recording time
            var currentRecordTime = lastRaw.Key;

            var isFirstSample = _prevSampleUtc == default;

            // 🌟 Calculate the true time delta between database snapshot points
            var elapsed = isFirstSample ? 0d : Math.Max((currentRecordTime - _prevSampleUtc).TotalSeconds, 0.001);

            var rates = new List<WaitTypeRate>();
            double totalWaitMs = 0, totalTasks = 0;

            if (!isFirstSample && elapsed > 0)
            {
                foreach (var c in lastRaw)
                {
                    _prevWaits.TryGetValue(c.WaitType, out var prev);

                    var dWait = Math.Max(c.WaitTimeMs - (prev?.WaitTimeMs ?? 0), 0);
                    var dTasks = Math.Max(c.WaitingTasksCount - (prev?.WaitingTasksCount ?? 0), 0);
                    var dSignal = Math.Max(c.SignalWaitTimeMs - (prev?.SignalWaitTimeMs ?? 0), 0);

                    var waitPerSec = dWait / elapsed;
                    if (waitPerSec > 0 || dTasks > 0)
                    {
                        rates.Add(new WaitTypeRate
                        {
                            WaitType = c.WaitType,
                            WaitMsPerSec = Math.Round(waitPerSec, 1),
                            WaitingTasksPerSec = Math.Round(dTasks / elapsed, 1),
                            SignalWaitMsPerSec = Math.Round(dSignal / elapsed, 1)
                        });
                    }
                    totalWaitMs += waitPerSec;
                    totalTasks += dTasks / elapsed;
                }
            }

            _prevWaits = lastRaw.ToDictionary(r => r.WaitType, r => r);

            // 🌟 Sync the cursor tracker using the real record time context
            _prevSampleUtc = currentRecordTime;

            var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
            Trace.WriteLine($"ComputeRateSample - Elapsed {log}sec");

            return new WaitRateSample
            {
                Timestamp = currentRecordTime,
                TotalWaitMsPerSec = Math.Round(totalWaitMs, 1),
                TotalWaitingTasksPerSec = Math.Round(totalTasks, 1),
                ElapsedSeconds = Math.Round(elapsed, 2),
                TopWaits = rates.OrderByDescending(r => r.WaitMsPerSec).Take(10).ToList(),
            };
        }
        private static bool IsPermissionError(Exception ex)
        {
            var msg = ex.Message;
            return msg.Contains("VIEW SERVER STATE", StringComparison.OrdinalIgnoreCase) || msg.Contains("permission", StringComparison.OrdinalIgnoreCase) || msg.Contains("229", StringComparison.Ordinal) || msg.Contains("297", StringComparison.Ordinal);
        }
        private static DeadlockEvent HydrateFromXml(DeadlockEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.RawXml)) return evt;

            var doc = System.Xml.Linq.XElement.Parse(evt.RawXml);
            var deadlock = doc.Descendants("deadlock").FirstOrDefault();
            if (deadlock is null) return evt;

            var victimId = deadlock.Element("victim-list")?
                .Element("victimProcess")?.Attribute("id")?.Value ?? "";

            var participants = deadlock.Element("process-list")?
                .Elements("process")
                .Select(p => new DeadlockParticipant
                {
                    SessionId = int.TryParse(p.Attribute("id")?.Value, out var id) ? id : -1,
                    LoginName = p.Attribute("loginname")?.Value ?? "",
                    HostName = p.Attribute("hostname")?.Value ?? "",
                    ProgramName = p.Attribute("clientapp")?.Value ?? "",
                    LastStatement = p.Element("inputbuf")?.Value?.Trim() ?? "",
                    IsVictim = p.Attribute("id")?.Value == victimId,
                })
                .ToList() ?? [];

            return new DeadlockEvent
            {
                Timestamp = evt.Timestamp,
                VictimSessionId = victimId,
                RawXml = evt.RawXml,
                Participants = participants,
            };
        }
    }
}