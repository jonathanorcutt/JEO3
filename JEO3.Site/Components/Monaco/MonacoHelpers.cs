using BlazorMonaco.Editor;
using JEO3.Core;

namespace JEO3.Site.Components
{
    public enum MonacoLanguageType
    {
        SQL,
        CSharp
    }
    public static class MonacoHelpers
    {
        internal static StandaloneEditorConstructionOptions EditorOptions(StandaloneCodeEditor editor, MonacoLanguageType language, string entityName)
        {
            var br = "\r\n";
            var defaultText = br + br + br + br + br + $"\t// Click on a {entityName} to get started :) ...  " + br + br + br + br + br
                + string.Join(br, A5C11.GetAsciiArt("JEO3.com", false).Split('\n').Select(v => " " + v.Replace("\r", "").Replace(" ", " ")));
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Value = defaultText,
                Minimap = new EditorMinimapOptions { Enabled = false },
                FontSize = 14,
                FontFamily = "Consolas, Cascadia Mono, Cascadia Code, Courier New, monospace",
                Language = language == MonacoLanguageType.SQL ? "sql" : "csharp",
                Theme = "vs",
                //LineNumbers = "off",
                //RenderLineHighlight = "none",
                Guides = new GuidesOptions() { Indentation = false },
                MouseWheelZoom = true,
                // Block the editor from downloading validation background workers for other languages
                // --- flat 3.4.0 settings to suppress background workers ---
                CodeLens = false,
                SnippetSuggestions = "none",
                QuickSuggestions = new QuickSuggestionsOptions { Other = "false", Comments = "false", Strings = "false" }
            };
        }
    }
}
