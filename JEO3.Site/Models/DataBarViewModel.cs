using System.Data;
using JEO3.Core;
using JEO3.Extensions;
using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Site.Models
{
    public class DataBarViewModel
    {
        private IDatabaseContext _context;

        public DataTable Tables { get; set; }
        public DataTable Columns { get; set; }
        public DataTable Procedures { get; set; }
        public DataTable Views { get; set; }
        public DataTable Functions { get; set; }
        public DataTable UDTs { get; set; }
        public DataTable Triggers { get; set; }
        public DataTable Indexes { get; set; }
        public DataTable Statistics { get; set; }
        public DataTable ExtendedProperties { get; set; }
        private List<QueryGenerationResult> _results;
        public List<QueryGenerationResult> Results => _results;

        public DataBarViewModel(IDatabaseContext context, List<QueryGenerationResult> results)
        {
            _context = context;
            _results = results;
        }

        public async Task Populate()
        {
            CancellationToken token = new();

            var actions = new[]
            {
                () => PopulateTables(),
                () => PopulateColumns(),
                () => PopulateProcedures(),
                () => PopulateViews(),
                () => PopulateFunctions(),
                () => PopulateUDTs(),
                () => PopulateTriggers(),
                () => PopulateIndexes(),
                () => PopulateStatistics(),
                () => PopulateExtendedProperties()
                //() => PopulateIndexHealth()
            };
            await Parallel.ForEachAsync(actions, async (action, token) =>
            {
                action.Invoke();
            });
        }

        private void PopulateTables()
        {
            Tables = _context.Tables.Join(_results, t => t.ObjectId, r => r.Tracker.Root.TableObjectId, (t, r) =>
            {
                // Compute fragmented-index count and average fragmentation in one pass
                // instead of two separate .Where().Count() + .Average() scans.
                int fragCount = 0; double sumFrag = 0;
                foreach (var idx in t.Indexes)
                {
                    if (idx.FragmentationPercentage > 0)
                    {
                        fragCount++;
                        sumFrag += idx.FragmentationPercentage ?? 0;
                    }
                }
                double avgFrag = fragCount == 0 ? 0 : Math.Round(sumFrag / fragCount, 2);

                return new
                {
                    PrimaryKeyColumn = t.ObjectId,
                    Name = new Meta<ITable>(t.SchemaName + "." + t.Name, t),
                    Rows = t.Rows,
                    Columns = t.Columns.Count,
                    ForeignKeys = t.ForeignKeys.Count,
                    //ReferencedByForeignKeys = t.ReferencedByForeignKeys.Count,
                    Relations = t.Relations.Count,
                    Indexes = t.Indexes.Count,
                    //MissingIndexes = t.MissingIndexes.Count,
                    FragmentedIndexes = fragCount,
                    AverageFragmentationPct = avgFrag,
                    //ReachableTables = r.Coverage.ReachableTableCount,
                    MaxTraversalDepth = r.Metrics.MaxTraversalDepth,
                    AverageTraversalDepth = Math.Round(r.Metrics.AverageTraversalDepth, 2),
                    JoinCount = r.Metrics.JoinCount,
                    ConfidenceScore = Math.Round(r.Metrics.Score.ConfidenceScore, 2),
                    Statistics = t.Statistics.Count,
                    ExtendedProperties = t.ExtendedProperties.Count,
                    //CheckConstraints = t.CheckConstraints.Count,
                    //Triggers = t.Triggers.Count,
                    //Synonyms = t.Synonyms.Count,
                };
            }).OrderByDescending(v => v.JoinCount).ToDataTable();
            //.OrderBy(v => v.Name, new MetaComparer<ITable>())
        }
        private void PopulateColumns()
        {
            Columns = _context.Columns.Join(_results, t => t.Table?.ObjectId, r => r.Tracker.Root.TableObjectId, (t, r) =>
            {
                // Single pass over t.Indexes instead of 9 separate Where().Count() scans.
                var s = ComputeColumnIndexStats(t);
                return new
                {
                    PrimaryKeyColumn = t.Table?.ObjectId ?? 0,
                    Name = new Meta<IColumn>(t.SchemaName + "." + t.Table?.Name + "." + t.Name, t),
                    Indexes = t.Indexes.Count,
                    MissingIndexes = t.MissingIndexes.Count,
                    ForeignKeys = t.ForeignKeys.Count,
                    ReferencedByForeignKeys = t.ReferencedByForeignKeys.Count,
                    KeyIndexCount = s.KeyIndex,
                    IncludeIndexCount = s.Include,
                    Clustered = s.Clustered,
                    NonClustered = s.NonClustered,
                    CoveringIndexes = s.Covering,
                    NonCoveringIndexes = s.NonCovering,
                    UniqueConstraints = s.UniqueConstraint,
                    CheckConstraints = t.CheckConstraints.Count,
                    MinFrag = Math.Round(s.MinFrag, 2),
                    MaxFrag = Math.Round(s.MaxFrag, 2),
                    AvgFrag = Math.Round(s.AvgFrag, 2),
                };
            }).OrderByDescending(v => v.Indexes).ToDataTable();
            //.OrderBy(v => v.Name, new MetaComparer<IColumn>()).ToDataTable();
        }

        // Computes all per-index column statistics in a single enumeration.
        // Replaces 9 separate .Where().Count() / .Min() / .Max() / .Average() calls,
        // reducing index-list traversals from O(9n) to O(n) per column.
        private static ColumnIndexStats ComputeColumnIndexStats(IColumn t)
        {
            var s = new ColumnIndexStats();
            double sumFrag = 0; int fragCount = 0;
            s.MinFrag = double.MaxValue; s.MaxFrag = double.MinValue;

            foreach (var idx in t.Indexes)
            {
                bool isKey = idx.KeyColumns.Any(x => x.ColumnId == t.ColumnId);
                bool isInclude = idx.IncludedColumns.Any(x => x.ColumnId == t.ColumnId);

                if (isKey) s.KeyIndex++;
                if (isInclude) s.Include++;
                if (idx.IndexType == IndexType.Clustered) s.Clustered++;
                if (idx.IndexType == IndexType.NonClustered) s.NonClustered++;
                if (isKey && idx.IsCovering) s.Covering++;
                if (isKey && !idx.IsCovering) s.NonCovering++;
                if (idx.IsUniqueConstraint) s.UniqueConstraint++;

                var frag = idx.FragmentationPercentage ?? 0;
                sumFrag += frag; fragCount++;
                if (frag < s.MinFrag) s.MinFrag = frag;
                if (frag > s.MaxFrag) s.MaxFrag = frag;
            }

            if (fragCount == 0) { s.MinFrag = 0; s.MaxFrag = 0; s.AvgFrag = 0; }
            else s.AvgFrag = sumFrag / fragCount;

            return s;
        }
        private void PopulateProcedures()
        {
            Procedures = _context.Procedures.Select(v => new
            {
                PrimaryKeyColumn = v.ObjectId,
                Name = v.SchemaName + "." + v.Name,
                ObjectType = v.ObjectType.ToString() ?? "",
                Parameters = v.Parameters.Count,
                ParameterNames = string.Join(", ", v.Parameters.Select(v => v.Name)),
                OutputColumns = v.OutputColumns.Count,
                OutputColumnNamess = string.Join(", ", v.OutputColumns.Select(v => v.name)),
                Definition = v.Definition,
                FullName = v.FullName,
            }).OrderByDescending(v => v.Parameters).ToDataTable();
            //.OrderBy(v => v.Name).ToDataTable();
        }
        private void PopulateViews()
        {
            Views = _context.Views.Select(v => new
            {
                PrimaryKeyColumn = v.ObjectId,
                Name = v.SchemaName + "." + v.Name,
                Columns = v.Columns.Count,
                Relations = v.Relations.Count,
                ColumnNames = string.Join(", ", v.Columns.Select(v => v.Name)),
                Definition = v.Definition
            }).OrderByDescending(v => v.Columns).ToDataTable();
            //.OrderBy(v => v.Name).ToDataTable();
        }
        private void PopulateFunctions()
        {
            Functions = _context.Functions.Select(v => new
            {
                PrimaryKeyColumn = v.ObjectId,
                Name = v.SchemaName + "." + v.Name,
                ProviderReturnType = v.ProviderReturnType,
                ReturnColumns = v.ReturnColumns.Count,
                Parameters = v.Parameters.Count,
                ReturnColumnsNames = string.Join(", ", v.ReturnColumns.Select(v => v.Name)),
                ParametersNames = string.Join(", ", v.Parameters.Select(v => v.Name)),
                IsDeterministic = v.IsDeterministic,
                IsSystemObject = v.IsSystemObject,
                Definition = v.Definition
            }).OrderByDescending(v => v.Parameters).ToDataTable();
            //.OrderBy(v => v.Name).ToDataTable();
        }
        private void PopulateUDTs()
        {
            UDTs = _context.UserDefinedTypes.Select(v => new
            {
                PrimaryKeyColumn = v.ObjectId,
                Name = v.SchemaName + "." + v.Name,
                Columns = v.Columns.Count,
                ColumnsNames = string.Join(", ", v.Columns.Select(v => v.Name)),
                MaxLength = v.MaxLength,
                IsNullable = v.IsNullable,
                Precision = v.Precision,
                Scale = v.Scale,
            }).OrderByDescending(v => v.Columns).ToDataTable();
            //.OrderBy(v => v.Name).ToDataTable();
        }
        private void PopulateTriggers()
        {
            Triggers = _context.Triggers.Select(v => new
            {
                PrimaryKeyColumn = v.ObjectId,
                Name = v.SchemaName + "." + v.Name,
                IsDisabled = v.IsDisabled,
                IsInsteadOfTrigger = v.IsInsteadOfTrigger,
                DefinitionHash = v.DefinitionHash,
                Definition = v.Definition
            }).OrderByDescending(v => v.Name).ToDataTable();
            //.OrderBy(v => v.Name).ToDataTable();
        }
        private void PopulateIndexes()
        {
            Indexes = _context.Indexes.Select(v => new
            {
                PrimaryKeyColumn = v.ObjectId,
                Name = v.SchemaName + "." + v.Name,
                ColumnsNames = string.Join(", ", v.Columns.Select(v => v.ColumnName)),
                Columns = v.Columns.Count,
                IncludedColumns = v.IncludedColumns.Count,
                KeyColumns = v.KeyColumns,
                IndexType = v.IndexType,
                IsClustered = v.IsClustered,
                IsCovering = v.IsCovering,
                IsDisabled = v.IsDisabled,
                IsFiltered = v.IsFiltered,
                IsPrimaryKey = v.IsPrimaryKey,
                IsUnique = v.IsUnique,
                FragmentationPercentage = v.FragmentationPercentage != null ? Math.Round((double)v.FragmentationPercentage, 1) : 0
            }).OrderByDescending(v => v.Columns).ToDataTable();
            //.OrderBy(v => v.Name).ToDataTable();
        }
        private void PopulateStatistics()
        {
            Statistics = _context.Statistics.Select(v => new
            {
                PrimaryKeyColumn = v.ObjectId,
                Name = v.SchemaName + "." + v.Name,
                StatsId = v.StatsId,
                CreateDate = v.CreateDate,
                CreatedBy = v.CreatedBy,
                ModifyDate = v.ModifyDate,
                FilterDefinition = v.FilterDefinition
            }).OrderByDescending(v => v.Name).OrderBy(v => v.Name).ToDataTable();
        }
        private void PopulateExtendedProperties()
        {
            ExtendedProperties = _context.ExtendedProperties.Select(v => new
            {
                PrimaryKeyColumn = v.ObjectId,
                Name = v.SchemaName + "." + v.Name,
                IndexName = v.IndexName,
                Class = v.Class,
                ClassDescription = v.ClassDescription,
                PropertyName = v.PropertyName,
                Value = v.Value,
            }).ToDataTable();
            //.OrderBy(v => v.Name).ToDataTable();
        }


        // For Optimization Only
        private struct ColumnIndexStats
        {
            public int KeyIndex; public int Include; public int Clustered; public int NonClustered;
            public int Covering; public int NonCovering; public int UniqueConstraint;
            public double MinFrag; public double MaxFrag; public double AvgFrag;
        }
    }
}
