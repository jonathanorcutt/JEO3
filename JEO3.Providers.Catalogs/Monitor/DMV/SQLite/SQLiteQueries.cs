namespace JEO3.Monitor
{
    internal static class SQLiteQueries
    {
        internal const string InitSqliteSchemaQuery = @"
        PRAGMA foreign_keys = ON;

        CREATE TABLE IF NOT EXISTS WaitRateSample (
            SampledAtUtc TEXT PRIMARY KEY NOT NULL,
            TotalWaitMsPerSec REAL NOT NULL,
            TotalWaitingTasksPerSec REAL NOT NULL,
            ElapsedSeconds REAL NOT NULL
        );

        CREATE TABLE IF NOT EXISTS WaitTypeRate (
            SampledAtUtc TEXT NOT NULL,
            WaitType TEXT NOT NULL,
            WaitMsPerSec REAL NOT NULL,
            WaitingTasksPerSec REAL NOT NULL,
            SignalWaitMsPerSec REAL NOT NULL,
            PRIMARY KEY (SampledAtUtc, WaitType),
            FOREIGN KEY (SampledAtUtc) REFERENCES WaitRateSample (SampledAtUtc) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS ActiveRequestRow (
            SampledAtUtc TEXT NOT NULL,
            SessionId INTEGER NOT NULL,
            BlockingSessionId INTEGER NOT NULL,
            WaitType TEXT NOT NULL,
            WaitTimeSec REAL NOT NULL,
            Status TEXT NOT NULL,
            Command TEXT NOT NULL,
            DatabaseName TEXT NOT NULL,
            LoginName TEXT NOT NULL,
            HostName TEXT NOT NULL,
            ProgramName TEXT NOT NULL,
            CpuTimeMs INTEGER NOT NULL,
            LogicalReads INTEGER NOT NULL,
            PhysicalReads INTEGER NOT NULL,
            Writes INTEGER NOT NULL,
            OpenTxnCount INTEGER NOT NULL,
            PercentComplete REAL NOT NULL,
            StatementText TEXT NOT NULL,
            PRIMARY KEY (SampledAtUtc, SessionId)
        );

        CREATE TABLE IF NOT EXISTS BlockingRow (
            SampledAtUtc TEXT NOT NULL,
            SessionId INTEGER NOT NULL,
            BlockingSessionId INTEGER NOT NULL,
            IsBlocked INTEGER NOT NULL CHECK (IsBlocked IN (0, 1)),
            Status TEXT NOT NULL,
            LoginName TEXT NOT NULL,
            HostName TEXT NOT NULL,
            ProgramName TEXT NOT NULL,
            Command TEXT NOT NULL,
            WaitType TEXT NOT NULL,
            WaitTimeSec REAL NOT NULL,
            RequestStartTime TEXT,
            TransactionBeginTime TEXT,
            OpenTranDurationSec REAL,
            QueryText TEXT NOT NULL,
            IsVictim INTEGER NOT NULL CHECK (IsVictim IN (0, 1)),
            PRIMARY KEY (SampledAtUtc, SessionId)
        );

        CREATE TABLE IF NOT EXISTS DeadlockEvent (
            Timestamp TEXT NOT NULL,
            VictimSessionId TEXT NOT NULL,
            RawXml TEXT NOT NULL,
            PRIMARY KEY (Timestamp, VictimSessionId)
        );

        CREATE TABLE IF NOT EXISTS DeadlockParticipant (
            Timestamp TEXT NOT NULL,
            VictimSessionId TEXT NOT NULL,
            SessionId TEXT NOT NULL,
            LoginName TEXT NOT NULL,
            HostName TEXT NOT NULL,
            ProgramName TEXT NOT NULL,
            LastStatement TEXT NOT NULL,
            IsVictim INTEGER NOT NULL CHECK (IsVictim IN (0, 1)),
            PRIMARY KEY (Timestamp, VictimSessionId, SessionId),
            FOREIGN KEY (Timestamp, VictimSessionId) REFERENCES DeadlockEvent (Timestamp, VictimSessionId) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS idx_WaitTypeRate_SampledAtUtc ON WaitTypeRate (SampledAtUtc);
        CREATE INDEX IF NOT EXISTS idx_ActiveRequestRow_SampledAtUtc ON ActiveRequestRow (SampledAtUtc);
        CREATE INDEX IF NOT EXISTS idx_BlockingRow_SampledAtUtc ON BlockingRow (SampledAtUtc);
        CREATE INDEX IF NOT EXISTS idx_WaitRateSample_TotalWaitMsPerSec ON WaitRateSample (TotalWaitMsPerSec DESC);
    ";

        internal const string PurgeHistoryAfter7Days = @"
-- Enforce foreign keys so cascading deletes work
PRAGMA foreign_keys = ON;

-- Delete metrics older than 7 days
DELETE FROM WaitRateSample 
WHERE SampledAtUtc < datetime('now', '-7 days');

-- Delete active requests older than 7 days
DELETE FROM ActiveRequestRow 
WHERE SampledAtUtc < datetime('now', '-7 days');

-- Delete blocking chains older than 7 days
DELETE FROM BlockingRow 
WHERE SampledAtUtc < datetime('now', '-7 days');

-- Delete old deadlock records
DELETE FROM DeadlockEvent 
WHERE Timestamp < datetime('now', '-7 days');

-- Reclaim unused disk space and defragment the SQLite file
VACUUM;";

    }
}
