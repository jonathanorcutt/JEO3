using System.Linq.Expressions;

namespace JEO3.Providers.Extensions
{
    public static class ProviderExtensions
    {
        /// <summary>
        /// Get All - retrieves all entities of type T from its table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static async Task<IReadOnlyList<T>> GetAll<T>(this IDatabaseProvider provider) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.GetAll<T>(provider);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.GetAll<T>(provider);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.GetAll<T>(provider);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.GetAll<T>(provider);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(GetAll)} - Provider type '{type}' not implemented.");
        }

        /// <summary>
        /// Get By Id - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="provider"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<T?> GetById<T, K>(this IDatabaseProvider provider, K id) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.GetById<T, K>(provider, id);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.GetById<T, K>(provider, id);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.GetById<T, K>(provider, id);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.GetById<T, K>(provider, id);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(GetById)} - Provider type '{type}' not implemented.");
        }
        
        /// <summary>
        /// Get By IDs - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="provider"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<IReadOnlyList<T>> GetByIds<T, K>(this IDatabaseProvider provider, IEnumerable<K> ids) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.GetByIds<T, K>(provider, ids);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.GetByIds<T, K>(provider, ids);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.GetByIds<T, K>(provider, ids);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.GetByIds<T, K>(provider, ids);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(GetByIds)} - Provider type '{type}' not implemented.");
        }

        /// <summary>
        /// Get By Composite - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="criteriaEntity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<T?> GetByCompositeKey<T>(this IDatabaseProvider provider, T criteriaEntity) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.GetByCompositeKey<T>(provider, criteriaEntity);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.GetByCompositeKey<T>(provider, criteriaEntity);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.GetByCompositeKey<T>(provider, criteriaEntity);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.GetByCompositeKey<T>(provider, criteriaEntity);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(GetByCompositeKey)} - Provider type '{type}' not implemented.");
        }

        /// <summary>
        /// Get By FK - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="provider"></param>
        /// <param name="fk"></param>
        /// <param name="value"></param>
        /// <returns></returns>              
        public static async Task<IReadOnlyList<T>> GetByForeignKey<T, TValue>(this IDatabaseProvider provider, Expression<Func<T, TValue>> fk, TValue value) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.GetByForeignKey<T, TValue>(provider, fk, value);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.GetByForeignKey<T, TValue>(provider, fk, value);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.GetByForeignKey<T, TValue>(provider, fk, value);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.GetByForeignKey<T, TValue>(provider, fk, value);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(GetByForeignKey)} - Provider type '{type}' not implemented.");
        }
        
        /// <summary>
        /// Get By FK - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="provider"></param>
        /// <param name="fk"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static async Task<IReadOnlyList<T>> GetByForeignKeys<T, TValue>(this IDatabaseProvider provider, Expression<Func<T, TValue>> fk, IEnumerable<TValue> values) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.GetByForeignKeys<T, TValue>(provider, fk, values);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.GetByForeignKeys<T, TValue>(provider, fk, values);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.GetByForeignKeys<T, TValue>(provider, fk, values);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.GetByForeignKeys<T, TValue>(provider, fk, values);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(GetByForeignKeys)} - Provider type '{type}' not implemented.");
        }
        
        /// <summary>
        /// Create - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="provider"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<K> Create<T, K>(this IDatabaseProvider provider, T entity) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.Create<T, K>(provider, entity);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.Create<T, K>(provider, entity);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.Create<T, K>(provider, entity);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.Create<T, K>(provider, entity);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(Create)} - Provider type '{type}' not implemented.");
        }

        /// <summary>
        /// Update - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<int> Update<T>(this IDatabaseProvider provider, T entity) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.Update<T>(provider, entity);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.Update<T>(provider, entity);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.Update<T>(provider, entity);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.Update<T>(provider, entity);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(Update)} - Provider type '{type}' not implemented.");
        }

        /// <summary>
        /// Upsert Range - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<int> UpsertRange<T>(this IDatabaseProvider provider, IEnumerable<T> entities) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.UpsertRange<T>(provider, entities);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.UpsertRange<T>(provider, entities);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.UpsertRange<T>(provider, entities);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.UpsertRange<T>(provider, entities);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(UpsertRange)} - Provider type '{type}' not implemented.");
        }

        /// <summary>
        /// Update - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<int> UpdateComposite<T>(this IDatabaseProvider provider, T entity) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.UpdateComposite<T>(provider, entity);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.UpdateComposite<T>(provider, entity);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.UpdateComposite<T>(provider, entity);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.UpdateComposite<T>(provider, entity);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(UpdateComposite)} - Provider type '{type}' not implemented.");
        }

        /// <summary>
        /// Delete - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="provider"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<int> Delete<T, K>(this IDatabaseProvider provider, K id) where T : class, new()
        {
            var type = provider.GetDatabaseProviderType();
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return await MSSQL.SqlServerDatabaseProviderExtensions.Delete<T, K>(provider, id);
                case DatabaseProviderType.Oracle:
                    return await Oracle.OracleDatabaseProviderExtensions.Delete<T, K>(provider, id);
                case DatabaseProviderType.Postgres:
                    return await Postgres.PostgresDatabaseProviderExtensions.Delete<T, K>(provider, id);
                case DatabaseProviderType.SQLite:
                    return await Sqlite.SqliteDatabaseProviderExtensions.Delete<T, K>(provider, id);
                default:
                    break;
            }
            throw new NotImplementedException($"{nameof(Delete)} - Provider type '{type}' not implemented.");
        }
    }
}
