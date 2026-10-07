using JEO3.Core;

namespace JEO3.Providers.Catalogs
{
    public sealed class OracleMonitorQueryStoreCatalog : BaseMonitorQueryStoreCatalog
    {
        public override string GetActiveRequestsQuery(EnvironmentType environment, DateTime utcStartTime)
            => $"""
                SELECT ARR.Timestamp, ARR.SessionId, ARR.BlockingSessionId, ARR.WaitType, ARR.WaitTimeSec, ARR.Status,
                   ARR.Command, ARR.DatabaseName, ARR.LoginName, ARR.HostName, ARR.ProgramName,
                   ARR.CpuTimeMs, ARR.LogicalReads, ARR.PhysicalReads, ARR.Writes, ARR.OpenTxnCount,
                   ARR.PercentComplete, ARR.StatementText, ARR.IsBlocked
                FROM Monitor.{GetSchema(environment)}.ActiveRequestRow ARR
                WHERE ARR.Timestamp >= '{utcStartTime:o}'
                """;
        public override string GetWaitTypesQuery(EnvironmentType environment, DateTime utcStartTime)
            => $"""
                SELECT WT.Timestamp, WT.WaitType, WT.WaitMsPerSec, WT.WaitingTasksPerSec, WT.SignalWaitMsPerSec
                FROM Monitor.{GetSchema(environment)}.WaitTypeRate WT WHERE WT.Timestamp >= '{utcStartTime:o}'
                """;
        public override string GetWaitSamplesQuery(EnvironmentType environment, DateTime utcStartTime)
            => $"""
                SELECT WR.Timestamp, WR.TotalWaitMsPerSec, WR.TotalWaitingTasksPerSec, WR.ElapsedSeconds
                FROM Monitor.{GetSchema(environment)}.WaitRateSample WR WHERE WR.Timestamp >= '{utcStartTime:o}'
                """;
        public override string GetBlockingQuery(EnvironmentType environment, DateTime utcStartTime)
            => $"""
                SELECT BR.Timestamp, BR.SessionId, BR.BlockingSessionId, BR.IsBlocked, BR.Status, BR.LoginName, BR.HostName,                 
                BR.ProgramName, BR.Command, BR.WaitType, BR.WaitTimeSec, BR.RequestStartTime, BR.TransactionBeginTime, 
                BR.OpenTranDurationSec, BR.QueryText, BR.IsVictim
                FROM Monitor.{GetSchema(environment)}.BlockingRow BR WHERE BR.Timestamp >= '{utcStartTime:o}'
                """;
        public override string GetTopExpensiveQueryQuery(EnvironmentType environment, DateTime utcStartTime)
            => $"""
                SELECT EQ.Timestamp, EQ.SqlHandleHex, EQ.PlanHandleHex, EQ.DatabaseName, EQ.QueryText, EQ.ExecutionCount, 
                EQ.TotalElapsedMs, EQ.AvgElapsedMs, EQ.MaxElapsedMs, EQ.TotalCpuMs, EQ.AvgCpuMs, EQ.MaxCpuMs, EQ.TotalLogicalReads,
                EQ.AvgLogicalReads, EQ.MaxLogicalReads, EQ.TotalLogicalWrites, EQ.AvgLogicalWrites, EQ.MaxLogicalWrites, EQ.TotalIo, 
                EQ.AvgIo, EQ.MaxIo, EQ.QueryImpact, EQ.LastExecutionTime
                FROM Monitor.{GetSchema(environment)}.TopExpensiveQueryRow EQ WHERE EQ.Timestamp >= '{utcStartTime:o}'
                """;
        public override string GetPerformanceMetricQuery(EnvironmentType environment, DateTime utcStartTime)
            => $"""
                SELECT PM.Timestamp, PM.CpuPercent, PM.MemoryUsagePercent, PM.TotalServerMemoryMb, PM.TargetServerMemoryMb, 
                PM.SqlMemoryUsageMb, PM.PageLifeExpectancy, PM.IoStallReadMs, PM.IoStallWriteMs, PM.BatchRequestsPerSec, PM.UserConnections
                FROM Monitor.{GetSchema(environment)}.PerformanceMetricRow PM WHERE PM.Timestamp >= '{utcStartTime:o}'
                """;
        public override string GetDeadlockEventQuery(EnvironmentType environment, DateTime utcStartTime)
            => $"""
                SELECT DE.Timestamp, DE.VictimSessionId, DE.RawXml 
                FROM Monitor.{GetSchema(environment)}.DeadlockEvent DE WHERE DE.Timestamp >= '{utcStartTime:o}'
                """;
        public override string GetDeadlockParticipantQuery(EnvironmentType environment, DateTime utcStartTime)
            => $"""
               SELECT DP.Timestamp, DP.VictimSessionId, DP.SessionId, DP.LoginName, DP.HostName, DP.ProgramName, DP.LastStatement, DP.IsVictim
               FROM Monitor.{GetSchema(environment)}.DeadlockParticipant DP WHERE DP.Timestamp  >='{utcStartTime:o}'
               """;
        private string GetSchema(EnvironmentType type) => EnvironmentResolver.ResolveTablePrefix(type);

        //var deQuery = $"""
        //    SELECT DE.Timestamp, DE.VictimSessionId, DE.RawXml 
        //    FROM Monitor.{GetSchema(environment)}.DeadlockEvent DE WHERE DE.Timestamp >= '{utcStartTime:o}'
        //    """;

        //var dpQuery = $"""
        //    SELECT DP.Timestamp, DP.VictimSessionId, DP.SessionId, DP.LoginName, DP.HostName, DP.ProgramName, DP.LastStatement, DP.IsVictim
        //    FROM Monitor.{GetSchema(environment)}.DeadlockParticipant DP WHERE DP.Timestamp  >='{utcStartTime:o}'
        //    """;


    }
}
