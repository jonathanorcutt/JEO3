using System.IO.Compression;
using System.Xml.Linq;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;

namespace JEO3.Diagrams.Export
{
    public static class VisioDiagramExporter
    {
        const string resourceName = "JEO3.Diagrams.Export.JEO3VisioTemplate.vsdx";
        private const double PixelsPerInch = 96.0;
        private const string VisioNs = "http://schemas.microsoft.com/office/visio/2012/main";
        private const string RelationshipNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace V = VisioNs;
        private static readonly XNamespace R = RelationshipNs;

        public static byte[] Export(BlazorDiagram diagram)
        {
            ArgumentNullException.ThrowIfNull(diagram);

            var assembly = typeof(VisioDiagramExporter).Assembly;

            using var resource = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded Visio template '{resourceName}' was not found.");

            using var output = new MemoryStream();

            // The using blocks guarantee that the ZIP architecture closes and appends EOCD bytes
            using (var template = new ZipArchive(resource, ZipArchiveMode.Read, leaveOpen: true))
            {
                using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
                {
                    CopyTemplate(template, archive, diagram);
                }
            }

            return output.ToArray();
        }
        private static void CopyTemplate(
            ZipArchive template,
            ZipArchive output,
            BlazorDiagram diagram)
        {
            foreach (var entry in template.Entries)
            {
                if (entry.FullName.Equals("visio/pages/page1.xml", StringComparison.OrdinalIgnoreCase))
                {
                    WritePage(output, diagram);
                    continue;
                }

                if (entry.FullName.Equals("visio/pages/pages.xml", StringComparison.OrdinalIgnoreCase))
                {
                    WritePages(output, diagram);
                    continue;
                }

                if (entry.FullName.Equals("[Content_Types].xml", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateContentTypes(entry, output);
                    continue;
                }

                CopyEntry(entry, output);
            }
        }

        private static void UpdateContentTypes(ZipArchiveEntry source, ZipArchive destination)
        {
            XDocument doc;
            using (var sourceStream = source.Open())
            {
                doc = XDocument.Load(sourceStream);
            }

            XNamespace ct = "http://openxmlformats.org";

            var hasPage1 = doc.Root?.Elements(ct + "Override")
                .Any(e => e.Attribute("PartName")?.Value == "/visio/pages/page1.xml") ?? false;

            if (!hasPage1 && doc.Root != null)
            {
                doc.Root.Add(new XElement(ct + "Override",
                    new XAttribute("PartName", "/visio/pages/page1.xml"),
                    new XAttribute("ContentType", "application/vnd.ms-visio.page+xml")
                ));
            }

            var target = destination.CreateEntry(source.FullName, CompressionLevel.Optimal);
            using var targetStream = target.Open();
            doc.Save(targetStream);
        }
        private static void CopyEntry(
            ZipArchiveEntry source,
            ZipArchive destination)
        {
            var target =
                destination.CreateEntry(
                    source.FullName,
                    CompressionLevel.Optimal);

            using var sourceStream = source.Open();
            using var targetStream = target.Open();

            sourceStream.CopyTo(targetStream);
        }

        private static void WritePage(
            ZipArchive archive,
            BlazorDiagram diagram)
        {
            var entry =
                archive.CreateEntry(
                    "visio/pages/page1.xml",
                    CompressionLevel.Optimal);

            using var stream = entry.Open();

            var document =
                CreatePageDocument(diagram);

            document.Save(stream);
        }
        // Add this near your PixelsPerInch constant
        private const double SpreadFactor = 2.0; // Increase to 3.0 if you need even more breathing room
        private const double LineHeightInches = 0.18; // Approximate height per row of text
                                                      // 1. CRITICAL: You MUST use this specific format method for every single double/float.
                                                      // This prevents culture-based decimal commas (e.g., 4,25) from corrupting the XML.
        private static string FormatDouble(double value)
        {
            // Cap precision to 4 decimal places and force periods instead of commas
            return value.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
        }

        // Helper for standard static string values
        private static XElement Cell(string name, string value) =>
            new XElement(V + "Cell", new XAttribute("N", name), new XAttribute("V", value));

        private static void WritePages(ZipArchive archive, BlazorDiagram diagram)
        {
            var entry = archive.CreateEntry("visio/pages/pages.xml", System.IO.Compression.CompressionLevel.Optimal);
            using var stream = entry.Open();

            var bounds = CalculateBounds(diagram);

            // Calculate new spread bounds
            var width = Math.Max((bounds.Width * SpreadFactor) / PixelsPerInch + 2, 8.5);
            var height = Math.Max((bounds.Height * SpreadFactor) / PixelsPerInch + 2, 11);

            var pages = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(V + "Pages",
                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                    new XAttribute(XNamespace.Xmlns + "r", RelationshipNs),
                    new XElement(V + "Page",
                        new XAttribute("ID", "0"),
                        new XAttribute("NameU", "Page-1"),
                        new XAttribute("Name", "Page-1"),
                        new XAttribute("ViewScale", "1"),
                        new XAttribute("ViewCenterX", FormatDouble(width / 2)),
                        new XAttribute("ViewCenterY", FormatDouble(height / 2)),
                        new XElement(V + "PageSheet",
                            new XAttribute("LineStyle", "0"),
                            new XAttribute("FillStyle", "0"),
                            new XAttribute("TextStyle", "0"),
                            Cell("PageWidth", FormatDouble(width)),
                            Cell("PageHeight", FormatDouble(height)),
                            Cell("PageScale", "1"),
                            Cell("DrawingScale", "1")
                        ),
                        new XElement(V + "Rel",
                            new XAttribute(R + "id", "rId1"))
                    )
                ));

            pages.Save(stream);
        }

        private static XElement CreateTableShape(TableNodeModel node, int id, DiagramBounds bounds)
        {
            var columnCount = node.ColumnInfos?.Count > 0 ? node.ColumnInfos.Count : node.ColumnNodes?.Count ?? 0;
            var lineCount = columnCount + 1;
            var calculatedHeight = Math.Max(lineCount * LineHeightInches, 0.5);

            var width = Math.Max(node.Size?.Width / PixelsPerInch ?? 0, 2.5);
            var height = Math.Max(node.Size?.Height / PixelsPerInch ?? 0, calculatedHeight);

            var x = ((node.Position.X - bounds.MinX) * SpreadFactor) / PixelsPerInch + 1;
            var y = ((bounds.MaxY - node.Position.Y) * SpreadFactor) / PixelsPerInch + 1;

            var text = BuildTableText(node);

            return new XElement(V + "Shape",
                new XAttribute("ID", id.ToString()),
                new XAttribute("NameU", $"Shape_{id}"), // Safe unique name, no special chars
                new XAttribute("Name", string.IsNullOrWhiteSpace(node.Title) ? $"Table_{id}" : node.Title),
                new XAttribute("Type", "Shape"),

                Cell("PinX", FormatDouble(x + width / 2)),
                Cell("PinY", FormatDouble(y + height / 2)),
                Cell("Width", FormatDouble(width)),
                Cell("Height", FormatDouble(height)),
                Cell("Angle", "0"),
                Cell("LineWeight", "0.01"),

                new XElement(V + "Section",
                    new XAttribute("N", "Character"),
                    new XElement(V + "Row",
                        new XAttribute("IX", "0"),
                        Cell("Font", "0"),
                        Cell("Size", "0.10"))
                ),

                new XElement(V + "Section",
                    new XAttribute("N", "Geometry"),
                    new XAttribute("IX", "0"), // Ensure section index is explicitly declared
                    new XElement(V + "Cell", new XAttribute("N", "NoFill"), new XAttribute("V", "0")),
                    new XElement(V + "Cell", new XAttribute("N", "NoLine"), new XAttribute("V", "0")),
                    new XElement(V + "Row", new XAttribute("T", "RelMoveTo"), new XAttribute("IX", "1"), Cell("X", "0"), Cell("Y", "0")),
                    new XElement(V + "Row", new XAttribute("T", "RelLineTo"), new XAttribute("IX", "2"), Cell("X", "1"), Cell("Y", "0")),
                    new XElement(V + "Row", new XAttribute("T", "RelLineTo"), new XAttribute("IX", "3"), Cell("X", "1"), Cell("Y", "1")),
                    new XElement(V + "Row", new XAttribute("T", "RelLineTo"), new XAttribute("IX", "4"), Cell("X", "0"), Cell("Y", "1")),
                    new XElement(V + "Row", new XAttribute("T", "RelLineTo"), new XAttribute("IX", "5"), Cell("X", "0"), Cell("Y", "0"))
                ),
                new XElement(V + "Text", new XText(text ?? ""))
            );
        }

        private static DiagramPoint GetCenter(TableNodeModel node, DiagramBounds bounds)
        {
            var width = node.Size?.Width ?? (2.5 * PixelsPerInch);
            var height = node.Size?.Height ?? 0;

            var x = ((node.Position.X + width / 2 - bounds.MinX) * SpreadFactor) / PixelsPerInch + 1;
            var y = ((bounds.MaxY - node.Position.Y - height / 2) * SpreadFactor) / PixelsPerInch + 1;

            return new DiagramPoint(x, y);
        }
        private static XDocument CreatePageDocument(
            BlazorDiagram diagram)
        {
            var nodes =
                diagram.Nodes
                    .OfType<TableNodeModel>()
                    .ToList();

            var bounds =
                CalculateBounds(diagram);

            var shapes =
                new XElement(V + "Shapes");

            var shapeIds =
                new Dictionary<NodeModel, int>();

            var nextId = 1;

            foreach (var node in nodes)
            {
                var id = nextId++;

                shapeIds[node] = id;

                shapes.Add(
                    CreateTableShape(
                        node,
                        id,
                        bounds));
            }

            var connects =
                new XElement(V + "Connects");

            foreach (var link in diagram.Links)
            {
                if (link.Source?.Model
                    is not TableNodeModel source)
                    continue;

                if (link.Target?.Model
                    is not TableNodeModel target)
                    continue;

                if (!shapeIds.TryGetValue(
                        source,
                        out var sourceId))
                    continue;

                if (!shapeIds.TryGetValue(
                        target,
                        out var targetId))
                    continue;

                var connectorId = nextId++;

                shapes.Add(
                    CreateConnectorShape(
                        connectorId,
                        source,
                        target,
                        bounds));

                connects.Add(
                    new XElement(
                        V + "Connect",

                        new XAttribute(
                            "FromSheet",
                            connectorId),

                        new XAttribute(
                            "FromCell",
                            "BeginX"),

                        new XAttribute(
                            "FromPart",
                            "9"),

                        new XAttribute(
                            "ToSheet",
                            sourceId),

                        new XAttribute(
                            "ToCell",
                            "Connections.X1"),

                        new XAttribute(
                            "ToPart",
                            "100")));

                connects.Add(
                    new XElement(
                        V + "Connect",

                        new XAttribute(
                            "FromSheet",
                            connectorId),

                        new XAttribute(
                            "FromCell",
                            "EndX"),

                        new XAttribute(
                            "FromPart",
                            "12"),

                        new XAttribute(
                            "ToSheet",
                            targetId),

                        new XAttribute(
                            "ToCell",
                            "Connections.X1"),

                        new XAttribute(
                            "ToPart",
                            "100")));
            }

            return new XDocument(
                new XDeclaration(
                    "1.0",
                    "utf-8",
                    "yes"),

                new XElement(
                    V + "PageContents",

                    new XAttribute(
                        XNamespace.Xml + "space",
                        "preserve"),

                    shapes,
                    connects));
        }

        private static XElement CreateConnectorShape(int id, TableNodeModel source, TableNodeModel target, DiagramBounds bounds)
        {
            var sourcePoint = GetCenter(source, bounds);
            var targetPoint = GetCenter(target, bounds);

            return new XElement(
                V + "Shape",
                new XAttribute("ID", id),
                new XAttribute("NameU", $"JEO3_Connector_{id}"),
                new XAttribute("Type", "Shape"),
                new XAttribute("LineStyle", "0"),

                Cell("BeginX", sourcePoint.X),
                Cell("BeginY", sourcePoint.Y),
                Cell("EndX", targetPoint.X),
                Cell("EndY", targetPoint.Y),
                Cell("LineColor", "#64FF64"),
                Cell("EndArrow", "5"),

                new XElement(
                    V + "Section",
                    new XAttribute("N", "Geometry"),
                    new XElement(
                        V + "Row",
                        new XAttribute("T", "MoveTo"),
                        new XAttribute("IX", "1"),
                        Cell("X", 0), // Base relative starting point
                        Cell("Y", 0)),
                    new XElement(
                        V + "Row",
                        new XAttribute("T", "LineTo"),
                        new XAttribute("IX", "2"),
                        Cell("X", 1), // Standard scale unit line vector endpoints
                        Cell("Y", 1))
                )
            );
        }

        private static string BuildTableText(
            TableNodeModel node)
        {
            var lines =
                new List<string>
                {
                    node.Title ?? "Table"
                };

            if (node.ColumnInfos.Count > 0)
            {
                foreach (var column in node.ColumnInfos)
                {
                    var prefix =
                        column.IsPrimaryKey
                            ? "PK "
                            : column.IsForeignKey
                                ? "FK "
                                : "";

                    lines.Add(
                        $"    {prefix}" +
                        $"{column.Name} : " +
                        $"{column.DataType}");
                }
            }
            else
            {
                foreach (var column in node.ColumnNodes)
                {
                    var prefix =
                        column.IsPrimaryKey
                            ? "PK "
                            : column.IsForeignKey
                                ? "FK "
                                : "";

                    lines.Add(
                        $"    {prefix}" +
                        $"{column.ColumnName} : " +
                        $"{column.DataType}");
                }
            }

            return string.Join(
                Environment.NewLine,
                lines);
        }

        private static DiagramBounds CalculateBounds(
            BlazorDiagram diagram)
        {
            var nodes =
                diagram.Nodes
                    .OfType<TableNodeModel>()
                    .ToList();

            if (nodes.Count == 0)
                return new DiagramBounds(
                    0,
                    0,
                    1,
                    1);

            var minX =
                nodes.Min(x => x.Position.X);

            var minY =
                nodes.Min(x => x.Position.Y);

            var maxX =
                nodes.Max(x =>
                    x.Position.X +
                    x.Size.Width);

            var maxY =
                nodes.Max(x =>
                    x.Position.Y +
                    x.Size.Height);

            return new DiagramBounds(
                minX,
                minY,
                maxX,
                maxY);
        }

        private static XElement Cell(
            string name,
            object value)
        {
            return new XElement(
                V + "Cell",
                new XAttribute("N", name),
                new XAttribute(
                    "V",
                    value is double d
                        ? Format(d)
                        : value.ToString()!));
        }

        private static string Format(double value)
        {
            return value.ToString(
                "0.####",
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string Sanitize(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Table";

            var chars =
                value.Where(
                    x => char.IsLetterOrDigit(x) ||
                         x == '_')
                    .ToArray();

            return new string(chars);
        }

        private readonly record struct DiagramBounds(
            double MinX,
            double MinY,
            double MaxX,
            double MaxY)
        {
            public double Width =>
                MaxX - MinX;

            public double Height =>
                MaxY - MinY;
        }

        private readonly record struct DiagramPoint(
            double X,
            double Y);
    }
}