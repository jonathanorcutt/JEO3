using System.Data;
using System.Text;
using JEO3.Providers.Catalogs;
using Oracle.ManagedDataAccess.Client;

namespace JEO3.Providers
{
    public sealed class OracleDatabaseProvider : DatabaseProvider
    {
        #region Initialization
        internal OracleDatabaseProvider(IConnectionDto connection)
            : base(connection)
        {
            this.SchemaCatalog = new OracleSchemaQueryCatalog();
        }
        #endregion

        #region Functions
        public override async Task<DataSet> GetDataset(string query, CommandType commandType = CommandType.Text)
        {
            DataSet dataSet = new DataSet();
            using (OracleConnection connection = new OracleConnection(this.Connection?.ConnectionString))
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
                    using (OracleDataAdapter adapter = new OracleDataAdapter(command))
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
                using (var conn = new OracleConnection(this.Connection?.ConnectionString))
                {
                    using (var cmd = new OracleCommand(query, conn))
                    {
                        await conn.OpenAsync();

                        // Execute the reader synchronously to prevent driver-level threading deadlocks
                        using (var reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            var columnNames = GetUniqueColumnNames(reader);

                            // Build Schema Columns Safely
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Type type = reader.GetFieldType(i);

                                // Intercept binary blobs or Oracle's custom vendor structures
                                Type columnType = (type == typeof(byte[]) || type.Name.Contains("Blob") || type.Name.Contains("Binary"))
                                    ? typeof(string)
                                    : type;

                                dt.Columns.Add(columnNames[i], columnType);
                            }

                            // 2. Stream the Rows
                            while (reader.Read())
                            {
                                DataRow row = dt.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    // MUST use synchronous check for Oracle driver stability
                                    if (reader.IsDBNull(i))
                                    {
                                        row[i] = DBNull.Value;
                                        continue;
                                    }

                                    object rawValue = reader.GetValue(i);
                                    string typeName = rawValue.GetType().Name;

                                    // Extract binary strings cleanly without using generic GetFieldValue traps
                                    if (rawValue is byte[] bytes)
                                    {
                                        row[i] = Encoding.ASCII.GetString(bytes);
                                    }
                                    else if (typeName.Contains("Blob") || typeName.Contains("Binary"))
                                    {
                                        // Handle Oracle's proprietary BLOB/RAW stream values safely
                                        using (var stream = reader.GetStream(i))
                                        using (var ms = new MemoryStream())
                                        {
                                            stream.CopyTo(ms);
                                            row[i] = Encoding.ASCII.GetString(ms.ToArray());
                                        }
                                    }
                                    else
                                    {
                                        // Coerce Oracle special numbers (OracleDecimal, etc.) straight to standard .NET native values
                                        row[i] = Convert.IsDBNull(rawValue) ? DBNull.Value : rawValue;
                                    }
                                }
                                dt.Rows.Add(row);
                            }
                            await conn.CloseAsync();
                            await conn.DisposeAsync();
                            // Yield execution back to the caller cleanly to satisfy the async signature
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                bool stop = true;

                throw;
            }
        }
        #endregion

        #region Volatile
        public override async Task<int?> ExecuteNonQuery(string query, CommandType commandType = CommandType.Text)
        {
            try
            {
                using (OracleConnection conn = new OracleConnection(this.Connection?.ConnectionString))
                {
                    await conn.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand(query, conn))
                    {
                        cmd.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
                        cmd.CommandText = query;
                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        public override async Task<int?> ExecuteScalar(string query, CommandType commandType = CommandType.Text) { throw new NotImplementedException(); }
        #endregion
    }
}