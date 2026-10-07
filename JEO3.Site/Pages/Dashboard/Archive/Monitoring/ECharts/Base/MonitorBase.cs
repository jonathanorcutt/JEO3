using JEO3.Monitor;
using JEO3.Monitor.Models;
using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Components.ECharts
{
    public abstract class MonitorBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected IMonitorDataService DataService { get; set; } = default!;

        [Parameter] public virtual string Width { get; set; } = "785px";
        [Parameter] public virtual string Height { get; set; } = "250px";

        protected bool IsDisposed { get; set; }

        // --- 1. CUMULATIVE & TIME-SERIES LOGS (Appended & Capped) ---
        protected readonly List<WaitRateSample> WaitHistory = new(10000);
        protected readonly List<MonitorHistorySample> PerformanceHistory = new(10000);
        protected readonly List<DeadlockEvent> RecentDeadlocks = new(2000);

        // --- 2. LIVE STATE (Overwritten on update) ---
        protected IReadOnlyList<ActiveRequestRow> ActiveRequests = Array.Empty<ActiveRequestRow>();
        protected IReadOnlyList<BlockingRow> BlockingChain = Array.Empty<BlockingRow>();
        protected IReadOnlyList<TopExpensiveQueryRow> TopExpensiveQueries = Array.Empty<TopExpensiveQueryRow>();

        // --- 3. STATUS ---
        protected MonitorPollingStatus Status { get; private set; }
        protected string? LastError { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            // Kicks off the shared poll only if nobody has yet. Subsequent charts just attach.
            await DataService.EnsureStartedAsync();

            var initial = DataService.CurrentSnapshot;
            if (initial != null)
            {
                Status = initial.Status;
                LastError = initial.LastError;

                if (initial.History != null) WaitHistory.AddRange(initial.History);
                if (initial.PerformanceHistory != null) PerformanceHistory.AddRange(initial.PerformanceHistory);
                if (initial.RecentDeadlocks != null) RecentDeadlocks.AddRange(initial.RecentDeadlocks);

                if (initial.ActiveRequests != null) ActiveRequests = initial.ActiveRequests;
                if (initial.BlockingChain != null) BlockingChain = initial.BlockingChain;
                if (initial.TopExpensiveQueries != null) TopExpensiveQueries = initial.TopExpensiveQueries;

                await RenderInitialAsync(initial);
            }

            DataService.OnDelta += HandleDelta;
        }

        private void HandleDelta(MonitorDeltaResponse delta)
        {
            if (IsDisposed) return;

            _ = InvokeAsync(async () =>
            {
                Status = delta.Status;
                LastError = delta.LastError;
                bool hasUpdates = false;

                if (delta.History?.Count > 0)
                {
                    WaitHistory.AddRange(delta.History);
                    if (WaitHistory.Count > MaxRetention) WaitHistory.RemoveRange(0, WaitHistory.Count - MaxRetention);
                    hasUpdates = true;
                }

                if (delta.PerformanceHistory?.Count > 0)
                {
                    PerformanceHistory.AddRange(delta.PerformanceHistory);
                    if (PerformanceHistory.Count > MaxRetention) PerformanceHistory.RemoveRange(0, PerformanceHistory.Count - MaxRetention);
                    hasUpdates = true;
                }

                if (delta.RecentDeadlocks?.Count > 0)
                {
                    foreach (var dl in delta.RecentDeadlocks)
                    {
                        if (!RecentDeadlocks.Any(d => d.Timestamp == dl.Timestamp && d.VictimSessionId == dl.VictimSessionId))
                        {
                            RecentDeadlocks.Add(dl);
                        }
                    }
                    hasUpdates = true;
                }

                if (delta.ActiveRequests != null) { ActiveRequests = delta.ActiveRequests; hasUpdates = true; }
                if (delta.BlockingChain != null) { BlockingChain = delta.BlockingChain; hasUpdates = true; }
                if (delta.TopExpensiveQueries?.Count > 0) { TopExpensiveQueries = delta.TopExpensiveQueries; hasUpdates = true; }

                if (hasUpdates) await AppendDeltasAsync(delta);
                StateHasChanged();
            });
        }

        protected abstract Task RenderInitialAsync(MonitorSnapshot initial);
        protected abstract Task AppendDeltasAsync(MonitorDeltaResponse delta);

        public virtual ValueTask DisposeAsync()
        {
            IsDisposed = true;
            DataService.OnDelta -= HandleDelta;
            return ValueTask.CompletedTask;
        }
    }
}
