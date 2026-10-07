using Blazor.Diagrams;
using GraphShape.Algorithms.Layout;
using QuikGraph;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {
        public static async Task ApplyGraphShapeLayoutSugiyama(BlazorDiagram diagram, SugiyamaParameters parameters)
        {
            if (!diagram.Nodes.Any()) return;

            // CONVERT BLAZOR.DIAGRAM TO A QUIKGRAPH STRUCTURE
            var quikGraph = new BidirectionalGraph<TableNodeModel, Edge<TableNodeModel>>();

            foreach (var node in diagram.Nodes)
            {
                quikGraph.AddVertex((TableNodeModel)node);
            }

            foreach (var link in diagram.Links)
            {
                // The base Anchor class exposes .Model (which is the NodeModel under the hood)
                TableNodeModel? sourceNode = link.Source?.Model as TableNodeModel;
                TableNodeModel? targetNode = link.Target?.Model as TableNodeModel;

                // Only establish a QuikGraph edge if both sides resolve to true structural database nodes
                if (sourceNode != null && targetNode != null)
                {
                    quikGraph.AddEdge(new Edge<TableNodeModel>(sourceNode, targetNode));
                }
            }

            // 2. PREPARE THE LAYOUT CONTEXT FOR GRAPHSAPE
            var positions = diagram.Nodes.ToDictionary(
                node => (TableNodeModel)node,
                node => new GraphShape.Point(node.Position.X, node.Position.Y)
            );

            var sizes = diagram.Nodes.ToDictionary(
                node => (TableNodeModel)node,
                node => new GraphShape.Size(node.Size?.Width ?? 100, node.Size?.Height ?? 40)
            );

            var context = new LayoutContext<TableNodeModel, Edge<TableNodeModel>, BidirectionalGraph<TableNodeModel, Edge<TableNodeModel>>>(
                quikGraph,
                positions,
                sizes,
                LayoutMode.Simple
            );

            // 3. INSTANTIATE THE PARAMETERS AND RUN THE ALGORITHM DIRECTLY
            // Instantiating the typed parameters object directly bypassing the factory copy constraint
            var parametersNew = new SugiyamaLayoutParameters
            {
                Direction = parameters.Direction == DiagramLayoutDirection.LeftToRight ? LayoutDirection.LeftToRight
                    : parameters.Direction == DiagramLayoutDirection.RightToLeft ? LayoutDirection.RightToLeft
                    : parameters.Direction == DiagramLayoutDirection.TopToBottom ? LayoutDirection.TopToBottom
                    : parameters.Direction == DiagramLayoutDirection.BottomToTop ? LayoutDirection.BottomToTop : LayoutDirection.LeftToRight,
                // JEO3
                LayerGap = parameters.LayerGap,
                SliceGap = parameters.SliceGap,
                OptimizeWidth = parameters.OptimizeWidth,
                EdgeRouting = (parameters.EdgeRouting == DiagramEdgeRoutingFunction.Traditional) ? SugiyamaEdgeRouting.Traditional : SugiyamaEdgeRouting.Orthogonal,
                MinimizeEdgeLength = parameters.MinimizeEdgeLength,
                WidthPerHeight = parameters.WidthPerHeight,
                PositionMode = parameters.PositionMode
            };

            var factory = new StandardLayoutAlgorithmFactory<TableNodeModel, Edge<TableNodeModel>, BidirectionalGraph<TableNodeModel, Edge<TableNodeModel>>>();
            var algorithm = factory.CreateAlgorithm("Sugiyama", context, parametersNew);

            await ComputeQueryNodeAlgorithm(algorithm);

            // 4. UPDATE NODEMODEL POSITIONS BACK ON THE BLAZOR CANVAS
            diagram.Batch(() =>
            {
                foreach (var kvp in algorithm.VerticesPositions)
                {
                    TableNodeModel uiComponentNode = kvp.Key;
                    GraphShape.Point calculatedCoord = kvp.Value;

                    // SetPosition maps natively to Blazor.Diagrams 3.0 layout updates
                    uiComponentNode.SetPosition(calculatedCoord.X, calculatedCoord.Y);
                }
            });
        }
    }
}
