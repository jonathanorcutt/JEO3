namespace JEO3.Diagrams
{
    public enum DiagramType
    {
        ERD,
        Node
    }
    public enum GraphLayoutShape
    {
        FruchtermanReingold,
        KamadaKawai,
        EntityRelation,
        CircularStar,
        ISOM,
        LinLog,
        Random,
        FruchtermanReingoldBounded,
        Sugiyama,
        TreeBallon,
        TreeDouble,
        TreeSimple,
        None
    }
    public enum DiagramLayoutDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }

    public enum DiagramCoolingFunction
    {
        Exponential,
        Orthoganal
    }

    public enum DiagramEdgeRoutingFunction
    {
        Traditional,
        Orthoganal
    }

    public enum SpanTreeGeneration
    {
        //
        // Summary:
        //     BFS (Breadth-First Search).
        BFS,
        //
        // Summary:
        //     DFS (Depth-First Search).
        DFS
    }
}
