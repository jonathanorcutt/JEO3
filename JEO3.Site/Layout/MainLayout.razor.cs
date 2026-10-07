using JEO3.Site.Components;
using JEO3.Site.Components.Layout;
using JEO3.Site.Dashboard;
using Microsoft.JSInterop;

namespace JEO3.Site.Layout
{
    public partial class MainLayout
    {
        #region Properties

        // Navigation
        private List<MatrixNavAccordion.NavGroup> _navGroups = MatrixNavAccordion.DefaultNavGroups;

        // Lifecycle
        private bool _rendered;
        private bool _disposed;

        // Appearance
        private bool _leftSidebarExpanded = false;
        private bool _showGauge = true;

        // Animations
        private Costello _costello;
        private Bob _bob;

        #endregion

        #region Initialization

        protected override void OnInitialized()
        {
            W.OnDashboardStateChanged += DashboardChanged;
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _rendered = firstRender ? true : _rendered;
            if (firstRender)
            {
                if (W.UI.Appearance.IsSchemaLoaded)
                {
                    await JS.InvokeVoidAsync("bob.init", _bob.Id);
                }

                if (W.UI.Appearance.ShowAnimations)
                {
                    // await costello?.TriggerFlashMob(5, true);
                    // bob?.Play();
                    // await JS.InvokeVoidAsync("eval", "document.documentElement.style.setProperty('--rz-primary', '#FF0097');"); // '#00000000');");
                    // await JS.InvokeVoidAsync("eval", "document.documentElement.style.setProperty('--jeo-primary', '#FF0097');"); //'#00000000');");
                }

                _rendered = firstRender;
                _leftSidebarExpanded = W.UI.Appearance.IsSidebarExpandedLeft;
                await InvokeAsync(StateHasChanged);
            }
        }
        public void Dispose()
        {
            if (_disposed || W == null) { return; }
            W.OnDashboardStateChanged -= DashboardChanged;
            _disposed = true;
        }
        #endregion

        #region Global Events

        private void OnToggleSidebar(bool isExpanded)
        {
            _leftSidebarExpanded = isExpanded;
            W.UI.Appearance.IsSidebarExpandedLeft = isExpanded;
            StateHasChanged();
        }
        private void OnNavItemSelected(MatrixNavAccordion.NavItem item)
        {
            W.UI.Nav.SelectedTabIndex = item.TabIndex;
            W.UI.Nav.ExpandedGroupIndices.Add(WSNavGroupMap.GroupFor(item.TabIndex));
            W.NotifyMainLayoutStateChanged();
        }
        public void DashboardChanged()
        {
        }
        #endregion

        #region Guage
        private async Task OnGaugeToggleClicked(bool value)
        {
            _showGauge = !_showGauge;
            StateHasChanged();
        }
        #endregion
    }
}
