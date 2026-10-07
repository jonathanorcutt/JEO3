using System.Configuration;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using JEO3.Logging;
using JEO3.Monitor;
using JEO3.Providers.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace JEO3.API.Endpoints
{
    internal static class RegistrationExtensions
    {
        internal static void MapMonitorEndpoints(this WebApplication app, HybridCacheEntryOptions initialOptions)
        {
            app.MapGet("/api/monitor/initial", async (MonitorStateStore store,
                [FromServices] SqlCollectionService svc,
                [FromServices] HybridCache cache, // 🌟 Inject the hybrid cache provider
                HttpContext context) =>
            {
                // 1. Trigger the heavy SQL stored procedure background tasks
                // Passing the HTTP lifecycle request token ensures thread safety if the user closes the window mid-transit
                string cacheKey = $"monitor:initial:{EntityDescriptor.DefaultSchemaOverride}";

                var logger = context.RequestServices.GetService(typeof(ILogger<SqlCollectionService>)) as ILogger<SqlCollectionService>;

                var outerSw = Stopwatch.StartNew();
                logger?.LogInformation("/api/monitor/initial request starting");

                var responsePayload = await cache.GetOrCreateAsync(
                   cacheKey,
                   async token =>
                   {
                       var innerSw = Stopwatch.StartNew();
                       logger?.LogInformation("cache factory for /api/monitor/initial started");

                       await svc.PollInitial(context.RequestAborted);
                       logger?.LogInformation("svc.PollInitial completed in {ElapsedMs}ms", innerSw.Elapsed.TotalMilliseconds);

                       var initial = store.Current;

                       return new MonitorDeltaResponse()
                       {
                           ServerTimeUtc = DateTime.UtcNow,
                           Status = initial.Status,
                           LastError = initial.LastError,

                           // Ascending chronological order so charts render left-to-right immediately
                           History = initial.History.OrderBy(x => x.Timestamp).ToList(),
                           PerformanceHistory = initial.PerformanceHistory.OrderBy(x => x.Timestamp).ToList(),
                           RecentDeadlocks = initial.RecentDeadlocks.OrderBy(x => x.Timestamp).ToList(),

                           // Transient active states
                           ActiveRequests = initial.ActiveRequests,
                           BlockingChain = initial.BlockingChain,
                           TopExpensiveQueries = initial.TopExpensiveQueries
                       };
                   },
                   initialOptions);

                logger?.LogInformation("/api/monitor/initial completed in {ElapsedMs}ms", outerSw.Elapsed.TotalMilliseconds);

                return Results.Ok(responsePayload);
            });
            // Incremental delta polling
            app.MapGet("/api/monitor/delta", (DateTime? sinceUtc, MonitorStateStore store) =>
            {
                var current = store.Current;
                var cutoff = sinceUtc ?? DateTime.UtcNow.AddMinutes(-1);

                return Results.Ok(new MonitorDeltaResponse()
                {
                    ServerTimeUtc = DateTime.UtcNow,
                    Status = current.Status,
                    LastError = current.LastError,

                    // Return only samples generated after cutoff (ordered ascending for appending)
                    History = current.History
                        .Where(h => h.Timestamp > cutoff)
                        .OrderBy(h => h.Timestamp).ToList(),

                    PerformanceHistory = current.PerformanceHistory
                        .Where(p => p.Timestamp > cutoff)
                        .OrderBy(p => p.Timestamp).ToList(),

                    RecentDeadlocks = current.RecentDeadlocks
                        .Where(d => d.Timestamp > cutoff)
                        .OrderBy(d => d.Timestamp).ToList(),

                    // Transient state always reflects current state
                    ActiveRequests = current.ActiveRequests,
                    BlockingChain = current.BlockingChain,
                    TopExpensiveQueries = current.TopExpensiveQueries
                });
            });
            app.MapGet("/api/monitor/snapshot", (MonitorStateStore store) =>
            {
                var s = store.Current;

                return Results.Ok(new
                {
                    Status = s.Status.ToString(),
                    LastError = s.LastError,

                    History = s.History.Select(h => new
                    {
                        Timestamp = h.Timestamp,
                        TotalWaitMsPerSec = h.TotalWaitMsPerSec,
                        TotalWaitingTasksPerSec = h.TotalWaitingTasksPerSec,
                        ElapsedSeconds = h.ElapsedSeconds,

                        TopWaits = h.TopWaits.Select(w => new
                        {
                            Timestamp = w.Timestamp,
                            WaitType = w.WaitType,
                            WaitMsPerSec = w.WaitMsPerSec,
                            WaitingTasksPerSec = w.WaitingTasksPerSec,
                            SignalWaitMsPerSec = w.SignalWaitMsPerSec
                        })
                    }),

                    ActiveRequests = s.ActiveRequests.Select(r => new
                    {
                        Timestamp = r.Timestamp,
                        SessionId = r.SessionId,
                        BlockingSessionId = r.BlockingSessionId,
                        WaitType = r.WaitType,
                        WaitTimeSec = r.WaitTimeSec,
                        Status = r.Status,
                        DatabaseName = r.DatabaseName,
                        LoginName = r.LoginName,
                        CpuTimeMs = r.CpuTimeMs,
                        LogicalReads = r.LogicalReads
                    }),

                    BlockingChain = s.BlockingChain.Select(b => new
                    {
                        Timestamp = b.Timestamp,
                        SessionId = b.SessionId,
                        BlockingSessionId = b.BlockingSessionId,
                        IsBlocked = b.IsBlocked,
                        LoginName = b.LoginName,
                        WaitType = b.WaitType,
                        WaitTimeSec = b.WaitTimeSec,
                        OpenTranDurationSec = b.OpenTranDurationSec
                    }),

                    RecentDeadlocks = s.RecentDeadlocks.Select(d => new
                    {
                        Timestamp = d.Timestamp,
                        VictimSessionId = d.VictimSessionId,

                        Participants = d.Participants.Select(p => new
                        {
                            SessionId = p.SessionId,
                            LoginName = p.LoginName,
                            IsVictim = p.IsVictim
                        })
                    }),

                    TopExpensiveQueries = s.TopExpensiveQueries.Select(q => new
                    {
                        Timestamp = q.Timestamp,
                        SqlHandleHex = q.SqlHandleHex,
                        PlanHandleHex = q.PlanHandleHex,
                        DatabaseName = q.DatabaseName,
                        QueryText = q.QueryText,
                        ExecutionCount = q.ExecutionCount,
                        TotalElapsedMs = q.TotalElapsedMs,
                        AvgElapsedMs = q.AvgElapsedMs,
                        MaxElapsedMs = q.MaxElapsedMs,
                        TotalCpuMs = q.TotalCpuMs,
                        AvgCpuMs = q.AvgCpuMs,
                        MaxCpuMs = q.MaxCpuMs,
                        TotalLogicalReads = q.TotalLogicalReads,
                        AvgLogicalReads = q.AvgLogicalReads,
                        MaxLogicalReads = q.MaxLogicalReads,
                        TotalLogicalWrites = q.TotalLogicalWrites,
                        AvgLogicalWrites = q.AvgLogicalWrites,
                        MaxLogicalWrites = q.MaxLogicalWrites,
                        TotalIo = q.TotalIo,
                        AvgIo = q.AvgIo,
                        MaxIo = q.MaxIo,
                        QueryImpact = q.QueryImpact,
                        LastExecutionTime = q.LastExecutionTime
                    }),

                    PerformanceMetrics = s.PerformanceMetrics.Select(q => new
                    {
                        MemoryUsagePercent = q.MemoryUsagePercent,
                        CpuPercent = q.CpuPercent,
                        UserConnections = q.UserConnections,
                        PageLifeExpectancy = q.PageLifeExpectancy,
                        BatchRequestsPerSec = q.BatchRequestsPerSec,
                        Timestamp = q.Timestamp,
                        IoStallReadMs = q.IoStallReadMs,
                        IoStallWriteMs = q.IoStallWriteMs,
                        SqlMemoryUsageMb = q.SqlMemoryUsageMb,
                        TargetServerMemoryMb = q.TargetServerMemoryMb,
                        TotalServerMemoryMb = q.TotalServerMemoryMb
                    }),

                    PerformanceHistory = s.PerformanceHistory.Select(q => new
                    {
                        Performance = q.Performance,
                        Timestamp = q.Timestamp,
                        TopExpensiveQueries = q.TopExpensiveQueries
                    })
                });
            });
        }
        internal static List<ConnectionStringSettings> PopulateConnectionStrings(this WebApplicationBuilder builder)
        {
            // ── DYNAMICALLY EXTRACT STRUCTURED JSON CONNECTION STRINGS ───────────
            var connectionSettingsList = new List<ConnectionStringSettings>();
            var connStringsSection = builder.Configuration.GetSection("ConnectionStrings");

            if (connStringsSection.Exists())
            {
                foreach (var child in connStringsSection.GetChildren())
                {
                    // Fall back to standard primitive string if it isn't a complex object layout
                    string connectionString = child["ConnectionString"] ?? child.Value ?? string.Empty;
                    string providerName = child["ProviderName"] ?? string.Empty;

                    connectionSettingsList.Add(new ConnectionStringSettings(child.Key, connectionString, providerName));
                }
            }
            return connectionSettingsList;
        }
        internal static void RegisterExceptionHandling(this WebApplication app, string connString)
        {
            ExceptionUtility.Register(connString);
            AppDomain.CurrentDomain.FirstChanceException += new EventHandler<FirstChanceExceptionEventArgs>((o, e) =>
            {
                ExceptionUtility.LogException(e.Exception);
            });
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((o, e) =>
            {
                ExceptionUtility.LogException((Exception)e.ExceptionObject);
            });
        }
    }
}
