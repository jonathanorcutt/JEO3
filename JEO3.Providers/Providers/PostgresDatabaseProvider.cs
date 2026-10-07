using System.Data;
using System.Text;
using JEO3.Providers.Catalogs;
using Npgsql;

namespace JEO3.Providers
{
    public sealed class PostgresDatabaseProvider : DatabaseProvider
    {
        #region Initialization
        internal PostgresDatabaseProvider(IConnectionDto connection)
            : base(connection)
        {
            this.SchemaCatalog = new PostgresSchemaQueryCatalog();
        }
        #endregion

        #region Functions
        public override async Task<DataSet> GetDataset(string query, CommandType commandType = CommandType.Text)
        {
            DataSet dataSet = new DataSet();
            using (NpgsqlConnection connection = new NpgsqlConnection(this.Connection?.ConnectionString))
            {
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataSet);
                    }
                }
            }
            return dataSet;
        }
        public override async Task<DataTable> GetDataTable(string query, CommandType commandType = CommandType.Text)
        {
            try
            {
                using (var conn = new NpgsqlConnection(this.Connection?.ConnectionString))
                {
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            DataTable dt = new DataTable();
                            var columnNames = GetUniqueColumnNames(reader);

                            // Build Schema Columns Safely
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Type type = reader.GetFieldType(i);

                                // Handle byte[] translation to string, fallback cleanly to object for unmapped engine types
                                Type columnType = (type == typeof(byte[])) ? typeof(string) : (type ?? typeof(object));
                                dt.Columns.Add(columnNames[i], columnType);
                            }

                            // 2. Stream the Rows
                            while (await reader.ReadAsync())
                            {
                                DataRow row = dt.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    if (await reader.IsDBNullAsync(i)) // Use Async variant here to prevent blocking thread pool
                                    {
                                        row[i] = DBNull.Value;
                                        continue;
                                    }

                                    Type type = reader.GetFieldType(i);
                                    if (type == typeof(byte[]))
                                    {
                                        // Use Postgres-safe direct generic buffer reading
                                        var data = await reader.GetFieldValueAsync<byte[]>(i);
                                        row[i] = Encoding.ASCII.GetString(data);
                                    }
                                    else
                                    {
                                        row[i] = reader.GetValue(i);
                                    }
                                }
                                dt.Rows.Add(row);
                            }
                            await conn.CloseAsync();
                            await conn.DisposeAsync();
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region Volatile
        public override async Task<int?> ExecuteNonQuery(string query, CommandType commandType = CommandType.Text)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(this.Connection?.ConnectionString))
                {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
                        cmd.CommandText = query;
                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override async Task<int?> ExecuteScalar(string query, CommandType commandType = CommandType.Text) { throw new NotImplementedException(); }
        #endregion

    }
}