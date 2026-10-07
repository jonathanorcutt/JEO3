using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Dashboard
{
    public partial class ChartsTemplateDisplay : ComponentBase, IDisposable
    {
        #region Properties
        private bool _rendered { get; set; }
        private bool _isDisposed { get; set; }
        #endregion

        #region Initiillization
        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    if (firstRender)
        //    {
        //        _rendered = true;
        //    }
        //}
        #endregion

        //protected override void OnInitialized()
        //{
        //    W.OnDashboardStateChanged += HandleStateChanged;
        //}

        //private async void HandleStateChanged()
        //{
        //    if (_isDisposed) return;
        //    await InvokeAsync(StateHasChanged);
        //}

        //public void Dispose()
        //{
        //    if (W != null)
        //    {
        //        W.OnDashboardStateChanged -= HandleStateChanged;
        //    }
        //    _isDisposed = true;
        //}

    }
}
