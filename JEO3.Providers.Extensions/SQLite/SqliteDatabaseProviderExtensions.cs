using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using JEO3.Extensions;
using Microsoft.Data.Sqlite;
namespace JEO3.Providers.Extensions.Sqlite
{
    internal static class SqliteDatabaseProviderExtensions
    {
        #region Properties
        private static readonly ConcurrentDictionary<Type, EntityDescriptor> _descriptorCache = new();
        #endregion

        #region Meta

        /// <summary>
        /// Get All - retrieves all entities of type T from its table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        internal static async Task<IReadOnlyList<T>> GetAll<T>(this IDatabaseProvider provider)
            where T : class, new()
        {
            var meta = GetMetadata<T>();
            return await provider.GetInstances<T>($"SELECT * FROM {meta.TableName};");
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
        internal static async Task<T?> GetById<T, K>(this IDatabaseProvider provider, K id)
            where T : class, new()
        {
            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            var sql = $"SELECT * FROM {meta.TableName} WHERE {pk.ColumnName} = @Id;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.Add(new SqliteParameter("@Id", (object)id ?? DBNull.Value));

            var table = await ExecuteCommandToDataTable(provider, cmd, conn);
            var list = table.ToList<T>();
            return list.Count > 0 ? list[0] : null;
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
        internal static async Task<IReadOnlyList<T>> GetByIds<T, K>(this IDatabaseProvider provider, IEnumerable<K> ids)
            where T : class, new()
        {
            if (ids == null || !ids.Any()) return Array.Empty<T>();

            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {meta.TableName} WHERE {pk.ColumnName} IN (");
            using var cmd = new SqliteCommand("", conn);

            var paramNames = new List<string>();
            int idx = 0;
            foreach (var id in ids)
            {
                var pName = $"@PkId_{idx++}";
                paramNames.Add(pName);
                cmd.Parameters.Add(new SqliteParameter(pName, (object)id ?? DBNull.Value));
            }

            sqlBuilder.Append(string.Join(", ", paramNames)).Append(");");
            cmd.CommandText = sqlBuilder.ToString();

            var table = await ExecuteCommandToDataTable(provider, cmd, conn);
            return table.ToList<T>();
        }

        /// <summary>
        /// Get By Composite - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="criteriaEntity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static async Task<T?> GetByCompositeKey<T>(this IDatabaseProvider provider, T criteriaEntity)
            where T : class, new()
        {
            var meta = GetMetadata<T>();
            if (!meta.PrimaryKeys.Any())
                throw new InvalidOperationException($"No Primary Keys registered on entity {typeof(T).Name}");

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {meta.TableName} WHERE ");
            using var cmd = new SqliteCommand("", conn);

            var whereClauses = new List<string>();
            int idx = 0;

            foreach (var pk in meta.PrimaryKeys)
            {
                var paramName = $"@CompPk_{idx++}";
                var val = pk.Property.GetValue(criteriaEntity);

                whereClauses.Add($"{pk.ColumnName} = {paramName}");
                cmd.Parameters.Add(new SqliteParameter(paramName, val ?? DBNull.Value));
            }

            sqlBuilder.Append(string.Join(" AND ", whereClauses)).Append(";");
            cmd.CommandText = sqlBuilder.ToString();

            var table = await ExecuteCommandToDataTable(provider, cmd, conn);
            var list = table.ToList<T>();
            return list.Count > 0 ? list[0] : null;
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
        internal static async Task<IReadOnlyList<T>> GetByForeignKey<T, TValue>(this IDatabaseProvider provider, Expression<Func<T, TValue>> fk, TValue value)
            where T : class, new()
        {
            var meta = GetMetadata<T>();
            var fkColumnName = GeneralExtensions.GetColumnName(fk);

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            var sql = $"SELECT * FROM {meta.TableName} WHERE {fkColumnName} = @FkValue;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.Add(new SqliteParameter("@FkValue", (object)value ?? DBNull.Value));

            var table = await ExecuteCommandToDataTable(provider, cmd, conn);
            return table.ToList<T>();
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
        internal static async Task<IReadOnlyList<T>> GetByForeignKeys<T, TValue>(this IDatabaseProvider provider, Expression<Func<T, TValue>> fk, IEnumerable<TValue> values)
            where T : class, new()
        {
            if (values == null || !values.Any()) return Array.Empty<T>();

            var meta = GetMetadata<T>();
            var fkColumnName = GeneralExtensions.GetColumnName(fk);

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {meta.TableName} WHERE {fkColumnName} IN (");
            using var cmd = new SqliteCommand("", conn);

            var paramNames = new List<string>();
            int idx = 0;

            foreach (var val in values)
            {
                var pName = $"@FkVal_{idx++}";
                paramNames.Add(pName);
                cmd.Parameters.Add(new SqliteParameter(pName, (object)val ?? DBNull.Value));
            }

            sqlBuilder.Append(string.Join(", ", paramNames)).Append(");");
            cmd.CommandText = sqlBuilder.ToString();

            var table = await ExecuteCommandToDataTable(provider, cmd, conn);
            return table.ToList<T>();
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
        internal static async Task<K> Create<T, K>(this IDatabaseProvider provider, T entity)
            where T : class, new()
        {
            var meta = GetMetadata<T>();
            var columns = meta.InsertableColumns;
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            var colNames = string.Join(", ", columns.Select(c => c.ColumnName));
            var paramNames = string.Join(", ", columns.Select(c => $"@Param_{c.PropertyName}"));

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            using var cmd = new SqliteCommand("", conn);

            foreach (var col in columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new SqliteParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            if (pk.IsDbGenerated)
            {
                cmd.CommandText = $@"
                    INSERT INTO {meta.TableName} ({colNames}) VALUES ({paramNames});
                    SELECT last_insert_rowid();
                ";

                await conn.OpenAsync().ConfigureAwait(false);
                var scalar = await cmd.ExecuteScalarAsync().ConfigureAwait(false);

                if (scalar != null && scalar != DBNull.Value)
                {
                    var generatedId = (K)Convert.ChangeType(scalar, typeof(K));
                    pk.Property.SetValue(entity, generatedId);
                    return generatedId;
                }

                return default!;
            }

            var directId = (K)pk.Property.GetValue(entity)!;
            cmd.CommandText = $"INSERT INTO {meta.TableName} ({colNames}) VALUES ({paramNames});";

            await conn.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            return directId;
        }

        /// <summary>
        /// Update - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static async Task<int> Update<T>(this IDatabaseProvider provider, T entity)
            where T : class, new()
        {
            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            var setAssignments = string.Join(", ",
                meta.UpdatableColumns.Select(c => $"{c.ColumnName} = @Param_{c.PropertyName}"));

            var sql = $"UPDATE {meta.TableName} SET {setAssignments} WHERE {pk.ColumnName} = @Param_{pk.PropertyName};";

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            using var cmd = new SqliteCommand(sql, conn);

            foreach (var col in meta.Columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new SqliteParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            await conn.OpenAsync().ConfigureAwait(false);
            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Upsert Range - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static async Task<int> UpdateComposite<T>(this IDatabaseProvider provider, T entity)
            where T : class, new()
        {
            var meta = GetMetadata<T>();
            if (!meta.PrimaryKeys.Any())
                throw new InvalidOperationException($"No Primary Keys registered on entity {typeof(T).Name}");

            var setAssignments = string.Join(", ",
                meta.UpdatableColumns.Select(c => $"{c.ColumnName} = @Param_{c.PropertyName}"));

            var whereClauses = string.Join(" AND ",
                meta.PrimaryKeys.Select(pk => $"{pk.ColumnName} = @Param_{pk.PropertyName}"));

            var sql = $"UPDATE {meta.TableName} SET {setAssignments} WHERE {whereClauses};";

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            using var cmd = new SqliteCommand(sql, conn);

            foreach (var col in meta.Columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new SqliteParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            await conn.OpenAsync().ConfigureAwait(false);
            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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
        internal static async Task<int> Delete<T, K>(this IDatabaseProvider provider, K id)
            where T : class, new()
        {
            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            var sql = $"DELETE FROM {meta.TableName} WHERE {pk.ColumnName} = @Id;";

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.Add(new SqliteParameter("@Id", (object)id ?? DBNull.Value));

            await conn.OpenAsync().ConfigureAwait(false);
            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        internal static async Task<int> UpsertRange<T>(this IDatabaseProvider provider, IEnumerable<T> entities)
            where T : class, new()
        {
            if (entities == null || !entities.Any()) return 0;

            var meta = GetMetadata<T>();
            if (!meta.PrimaryKeys.Any())
                throw new InvalidOperationException($"Cannot execute bulk upsert: {typeof(T).Name} does not define primary keys.");

            var totalRowsAffected = 0;

            using var conn = new SqliteConnection(provider.Connection.ConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            foreach (var entity in entities)
            {
                var colNames = string.Join(", ", meta.Columns.Select(c => c.ColumnName));
                var paramNames = string.Join(", ", meta.Columns.Select(c => $"@Param_{c.PropertyName}"));
                var updateAssignments = string.Join(", ", meta.UpdatableColumns.Select(c => $"{c.ColumnName} = excluded.{c.ColumnName}"));

                var sql = $@"
                    INSERT INTO {meta.TableName} ({colNames})
                    VALUES ({paramNames})
                    ON CONFLICT({string.Join(", ", meta.PrimaryKeys.Select(pk => pk.ColumnName))})
                    DO UPDATE SET {updateAssignments};
                ";

                using var cmd = new SqliteCommand(sql, conn);

                foreach (var col in meta.Columns)
                {
                    var val = col.Property.GetValue(entity);
                    cmd.Parameters.Add(new SqliteParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
                }

                totalRowsAffected += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            return totalRowsAffected;
        }

        private static async Task<DataTable> ExecuteCommandToDataTable(this IDatabaseProvider provider, SqliteCommand cmd, SqliteConnection conn)
        {
            await conn.OpenAsync().ConfigureAwait(false);

            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            DataTable dt = new DataTable();
            var columnNames = GeneralExtensions.GetUniqueColumnNames(reader);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var type = reader.GetFieldType(i) ?? typeof(object);
                dt.Columns.Add(columnNames[i], type);
            }

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var row = dt.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }
                dt.Rows.Add(row);
            }

            return dt;
        }
        #endregion

        #region Helpers
        private static EntityDescriptor GetMetadata<T>() where T : class
        {
            return _descriptorCache.GetOrAdd(typeof(T), _ => EntityScanner.Scan<T>());
        }
        #endregion
    }
}
