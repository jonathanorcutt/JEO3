using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using GraphShape.Algorithms.Layout.Contextual;
using GraphShape.Algorithms.OverlapRemoval;
using QuikGraph;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {
        public static async Task ApplyGraphShapeLayoutTreeDouble(BlazorDiagram diagram, TreeDoubleParameters parameters)
        {
            var quikGraph = new BidirectionalGraph<NodeModel, Edge<NodeModel>>();

            foreach (var node in diagram.Nodes)
            {
                quikGraph.AddVertex(node);
            }

            foreach (var link in diagram.Links)
            {
                NodeModel? sourceNode = link.Source?.Model as NodeModel;
                NodeModel? targetNode = link.Target?.Model as NodeModel;

                if (sourceNode != null && targetNode != null)
                {
                    quikGraph.AddEdge(new Edge<NodeModel>(sourceNode, targetNode));
                }
            }

            // INITIALIZE BOUNDS MAPPING CONTEXT
            var positions = diagram.Nodes.ToDictionary(
                node => node,
                node => new GraphShape.Point(node.Position.X, node.Position.Y)
            );

            var sizes = diagram.Nodes.ToDictionary(
                node => node,
                node => new GraphShape.Size(node.Size?.Width ?? 100, node.Size?.Height ?? 40)
            );

            var context = new LayoutContext<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>>(
                quikGraph, positions, sizes, LayoutMode.Simple
            );

            var parametersNew = new DoubleTreeLayoutParameters
            {
                Direction = parameters.Direction == DiagramLayoutDirection.LeftToRight ? LayoutDirection.LeftToRight
                : parameters.Direction == DiagramLayoutDirection.RightToLeft ? LayoutDirection.RightToLeft
                : parameters.Direction == DiagramLayoutDirection.TopToBottom ? LayoutDirection.TopToBottom
                : parameters.Direction == DiagramLayoutDirection.BottomToTop ? LayoutDirection.BottomToTop : LayoutDirection.LeftToRight,
                LayerGap = parameters.LayerGap,
                VertexGap = parameters.VertexGap
            };

            var factory = new StandardLayoutAlgorithmFactory<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>>();
            var algorithm = factory.CreateAlgorithm("Tree", context, parametersNew);
            await ComputeNodeAlgorithm(algorithm);

            // Map results into Rectangles for the Overlap Remover
            var rectangles = algorithm.VerticesPositions.ToDictionary(
                kvp => (TableNodeModel)kvp.Key,
                kvp => new GraphShape.Rect(
                    kvp.Value.X,
                    kvp.Value.Y,
                    (int)(kvp.Key.Size?.Width ?? 100),
                    (int)(kvp.Key.Size?.Height ?? 40)
                )
            );

            // 2. Perform Overlap Removal
            var overlapParameters = new OverlapRemovalParameters { HorizontalGap = 30, VerticalGap = 30 };
            var overlapAlgorithm = new FSAAlgorithm<TableNodeModel>(rectangles, overlapParameters);
            await ComputeQueryNodeOverlapAlgorithm(overlapAlgorithm);

            // 3. SINGLE BATCH UPDATE - Apply the clean positions
            diagram.Batch(() =>
            {
                foreach (var kvp in overlapAlgorithm.Rectangles)
                {
                    // kvp.Key is the QueryTableNodeModel, kvp.Value is the adjusted Rect
                    kvp.Key.SetPosition(kvp.Value.X, kvp.Value.Y);
                }
            });
        }
    }
}
