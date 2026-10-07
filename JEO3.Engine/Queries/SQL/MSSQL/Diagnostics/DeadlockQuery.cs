//namespace JEO3.Engine
//{
//    internal static partial class MSSQLQueries
//    {
//        // ---------------------------------------------------------------------------
//        // Active requests snapshot (sys.dm_exec_requests + sessions + sql_text)
//        // Used by the monitoring panel for the "Active Requests" diagnostic section.
//        // Excludes system background spids (session_id <= 50) and idle sessions.
//        // ---------------------------------------------------------------------------
//        internal static string GetDeadlockQuery(DateTime sinceUtc) => $@"
//            SELECT 
//                XEventData.XEvent.value('@timestamp', 'datetime2') AS TimestampUtc,
//                XEventData.XEvent.value('(process-list/process/@id)[1]', 'varchar(50)') AS VictimSessionId,
//                CAST(XEventData.XEvent.query('.') AS nvarchar(max)) AS RawXml
//            FROM 
//            (
//                SELECT CAST(target_data AS XML) AS TargetData
//                FROM sys.dm_xe_session_targets st
//                JOIN sys.dm_xe_sessions s ON s.address = st.event_session_address
//                WHERE s.name = 'system_health' AND st.target_name = 'ring_buffer'
//            ) AS Data
//            CROSS APPLY TargetData.nodes('RingBufferTarget/event[@name=""xml_deadlock_report""]') AS XEventData(XEvent)
//            WHERE XEventData.XEvent.value('@timestamp', 'datetime2') >= '{sinceUtc:yyyy-MM-dd HH:mm:ss.fff}';";

//    }
//}
