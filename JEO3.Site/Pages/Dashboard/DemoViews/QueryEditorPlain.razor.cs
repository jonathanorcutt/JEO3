using System.Data;
using System.Diagnostics;
using BlazorMonaco;
using BlazorMonaco.Editor;
using JEO3.Core;
using JEO3.Generation;
using JEO3.Schema;
using JEO3.Site.Components;
using JEO3.Site.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace JEO3.Site.Dashboard
{
    public partial class QueryEditorPlain : IDisposable
    {
        #region Properties
        [Inject] private WSWorkspace W { get; set; }
        [Inject] private IJSRuntime JS { get; set; }

        [Parameter] public string Id { get; set; } = "txtQueryEditor";
        private StandaloneCodeEditor queryEditor;
        private ElementReference queryQueryEditorPaneDiv;
        private bool _isDisposed;
        #endregion

        #region Initialization
        protected override void OnInitialized()
        {
            W.OnDashboardStateChanged += HandleStateChanged;
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
            }
        }
        private async void HandleStateChanged()
        {
            if (_isDisposed) return;
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

        #region Monaco Editor
        private StandaloneEditorConstructionOptions EditorOptions(StandaloneCodeEditor editor)
            => MonacoHelpers.EditorOptions(editor, MonacoLanguageType.CSharp, "table");
        private async Task OnEditorInitialized()
        {
            W.UI.Appearance?.IsMonacoEditorReady = true;

            await this.ApplyAs400Theme();


            if (W.SelectedTable != null && W.Result?.Databars?.Results != null)
            {
                var res = W.Result.Databars.Results.FirstOrDefault(v => v.Tracker.Root.Table.Name == W.SelectedTable.Name);
                if (res != null)
                {
                    await UpdateEditorQuery(res.GeneratedQuery);
                }
            }

            await InvokeAsync(StateHasChanged);
        }
        private async Task OnEditorContentChanged(ModelContentChangedEvent e)
        {
            if (queryEditor != null && W.UI.Appearance.IsMonacoEditorReady)
            {
                // await queryEditor.SetScrollTop((int)500);
            }
        }
        public async Task UpdateEditorQuery(string query)
        {
            try
            {
                if (!W.UI.Appearance.IsMonacoEditorReady) return;
                await queryEditor.SetValue(query);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("// Monaco hit the interop timeout block due to the engine load,  swallow it safely so the thread survives");
            }
            //await InvokeAsync(StateHasChanged);
        }
        public async Task ApplySqlTheme()
        {
            TextModel model = await queryEditor.GetModel();

            await BlazorMonaco.Editor.Global.SetModelLanguage(JS, model, "sql");

            // Apply options dynamically
            await queryEditor.UpdateOptions(new EditorUpdateOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                FontSize = 11,
                FontFamily = "Consolas",
                Scrollbar = new EditorScrollbarOptions
                {
                    Vertical = "visible",
                    Horizontal = "visible"
                },
                Theme = "vs",
                LineNumbers = "on",
                MouseWheelZoom = true,
                CodeLens = true,
                RenderLineHighlight = "all",
                Guides = new GuidesOptions() { Indentation = true },
                ScrollBeyondLastLine = false,
                //Language = "sql",
                //    Value = blocker.QueryText ?? string.Empty,
                //    ReadOnly = true,
                //    Theme = "vs-dark",
                //    WordWrap = "on",
                //    LineNumbers = "on",
                //    MouseWheelZoom = true,
                //    CodeLens = true,
                //    RenderLineHighlight = "all",
                //    ScrollBeyondLastLine = false,
                //    AutomaticLayout = true,   // <-- important, see note below
                //    Minimap = new EditorMinimapOptions { Enabled = false },
                //    FontSize = 11,
                //    FontFamily = "Courier New",
                //    Scrollbar = new EditorScrollbarOptions
                //    {
                //        Vertical = "visible",
                //        Horizontal = "visible"
                //    },

            });
            await queryEditor.Focus();
        }
        private async Task ApplyAs400Theme()
        {
            var as400Theme = new MonacoThemeData
            {
                Base = "vs-dark",
                Inherit = true,
                Colors = new Dictionary<string, string>
            {
                { "editor.background", "#000000" },
                { "editor.foreground", "#00FF00" },
                { "editorCursor.foreground", "#00FF00" },
                { "editor.lineHighlightBackground", "#000000" }
            },
                Rules = new List<MonacoTokenRule>
            {
                new MonacoTokenRule { Token = "", Foreground = "00FF00" }
            }
            };

            // Define the theme payload in JS and force the switch
            await JS.InvokeVoidAsync("monacoInterop.defineAndSetTheme", "as400", as400Theme);
            await queryEditor.SetPosition(new Position() { LineNumber = 7, Column = 80 }, "");
            await queryEditor.Focus();
        }
        private async Task OnThemeChange(SqlEditorTheme sqlEditorTheme)
        {
            if (W.UI.Appearance.IsSchemaLoaded && sqlEditorTheme == SqlEditorTheme.AS400)
            {
                await ApplyAs400Theme();
            }
            else if (sqlEditorTheme == SqlEditorTheme.Light)
            {
                await ApplySqlTheme();
            }
        }

        // Extra - Unused But Keep For Future Use
        private async Task ChangeTextSize(int? newSize)
        {
            if (queryEditor == null) return;
            int? _currentFontSize = 14;

            // Capture the exact vertical pixel scroll position
            double savedScrollTop = await queryEditor.GetScrollTop();

            // Apply the updated font size options
            _currentFontSize = newSize;
            await queryEditor.UpdateOptions(new EditorUpdateOptions() { FontSize = _currentFontSize });

            // Immediately push the original scroll state back to the UI thread
            await queryEditor.SetScrollTop((int)savedScrollTop);
        }
        #endregion

        #region Toggle Full Screen
        private async Task ToggleQueryPaneExpansionFullScreen()
        {
            W.UI.Appearance.IsPaneFullScreenQueryEditor = !W.UI.Appearance.IsPaneFullScreenQueryEditor;
            var isFullscreen = await JS.InvokeAsync<bool>("eval", "document.fullscreenElement != null");

            if (isFullscreen)
            {
                await JS.InvokeVoidAsync("toggleElementFullscreen", queryQueryEditorPaneDiv, false);
            }
            else
            {
                await JS.InvokeVoidAsync("toggleElementFullscreen", queryQueryEditorPaneDiv, true);
            }
        }
        #endregion
    }
}
