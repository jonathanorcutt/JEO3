using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;
using JEO3.Providers;
using JEO3.Extensions;

namespace JEO3.Providers.Extensions.MSSQL
{
    internal sealed class SqlServerDatabaseProviderExtensions : IProviderOperations
    {
        #region Properties
        private readonly ConcurrentDictionary<Type, EntityDescriptor> _descriptorCache = new();
        #endregion

        #region Meta

        /// <summary>
        /// Get All - retrieves all entities of type T from its table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<T>> GetAll<T>(IDatabaseProvider provider) where T : class, new()
        {
            var meta = GetMetadata<T>();
            return await provider.GetInstances<T>($"SELECT * FROM {meta.FullTableName};");
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
        public async Task<T?> GetById<T, K>(IDatabaseProvider provider, K id) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            var sql = $"SELECT * FROM {meta.FullTableName} WHERE [{pk.ColumnName}] = @Id;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Id", (object)id ?? DBNull.Value));

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
        public async Task<IReadOnlyList<T>> GetByIds<T, K>(IDatabaseProvider provider, IEnumerable<K> ids) where T : class, new()
        {
            if (ids == null || !ids.Any()) return Array.Empty<T>();

            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {meta.FullTableName} WHERE [{pk.ColumnName}] IN (");
            using var cmd = new SqlCommand("", conn);

            // Replicating Dapper's IN parameter expansion loop natively
            var paramNames = new List<string>();
            int idx = 0;
            foreach (var id in ids)
            {
                var pName = $"@PkId_{idx++}";
                paramNames.Add(pName);
                cmd.Parameters.Add(new SqlParameter(pName, (object)id ?? DBNull.Value));
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
        public async Task<T?> GetByCompositeKey<T>(IDatabaseProvider provider, T criteriaEntity) where T : class, new()
        {
            var meta = _descriptorCache.GetOrAdd(typeof(T), _ => EntityScanner.Scan<T>());
            if (!meta.PrimaryKeys.Any())
                throw new InvalidOperationException($"No Primary Keys registered on entity {typeof(T).Name}");

            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {meta.FullTableName} WHERE ");
            using var cmd = new SqlCommand("", conn);

            var whereClauses = new List<string>();
            for (int i = 0; i < meta.PrimaryKeys.Count; i++)
            {
                var pk = meta.PrimaryKeys[i];
                var paramName = $"@CompPk_{i}";
                var val = pk.Property.GetValue(criteriaEntity);

                whereClauses.Add($"[{pk.ColumnName}] = {paramName}");
                cmd.Parameters.Add(new SqlParameter(paramName, val ?? DBNull.Value));
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
        public async Task<IReadOnlyList<T>> GetByForeignKey<T, TValue>(IDatabaseProvider provider, Expression<Func<T, TValue>> fk, TValue value) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var fkColumnName = GeneralExtensions.GetColumnName(fk);

            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            var sql = $"SELECT * FROM {meta.FullTableName} WHERE [{fkColumnName}] = @FkValue;";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@FkValue", (object)value ?? DBNull.Value));

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
        public async Task<IReadOnlyList<T>> GetByForeignKeys<T, TValue>(IDatabaseProvider provider, Expression<Func<T, TValue>> fk, IEnumerable<TValue> values) where T : class, new()
        {
            if (values == null || !values.Any()) return Array.Empty<T>();

            var meta = GetMetadata<T>();
            var fkColumnName = GeneralExtensions.GetColumnName(fk);

            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {meta.FullTableName} WHERE [{fkColumnName}] IN (");
            using var cmd = new SqlCommand("", conn);

            var paramNames = new List<string>();
            int idx = 0;
            foreach (var val in values)
            {
                var pName = $"@FkVal_{idx++}";
                paramNames.Add(pName);
                cmd.Parameters.Add(new SqlParameter(pName, (object)val ?? DBNull.Value));
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
        public async Task<K> Create<T, K>(IDatabaseProvider provider, T entity) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var columns = meta.InsertableColumns;
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            var colNames = string.Join(", ", columns.Select(c => $"[{c.ColumnName}]"));
            var paramNames = string.Join(", ", columns.Select(c => $"@Param_{c.PropertyName}"));

            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            using var cmd = new SqlCommand("", conn);

            // Reflect values out of the entity properties straight into the native parameter block
            foreach (var col in columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new SqlParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            if (pk.IsDbGenerated)
            {
                cmd.CommandText = $@"
                    INSERT INTO {meta.FullTableName} ({colNames}) VALUES ({paramNames});
                    SELECT SCOPE_IDENTITY();";

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
            cmd.CommandText = $"INSERT INTO {meta.FullTableName} ({colNames}) VALUES ({paramNames});";

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
        public async Task<int> Update<T>(IDatabaseProvider provider, T entity) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            var setAssignments = string.Join(", ", meta.UpdatableColumns.Select(c => $"[{c.ColumnName}] = @Param_{c.PropertyName}"));
            var sql = $"UPDATE {meta.FullTableName} SET {setAssignments} WHERE [{pk.ColumnName}] = @Param_{pk.PropertyName};";

            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            using var cmd = new SqlCommand(sql, conn);

            foreach (var col in meta.Columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new SqlParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
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
        public async Task<int> UpsertRange<T>(IDatabaseProvider provider, IEnumerable<T> entities) where T : class, new()
        {
            if (entities == null || !entities.Any()) return 0;

            var meta = _descriptorCache.GetOrAdd(typeof(T), _ => EntityScanner.Scan<T>());
            if (!meta.PrimaryKeys.Any())
                throw new InvalidOperationException($"Cannot execute bulk merge: {typeof(T).Name} does not define primary keys.");

            var totalRowsAffected = 0;
            const int batchSize = 100;
            var entityList = entities.ToList();

            // Reuse the connection string from your existing provider metadata contract
            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            for (int b = 0; entityList.Count > b; b += batchSize)
            {
                var batch = entityList.Skip(b).Take(batchSize).ToList();
                using var cmd = new SqlCommand("", conn);
                var sqlBuilder = new StringBuilder();

                for (int rowIdx = 0; batch.Count > rowIdx; rowIdx++)
                {
                    var entity = batch[rowIdx];
                    var selectCols = new List<string>();

                    foreach (var col in meta.Columns)
                    {
                        var paramName = $"@Param_B{b}_R{rowIdx}_{col.PropertyName}";
                        var val = col.Property.GetValue(entity);

                        // Handle complex types / lists by converting them to JSON strings
                        if (val != null)
                        {
                            var propType = col.Property.PropertyType;

                            // If it's a class or collection (excluding primitive types like string)
                            if (propType != typeof(string) && (propType.IsClass || propType.IsInterface))
                            {
                                // Using System.Text.Json (or JsonConvert.SerializeObject for Newtonsoft.Json)
                                val = System.Text.Json.JsonSerializer.Serialize(val);
                            }
                        }

                        cmd.Parameters.Add(new SqlParameter(paramName, val ?? DBNull.Value));
                        selectCols.Add($"{paramName} AS [{col.ColumnName}]");
                    }

                    sqlBuilder.AppendLine($"SELECT {string.Join(", ", selectCols)}");
                    if (rowIdx < batch.Count - 1) sqlBuilder.AppendLine("UNION ALL");
                }

                var colInsertList = string.Join(", ", meta.InsertableColumns.Select(c => $"[{c.ColumnName}]"));
                var valInsertList = string.Join(", ", meta.InsertableColumns.Select(c => $"Src.[{c.ColumnName}]"));
                var updateSetList = string.Join(", ", meta.UpdatableColumns.Select(c => $"Tgt.[{c.ColumnName}] = Src.[{c.ColumnName}]"));
                var matchingCriteria = string.Join(" AND ", meta.PrimaryKeys.Select(pk => $"Tgt.[{pk.ColumnName}] = Src.[{pk.ColumnName}]"));

                var mergeSql = $"""
                    ;WITH IngestionSource AS (
                        {sqlBuilder}
                    )
                    MERGE {meta.FullTableName} WITH (HOLDLOCK) AS Tgt
                    USING IngestionSource AS Src
                    ON ({matchingCriteria})
                    WHEN MATCHED THEN
                        UPDATE SET {updateSetList}
                    WHEN NOT MATCHED THEN
                        INSERT ({colInsertList}) VALUES ({valInsertList});
                    """;

                cmd.CommandText = mergeSql;
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
        public async Task<int> UpdateComposite<T>(IDatabaseProvider provider, T entity) where T : class, new()
        {
            var meta = _descriptorCache.GetOrAdd(typeof(T), _ => EntityScanner.Scan<T>());
            if (!meta.PrimaryKeys.Any())
                throw new InvalidOperationException($"No Primary Keys registered on entity {typeof(T).Name}");

            var setAssignments = string.Join(", ", meta.UpdatableColumns.Select(c => $"[{c.ColumnName}] = @Param_{c.PropertyName}"));
            var sqlBuilder = new StringBuilder($"UPDATE {meta.FullTableName} SET {setAssignments} WHERE ");

            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            using var cmd = new SqlCommand("", conn);

            // Map all properties to parameter buffers
            foreach (var col in meta.Columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new SqlParameter($"@Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            // Build compound composite match string
            var whereClauses = new List<string>();
            foreach (var pk in meta.PrimaryKeys)
            {
                whereClauses.Add($"[{pk.ColumnName}] = @Param_{pk.PropertyName}");
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
        public async Task<int> Delete<T, K>(IDatabaseProvider provider, K id) where T : class, new()
        {
            var meta = GetMetadata<T>();
            var pk = meta.PrimaryKeys.FirstOrDefault()
                ?? throw new InvalidOperationException($"No Primary Key defined for {typeof(T).Name}");

            var sql = $"DELETE FROM {meta.FullTableName} WHERE [{pk.ColumnName}] = @Id;";
            using var conn = new SqlConnection(provider.Connection.ConnectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@Id", (object)id ?? DBNull.Value));

            await conn.OpenAsync().ConfigureAwait(false);
            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private async Task<DataTable> ExecuteCommandToDataTable(IDatabaseProvider provider, SqlCommand cmd, SqlConnection conn)
        {
            // Leverages your custom unique column mapper loops cleanly inside an isolated data retrieval stream
            await conn.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            DataTable dt = new DataTable(); var columnNames = GeneralExtensions.GetUniqueColumnNames(reader);
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
        private EntityDescriptor GetMetadata<T>() where T : class
        {
            return _descriptorCache.GetOrAdd(typeof(T), _ => EntityScanner.Scan<T>());
        }
        #endregion
    }
}
