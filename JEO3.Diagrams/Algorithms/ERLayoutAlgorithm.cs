using Blazor.Diagrams.Core.Models;
using GraphShape.Algorithms.Layout;
using QuikGraph;
using QuikGraph.Algorithms;

namespace JEO3.Diagrams
{
    // Revisit In Future - Deprecated
    public sealed class ERLayoutAlgorithm : ILayoutAlgorithm<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>>
    {
        #region Properties
        private readonly ERParameters _p;
        private readonly LayoutContext<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>> _context;
        private readonly HashSet<NodeModel> _visited = new HashSet<NodeModel>();

        public event ProgressChangedEventHandler ProgressChanged;
        public event LayoutIterationEndedEventHandler<NodeModel> IterationEnded;
        public event EventHandler StateChanged;
        public event EventHandler Started;
        public event EventHandler Finished;
        public event EventHandler Aborted;

        public IDictionary<NodeModel, GraphShape.Point> VerticesPositions { get; private set; }
        public BidirectionalGraph<NodeModel, Edge<NodeModel>> VisitedGraph => _context.Graph;
        public object SyncRoot { get; } = new object();
        public ComputationState State { get; private set; } = ComputationState.NotRunning;
        #endregion

        #region Initialization
        public ERLayoutAlgorithm(ERParameters parameters, LayoutContext<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>> context)
        {
            _p = parameters;
            _context = context;
            VerticesPositions = new Dictionary<NodeModel, GraphShape.Point>();
        }
        #endregion

        #region Compute
        public void Compute()
        {
            Started?.Invoke(this, EventArgs.Empty);
            State = ComputationState.Running;
            StateChanged?.Invoke(this, EventArgs.Empty);

            var graph = _context.Graph;
            var sizes = _context.Sizes;
            _visited.Clear();

            var incoming = new HashSet<NodeModel>(graph.Edges.Select(e => e.Target));
            var roots = graph.Vertices.Where(v => !incoming.Contains(v)).ToList();
            if (!roots.Any() && graph.Vertices.Any())
                roots.Add(graph.Vertices.First());

            double startX = 50;
            double startY = 50;

            foreach (var root in roots)
            {
                if (_visited.Contains(root)) continue;

                RenderSubtree(root, startX, startY, graph, sizes);

                if (_p.ForceHorizontal)
                    startX += sizes[root].Width + _p.ParentChildSpacing;
                else
                    startY += sizes[root].Height + _p.ParentChildSpacing;
            }

            foreach (var orphan in graph.Vertices.Where(v => !_visited.Contains(v)))
            {
                RenderSubtree(orphan, startX, startY, graph, sizes);
                startY += sizes[orphan].Height + _p.ParentChildSpacing;
            }

            State = ComputationState.Finished;
            StateChanged?.Invoke(this, EventArgs.Empty);
            Finished?.Invoke(this, EventArgs.Empty);
        }
        private double RenderSubtree(NodeModel node, double x, double y, BidirectionalGraph<NodeModel, Edge<NodeModel>> graph, IDictionary<NodeModel, GraphShape.Size> sizes)
        {
            if (!_visited.Add(node))
                return 0;

            VerticesPositions[node] = new GraphShape.Point(x, y);

            var children = graph.OutEdges(node).Select(e => e.Target).ToList();
            if (!children.Any())
                return sizes[node].Height;

            double maxHeight = 0;
            double totalWidth = 0;

            if (_p.ForceHorizontal)
            {
                double childX = x;
                double childY = y + sizes[node].Height + _p.ParentChildSpacing;

                foreach (var child in children)
                {
                    if (_visited.Contains(child)) continue;
                    double subtreeHeight = RenderSubtree(child, childX, childY, graph, sizes);
                    maxHeight = Math.Max(maxHeight, subtreeHeight);

                    childX += sizes[child].Width + _p.SiblingSpacing;
                    totalWidth += sizes[child].Width + _p.SiblingSpacing;
                }
            }
            else if (_p.ForceVertical)
            {
                double childX = x + sizes[node].Width + _p.ParentChildSpacing;
                double childY = y;

                foreach (var child in children)
                {
                    if (_visited.Contains(child)) continue;
                    double subtreeHeight = RenderSubtree(child, childX, childY, graph, sizes);
                    childY += subtreeHeight + _p.SiblingSpacing;
                }
            }
            else
            {
                double childX = x;
                double childY = y + sizes[node].Height + _p.ParentChildSpacing;

                foreach (var child in children)
                {
                    if (_visited.Contains(child)) continue;
                    double subtreeHeight = RenderSubtree(child, childX, childY, graph, sizes);
                    maxHeight = Math.Max(maxHeight, subtreeHeight);

                    childX += sizes[child].Width + _p.SiblingSpacing;
                    totalWidth += sizes[child].Width + _p.SiblingSpacing;
                }

                if (_p.Alignment == 1)
                {
                    double centerOffset = (totalWidth / 2) - (sizes[node].Width / 2);
                    VerticesPositions[node] = new GraphShape.Point(x + centerOffset, y);
                }
                else if (_p.Alignment == 2)
                {
                    double rightOffset = totalWidth - sizes[node].Width;
                    VerticesPositions[node] = new GraphShape.Point(x + rightOffset, y);
                }
            }

            return sizes[node].Height + _p.ParentChildSpacing + maxHeight;
        }
        public object GetVertexInfo(NodeModel vertex) => null;
        public object GetEdgeInfo(Edge<NodeModel> edge) => null;
        public void Abort()
        {
            State = ComputationState.Aborted;
            StateChanged?.Invoke(this, EventArgs.Empty);
            Aborted?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}