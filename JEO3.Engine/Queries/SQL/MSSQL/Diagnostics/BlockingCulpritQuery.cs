//namespace JEO3.Engine
//{
//    internal static partial class MSSQLQueries
//    {
//        internal const string BlockingCulpritQuery = @"
//-- Root blockers: not blocked themselves, but blocking others
//SELECT
//    blocking.session_id     AS root_blocker_session_id,
//    blocking.login_name,
//    blocking.host_name,
//    blocking.program_name,
//    COUNT(blocked.session_id) AS sessions_blocked
//FROM sys.dm_exec_requests blocked
//JOIN sys.dm_exec_sessions blocking ON blocked.blocking_session_id = blocking.session_id
//WHERE blocked.blocking_session_id NOT IN (
//    SELECT session_id FROM sys.dm_exec_requests WHERE blocking_session_id <> 0
//)
//GROUP BY blocking.session_id, blocking.login_name, blocking.host_name, blocking.program_name
//ORDER BY sessions_blocked DESC;";
//    }
//}
