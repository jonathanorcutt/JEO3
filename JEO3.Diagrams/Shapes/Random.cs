using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using QuikGraph;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {
        public static async Task ApplyGraphShapeLayoutRandom(BlazorDiagram diagram, RandomParameters parameters)
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
            var parametersNew = new RandomLayoutParameters
            {
                Width = parameters.CanvasWidth,
                Height = parameters.CanvasHeight,
                XOffset = parameters.XOffset,
                YOffset = parameters.YOffset
            };

            var factory = new StandardLayoutAlgorithmFactory<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>>();
            var algorithm = factory.CreateAlgorithm("Random", context, parametersNew);
            await ComputeNodeAlgorithm(algorithm);

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
