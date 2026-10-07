using JEO3.Providers;

namespace JEO3.Schema
{
    /// <summary>
    /// Provider-agnostic snapshot of database metadata.
    /// This is the primary aggregate consumed by higher-level
    /// schema analysis, query generation, comparison, and traversal.
    /// </summary>
    public interface IDatabaseContext
    {
        IDatabaseProvider Provider { get; }
        IDatabase Database { get; }
        IReadOnlyList<ISchema> Schemas { get; }
        IReadOnlyList<ITable> Tables { get; }
        IReadOnlyList<IColumn> Columns { get; }
        IReadOnlyList<IView> Views { get; }
        IReadOnlyList<IProcedure> Procedures { get; }
        IReadOnlyList<IFunction> Functions { get; }
        IReadOnlyList<IUserDefinedType> UserDefinedTypes { get; }
        IReadOnlyList<IRelation> Relations { get; }
        IReadOnlyList<IForeignKey> ForeignKeys { get; }
        IReadOnlyList<IIndex> Indexes { get; }
        IReadOnlyList<IColumnIndexLink> ColumnIndexLinks { get; }
        IReadOnlyList<ICheckConstraint> CheckConstraints { get; }
        IReadOnlyList<IExtendedProperty> ExtendedProperties { get; }
        IReadOnlyList<IStatistic> Statistics { get; }
        IReadOnlyList<IMissingIndex> MissingIndexes { get; }
        IReadOnlyList<IDatabasePrincipal> Principals { get; }
        IReadOnlyList<DatabaseUser> Users { get; }
        IReadOnlyList<DatabaseRole> Roles { get; }
        IReadOnlyList<ApplicationRole> ApplicationRoles { get; }
        IReadOnlyList<ITrigger> Triggers { get; }
        IReadOnlyList<ISynonym> Synonyms { get; }
    }
}
