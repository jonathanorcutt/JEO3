using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace JEO3.Diagrams
{
    public static partial class DiagramBuilder
    {
        public static void ApplyDatabaseGridLayout(BlazorDiagram diagram, double horizontalGap = 40, double verticalGap = 40, int columnsPerRow = 4)
        {
            if (!diagram.Nodes.Any()) return;

            var tableNodes = diagram.Nodes.OfType<TableNodeModel>().ToList();

            double currentX = 50;
            double currentY = 50;
            double rowMaxHeight = 0;
            int col = 0;

            diagram.Batch(() =>
            {
                foreach (var node in tableNodes)
                {
                    double width = node.Size?.Width ?? 300;
                    double height = node.Size?.Height ?? 60;

                    node.SetPosition(currentX, currentY);

                    currentX += width + horizontalGap;
                    rowMaxHeight = Math.Max(rowMaxHeight, height);
                    col++;

                    if (col >= columnsPerRow)
                    {
                        col = 0;
                        currentX = 50;
                        currentY += rowMaxHeight + verticalGap;
                        rowMaxHeight = 0;
                    }
                }
            });
        }

        public static void ApplyForceDirectedLayout(BlazorDiagram diagram, ERParameters parameters)
        {
            var nodes = diagram.Nodes.ToList();
            var links = diagram.Links.ToList();

            if (!nodes.Any()) return;

            // Initial State: Load positions into memory so we don't trigger UI renders
            var positions = nodes.ToDictionary(n => n, n => new Point(n.Position.X, n.Position.Y));
            var displacements = new Dictionary<NodeModel, Point>();

            // Tuning Parameters
            //double idealDistance = 450.0; // The ideal length of the "spring" between connected tables
            double kSquared = parameters.IdealDistance * parameters.IdealDistance;
            //double temperature = 400.0; // Max pixels a node can move per tick (cools down over time)
            var rand = new Random(42); // Seeded so the layout doesn't jitter randomly on every reload

            // Pre-scatter: If nodes are stacked at 0,0, push them out to prevent a physics explosion
            foreach (var node in nodes)
            {
                if (positions[node].X == 0 && positions[node].Y == 0)
                {
                    positions[node] = new Point(rand.NextDouble() * 1000, rand.NextDouble() * 1000);
                }
            }

            // Physics Loop
            for (int step = 0; step < parameters.Iterations; step++)
            {
                // Reset forces for this tick
                foreach (var node in nodes) displacements[node] = new Point(0, 0);

                // Repulsion: Every node pushes every other node away
                for (int i = 0; i < nodes.Count; i++)
                {
                    var v = nodes[i];
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        var u = nodes[j];
                        double dx = positions[v].X - positions[u].X;
                        double dy = positions[v].Y - positions[u].Y;

                        // Nudge apart if perfectly stacked
                        if (dx == 0 && dy == 0)
                        {
                            dx = (rand.NextDouble() - 0.5) * 5;
                            dy = (rand.NextDouble() - 0.5) * 5;
                        }

                        double distance = Math.Sqrt((dx * dx) + (dy * dy));

                        if (distance > 0)
                        {
                            // Bounding Box Penalty: Push 5x harder if the actual table widths are overlapping
                            double combinedWidths = ((v.Size?.Width ?? 200) + (u.Size?.Width ?? 200)) / 2.0;
                            double force = (distance < combinedWidths + 100)
                                ? kSquared / distance * 3
                                : (kSquared / distance);

                            double fx = dx / distance * force;
                            double fy = dy / distance * force;

                            displacements[v] = new Point(displacements[v].X + fx, displacements[v].Y + fy);
                            displacements[u] = new Point(displacements[u].X - fx, displacements[u].Y - fy);
                        }
                    }
                }

                // Attraction: Connected nodes pull each other together
                foreach (var link in links)
                {
                    if (link.Source?.Model is not NodeModel v || link.Target?.Model is not NodeModel u) continue;
                    if (!positions.ContainsKey(v) || !positions.ContainsKey(u)) continue;

                    double dx = positions[v].X - positions[u].X;
                    double dy = positions[v].Y - positions[u].Y;
                    double distance = Math.Sqrt((dx * dx) + (dy * dy));

                    if (distance > 0)
                    {
                        double force = distance * distance / parameters.IdealDistance;
                        double fx = dx / distance * force;
                        double fy = dy / distance * force;

                        displacements[v] = new Point(displacements[v].X - fx, displacements[v].Y - fy);
                        displacements[u] = new Point(displacements[u].X + fx, displacements[u].Y + fy);
                    }
                }

                // Apply Velocities & Cool Down
                foreach (var node in nodes)
                {
                    double dx = displacements[node].X;
                    double dy = displacements[node].Y;
                    double distance = Math.Sqrt((dx * dx) + (dy * dy));

                    if (distance > 0)
                    {
                        // Cap the movement speed by the current "temperature"
                        double moveX = dx / distance * Math.Min(distance, parameters.Temperature);
                        double moveY = dy / distance * Math.Min(distance, parameters.Temperature);

                        positions[node] = new Point(positions[node].X + moveX, positions[node].Y + moveY);
                    }
                }

                parameters.Temperature *= 0.95; // Cool down the simulation to let nodes settle
            }

            // Normalize Coordinates & Commit to Canvas
            // Find the furthest top/left nodes so we can shift the whole diagram into positive viewport space
            double minX = positions.Values.Min(p => p.X);
            double minY = positions.Values.Min(p => p.Y);

            diagram.Batch(() =>
            {
                foreach (var node in nodes)
                {
                    // Shift back so the top-left-most node sits neatly at (50, 50)
                    double finalX = positions[node].X - minX + 50;
                    double finalY = positions[node].Y - minY + 50;

                    node.SetPosition(finalX, finalY);
                }
            });
        }

        public static void ApplyForceDirectedLayout2(BlazorDiagram diagram, ERParameters parameters)
        {
            var nodes = diagram.Nodes.ToList();
            var links = diagram.Links.ToList();

            if (!nodes.Any()) return;

            var positions = nodes.ToDictionary(n => n, n => new Point(n.Position.X, n.Position.Y));
            var displacements = new Dictionary<NodeModel, Point>();

            double kSquared = parameters.IdealDistance * parameters.IdealDistance;
            var rand = new Random(42);

            // Pre-scatter: Initial spread
            foreach (var node in nodes)
            {
                if (positions[node].X == 0 && positions[node].Y == 0)
                {
                    positions[node] = new Point(rand.NextDouble() * 500, rand.NextDouble() * 500); // Tighter initial seed bounds
                }
            }

            // Physics Loop
            for (int step = 0; step < parameters.Iterations; step++)
            {
                foreach (var node in nodes) displacements[node] = new Point(0, 0);

                // 1. REPULSION: Make it fall off aggressively at a distance (Inverse Square Law)
                for (int i = 0; i < nodes.Count; i++)
                {
                    var v = nodes[i];
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        var u = nodes[j];
                        double dx = positions[v].X - positions[u].X;
                        double dy = positions[v].Y - positions[u].Y;

                        if (dx == 0 && dy == 0)
                        {
                            dx = (rand.NextDouble() - 0.5) * 5;
                            dy = (rand.NextDouble() - 0.5) * 5;
                        }

                        double distance = Math.Sqrt((dx * dx) + (dy * dy));

                        if (distance > 0)
                        {
                            double combinedWidths = ((v.Size?.Width ?? 200) + (u.Size?.Width ?? 200)) / 2.0;
                            double force = 0;

                            if (distance < combinedWidths + 60)
                            {
                                // Hard floor barrier: Still push aggressively if physical boundaries overlap
                                force = (kSquared / distance) * 3.5;
                            }
                            else
                            {
                                // 🌟 DROP OFF FORCE: Divide by distance cubed so distant nodes stop pushing each other away!
                                force = (kSquared * parameters.IdealDistance) / (distance * distance);
                            }

                            double fx = dx / distance * force;
                            double fy = dy / distance * force;

                            displacements[v] = new Point(displacements[v].X + fx, displacements[v].Y + fy);
                            displacements[u] = new Point(displacements[u].X - fx, displacements[u].Y - fy);
                        }
                    }
                }

                // 2. ATTRACTION: Amplify tension so links act like strong elastic bands pulling branches inward
                foreach (var link in links)
                {
                    if (link.Source?.Model is not NodeModel v || link.Target?.Model is not NodeModel u) continue;
                    if (!positions.ContainsKey(v) || !positions.ContainsKey(u)) continue;

                    double dx = positions[v].X - positions[u].X;
                    double dy = positions[v].Y - positions[u].Y;
                    double distance = Math.Sqrt((dx * dx) + (dy * dy));

                    if (distance > 0)
                    {
                        // 🌟 INCREASED TENSION: Cubed distance factor forces outer loose branches to snap back aggressively
                        double force = (distance * distance * distance) / (parameters.IdealDistance * parameters.IdealDistance);
                        double fx = dx / distance * force;
                        double fy = dy / distance * force;

                        displacements[v] = new Point(displacements[v].X - fx, displacements[v].Y - fy);
                        displacements[u] = new Point(displacements[u].X + fx, displacements[u].Y + fy);
                    }
                }

                // 3. GRAVITY (New): Pull every individual node gently toward the absolute workspace origin
                // This keeps detached or weak components from floating away into the empty margins
                double centerX = positions.Values.Average(p => p.X);
                double centerY = positions.Values.Average(p => p.Y);
                foreach (var node in nodes)
                {
                    double gdx = centerX - positions[node].X;
                    double gdy = centerY - positions[node].Y;
                    displacements[node] = new Point(displacements[node].X + gdx * 0.05, displacements[node].Y + gdy * 0.05);
                }

                // Apply Velocities
                foreach (var node in nodes)
                {
                    double dx = displacements[node].X;
                    double dy = displacements[node].Y;
                    double distance = Math.Sqrt((dx * dx) + (dy * dy));

                    if (distance > 0)
                    {
                        double moveX = dx / distance * Math.Min(distance, parameters.Temperature);
                        double moveY = dy / distance * Math.Min(distance, parameters.Temperature);

                        positions[node] = new Point(positions[node].X + moveX, positions[node].Y + moveY);
                    }
                }

                parameters.Temperature *= 0.95;
            }

            // Normalize Coordinates & Commit to Canvas
            double minX = positions.Values.Min(p => p.X);
            double minY = positions.Values.Min(p => p.Y);

            diagram.Batch(() =>
            {
                foreach (var node in nodes)
                {
                    double finalX = positions[node].X - minX + 50;
                    double finalY = positions[node].Y - minY + 50;
                    node.SetPosition(finalX, finalY);
                }
            });
        }
    }
}