//using JEO3.Schema;

//namespace JEO3.Monitor
//{
//    internal static partial class MSSQLQueries
//    {
//        // TODO
//        internal static string WaitTimeQuery => Environment.MachineName == "MSI" ? WaitTimeDMV_ViewServerStateQuery : WaitTimeQuery_AZURE;
//        internal const string WaitTimeQuery_AZURE_BLOCK = @"
//SELECT wait_type, wait_time_ms, signal_wait_time_ms, waiting_tasks_count
//INTO #w1
//FROM sys.dm_db_wait_stats;
//WAITFOR DELAY '00:00:10';  -- poll interval
//-- Sample B + deltas
//SELECT
//    w2.wait_type,
//    (w2.wait_time_ms - w1.wait_time_ms) / 1.0 AS wait_ms_in_interval,
//    (w2.signal_wait_time_ms - w1.signal_wait_time_ms) / 1.0 AS signal_ms_in_interval,
//    (w2.wait_time_ms - w1.wait_time_ms - (w2.signal_wait_time_ms - w1.signal_wait_time_ms))
//        / 1.0 AS resource_wait_ms_in_interval,
//    w2.waiting_tasks_count - w1.waiting_tasks_count AS waits_in_interval,
//    ((w2.wait_time_ms - w1.wait_time_ms) / 1.0) / 10.0 AS wait_ms_per_sec  -- divide by interval length
//FROM sys.dm_db_wait_stats AS w2
//INNER JOIN #w1 AS w1 ON w1.wait_type = w2.wait_type
//WHERE (w2.wait_time_ms - w1.wait_time_ms) > 0
//ORDER BY wait_ms_in_interval DESC;";

//        internal const string WaitTimeQuery_AZURE = @"
//-- Query Store version: Safe for all Azure SQL Database and Managed Instance tiers
//DECLARE @CurrentIntervalId BIGINT;

//-- 1. Grab the ID of the active/most recent interval slot being tracked right now
//SELECT TOP 1 @CurrentIntervalId = runtime_stats_interval_id
//FROM sys.query_store_runtime_stats_interval
//ORDER BY start_time DESC;

//-- 2. Group resource waits by category within this interval window to compute the graph points
//SELECT TOP 20
//    -- Total category wait duration divided by total interval length to find Avg Ms/Sec
//    w.avg_query_wait_time_ms AS " + nameof(IWaitMetric.WaitTimeMsPerSec) + @",
//    w.total_query_wait_time_ms AS " + nameof(IWaitMetric.TotalWaitTimeMsPerSec) + @"TotalWaitTimeMsPerSec,
//    --w.min_query_wait_time_ms AS " + nameof(IWaitMetric.MinWaitTimeMsPerSec) + @"MinWaitTimeMsPerSec,
//    --w.max_query_wait_time_ms AS " + nameof(IWaitMetric.MaxWaitTimeMsPerSec) + @"MaxWaitTimeMsPerSec,
//    --w.last_query_wait_time_ms AS " + nameof(IWaitMetric.LastWaitTimeMsPerSec) + @"LastWaitTimeMsPerSec,        
//    CAST(DATEDIFF(SECOND, i.start_time, i.end_time) AS FLOAT) AS " + nameof(IWaitMetric.ActualDurationSec) + @"ActualDurationSec,
//    w.wait_category_desc AS " + nameof(IWaitMetric.WaitType) + @"WaitType, -- Grouped category text (e.g., LOCK, BUFFER_IO)

//    -- Execution aggregates mapped to the C# properties
//    0 AS Spid, -- Query Store is asynchronous history, active transaction SPIDs do not map here
//    DB_NAME() AS " + nameof(IWaitMetric.DatabaseName) + @",
//    '' AS " + nameof(IWaitMetric.LoginName) + @",
//    '' AS " + nameof(IWaitMetric.ApplicationName) + @",
//    '' AS " + nameof(IWaitMetric.ClientWorkstation) + @",
//    0 AS " + nameof(IWaitMetric.BlockedBySpid) + @",
//    'HistoricalSnapshot' AS " + nameof(IWaitMetric.RequestStatus) + @",

//    -- Resource consumption aggregates over the timeframe
//    SUM(r.avg_cpu_time) AS " + nameof(IWaitMetric.CpuTimeMs) + @",
//    SUM(r.avg_logical_io_reads) AS " + nameof(IWaitMetric.LogicalReads) + @",
//    SUM(r.avg_physical_io_reads) AS " + nameof(IWaitMetric.PhysicalReads) + @",
//    0 AS " + nameof(IWaitMetric.PhysicalWrites) + @",
//    0 AS " + nameof(IWaitMetric.OpenTxnCount) + @",

//    -- The text of the top driving query in that wait category bucket
//    --MAX(q.query_sql_text) 
//'' AS " + nameof(IWaitMetric.ActiveStatementText) + @",
//    --MAX(q.query_sql_text) 
//'' AS " + nameof(IWaitMetric.FullBatchText) + @" 

//FROM sys.query_store_wait_stats w
//JOIN sys.query_store_runtime_stats_interval i ON w.runtime_stats_interval_id = i.runtime_stats_interval_id
//JOIN sys.query_store_plan p ON w.plan_id = p.plan_id
//JOIN sys.query_store_query q ON p.query_id = q.query_id
//LEFT JOIN sys.query_store_runtime_stats r ON w.plan_id = r.plan_id AND w.runtime_stats_interval_id = r.runtime_stats_interval_id
//WHERE w.runtime_stats_interval_id = @CurrentIntervalId
//GROUP BY w.wait_category_desc, i.start_time, i.end_time, w.avg_query_wait_time_ms, w.total_query_wait_time_ms
//ORDER BY WaitTimeMsPerSec DESC;";

//        internal const string WaitTimeDMV_ViewServerStateQuery = @"
//WITH TopWaits AS (
//    SELECT TOP 20
//        CAST(ISNULL(SUM(p.waittime), 0) % 10000 AS BIGINT)  AS WaitTimeMsPerSec,
//        RTRIM(p.lastwaittype) AS WaitType,
//        p.spid AS Spid,
//        ISNULL(DB_NAME(p.dbid), '') AS DatabaseName,
//        ISNULL(p.loginame, '') AS LoginName,
//        ISNULL(p.program_name, '') AS ApplicationName,
//        ISNULL(p.hostname, '') AS ClientWorkstation,
//        ISNULL(p.blocked, 0) AS BlockedBySpid,
//        ISNULL(p.status, '') AS RequestStatus,
//        ISNULL(p.cpu, 0) AS CpuTimeMs,
//        --ISNULL(p.logical_reads, 0) AS LogicalReads,
//        ISNULL(p.physical_io, 0) AS PhysicalReads,
//        0 AS PhysicalWrites,
//        ISNULL(p.open_tran, 0) AS OpenTxnCount
//        --ISNULL(st.text, '') AS ActiveStatementText,
//        --ISNULL(st.text, '') AS FullBatchText
//    FROM sys.sysprocesses p
//    --WHERE p.dbid = DB_ID()
//     GROUP BY
//        RTRIM(p.lastwaittype),
//        p.spid,
//        ISNULL(DB_NAME(p.dbid), ''),
//        ISNULL(p.loginame, '') ,
//        ISNULL(p.program_name, '') ,
//        ISNULL(p.hostname, '') ,
//        ISNULL(p.blocked, 0),
//        ISNULL(p.status, '') ,
//        ISNULL(p.cpu, 0),
//        ISNULL(p.physical_io, 0) ,
//        ISNULL(p.open_tran, 0)
//    ) 

//   SELECT * FROM TopWaits";

//        internal const string WaitTimeDMV_ViewServerStateQuery_Blocking = @"
//-- 1. Create a temporary table to store the initial snapshot and start time
//IF OBJECT_ID('tempdb..#InitialSnapshot') IS NOT NULL 
//    DROP TABLE #InitialSnapshot;

//CREATE TABLE #InitialSnapshot (
//    WaitType NVARCHAR(60),
//    WaitTimeMs BIGINT,   -- The PascalCase column definition
//    StartTime DATETIME2
//);

//INSERT INTO #InitialSnapshot
//SELECT 
//    wait_type, 
//    wait_time_ms,        -- Pull raw lowercase from DMV
//    SYSDATETIME()
//FROM sys.dm_os_wait_stats;

//-- 2. Wait for the sample interval
//WAITFOR DELAY '00:00:15'; 

//-- 3. Capture end time
//DECLARE @EndTime DATETIME2 = SYSDATETIME();

//-- 4. Calculate dynamic deltas and fetch execution metadata using PascalCase aliases
//SELECT TOP 20
//    -- FIXED: now.wait_time_ms (DMV) minus prior.WaitTimeMs (PascalCase Snapshot Column)
//    (now.wait_time_ms - prior.WaitTimeMs) / 
//        (DATEDIFF_BIG(MILLISECOND, prior.StartTime, @EndTime) / 1000.0) AS WaitTimeMsPerSec,

//    DATEDIFF_BIG(MILLISECOND, prior.StartTime, @EndTime) / 1000.0 AS ActualDurationSec,
//    now.wait_type AS WaitType,

//    -- Active Execution & Connection Metadata
//    ISNULL(r.session_id, 0) AS Spid,
//    ISNULL(DB_NAME(r.database_id), '') AS DatabaseName,
//    ISNULL(s.login_name, '') AS LoginName,
//    ISNULL(s.program_name, '') AS ApplicationName,
//    ISNULL(s.host_name, '') AS ClientWorkstation,
//    ISNULL(r.blocking_session_id, 0) AS BlockedBySpid,
//    ISNULL(r.status, '') AS RequestStatus,

//    -- Resource Consumption Metrics
//    ISNULL(r.cpu_time, 0) AS CpuTimeMs,
//    ISNULL(r.logical_reads, 0) AS LogicalReads,
//    ISNULL(r.reads, 0) AS PhysicalReads,
//    ISNULL(r.writes, 0) AS PhysicalWrites,
//    ISNULL(r.open_transaction_count, 0) AS OpenTxnCount,

//    -- Query Text (Handled NULLs for clean C# string mapping)
//    ISNULL(SUBSTRING(st.text, (r.statement_start_offset/2)+1,   
//        ((CASE r.statement_end_offset   
//            WHEN -1 THEN DATALENGTH(st.text)  
//            ELSE r.statement_end_offset END   
//        - r.statement_start_offset)/2) + 1), '') AS ActiveStatementText,
//    ISNULL(st.text, '') AS FullBatchText

//FROM sys.dm_os_wait_stats now
//-- FIXED: Matching prior.WaitType to ensure join condition uses snapshot column
//JOIN #InitialSnapshot prior ON now.wait_type = prior.WaitType
//LEFT JOIN sys.dm_exec_requests r ON now.wait_type = r.wait_type
//LEFT JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
//OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) st

//WHERE now.wait_type NOT IN (
//    'CLR_SEMAPHORE', 'LAZYWRITER_SLEEP', 'RESOURCE_QUEUE', 'SLEEP_TASK',
//    'SLEEP_SYSTEMTASK', 'SQLTRACE_BUFFER_FLUSH', 'WAITFOR', 'LOGMGR_QUEUE',
//    'CHECKPOINT_QUEUE', 'REQUEST_FOR_DEADLOCK_SEARCH', 'XE_TIMER_EVENT',
//    'BROKER_TO_FLUSH', 'BROKER_TASK_STOP', 'CLR_MANUAL_EVENT', 'CLR_AUTO_EVENT',
//    'DISPATCHER_QUEUE_SEMAPHORE', 'FT_IFTS_SCHEDULER_IDLE_WAIT', 'XE_DISPATCHER_WAIT',
//    'XE_LIVE_TARGET_TVF', 'NUMA_STATIC_PRIMARY_ALLOC_QUEUE', 'DIRTY_PAGE_POLL',
//    'HADR_FILESTREAM_IOMGR_IOCOMPLETION', 'SP_SERVER_DIAGNOSTICS_SLEEP', 'QDS_ASYNC_QUEUE'
//)
//-- FIXED: now.wait_time_ms (DMV) minus prior.WaitTimeMs (Snapshot)
//AND (now.wait_time_ms - prior.WaitTimeMs) > 0
//ORDER BY WaitTimeMsPerSec DESC;

//-- Clean up
//DROP TABLE #InitialSnapshot;";
//    }
//}
