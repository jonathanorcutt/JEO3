using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Options;
using GraphShape.Algorithms.Layout;
using GraphShape.Algorithms.OverlapRemoval;
using JEO3.Core;
using JEO3.Generation.Models;
using QuikGraph;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {
        #region Crayons

        // NOTE: Would Like To See Parent Node Coloring Different From Child Node Coloring At Depth N
        // E.g. If Starting With "Person.Person" Table, "BusinessEntity" Table Color Different Than "Person.PersonPhone" Table Color
        public static readonly List<string> _randomColors =
        [
            ColorHex.Black,
            ColorHex.Purple,
            ColorHex.Red,
            ColorHex.MatrixGlow,
            ColorHex.CornflowerBlue,
            ColorHex.Blurple,
            ColorHex.ToxicGreen,
            ColorHex.VividViolet,
            ColorHex.TigerOrange,
            ColorHex.AeroBlue,
            ColorHex.BubblegumPink,
            ColorHex.ClassicMustard,
            ColorHex.IntenseBlue,
            ColorHex.LightBlurple,
            ColorHex.LightDodgerBlue
        ];

        #endregion

        #region Precursor - Get Execution Nodes

        public static List<QueryTable> GetFinalExecutionNodes(this QueryGenerationResult result)
        {
            var allNodes = result.ExecutionNodes;
            if (allNodes == null || !allNodes.Any()) return [];

            var activeGraph = BuildActiveGraph(result.Tracker);
            var sourceNodeIdsWithChildren = activeGraph.Edges.Select(edge => edge.SourceNodeId).ToHashSet();
            var terminalLeafNodes = activeGraph.Nodes.Where(node => !sourceNodeIdsWithChildren.Contains(node.NodeId)).ToList();
            var activePathNodeIds = new HashSet<Guid>();

            foreach (var leaf in terminalLeafNodes)
            {
                var current = leaf;
                activePathNodeIds.Add(current.NodeId);

                while (true)
                {
                    var incomingEdge = activeGraph.Edges.FirstOrDefault(edge => edge.TargetNodeId == current.NodeId);
                    if (incomingEdge == null) break;

                    var parentNode = activeGraph.Nodes.FirstOrDefault(node => node.NodeId == incomingEdge.SourceNodeId);
                    if (parentNode == null) break;

                    activePathNodeIds.Add(parentNode.NodeId);
                    current = parentNode;
                }
            }

            return allNodes.Where(node => activePathNodeIds.Contains(node.Id)).ToList();
        }

        /// <summary>
        /// Deconstructs the tracked traversal into a structural layout for the diagram,
        /// ensuring only the actual evaluated execution nodes and paths are materialized.
        /// </summary>
        public static DiagramGraphModel BuildActiveGraph(IQueryTracker tracker)
        {
            var graph = new DiagramGraphModel();

            if (tracker?.Root == null || !tracker.Nodes.Any())
                return graph;

            // Project execution nodes into clear visual elements
            foreach (var node in tracker.Nodes)
            {
                graph.Nodes.Add(new DiagramNodeViewModel
                {
                    NodeId = node.Id,
                    TableName = node.Table.Name,
                    Alias = node.Alias,
                    Depth = node.Depth,
                    SqlSafeName = node.Table.SqlSafeName
                });

                // 2. If the node has a parent, materialize the execution line (Edge) connecting them
                if (node.Parent != null && node.Relationship != null)
                {
                    graph.Edges.Add(new DiagramEdgeViewModel
                    {
                        SourceNodeId = node.Parent.Id,
                        TargetNodeId = node.Id,
                        RelationshipName = node.Relationship.KeyName,
                        // Heuristic metadata trick: check if the key relationship is nullable 
                        JoinType = node.Relationship.IsNullable ? "LEFT JOIN" : "INNER JOIN"
                    });
                }
            }

            return graph;
        }

        #endregion

        #region Create Diagram

        public static BlazorDiagram CreateDiagramFromQueryTables(IReadOnlyList<QueryTable> queryTables, bool lightTheme = false)
        {
            try
            {
                var options = new BlazorDiagramOptions
                {
                    AllowMultiSelection = true,
                    Virtualization = { Enabled = false },
                    Zoom = { Enabled = true },
                    AllowPanning = true,
                    GridSnapToCenter = true,
                    GridSize = 5
                };

                var diagram = new BlazorDiagram(options);

                if (queryTables == null || !queryTables.Any())
                    return diagram;

                // --- NEW: ANCESTRAL LEAF TRIMMER ---
                // Find all nodes that act as a parent (so we know who the terminal leaves are)
                // Map all tables that act as children in the current query subset
                var parentAliases = queryTables.Where(q => q.Parent != null).Select(q => q.Parent.Alias).ToHashSet();

                // Track which table types we have already rendered as terminal leaves
                var renderedLeafTypes = new HashSet<string>();
                var nodesToSkip = new HashSet<string>();

                foreach (var qTable in queryTables)
                {
                    // Only consider nodes that have no children (terminal leaves)
                    if (parentAliases.Contains(qTable.Alias)) continue;

                    // CHECK Is this a duplicate instance of an ancestor? (Existing logic)
                    var currentAncestor = qTable.Parent;
                    bool isDuplicateAncestor = false;
                    while (currentAncestor != null)
                    {
                        if (currentAncestor.Table?.Name == qTable.Table?.Name)
                        {
                            isDuplicateAncestor = true;
                            break;
                        }
                        currentAncestor = currentAncestor.Parent;
                    }

                    // CHECK 2: Is this a duplicate of another leaf type we already rendered?
                    // This collapses the "ProductVendor" instances across different branches
                    bool isDuplicateLeafType = renderedLeafTypes.Contains(qTable.Table?.Name);

                    if (isDuplicateAncestor || isDuplicateLeafType)
                    {
                        nodesToSkip.Add(qTable.Alias);
                    }
                    else
                    {
                        // Mark this type as "taken" so future ProductVendor instances get trimmed
                        renderedLeafTypes.Add(qTable.Table?.Name);
                    }
                }
                // ------------------------------------

                var nodeMapping = new Dictionary<string, TableNodeModel>();
                var depthCounters = new Dictionary<int, int>();
                Random random = new Random();
                var isRootNode = true;

                // First Pass: Layout Nodes
                foreach (var qTable in queryTables)
                {
                    // OMIT TRIMMED LEAVES
                    if (nodesToSkip.Contains(qTable.Alias)) continue;
                    if (nodeMapping.ContainsKey(qTable.Alias)) continue;

                    if (!depthCounters.ContainsKey(qTable.Depth))
                        depthCounters[qTable.Depth] = 0;

                    double xPos = 50 + (depthCounters[qTable.Depth] * 50);
                    double yPos = 50 + (qTable.Depth * 50);
                    depthCounters[qTable.Depth]++;

                    var node = new TableNodeModel(new Point(xPos, yPos), qTable.Table)
                    {
                        Title = qTable.Table.Name,
                        IsRootNode = isRootNode
                    };

                    if (isRootNode) isRootNode = false;

                    var lst = lightTheme == false ? _randomColors : _randomColors;
                    string nodeBg = qTable.Depth < lst.Count ? lst[qTable.Depth] : "white";
                    string textColor = lightTheme == false ? "rgba(215,215,215,1)" : "white";

                    node.BackgroundColor = nodeBg;
                    node.TextColor = textColor;
                    node.Depth = qTable.Depth;
                    node.Padding = 1;
                    node.MinWidth = 50;
                    node.FontSize = 6;

                    node.AddPort(PortAlignment.Top);
                    node.AddPort(PortAlignment.Bottom);

                    diagram.Nodes.Add(node);
                    nodeMapping[qTable.Alias] = node;
                }

                var processedEdges = new HashSet<string>();

                // Second Pass: Node-to-Node Links & Key Labels (RESTORED TO ORIGINAL)
                foreach (var qTable in queryTables)
                {
                    // Skip links where either end was trimmed
                    if (qTable.Parent == null || nodesToSkip.Contains(qTable.Alias) || nodesToSkip.Contains(qTable.Parent.Alias))
                        continue;

                    if (!nodeMapping.ContainsKey(qTable.Alias))
                        continue;

                    string fkName = qTable.Relationship?.KeyName ?? $"{qTable.Parent.Alias}->{qTable.Alias}";

                    if (processedEdges.Contains(fkName))
                        continue;

                    processedEdges.Add(fkName);

                    string parentAlias = qTable.Parent.Alias;
                    if (!nodeMapping.ContainsKey(parentAlias)) continue;

                    var parentNode = nodeMapping[parentAlias];
                    var childNode = nodeMapping[qTable.Alias];

                    // RESTORED: Direct Node-to-Node linking
                    var link = new LinkModel(parentNode, childNode)
                    {
                        Color = lightTheme == false ? "rgba(100,255,100,.9)" : "rgba(0, 0, 0, .9)"
                    };

                    string labelText = string.Join(" & ", qTable.Relationship?.ColumnPairs.Select(c => c.ParentColumn?.Name) ?? new[] { "Join" });
                    var label = new LinkLabelModel(link, labelText);
                    link.Labels.Add(label);

                    diagram.Links.Add(link);
                }

                // Third Pass - Tracking Visual Lineage & Populating Child Collections
                foreach (var link in diagram.Links)
                {
                    if (link.Source?.Model is TableNodeModel sourceNode &&
                        link.Target?.Model is TableNodeModel targetNode)
                    {
                        if (!sourceNode.ChildNodes.Contains(targetNode))
                        {
                            sourceNode.ChildNodes.Add(targetNode);
                        }
                    }
                }

                // --- High-Performance Affinity Stitching & Cleanup Pass ---
                var connectionCounts = new Dictionary<NodeModel, int>();
                foreach (var link in diagram.Links)
                {
                    if (link.Source.Model is NodeModel src)
                        connectionCounts[src] = connectionCounts.GetValueOrDefault(src) + 1;
                    if (link.Target.Model is NodeModel tgt)
                        connectionCounts[tgt] = connectionCounts.GetValueOrDefault(tgt) + 1;
                }

                var visualOrphans = diagram.Nodes
                    .Where(n => !connectionCounts.ContainsKey(n))
                    .Cast<TableNodeModel>()
                    .ToList();

                var nodesToPurge = new List<NodeModel>();

                foreach (var orphan in visualOrphans)
                {
                    var orphanData = queryTables.FirstOrDefault(q => q.Alias == orphan.Title || q.Table.Name == orphan.Title);

                    if (orphanData?.Relationship == null)
                    {
                        nodesToPurge.Add(orphan);
                        continue;
                    }

                    var candidateTargets = diagram.Nodes
                        .Cast<TableNodeModel>()
                        .Where(n => n != orphan &&
                                   (n.Title == orphanData.Relationship.ParentTable.Name ||
                                    n.Title == orphanData.Relationship.ReferencedTable.Name))
                        .ToList();

                    if (candidateTargets.Any())
                    {
                        var bestTarget = candidateTargets
                            .OrderByDescending(target => connectionCounts.GetValueOrDefault(target, 0))
                            .First();

                        // RESTORED: Direct Node-to-Node Stitching
                        var stitchLink = new LinkModel(bestTarget, orphan)
                        {
                            Color = lightTheme == false ? "rgba(100,255,100,.9)" : "rgba(128, 128, 128, 0.9)"
                        };

                        string labelText = $"{orphanData.Relationship.ColumnPairs.FirstOrDefault()?.ParentColumn.Name ?? "ID"}";
                        stitchLink.Labels.Add(new LinkLabelModel(stitchLink, labelText));

                        diagram.Links.Add(stitchLink);

                        connectionCounts[bestTarget] = connectionCounts.GetValueOrDefault(bestTarget, 0) + 1;
                        connectionCounts[orphan] = 1;
                    }
                    else
                    {
                        nodesToPurge.Add(orphan);
                    }
                }

                foreach (var deadNode in nodesToPurge)
                {
                    diagram.Nodes.Remove(deadNode);
                }


                return diagram;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static BlazorDiagram CreateDiagramFromQueryTablesFull(IReadOnlyList<QueryTable> queryTables, bool lightTheme = false)
        {
            try
            {
                var options = new BlazorDiagramOptions
                {
                    AllowMultiSelection = true,
                    Virtualization = { Enabled = false },
                    Zoom = { Enabled = true },
                    AllowPanning = true,
                    GridSnapToCenter = true,
                    GridSize = 5
                };

                var diagram = new BlazorDiagram(options);

                if (queryTables == null || !queryTables.Any())
                    return diagram;

                var parentAliases = queryTables.Where(q => q.Parent != null).Select(q => q.Parent.Alias).ToHashSet();
                var renderedLeafTypes = new HashSet<string>();
                var nodesToSkip = new HashSet<string>();

                foreach (var qTable in queryTables)
                {
                    if (parentAliases.Contains(qTable.Alias)) continue;

                    var currentAncestor = qTable.Parent;
                    bool isDuplicateAncestor = false;
                    while (currentAncestor != null)
                    {
                        if (currentAncestor.Table?.Name == qTable.Table?.Name)
                        {
                            isDuplicateAncestor = true;
                            break;
                        }
                        currentAncestor = currentAncestor.Parent;
                    }

                    bool isDuplicateLeafType = renderedLeafTypes.Contains(qTable.Table?.Name);

                    if (isDuplicateAncestor || isDuplicateLeafType)
                    {
                        nodesToSkip.Add(qTable.Alias);
                    }
                    else
                    {
                        renderedLeafTypes.Add(qTable.Table?.Name);
                    }
                }

                var nodeMapping = new Dictionary<string, TableNodeModel>();
                var depthCounters = new Dictionary<int, int>();
                var isRootNode = true;

                foreach (var qTable in queryTables)
                {
                    if (nodesToSkip.Contains(qTable.Alias)) continue;
                    if (nodeMapping.ContainsKey(qTable.Alias)) continue;

                    if (!depthCounters.ContainsKey(qTable.Depth))
                        depthCounters[qTable.Depth] = 0;

                    double xPos = 30 + (depthCounters[qTable.Depth] * 30);
                    double yPos = 30 + (qTable.Depth * 30);
                    depthCounters[qTable.Depth]++;

                    var node = new TableNodeModel(new Point(xPos, yPos), qTable.Table)
                    {
                        Title = qTable.Table.Name,
                        IsRootNode = isRootNode
                    };

                    // --- ONLY NEW LINE vs the original method ---
                    node.PopulateColumnInfo(qTable.Table);
                    // ----------------------------------------------

                    if (isRootNode) isRootNode = false;

                    node.BackgroundColor = "#00000";
                    node.TextColor = "#fffff";
                    node.Depth = qTable.Depth;
                    node.Padding = 2;

                    diagram.Nodes.Add(node);
                    nodeMapping[qTable.Alias] = node;
                }

                var processedEdges = new HashSet<string>();

                foreach (var qTable in queryTables)
                {
                    if (qTable.Parent == null || nodesToSkip.Contains(qTable.Alias) || nodesToSkip.Contains(qTable.Parent.Alias))
                        continue;

                    if (!nodeMapping.ContainsKey(qTable.Alias))
                        continue;

                    string fkName = qTable.Relationship?.KeyName ?? $"{qTable.Parent.Alias}->{qTable.Alias}";
                    if (processedEdges.Contains(fkName)) continue;
                    processedEdges.Add(fkName);

                    string parentAlias = qTable.Parent.Alias;
                    if (!nodeMapping.ContainsKey(parentAlias)) continue;

                    var parentNode = nodeMapping[parentAlias];
                    var childNode = nodeMapping[qTable.Alias];

                    var link = new LinkModel(parentNode, childNode)
                    {
                        Color = "rgba(100,255,100,.9)"
                    };

                    string labelText = string.Join(" & ", qTable.Relationship?.ColumnPairs.Select(c => c.ParentColumn?.Name) ?? new[] { "Join" });
                    link.Labels.Add(new LinkLabelModel(link, labelText));

                    diagram.Links.Add(link);
                }

                foreach (var link in diagram.Links)
                {
                    if (link.Source?.Model is TableNodeModel sourceNode &&
                        link.Target?.Model is TableNodeModel targetNode)
                    {
                        if (!sourceNode.ChildNodes.Contains(targetNode))
                            sourceNode.ChildNodes.Add(targetNode);
                    }
                }

                // --- High-Performance Affinity Stitching & Cleanup Pass ---
                var connectionCounts = new Dictionary<NodeModel, int>();
                foreach (var link in diagram.Links)
                {
                    if (link.Source.Model is NodeModel src)
                        connectionCounts[src] = connectionCounts.GetValueOrDefault(src) + 1;
                    if (link.Target.Model is NodeModel tgt)
                        connectionCounts[tgt] = connectionCounts.GetValueOrDefault(tgt) + 1;
                }

                var visualOrphans = diagram.Nodes
                    .Where(n => !connectionCounts.ContainsKey(n))
                    .Cast<TableNodeModel>()
                    .ToList();

                var nodesToPurge = new List<NodeModel>();

                foreach (var orphan in visualOrphans)
                {
                    var orphanData = queryTables.FirstOrDefault(q => q.Alias == orphan.Title || q.Table.Name == orphan.Title);

                    if (orphanData?.Relationship == null)
                    {
                        nodesToPurge.Add(orphan);
                        continue;
                    }

                    var candidateTargets = diagram.Nodes
                        .Cast<TableNodeModel>()
                        .Where(n => n != orphan &&
                                   (n.Title == orphanData.Relationship.ParentTable.Name ||
                                    n.Title == orphanData.Relationship.ReferencedTable.Name))
                        .ToList();

                    if (candidateTargets.Any())
                    {
                        var bestTarget = candidateTargets
                            .OrderByDescending(target => connectionCounts.GetValueOrDefault(target, 0))
                            .First();

                        // RESTORED: Direct Node-to-Node Stitching
                        var stitchLink = new LinkModel(bestTarget, orphan)
                        {
                            Color = lightTheme == false ? "rgba(100,255,100,.9)" : "rgba(128, 128, 128, 0.9)"
                        };

                        string labelText = $"{orphanData.Relationship.ColumnPairs.FirstOrDefault()?.ParentColumn.Name ?? "ID"}";
                        stitchLink.Labels.Add(new LinkLabelModel(stitchLink, labelText));

                        diagram.Links.Add(stitchLink);

                        connectionCounts[bestTarget] = connectionCounts.GetValueOrDefault(bestTarget, 0) + 1;
                        connectionCounts[orphan] = 1;
                    }
                    else
                    {
                        nodesToPurge.Add(orphan);
                    }
                }

                foreach (var deadNode in nodesToPurge)
                {
                    diagram.Nodes.Remove(deadNode);
                }

                return diagram;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Apply Zoom / Pan

        public static void SetViewport(BlazorDiagram diagram, double? targetZoom = null, double? targetPanX = 200, double? targetPanY = 200)
        {
            // Resolve Zoom State (use target if provided, fallback to current diagram zoom)
            double finalZoom = diagram.Zoom;
            bool isZoomChanging = targetZoom.HasValue;

            if (isZoomChanging)
            {
                finalZoom = Math.Clamp(targetZoom.Value, 0.1, 2.0);
            }

            // Resolve Pan State (use target if provided, fallback to current diagram pan)
            double finalPanX = targetPanX ?? diagram.Pan.X;
            double finalPanY = targetPanY ?? diagram.Pan.Y;

            // Process Contextual Zoom-Pan Math if Zoom is actually altering
            var bounds = diagram.Container;
            if (isZoomChanging && bounds != null)
            {
                var currentZoom = diagram.Zoom;
                var centerX = bounds.Width / 2.0;
                var centerY = bounds.Height / 2.0;

                // Apply matrix calculation on top of whichever base pan was requested
                finalPanX = centerX - ((centerX - finalPanX) * (finalZoom / currentZoom));
                finalPanY = centerY - ((centerY - finalPanY) * (finalZoom / currentZoom));
            }

            // Batch update state changes atomically to prevent rendering jitter
            if (finalPanX != diagram.Pan.X || finalPanY != diagram.Pan.Y)
            {
                diagram.SetPan(finalPanX, finalPanY);
            }

            if (isZoomChanging)
            {
                diagram.SetZoom(finalZoom);
            }
        }

        #endregion

        #region Apply Layout Shape To Diagram

        private static async Task ComputeNodeAlgorithm(ILayoutAlgorithm<NodeModel, Edge<NodeModel>, BidirectionalGraph<NodeModel, Edge<NodeModel>>> algorithm)
        {
            try
            {
                Task task = Task.Run(() =>
                {
                    algorithm.Compute();
                });

                await Task.WhenAll(task);
            }
            catch (Exception ex)
            {
                await Task.FromException(ex);
            }
        }
        private static async Task ComputeQueryNodeAlgorithm(ILayoutAlgorithm<TableNodeModel, Edge<TableNodeModel>, BidirectionalGraph<TableNodeModel, Edge<TableNodeModel>>> algorithm)
        {
            Task task = Task.Run(() =>
            {
                algorithm.Compute();
            });

            await Task.WhenAll(task);
        }
        private static async Task ComputeNodeOverlapAlgorithm(FSAAlgorithm<NodeModel> algorithm)
        {
            Task task = Task.Run(() =>
            {
                algorithm.Compute();
            });

            await Task.WhenAll(task);
        }
        private static async Task ComputeQueryNodeOverlapAlgorithm(FSAAlgorithm<TableNodeModel> algorithm)
        {
            Task task = Task.Run(() =>
            {
                algorithm.Compute();
            });

            await Task.WhenAll(task);
        }

        #endregion

        #region Apply Grouping

        public static void HandleClusterSelection(BlazorDiagram diagram, NodeModel draggedNode)
        {
            if (draggedNode == null || !diagram.Links.Any()) return;

            // Use fantastic BFS link walker to find all child targets downstream
            var descendantDepths = GetDescendantDepthsDirectly(draggedNode, diagram.Links);

            if (!descendantDepths.Any()) return;

            // Open a single transactional render batch on the canvas
            diagram.Batch(() =>
            {
                foreach (var childNode in descendantDepths.Keys)
                {
                    // Set unselectOthers to false! This tells the library to append 
                    // the children to the selection matrix instead of wiping out the parent.
                    diagram.SelectModel(childNode, unselectOthers: false);
                }
            });
        }

        private static Dictionary<NodeModel, int> GetDescendantDepthsDirectly(NodeModel root, IEnumerable<BaseLinkModel> allLinks)
        {
            var visited = new Dictionary<NodeModel, int>();
            var queue = new Queue<(NodeModel Node, int Depth)>();

            // Find all initial outgoing links where our root is the Source
            var outgoingLinks = allLinks.Where(l => l.Source?.Model == root);
            foreach (var link in outgoingLinks)
            {
                if (link.Target?.Model is NodeModel targetNode)
                {
                    queue.Enqueue((targetNode, 1));
                }
            }

            // Standard BFS loop through the canvas link hierarchy
            while (queue.Count > 0)
            {
                var (currentNode, currentDepth) = queue.Dequeue();

                // Track the deepest path found to each node to prevent premature termination on complex routes
                if (!visited.ContainsKey(currentNode) || visited[currentNode] < currentDepth)
                {
                    visited[currentNode] = currentDepth;

                    // Query links where the current child is acting as the parent source
                    var nextLinks = allLinks.Where(l => l.Source?.Model == currentNode);
                    foreach (var link in nextLinks)
                    {
                        if (link.Target?.Model is NodeModel nextTarget)
                        {
                            queue.Enqueue((nextTarget, currentDepth + 1));
                        }
                    }
                }
            }

            return visited;
        }

        #endregion
    }
}
