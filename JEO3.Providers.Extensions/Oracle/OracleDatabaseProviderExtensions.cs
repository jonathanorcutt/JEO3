using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using JEO3.Providers;
using JEO3.Extensions;

namespace JEO3.Providers.Extensions.Oracle
{
    internal sealed class OracleDatabaseProviderExtensions : IProviderOperations
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
            return await provider.GetInstances<T>($"SELECT * FROM {QualifiedTable(meta)}");
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

            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            var sql = $"SELECT * FROM {QualifiedTable(meta)} WHERE {Q(pk.ColumnName)} = :Id";
            using var cmd = new OracleCommand(sql, conn) { BindByName = true };
            cmd.Parameters.Add(new OracleParameter("Id", (object)id ?? DBNull.Value));

            var table = await ExecuteCommandToDataTable(provider, cmd, conn);
            var list = table.ToList<T>();
            return list.Count > 0 ? list[0] : null;
        }
        /// <summary>
        /// Get By IDs - Requires use of class and key attribute decorations JeoTable and JeoKey.
        /// NOTE: Oracle limits an IN (...) list to 1000 elements (ORA-01795) - this does not chunk
        /// the input, so callers passing more than 1000 ids will hit that error.
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

            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {QualifiedTable(meta)} WHERE {Q(pk.ColumnName)} IN (");
            using var cmd = new OracleCommand("", conn) { BindByName = true };

            var paramNames = new List<string>();
            int idx = 0;
            foreach (var id in ids)
            {
                var pName = $"PkId_{idx++}";
                paramNames.Add($":{pName}");
                cmd.Parameters.Add(new OracleParameter(pName, (object)id ?? DBNull.Value));
            }

            sqlBuilder.Append(string.Join(", ", paramNames)).Append(')');
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

            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {QualifiedTable(meta)} WHERE ");
            using var cmd = new OracleCommand("", conn) { BindByName = true };

            var whereClauses = new List<string>();
            for (int i = 0; i < meta.PrimaryKeys.Count; i++)
            {
                var pk = meta.PrimaryKeys[i];
                var paramName = $"CompPk_{i}";
                var val = pk.Property.GetValue(criteriaEntity);

                whereClauses.Add($"{Q(pk.ColumnName)} = :{paramName}");
                cmd.Parameters.Add(new OracleParameter(paramName, val ?? DBNull.Value));
            }

            sqlBuilder.Append(string.Join(" AND ", whereClauses));
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

            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            var sql = $"SELECT * FROM {QualifiedTable(meta)} WHERE {Q(fkColumnName)} = :FkValue";
            using var cmd = new OracleCommand(sql, conn) { BindByName = true };
            cmd.Parameters.Add(new OracleParameter("FkValue", (object)value ?? DBNull.Value));

            var table = await ExecuteCommandToDataTable(provider, cmd, conn);
            return table.ToList<T>();
        }
        /// <summary>
        /// Get By FK - Requires use of class and key attribute decorations JeoTable and JeoKey.
        /// NOTE: Oracle limits an IN (...) list to 1000 elements (ORA-01795) - this does not chunk
        /// the input, so callers passing more than 1000 values will hit that error.
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

            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            var sqlBuilder = new StringBuilder($"SELECT * FROM {QualifiedTable(meta)} WHERE {Q(fkColumnName)} IN (");
            using var cmd = new OracleCommand("", conn) { BindByName = true };

            var paramNames = new List<string>();
            int idx = 0;
            foreach (var val in values)
            {
                var pName = $"FkVal_{idx++}";
                paramNames.Add($":{pName}");
                cmd.Parameters.Add(new OracleParameter(pName, (object)val ?? DBNull.Value));
            }

            sqlBuilder.Append(string.Join(", ", paramNames)).Append(')');
            cmd.CommandText = sqlBuilder.ToString();

            var table = await ExecuteCommandToDataTable(provider, cmd, conn);
            return table.ToList<T>();
        }
        /// <summary>
        /// Create - Requires use of class and key attribute decorations JeoTable and JeoKey.
        /// DB-generated keys use Oracle's RETURNING ... INTO output-parameter form (Oracle has no
        /// SCOPE_IDENTITY() equivalent); this assumes the PK is populated via an identity column or
        /// trigger/sequence default, which is returned through an output bind rather than a scalar query.
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

            var colNames = string.Join(", ", columns.Select(c => Q(c.ColumnName)));
            var paramNames = string.Join(", ", columns.Select(c => $":Param_{c.PropertyName}"));

            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            using var cmd = new OracleCommand("", conn) { BindByName = true };

            foreach (var col in columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new OracleParameter($"Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            if (pk.IsDbGenerated)
            {
                var outParam = new OracleParameter("OutId", OracleDbType.Decimal, ParameterDirection.Output);
                cmd.Parameters.Add(outParam);

                cmd.CommandText = $"INSERT INTO {QualifiedTable(meta)} ({colNames}) VALUES ({paramNames}) RETURNING {Q(pk.ColumnName)} INTO :OutId";

                await conn.OpenAsync().ConfigureAwait(false);
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

                if (outParam.Value is OracleDecimal oracleDecimal && !oracleDecimal.IsNull)
                {
                    var generatedId = (K)Convert.ChangeType(oracleDecimal.Value, typeof(K));
                    pk.Property.SetValue(entity, generatedId);
                    return generatedId;
                }
                return default!;
            }

            // For manual key tracking parameters
            var directId = (K)pk.Property.GetValue(entity)!;
            cmd.CommandText = $"INSERT INTO {QualifiedTable(meta)} ({colNames}) VALUES ({paramNames})";

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

            var setAssignments = string.Join(", ", meta.UpdatableColumns.Select(c => $"{Q(c.ColumnName)} = :Param_{c.PropertyName}"));
            var sql = $"UPDATE {QualifiedTable(meta)} SET {setAssignments} WHERE {Q(pk.ColumnName)} = :Param_{pk.PropertyName}";

            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            using var cmd = new OracleCommand(sql, conn) { BindByName = true };

            foreach (var col in meta.Columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new OracleParameter($"Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            await conn.OpenAsync().ConfigureAwait(false);
            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        /// <summary>
        /// Upsert Range - Requires use of class and key attribute decorations JeoTable and JeoKey.
        /// Uses Oracle's native MERGE (the statement SQL Server's MERGE was itself modeled on), with
        /// each source row expressed as "SELECT ... FROM DUAL" since Oracle requires a FROM clause.
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

            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            await conn.OpenAsync().ConfigureAwait(false);

            for (int b = 0; entityList.Count > b; b += batchSize)
            {
                var batch = entityList.Skip(b).Take(batchSize).ToList();
                using var cmd = new OracleCommand("", conn) { BindByName = true };
                var sqlBuilder = new StringBuilder();

                for (int rowIdx = 0; batch.Count > rowIdx; rowIdx++)
                {
                    var entity = batch[rowIdx];
                    var selectCols = new List<string>();

                    foreach (var col in meta.Columns)
                    {
                        var paramName = $"Param_B{b}_R{rowIdx}_{col.PropertyName}";
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

                        cmd.Parameters.Add(new OracleParameter(paramName, val ?? DBNull.Value));
                        selectCols.Add($":{paramName} AS {Q(col.ColumnName)}");
                    }

                    sqlBuilder.AppendLine($"SELECT {string.Join(", ", selectCols)} FROM DUAL");
                    if (rowIdx < batch.Count - 1) sqlBuilder.AppendLine("UNION ALL");
                }

                var colInsertList = string.Join(", ", meta.InsertableColumns.Select(c => Q(c.ColumnName)));
                var valInsertList = string.Join(", ", meta.InsertableColumns.Select(c => $"Src.{Q(c.ColumnName)}"));
                var updateSetList = string.Join(", ", meta.UpdatableColumns.Select(c => $"Tgt.{Q(c.ColumnName)} = Src.{Q(c.ColumnName)}"));
                var matchingCriteria = string.Join(" AND ", meta.PrimaryKeys.Select(pk => $"Tgt.{Q(pk.ColumnName)} = Src.{Q(pk.ColumnName)}"));

                var mergeSql = $"""
                    MERGE INTO {QualifiedTable(meta)} Tgt
                    USING (
                        {sqlBuilder}
                    ) Src
                    ON ({matchingCriteria})
                    WHEN MATCHED THEN
                        UPDATE SET {updateSetList}
                    WHEN NOT MATCHED THEN
                        INSERT ({colInsertList}) VALUES ({valInsertList})
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

            var setAssignments = string.Join(", ", meta.UpdatableColumns.Select(c => $"{Q(c.ColumnName)} = :Param_{c.PropertyName}"));
            var sqlBuilder = new StringBuilder($"UPDATE {QualifiedTable(meta)} SET {setAssignments} WHERE ");

            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            using var cmd = new OracleCommand("", conn) { BindByName = true };

            foreach (var col in meta.Columns)
            {
                var val = col.Property.GetValue(entity);
                cmd.Parameters.Add(new OracleParameter($"Param_{col.PropertyName}", val ?? DBNull.Value));
            }

            var whereClauses = new List<string>();
            foreach (var pk in meta.PrimaryKeys)
            {
                whereClauses.Add($"{Q(pk.ColumnName)} = :Param_{pk.PropertyName}");
            }

            sqlBuilder.Append(string.Join(" AND ", whereClauses));
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

            var sql = $"DELETE FROM {QualifiedTable(meta)} WHERE {Q(pk.ColumnName)} = :Id";
            using var conn = new OracleConnection(provider.Connection.ConnectionString);
            using var cmd = new OracleCommand(sql, conn) { BindByName = true };
            cmd.Parameters.Add(new OracleParameter("Id", (object)id ?? DBNull.Value));

            await conn.OpenAsync().ConfigureAwait(false);
            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        private async Task<DataTable> ExecuteCommandToDataTable(IDatabaseProvider provider, OracleCommand cmd, OracleConnection conn)
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
        private EntityDescriptor GetMetadata<T>() where T : class
        {
            return _descriptorCache.GetOrAdd(typeof(T), _ => EntityScanner.Scan<T>());
        }
        /// <summary>
        /// Double-quotes an identifier for Oracle, preserving exact casing
        /// (Oracle folds unquoted identifiers to uppercase).
        /// </summary>
        private static string Q(string identifier) => $"\"{identifier}\"";
        /// <summary>
        /// Builds a schema-qualified, correctly-quoted table reference.
        /// Deliberately does NOT use EntityDescriptor.FullTableName, which is hardcoded
        /// to SQL Server bracket syntax ([schema].[table]) and is not valid in Oracle.
        /// </summary>
        private static string QualifiedTable(EntityDescriptor meta)
        {
            var schema = string.IsNullOrEmpty(EntityDescriptor.DefaultSchemaOverride) ? meta.Schema : EntityDescriptor.DefaultSchemaOverride;
            return $"{Q(schema)}.{Q(meta.TableName)}";
        }
        #endregion
    }
}
