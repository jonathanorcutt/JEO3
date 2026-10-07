//namespace JEO3.Engine
//{
//    internal static partial class MSSQLQueries
//    {
//        // ---------------------------------------------------------------------------
//        // Active requests snapshot (sys.dm_exec_requests + sessions + sql_text)
//        // Used by the monitoring panel for the "Active Requests" diagnostic section.
//        // Excludes system background spids (session_id <= 50) and idle sessions.
//        // ---------------------------------------------------------------------------
//        internal const string ActiveRequestsQuery = @"
//SELECT TOP 50
//    r.session_id                                              AS SessionId,
//    r.blocking_session_id                                     AS BlockingSessionId,
//    r.wait_type                                               AS WaitType,
//    ISNULL(r.wait_time, 0) / 1000.0                           AS WaitTimeSec,
//    r.status                                                  AS Status,
//    r.command                                                 AS Command,
//    ISNULL(DB_NAME(r.database_id), '')                        AS DatabaseName,
//    ISNULL(s.login_name, '')                                  AS LoginName,
//    ISNULL(s.host_name, '')                                   AS HostName,
//    ISNULL(s.program_name, '')                                AS ProgramName,
//    ISNULL(r.cpu_time, 0)                                     AS CpuTimeMs,
//    ISNULL(r.logical_reads, 0)                                AS LogicalReads,
//    ISNULL(r.reads, 0)                                        AS PhysicalReads,
//    ISNULL(r.writes, 0)                                       AS Writes,
//    ISNULL(r.open_transaction_count, 0)                       AS OpenTxnCount,
//    ISNULL(r.percent_complete, 0)                             AS PercentComplete,
//    ISNULL(
//        SUBSTRING(t.text,
//            (r.statement_start_offset / 2) + 1,
//            ((CASE r.statement_end_offset WHEN -1 THEN DATALENGTH(t.text) ELSE r.statement_end_offset END
//              - r.statement_start_offset) / 2) + 1),
//        '') AS StatementText
//FROM sys.dm_exec_requests r
//JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
//OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) t
//WHERE r.session_id > 50
//  AND r.session_id <> @@SPID
//ORDER BY r.wait_time DESC, r.cpu_time DESC;";
//    }
//}
