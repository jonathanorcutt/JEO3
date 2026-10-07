using System.Data;
using System.Data.Common;
using JEO3.Providers.Catalogs;
using Microsoft.Data.Sqlite;

namespace JEO3.Providers
{
    public sealed class SqliteDatabasesProvider : DatabaseProvider
    {
        #region Initialization
        public SqliteDatabasesProvider(IConnectionDto connection)
            : base(connection)
        {
            this.SchemaCatalog = new MSSchemaQueryCatalog();
        }
        #endregion

        #region Functions
        public override async Task<DataSet> GetDataset(string query, CommandType commandType = CommandType.Text)
        {
            DataSet dataSet = new DataSet();
            using (SqliteConnection connection = new SqliteConnection(this.Connection?.ConnectionString))
            {
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
                    DbProviderFactory factory = DbProviderFactories.GetFactory(connection);
                    using (DbDataAdapter adapter = factory.CreateDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        await Task.Run(() => adapter.Fill(dataSet));
                    }
                }
            }
            return dataSet;
        }
        public override Task<DataTable> GetDataTable(string query, CommandType commandType = CommandType.Text)
        {
            DataTable dataTable = new DataTable();

            using (var connection = new SqliteConnection(this.Connection.ConnectionString))
            {
                connection.Open();
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        // Automatically maps the reader columns and rows to the DataTable
                        dataTable.Load(reader);
                    }
                }
            }

            return Task.FromResult(dataTable);
        }
        #endregion

        #region Volatile
        public override async Task<int?> ExecuteNonQuery(string query, CommandType commandType = CommandType.Text)
        {
            try
            {
                DataTable dataTable = new DataTable();

                using (var connection = new SqliteConnection(this.Connection.ConnectionString))
                {
                    connection.Open();
                    using (var cmd = new SqliteCommand(query, connection))
                    {
                        cmd.CommandType = commandType == CommandType.StoredProcedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
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
