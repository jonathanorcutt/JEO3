namespace JEO3.Providers.Catalogs
{
    public static partial class MSMonitorQueries
    {
        internal const string PerformanceMetricQuery = @"
/* ==========================================================
   SQL MONITOR — SERVER PERFORMANCE METRICS (CPU, MEMORY, I/O)
   Fixed XML sorting error (Msg 305)
   ========================================================== */

SELECT 
    SYSUTCDATETIME() AS Timestamp,
    (
        SELECT TOP 1 
            x.record.value('(./Record/SchedulerMonitorEvent/SystemHealth/ProcessUtilization)[1]', 'int') 
        FROM (
            SELECT CAST(record AS XML) AS record
            FROM sys.dm_os_ring_buffers
            WHERE ring_buffer_type = N'RING_BUFFER_SCHEDULER_MONITOR'
              AND record LIKE '%<SystemHealth>%'
        ) AS x
        ORDER BY x.record.value('(./Record/@time)[1]', 'bigint') DESC
    ) AS CpuPercent,

    -- Memory Metrics
    (
        SELECT MAX(cntr_value) * 1.0 / 1024 
        FROM sys.dm_os_performance_counters 
        WHERE counter_name = N'Total Server Memory (KB)' 
          AND object_name LIKE N'%Memory Manager%'
    ) AS TotalServerMemoryMb,

    (
        SELECT MAX(cntr_value) * 1.0 / 1024 
        FROM sys.dm_os_performance_counters 
        WHERE counter_name = N'Target Server Memory (KB)' 
          AND object_name LIKE N'%Memory Manager%'
    ) AS TargetServerMemoryMb,

    (
        SELECT MAX(cntr_value) 
        FROM sys.dm_os_performance_counters 
        WHERE counter_name = N'Page life expectancy' 
          AND object_name LIKE N'%Buffer Manager%'
    ) AS PageLifeExpectancy,

    -- I/O Stall Metrics
    (
        SELECT ISNULL(SUM(io_stall_read_ms), 0) 
        FROM sys.dm_io_virtual_file_stats(NULL, NULL)
    ) AS IoStallReadMs,

    (
        SELECT ISNULL(SUM(io_stall_write_ms), 0) 
        FROM sys.dm_io_virtual_file_stats(NULL, NULL)
    ) AS IoStallWriteMs,

    -- Throughput & Connection Metrics
    (
        SELECT MAX(cntr_value) 
        FROM sys.dm_os_performance_counters 
        WHERE counter_name = N'Batch Requests/sec' 
          AND object_name LIKE N'%SQL Statistics%'
    ) AS BatchRequestsPerSec,

    (
        SELECT MAX(cntr_value) 
        FROM sys.dm_os_performance_counters 
        WHERE counter_name = N'User Connections' 
          AND object_name LIKE N'%General Statistics%'
    ) AS UserConnections;
";
    }
}