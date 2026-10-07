using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using QuikGraph;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {

        // FructermanReingoldBounded
        public static async Task ApplyGraphShapeLayoutFructermanReingoldBounded(BlazorDiagram diagram, FruchtermanReingoldParameters parameters)
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
            var parametersNew = new BoundedFRLayoutParameters
            {
                MaxIterations = parameters.MaxIterations,
                AttractionMultiplier = parameters.AttractionMultiplier,
                RepulsiveMultiplier = parameters.RepulsiveMultiplier,
                Height = parameters.CanvasHeight,
                Width = parameters.CanvasWidth,
                CoolingFunction = parameters.CoolingFunction == DiagramCoolingFunction.Exponential ? FRCoolingFunction.Exponential : FRCoolingFunction.Linear,
                Lambda = parameters.Lambda
            };

            // "CompoundFR" is the standard key for this algorithm
            var factory = new StandardLayoutAlgorithmFactory<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>>();
            var algorithm = factory.CreateAlgorithm("BoundedFR", context, parametersNew);
            await ComputeNodeAlgorithm(algorithm);

            // Map node coordinates and physical sizes into a GraphShape Rect layout dictionary
            var rectangles = algorithm.VerticesPositions.ToDictionary(
                kvp => (TableNodeModel)kvp.Key,
                kvp => new GraphShape.Rect(
                    kvp.Value.X,
                    kvp.Value.Y,
                    (int)(kvp.Key.Size?.Width ?? 100),
                    (int)(kvp.Key.Size?.Height ?? 40)
                )
            );

            //// 2. Pass the rectangles directly into the FSAAlgorithm constructor
            //var overlapParameters = new OverlapRemovalParameters { HorizontalGap = 30, VerticalGap = 30 };
            //var overlapAlgorithm = new FSAAlgorithm<QueryTableNodeModel>(rectangles, overlapParameters);

            //// 3. Execute the Force-directed Semantic Adjustment (FSA) pipeline
            //overlapAlgorithm.Compute();

            //// 4. Update positions on the Blazor canvas using the adjusted layout results
            //diagram.Batch(() =>
            //{
            //    foreach (var kvp in overlapAlgorithm.Rectangles)
            //    {
            //        QueryTableNodeModel uiComponentNode = kvp.Key;
            //        GraphShape.Rect safeBox = kvp.Value;

            //        uiComponentNode.SetPosition(safeBox.X, safeBox.Y);
            //    }
            //});

            // FLUSH COORDINATES TO INTERACTIVE BLAZOR CANVAS
            diagram.Batch(() =>
            {
                foreach (var kvp in algorithm.VerticesPositions)
                {
                    NodeModel uiComponentNode = kvp.Key;
                    GraphShape.Point calculatedCoord = kvp.Value;

                    uiComponentNode.SetPosition(calculatedCoord.X, calculatedCoord.Y);
                }
            });
        }
    }
}
