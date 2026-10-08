using System.Linq.Expressions;

namespace JEO3.Providers.Extensions
{
    internal interface IProviderOperations
    {
        Task<IReadOnlyList<T>> GetAll<T>(IDatabaseProvider provider) where T : class, new();
        Task<T?> GetById<T, K>(IDatabaseProvider provider, K id) where T : class, new();
        Task<IReadOnlyList<T>> GetByIds<T, K>(IDatabaseProvider provider, IEnumerable<K> ids) where T : class, new();
        Task<T?> GetByCompositeKey<T>(IDatabaseProvider provider, T criteriaEntity) where T : class, new();
        Task<IReadOnlyList<T>> GetByForeignKey<T, TValue>(IDatabaseProvider provider, Expression<Func<T, TValue>> fk, TValue value) where T : class, new();
        Task<IReadOnlyList<T>> GetByForeignKeys<T, TValue>(IDatabaseProvider provider, Expression<Func<T, TValue>> fk, IEnumerable<TValue> values) where T : class, new();
        Task<K> Create<T, K>(IDatabaseProvider provider, T entity) where T : class, new();
        Task<int> Update<T>(IDatabaseProvider provider, T entity) where T : class, new();
        Task<int> UpsertRange<T>(IDatabaseProvider provider, IEnumerable<T> entities) where T : class, new();
        Task<int> UpdateComposite<T>(IDatabaseProvider provider, T entity) where T : class, new();
        Task<int> Delete<T, K>(IDatabaseProvider provider, K id) where T : class, new();
    }
}
