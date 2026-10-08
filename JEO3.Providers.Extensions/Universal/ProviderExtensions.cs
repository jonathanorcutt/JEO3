using System.Linq.Expressions;

namespace JEO3.Providers.Extensions
{
    public static class ProviderExtensions
    {
        private static readonly Dictionary<DatabaseProviderType, IProviderOperations> _ops = new()
        {
            [DatabaseProviderType.MSSQL] = new MSSQL.SqlServerDatabaseProviderExtensions(),
            [DatabaseProviderType.Oracle] = new Oracle.OracleDatabaseProviderExtensions(),
            [DatabaseProviderType.Postgres] = new Postgres.PostgresDatabaseProviderExtensions(),
            [DatabaseProviderType.SQLite] = new Sqlite.SqliteDatabaseProviderExtensions(),
        };

        private static IProviderOperations Ops(IDatabaseProvider provider)
        {
            var type = provider.GetDatabaseProviderType();
            return _ops.TryGetValue(type, out var ops) ? ops : throw new NotImplementedException($"Provider type '{type}' not implemented.");
        }

        public static Task<IReadOnlyList<T>> GetAll<T>(this IDatabaseProvider provider) where T : class, new() => Ops(provider).GetAll<T>(provider);
        public static Task<T?> GetById<T, K>(this IDatabaseProvider provider, K id) where T : class, new() => Ops(provider).GetById<T, K>(provider, id);
        public static Task<IReadOnlyList<T>> GetByIds<T, K>(this IDatabaseProvider provider, IEnumerable<K> ids) where T : class, new() => Ops(provider).GetByIds<T, K>(provider, ids);
        public static Task<T?> GetByCompositeKey<T>(this IDatabaseProvider provider, T criteriaEntity) where T : class, new() => Ops(provider).GetByCompositeKey(provider, criteriaEntity);
        public static Task<IReadOnlyList<T>> GetByForeignKey<T, TValue>(this IDatabaseProvider provider, Expression<Func<T, TValue>> fk, TValue value) where T : class, new() => Ops(provider).GetByForeignKey(provider, fk, value);
        public static Task<IReadOnlyList<T>> GetByForeignKeys<T, TValue>(this IDatabaseProvider provider, Expression<Func<T, TValue>> fk, IEnumerable<TValue> values) where T : class, new() => Ops(provider).GetByForeignKeys(provider, fk, values);
        public static Task<K> Create<T, K>(this IDatabaseProvider provider, T entity) where T : class, new() => Ops(provider).Create<T, K>(provider, entity);
        public static Task<int> Update<T>(this IDatabaseProvider provider, T entity) where T : class, new() => Ops(provider).Update(provider, entity);
        public static Task<int> UpsertRange<T>(this IDatabaseProvider provider, IEnumerable<T> entities) where T : class, new() => Ops(provider).UpsertRange(provider, entities);
        public static Task<int> UpdateComposite<T>(this IDatabaseProvider provider, T entity) where T : class, new() => Ops(provider).UpdateComposite(provider, entity);
        public static Task<int> Delete<T, K>(this IDatabaseProvider provider, K id) where T : class, new() => Ops(provider).Delete<T, K>(provider, id);
    }
}