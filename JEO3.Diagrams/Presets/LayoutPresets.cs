namespace JEO3.Diagrams
{
    public enum LayoutPresetType
    {
        JEO,
        BalancedDefault,
        StrongCouplingClustered,
        LooseSprawlingExploration,
        HighDensityCompact
    }

    public static class LayoutPresets
    {
        private static readonly Dictionary<(GraphLayoutShape, LayoutPresetType), BaseLayoutParameters> Registry = [];

        static LayoutPresets()
        {
            // --- FRUCHTERMAN REINGOLD PRESETS ---
            Registry[(GraphLayoutShape.FruchtermanReingold, LayoutPresetType.JEO)] = new FruchtermanReingoldParameters { AttractionMultiplier = .7, RepulsiveMultiplier = 5.5, MaxIterations = 200 };
            Registry[(GraphLayoutShape.FruchtermanReingold, LayoutPresetType.BalancedDefault)] = new FruchtermanReingoldParameters { AttractionMultiplier = 1.0, RepulsiveMultiplier = 5, MaxIterations = 100 };
            Registry[(GraphLayoutShape.FruchtermanReingold, LayoutPresetType.StrongCouplingClustered)] = new FruchtermanReingoldParameters { AttractionMultiplier = 2.5, RepulsiveMultiplier = 0.8, MaxIterations = 150 };
            Registry[(GraphLayoutShape.FruchtermanReingold, LayoutPresetType.LooseSprawlingExploration)] = new FruchtermanReingoldParameters { AttractionMultiplier = 0.4, RepulsiveMultiplier = 4.5, MaxIterations = 120 };
            Registry[(GraphLayoutShape.FruchtermanReingold, LayoutPresetType.HighDensityCompact)] = new FruchtermanReingoldParameters { AttractionMultiplier = 1.8, RepulsiveMultiplier = 2.0, MaxIterations = 200 };

            // --- BOUNDED FR PRESETS ---
            Registry[(GraphLayoutShape.FruchtermanReingoldBounded, LayoutPresetType.JEO)] = new FruchtermanReingoldParameters { AttractionMultiplier = 1.0, RepulsiveMultiplier = 5, MaxIterations = 200 };
            Registry[(GraphLayoutShape.FruchtermanReingoldBounded, LayoutPresetType.BalancedDefault)] = new FruchtermanReingoldParameters { AttractionMultiplier = 1.0, RepulsiveMultiplier = 1.2, MaxIterations = 100 };
            Registry[(GraphLayoutShape.FruchtermanReingoldBounded, LayoutPresetType.StrongCouplingClustered)] = new FruchtermanReingoldParameters { AttractionMultiplier = 3.0, RepulsiveMultiplier = 0.5, MaxIterations = 120 };
            Registry[(GraphLayoutShape.FruchtermanReingoldBounded, LayoutPresetType.LooseSprawlingExploration)] = new FruchtermanReingoldParameters { AttractionMultiplier = 0.5, RepulsiveMultiplier = 3.5, MaxIterations = 100 };
            Registry[(GraphLayoutShape.FruchtermanReingoldBounded, LayoutPresetType.HighDensityCompact)] = new FruchtermanReingoldParameters { AttractionMultiplier = 1.5, RepulsiveMultiplier = 2.5, MaxIterations = 180 };

            // --- LINLOG PRESETS (Excellent for finding sub-system database island contexts) ---
            Registry[(GraphLayoutShape.LinLog, LayoutPresetType.JEO)] = new LinLogParameters { AttractionExponent = 1.0, RepulsiveExponent = 0.0, GravitationMultiplier = 0.1 };
            Registry[(GraphLayoutShape.LinLog, LayoutPresetType.BalancedDefault)] = new LinLogParameters { AttractionExponent = 1.0, RepulsiveExponent = 0.0, GravitationMultiplier = 0.1 };
            Registry[(GraphLayoutShape.LinLog, LayoutPresetType.StrongCouplingClustered)] = new LinLogParameters { AttractionExponent = 2.0, RepulsiveExponent = -1.0, GravitationMultiplier = 0.3 };
            Registry[(GraphLayoutShape.LinLog, LayoutPresetType.LooseSprawlingExploration)] = new LinLogParameters { AttractionExponent = 0.5, RepulsiveExponent = 1.5, GravitationMultiplier = 0.02 };
            Registry[(GraphLayoutShape.LinLog, LayoutPresetType.HighDensityCompact)] = new LinLogParameters { AttractionExponent = 1.2, RepulsiveExponent = 0.5, GravitationMultiplier = 0.5 };

            // --- SUGIYAMA PRESETS ---
            Registry[(GraphLayoutShape.Sugiyama, LayoutPresetType.JEO)] = new SugiyamaParameters { LayerGap = 30, SliceGap = 30 };
            Registry[(GraphLayoutShape.Sugiyama, LayoutPresetType.BalancedDefault)] = new SugiyamaParameters { LayerGap = 50, SliceGap = 30 };
            Registry[(GraphLayoutShape.Sugiyama, LayoutPresetType.StrongCouplingClustered)] = new SugiyamaParameters { LayerGap = 25, SliceGap = 15 };
            Registry[(GraphLayoutShape.Sugiyama, LayoutPresetType.LooseSprawlingExploration)] = new SugiyamaParameters { LayerGap = 120, SliceGap = 80 };
            Registry[(GraphLayoutShape.Sugiyama, LayoutPresetType.HighDensityCompact)] = new SugiyamaParameters { LayerGap = 35, SliceGap = 20, MinimizeEdgeLength = true };

            // --- KAMADA KAWAI PRESETS ---
            Registry[(GraphLayoutShape.KamadaKawai, LayoutPresetType.JEO)] = new KkParameters { DisconnectedMultiplier = .5, K = 1, LengthFactor = 0.5, MaxIterations = 500, ExchangeVertices = false };
            Registry[(GraphLayoutShape.KamadaKawai, LayoutPresetType.BalancedDefault)] = new KkParameters { DisconnectedMultiplier = .7, K = 1.0, LengthFactor = 1.0, MaxIterations = 750 };
            Registry[(GraphLayoutShape.KamadaKawai, LayoutPresetType.StrongCouplingClustered)] = new KkParameters { DisconnectedMultiplier = .7, K = 2.5, LengthFactor = 0.4, MaxIterations = 850 };
            Registry[(GraphLayoutShape.KamadaKawai, LayoutPresetType.LooseSprawlingExploration)] = new KkParameters { DisconnectedMultiplier = .7, K = 0.5, LengthFactor = 2.5, MaxIterations = 820 };
            Registry[(GraphLayoutShape.KamadaKawai, LayoutPresetType.HighDensityCompact)] = new KkParameters { K = 1.5, LengthFactor = 0.7, MaxIterations = 1000 };
        }

        public static BaseLayoutParameters GetPreset(GraphLayoutShape shape, LayoutPresetType presetType)
        {
            if (Registry.TryGetValue((shape, presetType), out var config))
            {
                return config.Clone();
            }

            // Fallback safety infrastructure mapping
            return shape switch
            {
                GraphLayoutShape.Sugiyama => new SugiyamaParameters(),
                GraphLayoutShape.LinLog => new LinLogParameters(),
                GraphLayoutShape.KamadaKawai => new KkParameters(),
                GraphLayoutShape.ISOM => new IsomParameters(),
                GraphLayoutShape.TreeSimple => new TreeSimpleParameters(),
                GraphLayoutShape.TreeDouble => new TreeDoubleParameters(),
                GraphLayoutShape.TreeBallon => new TreeBalloonParameters(),
                GraphLayoutShape.Random => new RandomParameters(),
                //GraphLayoutShape.EntityRelation => new ERParameters(),
                _ => new FruchtermanReingoldParameters()
            };
        }
    }
}
