using JEO3.Engine.Entities;
using JEO3.Engine.Entities.JEO3.Engine.Models;
using JEO3.Engine.Models;
using JEO3.Schema;

namespace JEO3.Engine
{
    public interface IFlatDatabaseContext
    {
        string? ConnectionString { get; }
        string? DatabaseName { get; }
        string? ProviderName { get; }
        IReadOnlyList<Database> Databases { get; }
        IReadOnlyList<Schema.Schema> Schemas { get; }
        IReadOnlyList<Table> Tables { get; }
        IReadOnlyList<Column> Columns { get; }
        IReadOnlyList<FlatView> Views { get; }
        IReadOnlyList<FlatStoredProcedure> Procedures { get; }
        IReadOnlyList<FlatFunction> Functions { get; }
        IReadOnlyList<FlatUserDefinedType> UserDefinedTypes { get; }
        IReadOnlyList<FlatRelation> Relations { get; }
        IReadOnlyList<ForeignKey> ForeignKeys { get; }
        IReadOnlyList<FlatIndex> Indexes { get; }
        IReadOnlyList<ColumnIndexLink> ColumnIndexLinks { get; }
        IReadOnlyList<CheckConstraint> CheckConstraints { get; }
        IReadOnlyList<ExtendedProperty> ExtendedProperties { get; }
        IReadOnlyList<Statistic> Statistics { get; }
        IReadOnlyList<MissingIndex> MissingIndexes { get; }
        IReadOnlyList<Trigger> Triggers { get; }
        IReadOnlyList<Synonym> Synonyms { get; }
        IReadOnlyList<FlatDatabasePrincipal> Principals { get; }
        IReadOnlyList<FlatDatabasePermission> Permissions { get; }
    }
}
