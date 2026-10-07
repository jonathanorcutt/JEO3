namespace JEO3.Monitor
{
    public static class SqlScaffoldingQueries
    {
        #region Database
        // Database
        public const string DropDatabaseQuery = @"
USE master;

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{0}')
BEGIN
    ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{0}];
END";
        public const string CreateDatabaseQuery = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{0}') BEGIN EXEC('CREATE DATABASE {0}'); END";
        #endregion

        #region Schema
        // Schema
        public const string CreateSchemaNameQuery = "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{0}') BEGIN EXEC('CREATE SCHEMA {0}'); END";
        #endregion

        #region Tables
        // Inside JEO3.Monitor.SqlScaffoldingQueries class
        public const string CreateTablesQuery = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Configuration].[MonitorInstance]') AND type in (N'U'))
BEGIN
    CREATE TABLE [Configuration].[MonitorInstance](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [UserName] [nvarchar](50) NOT NULL,
        [MachineName] [nvarchar](100) NOT NULL,
        [ApiBaseUrl] [nvarchar](100) NOT NULL,
        [TimestampLastRegistered] [datetime] NULL,
        [TimestampLastActive] [datetime] NULL,
        CONSTRAINT [PK_MonitorInstance] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Configuration].[ExceptionLog]') AND type in (N'U'))
BEGIN
    CREATE TABLE [Configuration].[ExceptionLog](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [TimeStamp] [datetime2](3) NULL,
        [UserName] [nvarchar](75) NULL,
        [Type] [nvarchar](260) NOT NULL,
        [Namespace] [nvarchar](260) NULL,
        [Class] [nvarchar](260) NOT NULL,
        [Method] [nvarchar](260) NULL,
        [LineNumber] [nvarchar](50) NULL,
        [Message] [nvarchar](4000) NOT NULL,
        [AttemptedMethod] [nvarchar](512) NULL,
        [AttemptedMethodReturnType] [nvarchar](512) NULL,
        [AttemptedMethodSignature] [nvarchar](512) NULL,
        [ClassFullName] [nvarchar](512) NULL,
        [StackTrace] [nvarchar](max) NOT NULL,
        [AssemblyFilePath] [nvarchar](260) NULL,
        [AssemblyVersion] [nvarchar](64) NULL,
     CONSTRAINT [PK_ExceptionLog] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[WaitRateSample]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{0}].[WaitRateSample] (
        [WaitRateSampleId] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL, 
        [TotalWaitMsPerSec] FLOAT NOT NULL,
        [TotalWaitingTasksPerSec] FLOAT NOT NULL,
        [ElapsedSeconds] FLOAT NOT NULL,
        CONSTRAINT [PK_{0}_WaitRateSample] PRIMARY KEY CLUSTERED ([Timestamp] ASC) -- 🌟 Key for MetaUpsert
    ) WITH (DATA_COMPRESSION = PAGE);
END;


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[WaitStatRow]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{0}].[WaitStatRow] (
        [WaitStatRowId] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL, 
        [WaitType] NVARCHAR(60) NOT NULL, 
        [WaitingTasksCount] BIGINT NOT NULL,
        [WaitTimeMs] BIGINT NOT NULL,
        [MaxWaitTimeMs] BIGINT NOT NULL,
        [SignalWaitTimeMs] BIGINT NOT NULL,
        CONSTRAINT [PK_{0}_WaitStatRow] PRIMARY KEY CLUSTERED ([WaitStatRowId] ASC)
    ) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[WaitTypeRate]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{0}].[WaitTypeRate] (
        [WaitTypeRateId] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL, 
        [WaitType] NVARCHAR(128) NOT NULL, 
        [WaitMsPerSec] FLOAT NOT NULL,
        [WaitingTasksPerSec] FLOAT NOT NULL,
        [SignalWaitMsPerSec] FLOAT NOT NULL,
        CONSTRAINT [PK_{0}_WaitTypeRate] PRIMARY KEY CLUSTERED ([WaitTypeRateId] ASC)
    ) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[ActiveRequestRow]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{0}].[ActiveRequestRow] (
        [ActiveRequestRowId] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL,
        [SessionId] INT NOT NULL,
        [BlockingSessionId] INT NULL,
        [WaitType] NVARCHAR(128) NOT NULL,
        [WaitTimeSec] FLOAT NOT NULL,
        [Status] VARCHAR(25) NOT NULL, 
        [Command] VARCHAR(100) NOT NULL, 
        [DatabaseName] NVARCHAR(128) NOT NULL,
        [LoginName] NVARCHAR(128) NOT NULL,
        [HostName] NVARCHAR(128) NOT NULL,
        [ProgramName] NVARCHAR(256) NOT NULL,
        [CpuTimeMs] INT NOT NULL,
        [LogicalReads] BIGINT NOT NULL,
        [PhysicalReads] BIGINT NOT NULL,
        [Writes] BIGINT NOT NULL,
        [OpenTxnCount] INT NOT NULL,
        [PercentComplete] FLOAT NOT NULL,
        [StatementText] NVARCHAR(MAX) NOT NULL,
        [IsBlocked] BIT NOT NULL,
        CONSTRAINT [PK_{0}_ActiveRequestRow] PRIMARY KEY CLUSTERED ([ActiveRequestRowId] ASC)
    ) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[BlockingRow]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{0}].[BlockingRow] (
        [BlockingRowId] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL,
        [SessionId] INT NOT NULL,
        [BlockingSessionId] INT NOT NULL,
        [IsBlocked] BIT NOT NULL,
        [Status] VARCHAR(25) NOT NULL,
        [LoginName] NVARCHAR(128) NOT NULL,
        [HostName] NVARCHAR(128) NOT NULL,
        [ProgramName] NVARCHAR(256) NOT NULL,
        [Command] VARCHAR(100) NOT NULL,
        [WaitType] NVARCHAR(128) NOT NULL,
        [WaitTimeSec] FLOAT NOT NULL,
        [RequestStartTime] DATETIME2(3) NULL,
        [TransactionBeginTime] DATETIME2(3) NULL,
        [OpenTranDurationSec] FLOAT NULL,
        [QueryText] NVARCHAR(MAX) NOT NULL,
        [IsVictim] BIT NOT NULL,
        CONSTRAINT [PK_{0}_BlockingRow] PRIMARY KEY CLUSTERED ([BlockingRowId] ASC)
    ) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[TopExpensiveQueryRow]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{0}].[TopExpensiveQueryRow] (
        [TopExpensiveQueryRowId] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL,
        [SqlHandleHex] VARCHAR(130) NOT NULL,
        [PlanHandleHex] VARCHAR(130) NOT NULL,
        [DatabaseName] NVARCHAR(128) NOT NULL,
        [QueryText] NVARCHAR(MAX) NOT NULL,
        [ExecutionCount] BIGINT NOT NULL,
        [TotalElapsedMs] FLOAT NOT NULL,
        [AvgElapsedMs] FLOAT NOT NULL,
        [MaxElapsedMs] FLOAT NOT NULL,
        [TotalCpuMs] FLOAT NOT NULL,
        [AvgCpuMs] FLOAT NOT NULL,
        [MaxCpuMs] FLOAT NOT NULL,
        [TotalLogicalReads] BIGINT NOT NULL,
        [AvgLogicalReads] FLOAT NOT NULL,
        [MaxLogicalReads] BIGINT NOT NULL,
        [TotalLogicalWrites] BIGINT NOT NULL,
        [AvgLogicalWrites] FLOAT NOT NULL,
        [MaxLogicalWrites] BIGINT NOT NULL,
        [TotalIo] BIGINT NOT NULL,
        [AvgIo] FLOAT NOT NULL,
        [MaxIo] BIGINT NOT NULL,
        [QueryImpact] FLOAT NOT NULL,
        [LastExecutionTime] DATETIME2(3) NOT NULL,
        CONSTRAINT [PK_{0}_TopExpensiveQueryRow] PRIMARY KEY CLUSTERED ([TopExpensiveQueryRowId] ASC)
    ) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[PerformanceMetricRow]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{0}].[PerformanceMetricRow] (
        [PerformanceMetricRowId] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL,
        [CpuPercent] FLOAT NOT NULL,
        [MemoryUsagePercent] FLOAT NOT NULL,
        [TotalServerMemoryMb] FLOAT NOT NULL,
        [TargetServerMemoryMb] FLOAT NOT NULL,
        [SqlMemoryUsageMb] FLOAT NOT NULL,
        [PageLifeExpectancy] FLOAT NOT NULL,
        [IoStallReadMs] FLOAT NOT NULL,
        [IoStallWriteMs] FLOAT NOT NULL,
        [BatchRequestsPerSec] FLOAT NOT NULL,
        [UserConnections] FLOAT NOT NULL,
        CONSTRAINT [PK_{0}_PerformanceMetricRow] PRIMARY KEY CLUSTERED ([PerformanceMetricRowId] ASC)
    ) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[DeadlockEvent]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{0}].[DeadlockEvent] (
        [DeadlockEventId] BIGINT IDENTITY(1,1) NOT NULL,
        [Timestamp] DATETIME2(3) NOT NULL,
        [VictimSessionId] INT NOT NULL,
        [RawXml] XML NOT NULL,
        CONSTRAINT [PK_{0}_DeadlockEvent] PRIMARY KEY CLUSTERED ([DeadlockEventId] ASC)
    );
END;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}].[DeadlockParticipant]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{0}].[DeadlockParticipant] (
        [DeadlockEventId] BIGINT NOT NULL, 
        [Timestamp] DATETIME2(3) NOT NULL, 
        [SessionId] INT NOT NULL, 
        [LoginName] NVARCHAR(128) NOT NULL,
        [HostName] NVARCHAR(128) NOT NULL,
        [ProgramName] NVARCHAR(256) NOT NULL,
        [LastStatement] NVARCHAR(MAX) NOT NULL,
        [IsVictim] BIT NOT NULL, 
        CONSTRAINT [PK_{0}_DeadlockParticipant] PRIMARY KEY CLUSTERED ([Timestamp] ASC, [SessionId] ASC) -- 🌟 Key for MetaUpsert
    ) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_{0}_WaitRateSample_Timestamp' AND object_id = OBJECT_ID(N'[{0}].[WaitRateSample]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_{0}_WaitRateSample_Timestamp] ON [{0}].[WaitRateSample] ([Timestamp] ASC) 
    INCLUDE ([TotalWaitMsPerSec], [TotalWaitingTasksPerSec], [ElapsedSeconds]) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_{0}_WaitStatRow_Timestamp_WaitType' AND object_id = OBJECT_ID(N'[{0}].[WaitStatRow]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_{0}_WaitStatRow_Timestamp_WaitType] ON [{0}].[WaitStatRow] ([Timestamp] ASC, [WaitType] ASC) 
    INCLUDE ([WaitingTasksCount], [WaitTimeMs], [MaxWaitTimeMs], [SignalWaitTimeMs]) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_{0}_WaitTypeRate_Timestamp_WaitType' AND object_id = OBJECT_ID(N'[{0}].[WaitTypeRate]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_{0}_WaitTypeRate_Timestamp_WaitType] ON [{0}].[WaitTypeRate] ([Timestamp] ASC, [WaitType] ASC) 
    INCLUDE ([WaitMsPerSec], [WaitingTasksPerSec], [SignalWaitMsPerSec]) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_{0}_ActiveRequestRow_Timestamp' AND object_id = OBJECT_ID(N'[{0}].[ActiveRequestRow]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_{0}_ActiveRequestRow_Timestamp] ON [{0}].[ActiveRequestRow] ([Timestamp] ASC) 
    INCLUDE ([SessionId], [BlockingSessionId], [WaitType], [WaitTimeSec], [Status], [Command], [DatabaseName], [LoginName], [CpuTimeMs], [LogicalReads], [Writes]) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_{0}_BlockingRow_Timestamp' AND object_id = OBJECT_ID(N'[{0}].[BlockingRow]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_{0}_BlockingRow_Timestamp] ON [{0}].[BlockingRow] ([Timestamp] ASC)
    INCLUDE ([SessionId], [BlockingSessionId], [IsBlocked], [Status], [LoginName], [WaitType], [WaitTimeSec], [OpenTranDurationSec]) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_{0}_TopExpensiveQueryRow_Timestamp' AND object_id = OBJECT_ID(N'[{0}].[TopExpensiveQueryRow]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_{0}_TopExpensiveQueryRow_Timestamp] ON [{0}].[TopExpensiveQueryRow] ([Timestamp] ASC)
    INCLUDE ([SqlHandleHex], [PlanHandleHex], [DatabaseName], [ExecutionCount], [TotalElapsedMs], [AvgElapsedMs], [MaxElapsedMs], [TotalCpuMs], [AvgCpuMs], [MaxCpuMs], [TotalLogicalReads], [AvgLogicalReads], [MaxLogicalReads], [QueryImpact], [LastExecutionTime]) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UX_{0}_PerformanceMetricRow_Timestamp' AND object_id = OBJECT_ID(N'[{0}].[PerformanceMetricRow]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_{0}_PerformanceMetricRow_Timestamp] ON [{0}].[PerformanceMetricRow] ([Timestamp] ASC)
    INCLUDE ([CpuPercent], [MemoryUsagePercent], [TotalServerMemoryMb], [TargetServerMemoryMb], [SqlMemoryUsageMb], [PageLifeExpectancy], [IoStallReadMs], [IoStallWriteMs], [BatchRequestsPerSec], [UserConnections]) WITH (DATA_COMPRESSION = PAGE);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_{0}_DeadlockEvent_Timestamp' AND object_id = OBJECT_ID(N'[{0}].[DeadlockEvent]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_{0}_DeadlockEvent_Timestamp] ON [{0}].[DeadlockEvent] ([Timestamp] ASC) 
    INCLUDE ([VictimSessionId]);
END;";
        #endregion

        #region Procedures

        // ── HIGH FREQUENCY PROCEDURE ──────────────────────────────────────────────────
        public const string ProcessProcedureHighName = "[{0}].[sp_ProcessHigh]";
        public const string CreateProcessProcedureHigh = @"
CREATE OR ALTER PROCEDURE " + ProcessProcedureHighName + @"
(
     @IsInitialLoad BIT = 0,
     @MinTimestamp DATETIME2 = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentTime DATETIME = GETUTCDATE();
    DECLARE @FallbackTime DATETIME = DATEADD(HOUR, -3, GETUTCDATE());
    DECLARE @QueryFilter DATETIME = COALESCE(@MinTimestamp, @FallbackTime);

    IF @QueryFilter < @FallbackTime 
    BEGIN 
        SET @QueryFilter = @FallbackTime; 
    END;

    IF @IsInitialLoad = 1
    BEGIN
        SELECT * FROM [{0}].[WaitStatRow] WHERE [Timestamp] >= @QueryFilter ORDER BY [Timestamp] ASC;
        SELECT * FROM [{0}].[PerformanceMetricRow] WHERE [Timestamp] >= @QueryFilter ORDER BY [Timestamp] ASC;    
    END
    ELSE
    BEGIN
        CREATE TABLE #TempWaitStats (
            [Timestamp] DATETIME, [WaitType] NVARCHAR(60), [WaitingTasksCount] BIGINT, 
            [WaitTimeMs] BIGINT, [MaxWaitTimeMs] BIGINT, [SignalWaitTimeMs] BIGINT
        );

        CREATE TABLE #TempPerformance (
            [Timestamp] DATETIME, [CpuPercent] INT, [MemoryUsagePercent] DECIMAL(5,2), [TotalServerMemoryMb] FLOAT, 
            [TargetServerMemoryMb] FLOAT, [SqlMemoryUsageMb] FLOAT, [PageLifeExpectancy] BIGINT, 
            [IoStallReadMs] BIGINT, [IoStallWriteMs] BIGINT, [BatchRequestsPerSec] BIGINT, [UserConnections] BIGINT
        );
       
        INSERT INTO #TempWaitStats
        SELECT
            @CurrentTime AS Timestamp, wait_type AS WaitType, waiting_tasks_count AS WaitingTasksCount,
            wait_time_ms AS WaitTimeMs, max_wait_time_ms AS MaxWaitTimeMs, signal_wait_time_ms AS SignalWaitTimeMs
        FROM sys.dm_os_wait_stats WITH (NOLOCK)
        WHERE 
            (waiting_tasks_count > 0 OR wait_time_ms > 0 OR max_wait_time_ms > 0 OR signal_wait_time_ms > 0)
            AND wait_type NOT IN (
            'SLEEP_TASK', 'SLEEP_SYSTEMTASK', 'SLEEP_DBSTARTUP', 'SLEEP_DBTASK', 'SLEEP_TEMPDBSTARTUP',
            'SLEEP_MASTERDBREADY', 'SLEEP_MASTERMDREADY', 'SLEEP_MASTERUPGRADED', 'SLEEP_MSDBSTARTUP',
            'SLEEP_BUFFERPOOL_FREEPAGES', 'BROKER_TO_FLUSH', 'BROKER_TASK_STOP', 'BROKER_EVENTHANDLER',
            'BROKER_TRANSMITTER', 'CHECKPOINT_QUEUE', 'DBMIRROR_EVENTS_QUEUE', 'DISPATCHER_QUEUE_SEMAPHORE',
            'CLR_AUTO_EVENT', 'CLR_MANUAL_EVENT', 'CLR_SEMAPHORE', 'FT_IFTS_SCHEDULER_IDLE_WAIT',
            'HADR_FILESTREAM_IOMGR_IOCOMPLETION', 'HADR_WORK_QUEUE', 'LAZYWRITER_SLEEP', 'LOGMGR_QUEUE',
            'ONDEMAND_TASK_QUEUE', 'REQUEST_FOR_DEADLOCK_SEARCH', 'RESOURCE_QUEUE', 'SERVER_IDLE_CHECK',
            'SNI_HTTP_ACCEPT', 'SP_SERVER_DIAGNOSTICS_SLEEP', 'SQLTRACE_BUFFER_FLUSH', 'SQLTRACE_INCREMENTAL_FLUSH_SLEEP',
            'WAITFOR', 'XE_DISPATCHER_WAIT', 'XE_TIMER_EVENT', 'XE_LIVE_TARGET_TVF', 'NUMA_STATIC_PRIMARY_ALLOC_QUEUE',
            'DIRTY_PAGE_POLL', 'QDS_ASYNC_QUEUE', 'QDS_CLEANUP_STALE_QUERIES_TASK_MAIN_LOOP_SLEEP'
        );

        INSERT INTO [{0}].[WaitStatRow] SELECT * FROM #TempWaitStats;

        /* Performance snapshot */
        INSERT INTO #TempPerformance
        SELECT
            @CurrentTime AS Timestamp,
            cpu.CpuPercent,
            CAST((CAST(proc_mem.physical_memory_in_use_kb AS FLOAT) / NULLIF(sys_mem.total_physical_memory_kb, 0)) * 100 AS DECIMAL(5, 2)) AS MemoryUsagePercent,
            MAX(CASE WHEN pc.counter_name = N'Total Server Memory (KB)' THEN pc.cntr_value END) * 1.0 / 1024 AS TotalServerMemoryMb,
            MAX(CASE WHEN pc.counter_name = N'Target Server Memory (KB)' THEN pc.cntr_value END) * 1.0 / 1024 AS TargetServerMemoryMb,
            CAST(proc_mem.physical_memory_in_use_kb AS FLOAT) AS SqlMemoryUsageMb,
            MAX(CASE WHEN pc.counter_name = N'Page life expectancy' THEN pc.cntr_value END) AS PageLifeExpectancy,
            io.IoStallReadMs,
            io.IoStallWriteMs,
            MAX(CASE WHEN pc.counter_name = N'Batch Requests/sec' THEN pc.cntr_value END) AS BatchRequestsPerSec,
            MAX(CASE WHEN pc.counter_name = N'User Connections' THEN pc.cntr_value END) AS UserConnections
        FROM sys.dm_os_sys_memory AS sys_mem WITH (NOLOCK)
        CROSS JOIN sys.dm_os_process_memory AS proc_mem WITH (NOLOCK)
        CROSS JOIN (
            SELECT
                ISNULL(SUM(io_stall_read_ms), 0) AS IoStallReadMs,
                ISNULL(SUM(io_stall_write_ms), 0) AS IoStallWriteMs
            FROM sys.dm_io_virtual_file_stats(NULL, NULL)
        ) AS io
        CROSS JOIN sys.dm_os_performance_counters AS pc WITH (NOLOCK)
        CROSS JOIN (
            SELECT TOP 1
                x.record.value('(./Record/SchedulerMonitorEvent/SystemHealth/ProcessUtilization)[1]', 'int') AS CpuPercent
            FROM (
                SELECT CAST(record AS XML) AS record
                FROM sys.dm_os_ring_buffers WITH (NOLOCK)
                WHERE ring_buffer_type = N'RING_BUFFER_SCHEDULER_MONITOR'
                  AND record LIKE '%<SystemHealth>%'
            ) AS x
            ORDER BY x.record.value('(./Record/@time)[1]', 'bigint') DESC
        ) AS cpu
        WHERE
            (
                pc.counter_name IN (
                    N'Total Server Memory (KB)',
                    N'Target Server Memory (KB)',
                    N'Page life expectancy',
                    N'Batch Requests/sec',
                    N'User Connections'
                )
            )
            AND (
                pc.object_name LIKE N'%Memory Manager%'
                OR pc.object_name LIKE N'%Buffer Manager%'
                OR pc.object_name LIKE N'%SQL Statistics%'
                OR pc.object_name LIKE N'%General Statistics%'
            )
        GROUP BY
            cpu.CpuPercent,
            proc_mem.physical_memory_in_use_kb,
            sys_mem.total_physical_memory_kb,
            io.IoStallReadMs,
            io.IoStallWriteMs;

        INSERT INTO [{0}].[PerformanceMetricRow] SELECT * FROM #TempPerformance WHERE CpuPercent IS NOT NULL;

        SELECT * FROM #TempWaitStats;
        SELECT * FROM #TempPerformance;

        DROP TABLE #TempWaitStats;
        DROP TABLE #TempPerformance;
    END
END";

        // ── MEDIUM FREQUENCY PROCEDURE ────────────────────────────────────────────────
        public const string ProcessProcedureMediumName = "[{0}].[sp_ProcessMedium]";
        public const string CreateProcessProcedureMedium = @"
CREATE OR ALTER PROCEDURE " + ProcessProcedureMediumName + @"
(
     @IsInitialLoad BIT = 0,
     @MinTimestamp DATETIME2 = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CurrentTime DATETIME = GETUTCDATE();
    DECLARE @FallbackTime DATETIME = DATEADD(HOUR, -3, GETUTCDATE());
    -- Use the passed timestamp, otherwise default to 3 hours ago (safety net)
    DECLARE @QueryFilter DATETIME = COALESCE(@MinTimestamp, @FallbackTime);
    
    -- Dynamic variable for Expensive Queries: start from the last known record, or the filter if empty
    DECLARE @LastTopExpensiveQueryTime DATETIME = COALESCE((SELECT MAX(Timestamp) FROM [{0}].[TopExpensiveQueryRow]), @QueryFilter);

    -- Safety Cap: If someone passes a date older than 3 hours, force it to 3 hours to prevent huge loads
    IF @QueryFilter < @FallbackTime 
    BEGIN 
        SET @QueryFilter = @FallbackTime; 
    END;

    IF @IsInitialLoad = 1
    BEGIN
        SELECT * FROM [{0}].[ActiveRequestRow] WHERE [Timestamp] >= @QueryFilter ORDER BY [Timestamp] ASC;        
        SELECT * FROM [{0}].[BlockingRow] WHERE [Timestamp] >= @QueryFilter ORDER BY [Timestamp] ASC;    
        SELECT * FROM [{0}].[TopExpensiveQueryRow] WHERE [Timestamp] >= @QueryFilter ORDER BY [Timestamp] ASC;
    END
    ELSE
    BEGIN
        CREATE TABLE #TempActiveRequests (
            [Timestamp] DATETIME, [SessionId] INT, [BlockingSessionId] INT, [WaitType] NVARCHAR(60), [WaitTimeSec] NUMERIC(18,3), 
            [Status] NVARCHAR(30), [Command] NVARCHAR(32), [DatabaseName] NVARCHAR(128), [LoginName] NVARCHAR(128), [HostName] NVARCHAR(128), 
            [ProgramName] NVARCHAR(128), [CpuTimeMs] INT, [LogicalReads] BIGINT, [PhysicalReads] BIGINT, [Writes] BIGINT, 
            [OpenTxnCount] INT, [PercentComplete] REAL, [StatementText] NVARCHAR(MAX), [IsBlocked] INT
        );

        CREATE TABLE #TempBlockingRows (
            [Timestamp] DATETIME, [SessionId] INT, [BlockingSessionId] INT, [IsBlocked] INT, [Status] NVARCHAR(30), 
            [LoginName] NVARCHAR(128), [HostName] NVARCHAR(128), [ProgramName] NVARCHAR(128), [Command] NVARCHAR(32), 
            [WaitType] NVARCHAR(60), [WaitTimeSec] NUMERIC(18,3), [RequestStartTime] DATETIME, [TransactionBeginTime] DATETIME, 
            [OpenTranDurationSec] INT, [QueryText] NVARCHAR(MAX), [IsVictim] INT
        );

        CREATE TABLE #TempExpensiveQueries (
            [Timestamp] DATETIME, [SqlHandleHex] VARCHAR(130), [PlanHandleHex] VARCHAR(130), [DatabaseName] NVARCHAR(128), 
            [QueryText] NVARCHAR(MAX), [ExecutionCount] BIGINT, [TotalElapsedMs] FLOAT, [AvgElapsedMs] FLOAT, [MaxElapsedMs] FLOAT, 
            [TotalCpuMs] FLOAT, [AvgCpuMs] FLOAT, [MaxCpuMs] FLOAT, [TotalLogicalReads] BIGINT, [AvgLogicalReads] FLOAT, 
            [MaxLogicalReads] BIGINT, [TotalLogicalWrites] BIGINT, [AvgLogicalWrites] FLOAT, [MaxLogicalWrites] BIGINT, 
            [TotalIo] BIGINT, [AvgIo] FLOAT, [MaxIo] BIGINT, [QueryImpact] FLOAT, [LastExecutionTime] DATETIME
        );

        INSERT INTO #TempActiveRequests
        SELECT TOP 50    
            @CurrentTime AS Timestamp, r.session_id AS SessionId, r.blocking_session_id AS BlockingSessionId,
            COALESCE(r.wait_type, 'UNKNOWN') AS WaitType, ISNULL(r.wait_time, 0) / 1000.0 AS WaitTimeSec,    
            r.status AS Status, r.command AS Command, ISNULL(DB_NAME(r.database_id), '') AS DatabaseName,    
            ISNULL(s.login_name, '') AS LoginName, ISNULL(s.host_name, '') AS HostName, ISNULL(s.program_name, '') AS ProgramName,    
            ISNULL(r.cpu_time, 0) AS CpuTimeMs, ISNULL(r.logical_reads, 0) AS LogicalReads, ISNULL(r.reads, 0) AS PhysicalReads,    
            ISNULL(r.writes, 0) AS Writes, ISNULL(r.open_transaction_count, 0) AS OpenTxnCount, ISNULL(r.percent_complete, 0) AS PercentComplete,    
            ISNULL(SUBSTRING(t.text, (r.statement_start_offset / 2) + 1, ((CASE r.statement_end_offset WHEN -1 THEN DATALENGTH(t.text) ELSE r.statement_end_offset END - r.statement_start_offset) / 2) + 1), '') AS StatementText,
            (CASE WHEN r.blocking_session_id > 0 THEN 1 ELSE 0 END) AS IsBlocked
        FROM sys.dm_exec_requests r WITH (NOLOCK)
        JOIN sys.dm_exec_sessions s WITH (NOLOCK) ON r.session_id = s.session_id
        OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) t
        WHERE r.session_id > 50 AND r.session_id <> @@SPID;

        INSERT INTO [{0}].[ActiveRequestRow] SELECT * FROM #TempActiveRequests;

        ;WITH BlockingChain AS (
            SELECT r.session_id AS blocked_session_id, r.blocking_session_id
            FROM sys.dm_exec_requests r WITH (NOLOCK) WHERE r.blocking_session_id <> 0
        ),
        InvolvedSessions AS (
            SELECT blocked_session_id AS session_id FROM BlockingChain
            UNION SELECT blocking_session_id AS session_id FROM BlockingChain
        )
        INSERT INTO #TempBlockingRows
        SELECT
            @CurrentTime AS Timestamp, s.session_id AS SessionId, r.blocking_session_id AS BlockingSessionId,
            (CASE WHEN r.blocking_session_id > 0 THEN 1 ELSE 0 END) AS IsBlocked, s.status AS Status,
            s.login_name AS LoginName, s.host_name AS HostName, s.program_name AS ProgramName, r.command AS Command,
            COALESCE(r.wait_type, 'UNKNOWN') AS WaitType, ISNULL(r.wait_time, 0) / 1000.0 AS WaitTimeSec, r.start_time AS RequestStartTime,
            tat.transaction_begin_time AS TransactionBeginTime,
            CASE WHEN tat.transaction_begin_time IS NOT NULL THEN DATEDIFF(SECOND, tat.transaction_begin_time, SYSUTCDATETIME()) ELSE NULL END AS OpenTranDurationSec,
            COALESCE(t.text, '') AS QueryText, (CASE WHEN r.blocking_session_id > 0 THEN 1 ELSE 0 END) AS IsVictim
        FROM InvolvedSessions inv
        JOIN sys.dm_exec_sessions s WITH (NOLOCK) ON s.session_id = inv.session_id
        LEFT JOIN sys.dm_exec_requests r WITH (NOLOCK) ON r.session_id = s.session_id
        LEFT JOIN sys.dm_exec_connections c WITH (NOLOCK) ON c.session_id = s.session_id
        LEFT JOIN sys.dm_tran_session_transactions tst WITH (NOLOCK) ON tst.session_id = s.session_id AND tst.is_user_transaction = 1
        LEFT JOIN sys.dm_tran_active_transactions tat WITH (NOLOCK) ON tat.transaction_id = tst.transaction_id
        OUTER APPLY sys.dm_exec_sql_text(COALESCE(r.sql_handle, c.most_recent_sql_handle)) t;

        INSERT INTO [{0}].[BlockingRow] SELECT * FROM #TempBlockingRows;

        INSERT INTO #TempExpensiveQueries
        SELECT TOP 50
            @CurrentTime AS Timestamp,
            sys.fn_varbintohexstr(qs.sql_handle) AS SqlHandleHex,
            sys.fn_varbintohexstr(qs.plan_handle) AS PlanHandleHex,
            ISNULL(DB_NAME(st.dbid), '<Unknown>') AS DatabaseName,
            SUBSTRING(st.text, (qs.statement_start_offset / 2) + 1, ((CASE qs.statement_end_offset WHEN -1 THEN DATALENGTH(st.text) ELSE qs.statement_end_offset END - qs.statement_start_offset) / 2) + 1) AS QueryText,
            qs.execution_count AS ExecutionCount,    
            qs.total_elapsed_time / 1000.0 AS TotalElapsedMs,
            (qs.total_elapsed_time / 1000.0) / NULLIF(qs.execution_count, 0) AS AvgElapsedMs,
            qs.max_elapsed_time / 1000.0 AS MaxElapsedMs,    
            qs.total_worker_time / 1000.0 AS TotalCpuMs,
            (qs.total_worker_time / 1000.0) / NULLIF(qs.execution_count, 0) AS AvgCpuMs,
            qs.max_worker_time / 1000.0 AS MaxCpuMs,    
            qs.total_logical_reads AS TotalLogicalReads,
            (qs.total_logical_reads * 1.0) / NULLIF(qs.execution_count, 0) AS AvgLogicalReads,
            qs.max_logical_reads AS MaxLogicalReads,    
            qs.total_logical_writes AS TotalLogicalWrites,
            (qs.total_logical_writes * 1.0) / NULLIF(qs.execution_count, 0) AS AvgLogicalWrites,
            qs.max_logical_writes AS MaxLogicalWrites,    
            (qs.total_logical_reads + qs.total_logical_writes) AS TotalIo,
            ((qs.total_logical_reads + qs.total_logical_writes) * 1.0) / NULLIF(qs.execution_count, 0) AS AvgIo,
            (qs.max_logical_reads + qs.max_logical_writes) AS MaxIo,
            LOG10(CASE WHEN ((qs.total_worker_time * 3) + qs.total_logical_reads + qs.total_logical_writes) <= 0 THEN 1 ELSE ((qs.total_worker_time * 3) + qs.total_logical_reads + qs.total_logical_writes) END) AS QueryImpact,
            qs.last_execution_time AS LastExecutionTime
        FROM sys.dm_exec_query_stats AS qs WITH (NOLOCK)
        CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS st
        WHERE qs.last_execution_time >= CAST(@LastTopExpensiveQueryTime AT TIME ZONE 'UTC' AT TIME ZONE 'Eastern Standard Time' AS DATETIME2) AND st.text NOT LIKE '%sys.dm_exec_query_stats%';

        INSERT INTO [{0}].[TopExpensiveQueryRow] SELECT * FROM #TempExpensiveQueries;

        SELECT * FROM #TempActiveRequests;
        SELECT * FROM #TempBlockingRows;
        SELECT * FROM #TempExpensiveQueries;

        DROP TABLE #TempActiveRequests;
        DROP TABLE #TempBlockingRows;
        DROP TABLE #TempExpensiveQueries;
    END
END";

        // Low
        public const string ProcessProcedureLowName = "[{0}].[sp_ProcessLow]";
        // ── LOW FREQUENCY PROCEDURE ───────────────────────────────────────────────────public const string ProcessProcedureLowName = "[{0}].[sp_ProcessLow]";
        public const string CreateProcessProcedureLow = @"
CREATE OR ALTER PROCEDURE " + ProcessProcedureLowName + @"
(
	@IsInitialLoad BIT = 0,
	@MinTimestamp DATETIME2 = NULL
)
AS
BEGIN
SET NOCOUNT ON;

DECLARE @FallbackTime DATETIME = DATEADD(HOUR, -3, GETUTCDATE());
DECLARE @QueryFilter DATETIME = COALESCE(@MinTimestamp, @FallbackTime);
DECLARE @LastDeadlockEventTime DATETIME = COALESCE((SELECT MAX(Timestamp) FROM [{0}].[DeadlockEvent]), @QueryFilter);

IF @QueryFilter < @FallbackTime
BEGIN
	SET @QueryFilter = @FallbackTime;
END;

IF @IsInitialLoad = 1
BEGIN
	SELECT * FROM [{0}].[DeadlockEvent] WHERE [Timestamp] >= @QueryFilter ORDER BY [Timestamp] ASC;
END
ELSE
BEGIN

CREATE TABLE #TempDeadlocks ([Timestamp] DATETIME2, [VictimSessionId] VARCHAR(50), [RawXml] XML);

INSERT INTO #TempDeadlocks
SELECT
	XEventData.XEvent.value('@timestamp', 'datetime2') AS Timestamp,XEventData.XEvent.value('(data[@name=""xml_report""]/value/deadlock/process-list/process[@id = ../../victim-list/victimProcess/@id]/@spid)[1]', 'varchar(50)') AS VictimSessionId,XEventData.XEvent.query('data[@name=""xml_report""]/value/deadlock') AS RawXml
FROM(SELECT CAST(target_data AS XML) AS TargetData
FROM sys.dm_xe_session_targets st WITH (NOLOCK)
	JOIN sys.dm_xe_sessions s WITH (NOLOCK) ON s.address = st.event_session_address
WHERE s.name = 'system_health' AND st.target_name = 'ring_buffer') AS Data
CROSS APPLY TargetData.nodes('RingBufferTarget/event[@name=""xml_deadlock_report""]') AS XEventData(XEvent)
WHERE XEventData.XEvent.value('@timestamp', 'datetime2') >= @LastDeadlockEventTime;

INSERT INTO [{0}].[DeadlockEvent] ([Timestamp], [VictimSessionId], [RawXml])
	SELECT [Timestamp], [VictimSessionId], [RawXml] FROM #TempDeadlocks;SELECT * FROM #TempDeadlocks;
	
DROP TABLE #TempDeadlocks;
END
END";

        public const string CreateExceptionLogProcedureName = "[Configuration].[sp_InsertExceptionLog]";
        public const string CreateExceptionLogProcedure = @"
CREATE OR ALTER PROCEDURE " + CreateExceptionLogProcedureName + @"
    @TimeStamp DATETIME2,
    @UserName NVARCHAR(75),
    @Type NVARCHAR(260),
    @Namespace NVARCHAR(260),
    @Class NVARCHAR(260),
    @Method NVARCHAR(260),
    @LineNumber NVARCHAR(50),
    @Message NVARCHAR(4000),
    @AttemptedMethod NVARCHAR(512),
    @AttemptedMethodReturnType NVARCHAR(512),
    @AttemptedMethodSignature NVARCHAR(512),
    @StackTrace NVARCHAR(MAX),
    @ClassFullName NVARCHAR(512),
    @AssemblyFilePath NVARCHAR(260),
    @AssemblyVersion NVARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [Configuration].[ExceptionLog]
    (
        TimeStamp,
        UserName,
        Type,
        Namespace,
        Class,
        Method,
        LineNumber,
        Message,
        AttemptedMethod,
        AttemptedMethodReturnType,
        AttemptedMethodSignature,
        StackTrace,
        ClassFullName,
        AssemblyFilePath,
        AssemblyVersion
    )
    VALUES
    (
        @TimeStamp,
        @UserName,
        @Type,
        @Namespace,
        @Class,
        @Method,
        @LineNumber,
        @Message,
        @AttemptedMethod,
        @AttemptedMethodReturnType,
        @AttemptedMethodSignature,
        @StackTrace,
        @ClassFullName,
        @AssemblyFilePath,
        @AssemblyVersion
    );
END;
";
        #endregion

        #region Retrieval

        public const string SelectFromUserTable = @"SELECT * FROM [{0}].[{1}] WHERE Timestamp >= '{2}' ORDER BY Timestamp ASC";
        public const string SelectMaxTimestampFromUserTable = @"SELECT MAX(Timestamp) AS [Timestamp] FROM [{0}].[{1}] WITH(NOLOCK) WHERE Timestamp >= '{2}'";

        #endregion

        #region Update
        public const string InsertUpdateMonitorInstanceQuery = @"
                UPDATE [Configuration].[MonitorInstance] WITH (UPDLOCK, HOLDLOCK)
                SET [TimestampLastActive] = '{4}'
                WHERE [ApiBaseUrl] = '{2}';

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO [Configuration].[MonitorInstance] 
                        ([UserName], [MachineName], [ApiBaseUrl], [TimestampLastRegistered], [TimestampLastActive])
                    VALUES 
                        ('{0}', '{1}', '{2}', '{3}', '{4}');
                END;";
        #endregion

        #region Purge

        public const string PurgeDataQuery = @"
                        DELETE FROM [{0}].WaitStatRow WHERE Timestamp < DATEADD(hour, -{1}, GETUTCDATE()); 
                        DELETE FROM [{0}].WaitTypeRate WHERE Timestamp < DATEADD(hour, -{1}, GETUTCDATE());
                        DELETE FROM [{0}].WaitRateSample WHERE Timestamp < DATEADD(hour, -{1}, GETUTCDATE()); 
                        DELETE FROM [{0}].ActiveRequestRow WHERE Timestamp < DATEADD(hour, -{1}, GETUTCDATE());
                        DELETE FROM [{0}].BlockingRow WHERE Timestamp < DATEADD(hour, -{1}, GETUTCDATE());
                        DELETE FROM [{0}].TopExpensiveQueryRow WHERE Timestamp < DATEADD(hour, -{1}, GETUTCDATE());
                        DELETE FROM [{0}].PerformanceMetricRow WHERE Timestamp < DATEADD(hour, -{1}, GETUTCDATE());
                        DELETE FROM [{0}].DeadlockEvent WHERE Timestamp < DATEADD(hour, -{1}, GETUTCDATE());
                        DELETE FROM [{0}].DeadlockParticipant WHERE Timestamp < DATEADD(hour, -{1}, GETUTCDATE());
";
        #endregion

        #region Views
        public const string CreateTableRowCountsViewName = "[{0}].[sp_ViewTableRowCounts]";
        public static string CreateTableRowCountsView = @"

CREATE OR ALTER PROCEDURE " + CreateTableRowCountsViewName + @"
AS
BEGIN

    DROP TABLE IF EXISTS #temp{0}Stats;

    CREATE TABLE #temp{0}Stats (
        TableName VARCHAR(50),
        RowCnt INT
    );

    -- Separate, isolated insert statements to avoid optimization and resource contention
    INSERT INTO #temp{0}Stats (TableName, RowCnt)
    SELECT 'ActiveRequestRow', COUNT(*) FROM [{0}].[ActiveRequestRow] WITH(NOLOCK);

    INSERT INTO #temp{0}Stats (TableName, RowCnt)
    SELECT 'BlockingRow', COUNT(*) FROM [{0}].[BlockingRow] WITH(NOLOCK);

    INSERT INTO #temp{0}Stats (TableName, RowCnt)
    SELECT 'DeadlockEvent', COUNT(*) FROM [{0}].[DeadlockEvent] WITH(NOLOCK);

    INSERT INTO #temp{0}Stats (TableName, RowCnt)
    SELECT 'DeadlockParticipant', COUNT(*) FROM [{0}].[DeadlockParticipant] WITH(NOLOCK);

    INSERT INTO #temp{0}Stats (TableName, RowCnt)
    SELECT 'PerformanceMetricRow', COUNT(*) FROM [{0}].[PerformanceMetricRow] WITH(NOLOCK);

    INSERT INTO #temp{0}Stats (TableName, RowCnt)
    SELECT 'TopExpensiveQueryRow', COUNT(*) FROM [{0}].[TopExpensiveQueryRow] WITH(NOLOCK);

    INSERT INTO #temp{0}Stats (TableName, RowCnt)
    SELECT 'WaitRateSample', COUNT(*) FROM [{0}].[WaitRateSample] WITH(NOLOCK);

    INSERT INTO #temp{0}Stats (TableName, RowCnt)
    SELECT 'WaitStatRow', COUNT(*) FROM [{0}].[WaitStatRow] WITH(NOLOCK);

    INSERT INTO #temp{0}Stats (TableName, RowCnt)
    SELECT 'WaitTypeRate', COUNT(*) FROM [{0}].[WaitTypeRate] WITH(NOLOCK);

    -- Output results sequentially ordered
    SELECT 
        TableName, 
        RowCnt 
    FROM #temp{0}Stats 
    ORDER BY 
        TableName;
END";
        #endregion
    }
}