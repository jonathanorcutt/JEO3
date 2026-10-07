using System.Data;
using System.Text;
using JEO3.Providers.Catalogs;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Types;

namespace JEO3.Providers
{
    public sealed class SqlServerDatabaseProvider : DatabaseProvider
    {
        #region Initialization
        internal SqlServerDatabaseProvider(IConnectionDto connection)
            : base(connection)
        {
            SchemaCatalog = new MSSchemaQueryCatalog();
        }
        #endregion

        #region Functions
        public override async Task<DataSet> GetDataset(string query, CommandType commandType = CommandType.Text)
        {
            try
            {
                var dataSet = new DataSet();

                await using var connection = new SqlConnection(Connection?.ConnectionString);
                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;

                    await connection.OpenAsync();
                    await using var reader = await command.ExecuteReaderAsync();

                    // Iterate through result sets
                    do
                    {
                        var dt = new DataTable();

                        // Build columns from reader metadata
                        var fieldCount = reader.FieldCount;
                        var columnTypeNames = new string[fieldCount];
                        for (int i = 0; i < fieldCount; i++)
                        {
                            var typeName = reader.GetDataTypeName(i);
                            columnTypeNames[i] = typeName;

                            Type columnType;
                            switch (typeName?.ToLower())
                            {
                                case "hierarchyid": columnType = typeof(SqlHierarchyId); break;
                                case "geometry": columnType = typeof(SqlGeometry); break;
                                case "geography": columnType = typeof(SqlGeography); break;
                                default:
                                    Type type = reader.GetFieldType(i);
                                    columnType = type == typeof(byte[]) ? typeof(string) : type ?? typeof(object);
                                    break;
                            }

                            dt.Columns.Add(reader.GetName(i), columnType);
                        }

                        // Read rows asynchronously
                        while (await reader.ReadAsync())
                        {
                            var row = dt.NewRow();
                            for (int i = 0; i < fieldCount; i++)
                            {
                                var typeName = columnTypeNames[i];
                                if (await reader.IsDBNullAsync(i))
                                {
                                    row[i] = DBNull.Value;
                                    continue;
                                }

                                switch (typeName?.ToLower())
                                {
                                    case "hierarchyid": row[i] = reader.GetFieldValue<SqlHierarchyId>(i); break;
                                    case "geometry": row[i] = reader.GetFieldValue<SqlGeometry>(i); break;
                                    case "geography": row[i] = reader.GetFieldValue<SqlGeography>(i); break;
                                    default:
                                        var fieldType = reader.GetFieldType(i);
                                        if (fieldType == typeof(byte[]))
                                        {
                                            var data = (byte[])reader.GetValue(i);
                                            row[i] = Encoding.ASCII.GetString(data);
                                        }
                                        else
                                        {
                                            row[i] = reader.GetValue(i);
                                        }
                                        break;
                                }
                            }
                            dt.Rows.Add(row);
                        }

                        dataSet.Tables.Add(dt);
                    }
                    while (await reader.NextResultAsync());

                    await connection.CloseAsync();
                }

                return dataSet;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override async Task<DataTable> GetDataTable(string query, CommandType commandType = CommandType.Text)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this.Connection?.ConnectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            DataTable dt = new DataTable();
                            var columnNames = GetUniqueColumnNames(reader);

                            // Column Metadata Loop
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string typeName = reader.GetDataTypeName(i);
                                Type columnType;

                                switch (typeName.ToLower())
                                {
                                    case "hierarchyid": columnType = typeof(SqlHierarchyId); break;
                                    case "geometry": columnType = typeof(SqlGeometry); break;
                                    case "geography": columnType = typeof(SqlGeography); break;
                                    default:
                                        Type type = reader.GetFieldType(i);
                                        columnType = type == typeof(byte[]) ? typeof(string) : type ?? typeof(object);
                                        break;
                                }
                                dt.Columns.Add(columnNames[i], columnType);
                            }

                            // Row Parsing Loop
                            while (await reader.ReadAsync())
                            {
                                DataRow row = dt.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string typeName = reader.GetDataTypeName(i);

                                    if (reader.IsDBNull(i))
                                    {
                                        row[i] = DBNull.Value;
                                        continue;
                                    }

                                    switch (typeName.ToLower())
                                    {
                                        case "hierarchyid": row[i] = reader.GetFieldValue<SqlHierarchyId>(i); break;
                                        case "geometry": row[i] = reader.GetFieldValue<SqlGeometry>(i); break;
                                        case "geography": row[i] = reader.GetFieldValue<SqlGeography>(i); break;
                                        default:
                                            Type type = reader.GetFieldType(i);
                                            if (type == typeof(byte[]))
                                            {
                                                var data = (byte[])reader.GetValue(i);
                                                row[i] = Encoding.ASCII.GetString(data);
                                            }
                                            else
                                            {
                                                row[i] = reader.GetValue(i);
                                            }
                                            break;
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
                await using var conn = new SqlConnection(this.Connection?.ConnectionString);
                await using var cmd = new SqlCommand(query, conn);

                cmd.CommandType = commandType == CommandType.StoredProcedure
                    ? System.Data.CommandType.StoredProcedure
                    : System.Data.CommandType.Text;

                await conn.OpenAsync();

                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public override async Task<int?> ExecuteScalar(string query, CommandType commandType = CommandType.Text)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(this.Connection?.ConnectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
                        cmd.CommandText = query;
                        object result = await cmd.ExecuteScalarAsync();

                        if (result == null || result == DBNull.Value)
                        {
                            return null;
                        }

                        // Convert safely to int
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
    }
}