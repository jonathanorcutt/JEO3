using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using GraphShape.Algorithms.Layout;
using GraphShape.Algorithms.OverlapRemoval;
using QuikGraph;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {
        public static async Task ApplyGraphShapeLayoutKamadaKawai(BlazorDiagram diagram, KkParameters parameters)
        {
            if (!diagram.Nodes.Any()) return;

            if (parameters.UseEntityRelationMode)
            {
                // 1. Inflate canvas bounds based on table count so KK doesn't force 60+ cards into 800x400
                double canvasScale = Math.Max(2.5, Math.Sqrt(diagram.Nodes.Count) * 0.4);
                parameters.CanvasWidth = parameters.CanvasWidth * canvasScale;
                parameters.CanvasHeight = parameters.CanvasHeight * canvasScale;

                // 2. Define center and radius of the initial placement ring
                double centerX = parameters.CanvasWidth / 2.0;
                double centerY = parameters.CanvasHeight / 2.0;
                double ringRadius = Math.Min(parameters.CanvasWidth, parameters.CanvasHeight) * 0.35;

                // 3. Move every node out of the (30,30) top-left corner onto the ring
                int index = 0;
                foreach (var node in diagram.Nodes)
                {
                    // Calculate equal slice of 360 degrees (2 * PI in radians) for each node
                    double theta = (2.0 * Math.PI / diagram.Nodes.Count) * index;

                    // Assign initial (X, Y) coordinates around the center point
                    double xPos = centerX + (ringRadius * Math.Cos(theta));
                    double yPos = centerY + (ringRadius * Math.Sin(theta));

                    node.Position = new Point(xPos, yPos);
                    index++;
                }
            }

            var quikGraph = new BidirectionalGraph<TableNodeModel, Edge<TableNodeModel>>();
            foreach (var node in diagram.Nodes) quikGraph.AddVertex((TableNodeModel)node);
            foreach (var link in diagram.Links)
            {
                if (link.Source?.Model is TableNodeModel src && link.Target?.Model is TableNodeModel tgt)
                    quikGraph.AddEdge(new Edge<TableNodeModel>(src, tgt));
            }

            var context = new LayoutContext<TableNodeModel, Edge<TableNodeModel>, BidirectionalGraph<TableNodeModel, Edge<TableNodeModel>>>(
                quikGraph,
                diagram.Nodes.ToDictionary(n => (TableNodeModel)n, n => new GraphShape.Point(n.Position.X, n.Position.Y)),
                diagram.Nodes.ToDictionary(n => (TableNodeModel)n, n => new GraphShape.Size(n.Size?.Width ?? 70, n.Size?.Height ?? 30)),
                LayoutMode.Simple
            );

            // KK requires explicit bounding space limits to distribute its spring formulas evenly
            var parametersNew = new KKLayoutParameters
            {
                Width = parameters.CanvasWidth,
                Height = parameters.CanvasHeight,
                MaxIterations = parameters.MaxIterations,
                DisconnectedMultiplier = parameters.DisconnectedMultiplier, // Controls how far completely independent table islands sit from each other
                K = parameters.K,
                LengthFactor = parameters.LengthFactor,
                ExchangeVertices = parameters.ExchangeVertices
            };

            var factory = new StandardLayoutAlgorithmFactory<TableNodeModel, Edge<TableNodeModel>, BidirectionalGraph<TableNodeModel, Edge<TableNodeModel>>>();
            var algorithm = factory.CreateAlgorithm("KK", context, parametersNew);
            await ComputeQueryNodeAlgorithm(algorithm);

            // 1. Map node coordinates and physical sizes into a GraphShape Rect layout dictionary
            var rectangles = algorithm.VerticesPositions.ToDictionary(
                kvp => (TableNodeModel)kvp.Key,
                kvp => new GraphShape.Rect(
                    kvp.Value.X,
                    kvp.Value.Y,
                    (int)(kvp.Key.Size?.Width ?? 70),
                    (int)(kvp.Key.Size?.Height ?? 30)
                )
            );

            // 2. Pass the rectangles directly into the FSAAlgorithm constructor
            var overlapParameters = new OverlapRemovalParameters { HorizontalGap = 30, VerticalGap = 30 };
            var overlapAlgorithm = new FSAAlgorithm<TableNodeModel>(rectangles, overlapParameters);

            // Run the heavy lifting on a background thread
            await Task.Run(() =>
            {
                // existing algorithm code here
                overlapAlgorithm.Compute();
            });

            // 4. Update positions on the Blazor canvas using the adjusted layout results
            diagram.Batch(() =>
            {
                foreach (var kvp in overlapAlgorithm.Rectangles)
                {
                    TableNodeModel uiComponentNode = kvp.Key;
                    GraphShape.Rect safeBox = kvp.Value;

                    uiComponentNode.SetPosition(safeBox.X, safeBox.Y);
                }
            });

            diagram.Batch(() =>
            {
                foreach (var kvp in algorithm.VerticesPositions)
                {
                    kvp.Key.SetPosition(kvp.Value.X, kvp.Value.Y);
                }
            });
        }
    }
}
