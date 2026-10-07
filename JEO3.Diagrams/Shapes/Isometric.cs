using Blazor.Diagrams;
using GraphShape.Algorithms.Layout;
using QuikGraph;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {
        public static async Task ApplyGraphShapeLayoutISOM(BlazorDiagram diagram, IsomParameters parameters)
        {
            if (!diagram.Nodes.Any()) return;

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
                diagram.Nodes.ToDictionary(n => (TableNodeModel)n, n => new GraphShape.Size(n.Size?.Width ?? 100, n.Size?.Height ?? 40)),
                LayoutMode.Simple
            );

            var parametersNew = new ISOMLayoutParameters
            {
                MaxEpochs = parameters.MaxEpochs,          // Total training cycles for the organization network
                InitialRadius = parameters.InitialRadius,       // Scope of impact when adjusting grouped node neighbors
                MinRadius = parameters.MinRadius,
                CoolingFactor = parameters.CoolingFactor,
                Height = parameters.CanvasHeight,
                Width = parameters.CanvasWidth,
                InitialAdaptation = parameters.InitialAdaptation,
                MinAdaptation = parameters.MinAdaptation,
                RadiusConstantTime = parameters.RadiusConstantTime
            };

            var factory = new StandardLayoutAlgorithmFactory<TableNodeModel, Edge<TableNodeModel>, BidirectionalGraph<TableNodeModel, Edge<TableNodeModel>>>();
            var algorithm = factory.CreateAlgorithm("ISOM", context, parametersNew);
            await ComputeQueryNodeAlgorithm(algorithm);

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
