//namespace JEO3.Engine
//{
//    internal static partial class MSSQLQueries
//    {
//        // ---------------------------------------------------------------------------
//        // Wait-stats snapshot query (MSSQL on-prem / standard edition / Azure SQL MI)
//        //
//        // Design: two separate ADO.NET calls are made by SqlServerMonitorService —
//        // one for Sample A, one for Sample B — separated by the service's PeriodicTimer
//        // interval. The SQL itself is stateless and fast (no WAITFOR inside).
//        //
//        // The service computes:
//        //   WaitTimeMsPerSec = (sampleB.WaitTimeMs - sampleA.WaitTimeMs)
//        //                      / elapsedSeconds
//        // where elapsedSeconds = (sampleB.CapturedAtUtc - sampleA.CapturedAtUtc).TotalSeconds
//        //
//        // Benign/internal SQL Server wait types are excluded by the WHERE clause so
//        // that the primary rate metric reflects real workload pressure only.
//        // The full unfiltered set is still readable from the detail row data.
//        // ---------------------------------------------------------------------------
//        internal const string WaitStatsSnapshotQuery = @"
//SELECT
//    wait_type                   AS WaitType,
//    waiting_tasks_count         AS WaitingTasksCount,
//    wait_time_ms                AS WaitTimeMs,
//    max_wait_time_ms            AS MaxWaitTimeMs,
//    signal_wait_time_ms         AS SignalWaitTimeMs,
//    SYSUTCDATETIME()            AS CapturedAtUtc
//FROM sys.dm_os_wait_stats
//WHERE wait_type NOT IN (
//    -- Benign SQL Server internal / background waits
//    -- (filtered from primary rate metric per user preference)
//    'SLEEP_TASK',
//    'SLEEP_SYSTEMTASK',
//    'SLEEP_DBSTARTUP',
//    'SLEEP_DBTASK',
//    'SLEEP_TEMPDBSTARTUP',
//    'SLEEP_MASTERDBREADY',
//    'SLEEP_MASTERMDREADY',
//    'SLEEP_MASTERUPGRADED',
//    'SLEEP_MSDBSTARTUP',
//    'SLEEP_BUFFERPOOL_FREEPAGES',
//    'BROKER_TO_FLUSH',
//    'BROKER_TASK_STOP',
//    'BROKER_EVENTHANDLER',
//    'BROKER_TRANSMITTER',
//    'CHECKPOINT_QUEUE',
//    'DBMIRROR_EVENTS_QUEUE',
//    'DISPATCHER_QUEUE_SEMAPHORE',
//    'CLR_AUTO_EVENT',
//    'CLR_MANUAL_EVENT',
//    'CLR_SEMAPHORE',
//    'FT_IFTS_SCHEDULER_IDLE_WAIT',
//    'HADR_FILESTREAM_IOMGR_IOCOMPLETION',
//    'HADR_WORK_QUEUE',
//    'LAZYWRITER_SLEEP',
//    'LOGMGR_QUEUE',
//    'ONDEMAND_TASK_QUEUE',
//    'REQUEST_FOR_DEADLOCK_SEARCH',
//    'RESOURCE_QUEUE',
//    'SERVER_IDLE_CHECK',
//    'SNI_HTTP_ACCEPT',
//    'SP_SERVER_DIAGNOSTICS_SLEEP',
//    'SQLTRACE_BUFFER_FLUSH',
//    'SQLTRACE_INCREMENTAL_FLUSH_SLEEP',
//    'WAITFOR',
//    'XE_DISPATCHER_WAIT',
//    'XE_TIMER_EVENT',
//    'XE_LIVE_TARGET_TVF',
//    'NUMA_STATIC_PRIMARY_ALLOC_QUEUE',
//    'DIRTY_PAGE_POLL',
//    'QDS_ASYNC_QUEUE',
//    'QDS_CLEANUP_STALE_QUERIES_TASK_MAIN_LOOP_SLEEP'
//)
//ORDER BY wait_time_ms DESC;";

//        // Same query WITHOUT the exclusion list — used for the raw detail panel
//        // so users can see all waits including benign ones.
//        internal const string WaitStatsSnapshotQueryUnfiltered = @"
//SELECT
//    wait_type                   AS WaitType,
//    waiting_tasks_count         AS WaitingTasksCount,
//    wait_time_ms                AS WaitTimeMs,
//    max_wait_time_ms            AS MaxWaitTimeMs,
//    signal_wait_time_ms         AS SignalWaitTimeMs,
//    SYSUTCDATETIME()            AS CapturedAtUtc
//FROM sys.dm_os_wait_stats
//ORDER BY wait_time_ms DESC;";
//    }
//}
