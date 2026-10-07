namespace JEO3.Providers.Catalogs
{
    public static partial class MSMonitorQueries
    {
        public const string TopExpensiveQueryQuery = @"
DECLARE @SortBy VARCHAR(20) = '{0}'
SELECT TOP (@TopCount)
SYSUTCDATETIME()            AS Timestamp,
    sys.fn_varbintohexstr(qs.sql_handle) AS SqlHandleHex,
    sys.fn_varbintohexstr(qs.plan_handle) AS PlanHandleHex,
    ISNULL(DB_NAME(st.dbid), '<Unknown>') AS DatabaseName,
    SUBSTRING(
        st.text, 
        (qs.statement_start_offset / 2) + 1, 
        ((CASE qs.statement_end_offset 
            WHEN -1 THEN DATALENGTH(st.text) 
            ELSE qs.statement_end_offset 
          END - qs.statement_start_offset) / 2) + 1
    ) AS QueryText,
    qs.execution_count AS ExecutionCount,
    
    -- Elapsed Time (ms)
    qs.total_elapsed_time / 1000.0 AS TotalElapsedMs,
    (qs.total_elapsed_time / 1000.0) / qs.execution_count AS AvgElapsedMs,
    qs.max_elapsed_time / 1000.0 AS MaxElapsedMs,
    
    -- CPU Time (ms)
    qs.total_worker_time / 1000.0 AS TotalCpuMs,
    (qs.total_worker_time / 1000.0) / qs.execution_count AS AvgCpuMs,
    qs.max_worker_time / 1000.0 AS MaxCpuMs,
    
    -- Logical Reads & Writes
    qs.total_logical_reads AS TotalLogicalReads,
    (qs.total_logical_reads * 1.0) / qs.execution_count AS AvgLogicalReads,
    qs.max_logical_reads AS MaxLogicalReads,
    
    qs.total_logical_writes AS TotalLogicalWrites,
    (qs.total_logical_writes * 1.0) / qs.execution_count AS AvgLogicalWrites,
    qs.max_logical_writes AS MaxLogicalWrites,
    
    -- Combined IO (Reads + Writes)
    (qs.total_logical_reads + qs.total_logical_writes) AS TotalIo,
    ((qs.total_logical_reads + qs.total_logical_writes) * 1.0) / qs.execution_count AS AvgIo,
    (qs.max_logical_reads + qs.max_logical_writes) AS MaxIo,
    
    -- Redgate Query Impact Calculation: LOG10((TotalCPU * 3) + TotalReads + TotalWrites)
    LOG10(
        CASE WHEN ((qs.total_worker_time * 3) + qs.total_logical_reads + qs.total_logical_writes) <= 0 
             THEN 1 
             ELSE ((qs.total_worker_time * 3) + qs.total_logical_reads + qs.total_logical_writes) 
        END
    ) AS QueryImpact,
    qs.last_execution_time AS LastExecutionTime
FROM sys.dm_exec_query_stats AS qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) AS st
WHERE st.text NOT LIKE '%sys.dm_exec_query_stats%' -- Filter self-monitoring noise
ORDER BY 
    CASE WHEN @SortBy = 'AvgElapsed' THEN (qs.total_elapsed_time / 1000.0) / qs.execution_count END DESC,
    CASE WHEN @SortBy = 'MaxElapsed' THEN qs.max_elapsed_time END DESC,
    CASE WHEN @SortBy = 'AvgIo' THEN ((qs.total_logical_reads + qs.total_logical_writes) * 1.0) / qs.execution_count END DESC,
    CASE WHEN @SortBy = 'MaxIo' THEN (qs.max_logical_reads + qs.max_logical_writes) END DESC,
    CASE WHEN @SortBy = 'AvgCpu' THEN (qs.total_worker_time / 1000.0) / qs.execution_count END DESC,
    CASE WHEN @SortBy = 'QueryImpact' THEN LOG10(CASE WHEN ((qs.total_worker_time * 3) + qs.total_logical_reads + qs.total_logical_writes) <= 0 THEN 1 ELSE ((qs.total_worker_time * 3) + qs.total_logical_reads + qs.total_logical_writes) END) END DESC,
    CASE WHEN @SortBy = 'ExecutionCount' THEN qs.execution_count END DESC;
";
    }
}