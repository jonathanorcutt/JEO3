using JEO3.Engine.Entities;
using JEO3.Engine.Entities.JEO3.Engine.Models;
using JEO3.Engine.Models;
using JEO3.Schema;

namespace JEO3.Engine
{
    public sealed class FlatDatabaseContext : IFlatDatabaseContext
    {
        public string? ConnectionString { get; init; }
        public string? DatabaseName { get; init; }
        public string? ProviderName { get; init; }
        public IReadOnlyList<Database> Databases { get; init; } = [];
        public IReadOnlyList<Schema.Schema> Schemas { get; init; } = [];
        public IReadOnlyList<Table> Tables { get; init; } = [];
        public IReadOnlyList<Column> Columns { get; init; } = [];
        public IReadOnlyList<FlatView> Views { get; init; } = [];
        public IReadOnlyList<FlatStoredProcedure> Procedures { get; init; } = [];
        public IReadOnlyList<FlatFunction> Functions { get; init; } = [];
        public IReadOnlyList<FlatUserDefinedType> UserDefinedTypes { get; init; } = [];
        public IReadOnlyList<FlatRelation> Relations { get; init; } = [];
        public IReadOnlyList<ForeignKey> ForeignKeys { get; init; } = [];
        public IReadOnlyList<FlatIndex> Indexes { get; init; } = [];
        public IReadOnlyList<ColumnIndexLink> ColumnIndexLinks { get; init; } = [];
        public IReadOnlyList<CheckConstraint> CheckConstraints { get; init; } = [];
        public IReadOnlyList<ExtendedProperty> ExtendedProperties { get; init; } = [];
        public IReadOnlyList<Statistic> Statistics { get; init; } = [];
        public IReadOnlyList<MissingIndex> MissingIndexes { get; init; } = [];
        public IReadOnlyList<Trigger> Triggers { get; init; } = [];
        public IReadOnlyList<Synonym> Synonyms { get; init; } = [];
        public IReadOnlyList<FlatDatabasePrincipal> Principals { get; init; } = [];
        public IReadOnlyList<FlatDatabasePermission> Permissions { get; init; } = [];

        public FlatDatabaseContext() { }
    }
}
