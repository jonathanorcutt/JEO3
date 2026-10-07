using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Dashboard
{
    public partial class DataDisplay : IDisposable
    {
        #region Properties

        [Inject] private WSWorkspace W { get; set; } = new();

        #endregion

        #region Initialization & Lifecycle

        protected override void OnInitialized()
        {
            W.OnDashboardStateChanged += HandleStateChanged;
        }

        private async void HandleStateChanged()
        {
            await InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            if (W != null)
            {
                W.OnDashboardStateChanged -= HandleStateChanged;
            }
        }

        #endregion
    }
}
