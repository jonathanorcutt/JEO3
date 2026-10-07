//namespace JEO3.Providers.Catalogs
//{
//    public static partial class MSMonitorQueries
//    {
//        internal const string TopExpensiveQueries = @"
///* ==========================================================
//   SQL MONITOR — LIVE TOP EXPENSIVE QUERIES
//   DMV ONLY — NO QUERY STORE
//   ========================================================== */

//DECLARE @TopN int = 25;

///*
//    Supported ranking modes:

//    avg_wait  = Average wait time across active requests
//    top_wait  = Highest wait time among active requests
//    avg_io    = Average logical reads across active requests
//    top_io    = Highest logical reads on an active request
//*/

//DECLARE @OrderBy varchar(20) = 'top_wait';


//;WITH ActiveQueries AS
//(
//    SELECT
//        r.session_id AS SessionId,
//        r.request_id AS RequestId,

//        r.status AS RequestStatus,
//        r.command AS CommandType,

//        r.database_id AS DatabaseId,

//        DB_NAME(r.database_id) AS DatabaseName,

//        r.start_time AS StartTime,

//        r.total_elapsed_time AS ElapsedMs,
//        r.cpu_time AS CpuMs,

//        r.wait_time AS WaitMs,
//        r.wait_type AS WaitType,

//        r.wait_resource AS WaitResource,

//        r.blocking_session_id AS BlockingSessionId,

//        r.logical_reads AS LogicalReads,
//        r.reads AS PhysicalReads,
//        r.writes AS Writes,

//        r.row_count AS RowCount,

//        s.login_name AS LoginName,
//        s.host_name AS HostName,
//        s.program_name AS ProgramName,

//        s.open_transaction_count AS OpenTransactions,

//        st.text AS SqlText,

//        r.statement_start_offset AS StatementStartOffset,
//        r.statement_end_offset AS StatementEndOffset

//    FROM sys.dm_exec_requests AS r

//    INNER JOIN sys.dm_exec_sessions AS s
//        ON s.session_id = r.session_id

//    OUTER APPLY sys.dm_exec_sql_text(r.sql_handle) AS st

//    WHERE r.session_id <> @@SPID
//      AND s.is_user_process = 1
//),

//Stats AS
//(
//    SELECT
//        a.*,

//        AVG(CAST(a.WaitMs AS decimal(18,2)))
//            OVER () AS AvgWaitMs,

//        MAX(a.WaitMs)
//            OVER () AS TopWaitMs,

//        AVG(CAST(a.LogicalReads AS decimal(18,2)))
//            OVER () AS AvgLogicalReads,

//        MAX(a.LogicalReads)
//            OVER () AS TopLogicalReads

//    FROM ActiveQueries AS a
//)

//SELECT TOP (@TopN)

//    SessionId,
//    RequestId,

//    DatabaseName,

//    RequestStatus,
//    CommandType,

//    StartTime,

//    ElapsedMs,
//    CpuMs,

//    WaitMs,
//    WaitType,
//    WaitResource,

//    BlockingSessionId,

//    LogicalReads,
//    PhysicalReads,
//    Writes,

//    RowCount,

//    LoginName,
//    HostName,
//    ProgramName,

//    OpenTransactions,

//    CAST(AvgWaitMs AS decimal(18,2))
//        AS AvgWaitMs,

//    TopWaitMs,

//    CAST(AvgLogicalReads AS decimal(18,2))
//        AS AvgLogicalReads,

//    TopLogicalReads,

//    SqlText,

//    /* Useful for Monaco/editor detail retrieval */
//    StatementStartOffset,
//    StatementEndOffset

//FROM Stats

//ORDER BY

//    CASE
//        WHEN @OrderBy = 'avg_wait'
//            THEN AvgWaitMs
//    END DESC,

//    CASE
//        WHEN @OrderBy = 'top_wait'
//            THEN WaitMs
//    END DESC,

//    CASE
//        WHEN @OrderBy = 'avg_io'
//            THEN AvgLogicalReads
//    END DESC,

//    CASE
//        WHEN @OrderBy = 'top_io'
//            THEN LogicalReads
//    END DESC,

//    ElapsedMs DESC

//OPTION (RECOMPILE);";
//    }
//}
