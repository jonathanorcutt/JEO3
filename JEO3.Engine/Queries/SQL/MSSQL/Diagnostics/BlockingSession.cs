//namespace JEO3.Engine
//{
//    internal static partial class MSSQLQueries
//    {
//        internal const string BlockingQuery = @"
//;WITH BlockingChain AS (
//    SELECT r.session_id AS blocked_session_id, r.blocking_session_id
//    FROM sys.dm_exec_requests r
//    WHERE r.blocking_session_id <> 0
//),
//InvolvedSessions AS (
//    SELECT blocked_session_id AS session_id FROM BlockingChain
//    UNION
//    SELECT blocking_session_id AS session_id FROM BlockingChain
//)
//SELECT
//    s.session_id                                              AS SessionId,
//    r.blocking_session_id                                     AS BlockingSessionId,
//    CASE WHEN r.blocking_session_id > 0 THEN 1 ELSE 0 END      AS IsBlocked,
//    s.status                                                   AS Status,
//    s.login_name                                               AS LoginName,
//    s.host_name                                                AS HostName,
//    s.program_name                                             AS ProgramName,
//    r.command                                                  AS Command,
//    r.wait_type                                                AS WaitType,
//    ISNULL(r.wait_time, 0) / 1000.0                             AS WaitTimeSec,
//    r.start_time                                                AS RequestStartTime,
//    tat.transaction_begin_time                                 AS TransactionBeginTime,
//    CASE WHEN tat.transaction_begin_time IS NOT NULL
//         THEN DATEDIFF(SECOND, tat.transaction_begin_time, SYSUTCDATETIME())
//         ELSE NULL END                                         AS OpenTranDurationSec,
//    t.text                                                     AS QueryText
//FROM InvolvedSessions inv
//JOIN sys.dm_exec_sessions s        ON s.session_id = inv.session_id
//LEFT JOIN sys.dm_exec_requests r   ON r.session_id = s.session_id
//LEFT JOIN sys.dm_exec_connections c ON c.session_id = s.session_id
//LEFT JOIN sys.dm_tran_session_transactions tst
//       ON tst.session_id = s.session_id AND tst.is_user_transaction = 1
//LEFT JOIN sys.dm_tran_active_transactions tat
//       ON tat.transaction_id = tst.transaction_id
//OUTER APPLY sys.dm_exec_sql_text(COALESCE(r.sql_handle, c.most_recent_sql_handle)) t
//ORDER BY IsBlocked, ISNULL(r.wait_time, 0) DESC;";
//    }
//}
