//using System;
//using System.Collections.Generic;
//using System.Text;
//using JEO3.Providers;
//using JEO3.Schema;

//namespace JEO3.Engine
//{
//    public sealed class MonitorDatasource(IDatabaseProvider databaseProvider) : IMonitorDataSource
//    {
//        public async Task<IReadOnlyList<WaitStatRow>> ReadWaitStatsAsync(CancellationToken ct)
//        {
//            string query = new MSQueryCollection().WaitStatsSnapshotQuery;
//            return await databaseProvider.GetInstances<WaitStatRow>(query).ConfigureAwait(false);
//        }
//        public async Task<IReadOnlyList<ActiveRequestRow>> ReadActiveRequestsAsync(CancellationToken ct)
//        {
//            string query = new MSQueryCollection().ActiveRequestsQuery;

//            return await databaseProvider.GetInstances<ActiveRequestRow>(query).ConfigureAwait(false);
//        }
//        public async Task<IReadOnlyList<BlockingRow>> ReadBlockingChainAsync(CancellationToken ct)
//        {
//            string query = new MSQueryCollection().BlockingQuery;

//            return await databaseProvider.GetInstances<BlockingRow>(query).ConfigureAwait(false);
//        }
//        public async Task<IReadOnlyList<DeadlockEvent>> ReadRecentDeadlocksAsync(DateTime sinceUtc, CancellationToken ct)
//        {
//            string query = new MSQueryCollection().GetDeadlockQuery(sinceUtc);
//            var raw = await databaseProvider.GetInstances<DeadlockEvent>(query).ConfigureAwait(false);
//            return raw.Select(HydrateFromXml).ToList();
//        }

//        private static DeadlockEvent HydrateFromXml(DeadlockEvent evt)
//        {
//            if (string.IsNullOrWhiteSpace(evt.RawXml)) return evt;

//            var doc = System.Xml.Linq.XElement.Parse(evt.RawXml);
//            var deadlock = doc.Descendants("deadlock").FirstOrDefault();
//            if (deadlock is null) return evt;

//            var victimId = deadlock.Element("victim-list")?
//                .Element("victimProcess")?.Attribute("id")?.Value ?? "";

//            var participants = deadlock.Element("process-list")?
//                .Elements("process")
//                .Select(p => new DeadlockParticipant
//                {
//                    SessionId = p.Attribute("id")?.Value ?? "",
//                    LoginName = p.Attribute("loginname")?.Value ?? "",
//                    HostName = p.Attribute("hostname")?.Value ?? "",
//                    ProgramName = p.Attribute("clientapp")?.Value ?? "",
//                    LastStatement = p.Element("inputbuf")?.Value?.Trim() ?? "",
//                    IsVictim = p.Attribute("id")?.Value == victimId,
//                })
//                .ToList() ?? [];

//            return new DeadlockEvent
//            {
//                TimestampUtc = evt.TimestampUtc,
//                VictimSessionId = victimId,
//                RawXml = evt.RawXml,
//                Participants = participants,
//            };
//        }
//    }
//}
