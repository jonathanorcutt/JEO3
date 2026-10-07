using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using GraphShape.Algorithms.OverlapRemoval;
using QuikGraph;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {
        // FruchtermanReingold
        public static async Task ApplyGraphShapeLayoutFruchtermanReingold(BlazorDiagram diagram, FruchtermanReingoldParameters parameters)
        {
            if (!diagram.Nodes.Any()) return;

            // BUILD THE QUIKGRAPH RELATION MATRIX
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

            // THE FRUCHTERMAN-REINGOLD (SPRING FORCE) PARALLEL ENGINE
            // This instantly shifts the layout logic from a flat hierarchical tree to ideal organic web!
            //var parameters = new FreeFRLayoutParameters
            //{
            //    AttractionMultiplier = attractionMultiplier,  // Controls how tightly joined tables pull together
            //    RepulsiveMultiplier = repulsiveMultiplier,   // Controls how strongly unlinked nodes push away to avoid overlaps
            //    MaxIterations = maxIterations,                // Physics steps to let layout settle perfectly
            //    CoolingFunction = FRCoolingFunction.Exponential,
            //     IdealEdgeLength = 10,
            //      Lambda = 0.95
            //};

            var factory = new StandardLayoutAlgorithmFactory<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>>();

            var parametersNew = new FreeFRLayoutParameters()
            {
                AttractionMultiplier = parameters.AttractionMultiplier,
                CoolingFunction = parameters.CoolingFunction == DiagramCoolingFunction.Exponential ? FRCoolingFunction.Exponential : FRCoolingFunction.Linear,
                IdealEdgeLength = parameters.IdealEdgeLength,
                Lambda = parameters.Lambda,
                MaxIterations = parameters.MaxIterations,
                RepulsiveMultiplier = parameters.RepulsiveMultiplier
            };

            // Pass "FR" as the target algorithm string parameter matching the FRAlgorithm registration map
            var algorithm = factory.CreateAlgorithm("FR", context, parametersNew);
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

            // 2. Pass the rectangles directly into the FSAAlgorithm constructor
            var overlapParameters = new OverlapRemovalParameters { HorizontalGap = 30, VerticalGap = 30 };
            var overlapAlgorithm = new FSAAlgorithm<TableNodeModel>(rectangles, overlapParameters);

            // 3. Execute the Force-directed Semantic Adjustment (FSA) pipeline
            await ComputeQueryNodeOverlapAlgorithm(overlapAlgorithm);

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
