using JEO3.Site.Components;
using JEO3.Site.Dashboard;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace JEO3.Site.Layout
{
    public partial class MainTopNav
    {
        #region Properties
        [Inject] public IJSRuntime JS { get; set; }
        [Inject] public DialogService DialogService { get; set; }
        [Inject] public WSWorkspace W { get; set; }
        [Parameter] public EventCallback<bool> OnSidebarToggleClicked { get; set; }
        [Parameter] public EventCallback<bool> OnGaugeToggleClicked { get; set; }
        private bool _isFullScreen;
        private Costello _costello;
        #endregion

        #region Initialization
        protected override void OnInitialized()
        {
            W.OnDashboardStateChanged += OnGlobalWorkspaceUpdated;
        }
        public void Dispose()
        {
            W?.OnDashboardStateChanged -= OnGlobalWorkspaceUpdated;
        }
        #endregion

        #region Global Events
        private async Task OnGlobalThemeColorChanged(string newHexColor)
        {
            if (string.IsNullOrWhiteSpace(newHexColor)) return;
            W.UI.Appearance.ActivePrimaryColor = newHexColor;
            await JS.InvokeVoidAsync("window.jeoThemeEngine.setPrimaryColor", newHexColor);
        }
        private async void OnGlobalWorkspaceUpdated()
        {
            await InvokeAsync(StateHasChanged);
        }

        #endregion

        #region Functions
        private async void FullScreenClicked()
        {
            _isFullScreen = !_isFullScreen;
            await JS.InvokeVoidAsync("toggleSiteFullscreen");
        }

        #endregion

    }
}
