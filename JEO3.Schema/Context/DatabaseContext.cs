using JEO3.Providers;

namespace JEO3.Schema
{
    public sealed class DatabaseContext : IDatabaseContext
    {
        public IDatabaseProvider Provider { get; internal init; }
        public IDatabase Database { get; internal init; }
        public IReadOnlyList<ISchema> Schemas { get; internal init; } = Array.Empty<ISchema>();
        public IReadOnlyList<ITable> Tables { get; internal init; } = Array.Empty<ITable>();
        public IReadOnlyList<IColumn> Columns { get; internal init; } = Array.Empty<IColumn>();
        public IReadOnlyList<IView> Views { get; internal init; } = Array.Empty<IView>();
        public IReadOnlyList<IProcedure> Procedures { get; internal init; } = Array.Empty<IProcedure>();
        public IReadOnlyList<IFunction> Functions { get; internal init; } = Array.Empty<IFunction>();
        public IReadOnlyList<IUserDefinedType> UserDefinedTypes { get; internal init; } = Array.Empty<IUserDefinedType>();
        public IReadOnlyList<IRelation> Relations { get; internal init; } = Array.Empty<IRelation>();
        public IReadOnlyList<IForeignKey> ForeignKeys { get; internal init; } = Array.Empty<IForeignKey>();
        public IReadOnlyList<IIndex> Indexes { get; internal init; } = Array.Empty<IIndex>();
        public IReadOnlyList<IColumnIndexLink> ColumnIndexLinks { get; internal init; } = Array.Empty<IColumnIndexLink>();
        public IReadOnlyList<ICheckConstraint> CheckConstraints { get; internal init; } = Array.Empty<ICheckConstraint>();
        public IReadOnlyList<IExtendedProperty> ExtendedProperties { get; internal init; } = Array.Empty<IExtendedProperty>();
        public IReadOnlyList<IStatistic> Statistics { get; internal init; } = Array.Empty<IStatistic>();
        public IReadOnlyList<IMissingIndex> MissingIndexes { get; internal init; } = Array.Empty<IMissingIndex>();
        public IReadOnlyList<ITrigger> Triggers { get; internal init; } = Array.Empty<ITrigger>();
        public IReadOnlyList<ISynonym> Synonyms { get; internal init; } = Array.Empty<ISynonym>();

        // To Finish..
        public IReadOnlyList<IDatabasePrincipal> Principals { get; internal init; } = Array.Empty<IDatabasePrincipal>();
        public IReadOnlyList<DatabaseUser> Users { get; internal init; } = Array.Empty<DatabaseUser>();
        public IReadOnlyList<DatabaseRole> Roles { get; internal init; } = Array.Empty<DatabaseRole>();
        public IReadOnlyList<ApplicationRole> ApplicationRoles { get; internal init; } = Array.Empty<ApplicationRole>();
    }
}