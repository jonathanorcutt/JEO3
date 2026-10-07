using JEO3.Monitor.Wmi;
using Microsoft.AspNetCore.Components;

namespace JEO3.Monitor.Charts.WMI
{
    public abstract class WmiMonitorComponentBase : ComponentBase, IDisposable
    {
        [Inject]
        protected WmiMonitorStore WmiStore { get; set; } = null!;

        protected WmiMonitorSnapshot Wmi => WmiStore.Current;

        protected virtual async Task NotifyRefreshed() { }

        protected override void OnInitialized()
        {
            WmiStore.Changed += OnWmiChanged;
        }

        private void OnWmiChanged()
        {
            NotifyRefreshed();
            _ = InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            WmiStore.Changed -= OnWmiChanged;
        }
    }
}