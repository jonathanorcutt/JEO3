using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using JEO3.Providers;
using JEO3.Extensions;

namespace JEO3.Providers.Extensions.Postgres
{
    internal static class PostgresDatabaseProviderExtensions
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
        internal static async Task<IReadOnlyList<T>> GetAll<T>(this IDatabaseProvider provider) where T : class, new()
        {
            var meta = GetMetadata<T>();
            return await provider.GetInstances<T>($"SELECT * FROM {QualifiedTable(meta)};");
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
        internal static async Task<T?> GetById<T, K>(this IDatabaseProvider provider, K id) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            var sql = $"SELECT * FROM {QualifiedTable(meta)} WHERE {Q(pk.ColumnName)} = @Id;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.Add(new NpgsqlParameter("@Id", (object)id ?? DBNull.Value));

            var table = await provider.ExecuteCommandToDataTable(cmd, conn);
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
        internal static async Task<IReadOnlyList<T>> GetByIds<T, K>(this IDatabaseProvider provider, IEnumerable<K> ids) where T : class, new()
        {
            if (ids == null || !ids.Any()) return Array.Empty<T>();

            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {QualifiedTable(meta)} WHERE {Q(pk.ColumnName)} IN (");
            using var cmd = new NpgsqlCommand("", conn);

            var paramNames = new List<string>();
            int idx = 0;
            foreach (var id in ids)
            {
                var pName = $"@PkId_{idx++}";
                paramNames.Add(pName);
                cmd.Parameters.Add(new NpgsqlParameter(pName, (object)id ?? DBNull.Value));
            }

            sqlBuilder.Append(string.Join(", ", paramNames)).Append(");");
            cmd.CommandText = sqlBuilder.ToString();

            var table = await provider.ExecuteCommandToDataTable(cmd, conn);
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
        internal static async Task<T?> GetByCompositeKey<T>(this IDatabaseProvider provider, T criteriaEntity) where T : class, new()
        {
            var meta = _descriptorCache.GetOrAdd(typeof(T), _ => EntityScanner.Scan<T>());
            if (!meta.PrimaryKeys.Any())
                throw new InvalidOperationException($"No Primary Keys registered on entity {typeof(T).Name}");

            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {QualifiedTable(meta)} WHERE ");
            using var cmd = new NpgsqlCommand("", conn);

            var whereClauses = new List<string>();
            for (int i = 0; i < meta.PrimaryKeys.Count; i++)
            {
                var pk = meta.PrimaryKeys[i];
                var paramName = $"@CompPk_{i}";
                var val = pk.Property.GetValue(criteriaEntity);

                whereClauses.Add($"{Q(pk.ColumnName)} = {paramName}");
                cmd.Parameters.Add(new NpgsqlParameter(paramName, val ?? DBNull.Value));
            }

            sqlBuilder.Append(string.Join(" AND ", whereClauses)).Append(";");
            cmd.CommandText = sqlBuilder.ToString();

            var table = await provider.ExecuteCommandToDataTable(cmd, conn);
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
        internal static async Task<IReadOnlyList<T>> GetByForeignKey<T, TValue>(this IDatabaseProvider provider, Expression<Func<T, TValue>> fk, TValue value) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var fkColumnName = GeneralExtensions.GetColumnName(fk);

            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            var sql = $"SELECT * FROM {QualifiedTable(meta)} WHERE {Q(fkColumnName)} = @FkValue;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.Add(new NpgsqlParameter("@FkValue", (object)value ?? DBNull.Value));

            var table = await provider.ExecuteCommandToDataTable(cmd, conn);
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
        internal static async Task<IReadOnlyList<T>> GetByForeignKeys<T, TValue>(this IDatabaseProvider provider, Expression<Func<T, TValue>> fk, IEnumerable<TValue> values) where T : class, new()
        {
            if (values == null || !values.Any()) return Array.Empty<T>();

            var meta = GetMetadata<T>();
            var fkColumnName = GeneralExtensions.GetColumnName(fk);

            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {QualifiedTable(meta)} WHERE {Q(fkColumnName)} IN (");
            using var cmd = new NpgsqlCommand("", conn);

            var paramNames = new List<string>();
            int idx = 0;
            foreach (var val in values)
            {
                var pName = $"@FkVal_{idx++}";
                paramNames.Add(pName);
                cmd.Parameters.Add(new NpgsqlParameter(pName, (object)val ?? DBNull.Value));
            }

            sqlBuilder.Append(string.Join(", ", paramNames)).Append(");");
            cmd.CommandText = sqlBuilder.ToString();

            var table = await ExecuteCommandToDataTable(provider, cmd, conn);
            return table.ToList<T>();
        }
      
        /// <summary>
        /// Create - Requires use of class and key attribute decorations JeoTable and JeoKey.
        /// Uses PostgreSQL's native RETURNING clause instead of a follow-up scalar query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="provider"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static async Task<K> Create<T, K>(this IDatabaseProvider provider, T entity) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var columns = meta.InsertableColumns;
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            var colNames = string.Join(", ", columns.Select(c => Q(c.ColumnName)));
            var paramNames = string.Join(", ", columns.Select(c => $"@Param_{c.PropertyName}"));

            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            using var cmd = new NpgsqlCommand("", conn);

            foreach (var col in columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new NpgsqlParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            if (pk.IsDbGenerated)
            {
                cmd.CommandText = $"INSERT INTO {QualifiedTable(meta)} ({colNames}) VALUES ({paramNames}) RETURNING {Q(pk.ColumnName)};";

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

            // For manual key tracking parameters
            var directId = (K)pk.Property.GetValue(entity)!;
            cmd.CommandText = $"INSERT INTO {QualifiedTable(meta)} ({colNames}) VALUES ({paramNames});";

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
        internal static async Task<int> Update<T>(this IDatabaseProvider provider, T entity) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            var setAssignments = string.Join(", ", meta.UpdatableColumns.Select(c => $"{Q(c.ColumnName)} = @Param_{c.PropertyName}"));
            var sql = $"UPDATE {QualifiedTable(meta)} SET {setAssignments} WHERE {Q(pk.ColumnName)} = @Param_{pk.PropertyName};";

            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            using var cmd = new NpgsqlCommand(sql, conn);

            foreach (var col in meta.Columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new NpgsqlParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            await conn.OpenAsync().ConfigureAwait(false);
            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
      
        /// <summary>
        /// Upsert Range - Requires use of class and key attribute decorations JeoTable and JeoKey.
        /// Uses PostgreSQL's INSERT ... ON CONFLICT (...) DO UPDATE idiom (the standard PG upsert,
        /// available since 9.5) rather than MERGE, since MERGE only exists from PostgreSQL 15 onward.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static async Task<int> UpsertRange<T>(this IDatabaseProvider provider, IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null || !entities.Any()) return 0;

            var meta = _descriptorCache.GetOrAdd(typeof(T), _ => EntityScanner.Scan<T>());
            if (!meta.PrimaryKeys.Any())
                throw new InvalidOperationException($"Cannot execute bulk merge: {typeof(T).Name} does not define primary keys.");

            var totalRowsAffected = 0;
            const int batchSize = 100;
            var entityList = entities.ToList();

            var colInsertList = string.Join(", ", meta.InsertableColumns.Select(c => Q(c.ColumnName)));
            var pkColList = string.Join(", ", meta.PrimaryKeys.Select(pk => Q(pk.ColumnName)));
            var updateSetList = string.Join(", ", meta.UpdatableColumns.Select(c => $"{Q(c.ColumnName)} = EXCLUDED.{Q(c.ColumnName)}"));

            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            for (int b = 0; entityList.Count > b; b += batchSize)
            {
                var batch = entityList.Skip(b).Take(batchSize).ToList();
                using var cmd = new NpgsqlCommand("", conn);
                var rowTuples = new List<string>();

                for (int rowIdx = 0; batch.Count > rowIdx; rowIdx++)
                {
                    var entity = batch[rowIdx];
                    var paramNames = new List<string>();

                    foreach (var col in meta.InsertableColumns)
                    {
                        var paramName = $"@Param_B{b}_R{rowIdx}_{col.PropertyName}";
                        var val = col.Property.GetValue(entity);

                        // Handle complex types / lists by converting them to JSON strings
                        if (val != null)
                        {
                            var propType = col.Property.PropertyType;
                            if (propType != typeof(string) && (propType.IsClass || propType.IsInterface))
                            {
                                val = System.Text.Json.JsonSerializer.Serialize(val);
                            }
                        }

                        cmd.Parameters.Add(new NpgsqlParameter(paramName, val ?? DBNull.Value));
                        paramNames.Add(paramName);
                    }

                    rowTuples.Add($"({string.Join(", ", paramNames)})");
                }

                var upsertSql = $"""
                    INSERT INTO {QualifiedTable(meta)} ({colInsertList})
                    VALUES {string.Join(", ", rowTuples)}
                    ON CONFLICT ({pkColList}) DO UPDATE SET {updateSetList};
                    """;

                cmd.CommandText = upsertSql;
                totalRowsAffected += await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            return totalRowsAffected;
        }
      
        /// <summary>
        /// Update - Requires use of class and key attribute decorations JeoTable and JeoKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static async Task<int> UpdateComposite<T>(this IDatabaseProvider provider, T entity) where T : class, new()
        {
            var meta = _descriptorCache.GetOrAdd(typeof(T), _ => EntityScanner.Scan<T>());
            if (!meta.PrimaryKeys.Any())
                throw new InvalidOperationException($"No Primary Keys registered on entity {typeof(T).Name}");

            var setAssignments = string.Join(", ", meta.UpdatableColumns.Select(c => $"{Q(c.ColumnName)} = @Param_{c.PropertyName}"));
            var sqlBuilder = new StringBuilder($"UPDATE {QualifiedTable(meta)} SET {setAssignments} WHERE ");

            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            using var cmd = new NpgsqlCommand("", conn);

            foreach (var col in meta.Columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new NpgsqlParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            var whereClauses = new List<string>();
            foreach (var pk in meta.PrimaryKeys)
            {
                whereClauses.Add($"{Q(pk.ColumnName)} = @Param_{pk.PropertyName}");
            }

            sqlBuilder.Append(string.Join(" AND ", whereClauses)).Append(";");
            cmd.CommandText = sqlBuilder.ToString();

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
        internal static async Task<int> Delete<T, K>(this IDatabaseProvider provider, K id) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            var sql = $"DELETE FROM {QualifiedTable(meta)} WHERE {Q(pk.ColumnName)} = @Id;";
            using var conn = new NpgsqlConnection(provider.Connection.ConnectionString);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.Add(new NpgsqlParameter("@Id", (object)id ?? DBNull.Value));

            await conn.OpenAsync().ConfigureAwait(false);
            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private static async Task<DataTable> ExecuteCommandToDataTable(this IDatabaseProvider provider, NpgsqlCommand cmd, NpgsqlConnection conn)
        {
            await conn.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            DataTable dt = new DataTable();
            var columnNames = GeneralExtensions.GetUniqueColumnNames(reader);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dt.Columns.Add(columnNames[i], reader.GetFieldType(i) ?? typeof(object));
            }
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                DataRow row = dt.NewRow();
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
     
        /// <summary>
        /// Double-quotes an identifier for PostgreSQL, preserving exact casing
        /// (PostgreSQL folds unquoted identifiers to lowercase).
        /// </summary>
        private static string Q(string identifier) => $"\"{identifier}\"";
       
        /// <summary>
        /// Builds a schema-qualified, correctly-quoted table reference.
        /// Deliberately does NOT use EntityDescriptor.FullTableName, which is hardcoded
        /// to SQL Server bracket syntax ([schema].[table]) and is not valid in PostgreSQL.
        /// </summary>
        private static string QualifiedTable(EntityDescriptor meta)
        {
            var schema = string.IsNullOrEmpty(EntityDescriptor.DefaultSchemaOverride) ? meta.Schema : EntityDescriptor.DefaultSchemaOverride;
            return $"{Q(schema)}.{Q(meta.TableName)}";
        }
        #endregion
    }
}
