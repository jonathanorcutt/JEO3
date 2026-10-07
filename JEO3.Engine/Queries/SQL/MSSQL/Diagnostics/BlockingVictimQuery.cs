//namespace JEO3.Engine
//{
//    internal static partial class MSSQLQueries
//    {
//        internal const string BlockingVictimQuery = @"
//-- Live blocking chain: who's blocking whom, right now
//SELECT
//    r.session_id            AS blocked_session_id,
//    r.blocking_session_id,
//    r.wait_type,
//    r.wait_time / 1000.0    AS wait_time_sec,
//    r.status,
//    r.command,
//    s.login_name,
//    s.host_name,
//    s.program_name,
//    t.text                  AS blocked_query_text,
//    r.start_time
//FROM sys.dm_exec_requests r
//JOIN sys.dm_exec_sessions s ON r.session_id = s.session_id
//CROSS APPLY sys.dm_exec_sql_text(r.sql_handle) t
//WHERE r.blocking_session_id <> 0
//ORDER BY r.wait_time DESC;";
//    }
//}
