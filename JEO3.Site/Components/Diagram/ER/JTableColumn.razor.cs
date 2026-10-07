using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Components.Database
{
    public partial class JTableColumn : IDisposable
    {
        private bool _shouldRender = true;

        [Parameter]
        public JTable Table { get; set; }

        [Parameter]
        public JColumn Column { get; set; }

        public bool HasLinks => Table.GetPort(Column)?.Links.Count > 0;

        public void Dispose()
        {
            Column.Changed -= ReRender;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Column.Changed += ReRender;
        }

        //protected override bool ShouldRender() => _shouldRender;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _shouldRender = false;
            await base.OnAfterRenderAsync(firstRender);
        }

        private void ReRender()
        {
            _shouldRender = true;
            StateHasChanged();
        }
    }
}
