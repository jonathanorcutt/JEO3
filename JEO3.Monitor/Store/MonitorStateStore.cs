namespace JEO3.Monitor
{
    public sealed class MonitorStateStore
    {
        private MonitorSnapshot _current = new() { Status = MonitorPollingStatus.Stopped };
        public MonitorSnapshot Current => Volatile.Read(ref _current);
        public void Replace(MonitorSnapshot snapshot) => Volatile.Write(ref _current, snapshot);

        public MonitorDeltaResponse GetDelta(DateTime sinceUtc)
        {
            var current = Current;

            return new MonitorDeltaResponse
            {
                ServerTimeUtc = DateTime.UtcNow,
                // Filter chronologically for easy array push/append on the frontend
                History = current.History
                    .Where(h => h.Timestamp > sinceUtc)
                    .OrderBy(h => h.Timestamp)
                    .ToList(),

                PerformanceHistory = current.PerformanceHistory
                    .Where(p => p.Timestamp > sinceUtc)
                    .OrderBy(p => p.Timestamp)
                    .ToList(),

                RecentDeadlocks = current.RecentDeadlocks
                    .Where(d => d.Timestamp > sinceUtc)
                    .OrderBy(d => d.Timestamp)
                    .ToList(),

                ActiveRequests = current.ActiveRequests,
                BlockingChain = current.BlockingChain,
                TopExpensiveQueries = current.TopExpensiveQueries
            };
        }
    }
}
