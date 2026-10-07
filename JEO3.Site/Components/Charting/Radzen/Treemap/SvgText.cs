using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace JEO3.Site.Components.Charting
{

    /// <summary>
    /// Razor reserves the lowercase &lt;text&gt; tag as a pseudo-element for literal text
    /// transitions (@: ... equivalent), so it rejects attributes on it even inside &lt;svg&gt;.
    /// This component exists purely so markup can use PascalCase &lt;SvgText&gt;, which Razor
    /// resolves as a component instead of the reserved tag, while still emitting a genuine
    /// SVG &lt;text&gt; element with all attributes passed through untouched.
    /// </summary>
    public class SvgText : ComponentBase
    {
        [Parameter(CaptureUnmatchedValues = true)]
        public Dictionary<string, object>? Attributes { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "text");
            if (Attributes is not null)
                builder.AddMultipleAttributes(1, Attributes);
            builder.AddContent(2, ChildContent);
            builder.CloseElement();
        }
    }
}