namespace JEO3.Site.Dashboard
{
    public static class WSNavGroupMap
    {
        // 0 Live, 1 Gallery, 2 ER Diagram, 3 Node Diagram, 4 Database Objects,
        // 5 Hotspots, 6 Distribution, 7 Density, 8 Charts, 9 Data,
        // 10 About, 11 Timeline, 12 Dashboard Builder
        private static readonly Dictionary<int, int> _tabToGroup = new()
        {
            [0] = 0,                              // Live
            [1] = 4,// Gallery
            [2] = 1,                    // Gallery
            [3] = 1,                     // ERD Diagrams
            [5] = 2,                    // Node Diagram
            [6] = 2,                    // Entity Types
            [7] = 2,                    // Hotspots
            [8] = 2,                    // Treemap
            [4] = 3,                    // Contour
            [9] = 3,                     // Browse Data
            [10] = 4,                   // About
            [11] = 4,                   // Timeline
            [12] = 1                    // Builder
        };

        public static int GroupFor(int tabIndex) => _tabToGroup.GetValueOrDefault(tabIndex, 0);
    }
}
