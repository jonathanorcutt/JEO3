namespace JEO3.Diagrams
{
    using Microsoft.AspNetCore.Components;

    public class TableNodeBase<TItem> : ComponentBase
    {
        #region Properties

        [Parameter]
        public TItem? NodeData { get; set; }

        [Parameter]
        public EventCallback<TItem> OnNodeSelected { get; set; }

        protected async Task HandleSelection()
        {
            if (NodeData is not null)
            {
                await OnNodeSelected.InvokeAsync(NodeData);
            }
        }

        #endregion
    }
}
