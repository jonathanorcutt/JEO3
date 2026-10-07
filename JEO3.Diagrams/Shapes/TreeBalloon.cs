using Blazor.Diagrams;
using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using QuikGraph;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {
        public static async Task ApplyGraphShapeLayoutTreeBalloon(BlazorDiagram diagram, NodeModel selectedVertex, TreeBalloonParameters parameters)
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

            // Map node sizes into the dictionary
            var sizes = diagram.Nodes.ToDictionary(
                node => node,
                node => new GraphShape.Size(node.Size?.Width ?? 100, node.Size?.Height ?? 40)
            );

            var parametersNew = new BalloonTreeLayoutParameters
            {
                MinRadius = parameters.MinRadius,
                Border = parameters.Border
            };

            // Graph, Positions, Root Node, and Parameters
            var algorithm = new GraphShape.Algorithms.Layout.BalloonTreeLayoutAlgorithm<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>>(
                quikGraph,
                positions,
                selectedVertex, // Argument 3: Must be the NodeModel root vertex
                parametersNew   // Argument 4: Layout parameters
            );


            // Await the task wrapping the synchronous GraphShape computation execution
            await Task.Run(() => algorithm.Compute());

            // MAP RESULTS INTO RECTANGLES USING CALCULATED POSITIONS
            var rectangles = algorithm.VerticesPositions.ToDictionary(
                kvp => kvp.Key,
                kvp => new GraphShape.Rect(
                    kvp.Value.X,
                    kvp.Value.Y,
                    kvp.Key.Size?.Width ?? 100,
                    kvp.Key.Size?.Height ?? 40
                )
            );
            //// PERFORM OVERLAP REMOVAL USING NODEMODEL CONSTRAINTS
            //var overlapParameters = new OverlapRemovalParameters { HorizontalGap = 40, VerticalGap = 40 };
            //var overlapAlgorithm = new FSAAlgorithm<NodeModel>(rectangles, overlapParameters);

            //// Await the overlap removal task
            //await Task.Run(() => overlapAlgorithm.Compute());
            //// SINGLE BATCH UPDATE - Commit structural mutations to the UI canvas concurrently
            //diagram.Batch(() =>
            //{
            //    foreach (var kvp in overlapAlgorithm.Rectangles)
            //    {
            //        // Set position directly using calculated coordinates
            //        kvp.Key.SetPosition(kvp.Value.X, kvp.Value.Y);
            //    }
            //});


            // MAP RAW BALLOON POSITIONS DIRECTLY
            diagram.Batch(() =>
            {
                foreach (var kvp in rectangles)
                {
                    kvp.Key.SetPosition(kvp.Value.X, kvp.Value.Y);
                }
            });
        }

    }
}
