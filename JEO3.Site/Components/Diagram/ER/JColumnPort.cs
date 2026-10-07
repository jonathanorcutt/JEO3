using System.Text.Json.Serialization;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;

namespace JEO3.Site.Components.Database
{
    public sealed class JColumnPort : PortModel
    {
        public JColumnPort(NodeModel parent, JColumn column, PortAlignment alignment = PortAlignment.Bottom)
            : base(parent, alignment, null, null)
        {
            Column = column;
        }

        [JsonIgnore]
        public JColumn Column { get; }
        public override bool CanAttachTo(ILinkable other)
        {
            // Avoid attaching to self port/node
            if (!base.CanAttachTo(other))
                return false;

            var targetPort = other as JColumnPort;
            var targetColumn = targetPort.Column;

            return Column.Type != targetColumn.Type
                ? false
                : Column.Primary && targetColumn.Primary
                ? false
                : (!Column.Primary || targetPort.Links.Count <= 0) &&
                (!targetColumn.Primary || Links.Count <= 1);
        }

    }
}
