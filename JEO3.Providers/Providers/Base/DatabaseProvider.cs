using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using JEO3.Extensions;
using JEO3.Providers.Catalogs;

namespace JEO3.Providers
{
    public abstract class DatabaseProvider : IDatabaseProvider
    {
        #region Properties
        public IConnectionDto Connection { get; private init; }
        public ISchemaQueryCatalog SchemaCatalog { get; protected init; }
        #endregion

        #region Initialization
        internal DatabaseProvider(IConnectionDto connection)
        {
            this.Connection = connection;
        }
        #endregion

        #region Functions
        public abstract Task<DataSet> GetDataset(string query, CommandType commandType = CommandType.Text);
        public abstract Task<DataTable> GetDataTable(string query, CommandType commandType = CommandType.Text);
        public Task<DataTable> GetDataTable(SchemaQueryType type, CommandType commandType = CommandType.Text) => GetDataTable(SchemaCatalog.GetQuery(type));
        public async Task<IReadOnlyList<T>> GetInstances<T>(SchemaQueryType type, CommandType commandType = CommandType.Text) where T : class, new()
            => await GetInstances<T>(SchemaCatalog.GetQuery(type));
        public async Task<IReadOnlyList<T>> GetInstances<T>(string query, CommandType commandType = CommandType.Text) where T : class, new()
        {
            var startTime = DateTime.Now;

            // Fetch the DataTable
            var table = await this.GetDataTable(query, commandType);

            // Convert DataTable to List of Type T
            List<T> list = table.ToList<T>();

            // Logging
            Debug.WriteLine($"Time To Load Type - {typeof(T).Name} via {this.GetType().Name} - {(decimal)Math.Round((DateTime.Now - startTime).TotalSeconds, 3)} seconds");

            return list;
        }
        #endregion

        #region Volatile
        public abstract Task<int?> ExecuteNonQuery(string query, CommandType commandType = CommandType.Text);
        public abstract Task<int?> ExecuteScalar(string query, CommandType commandType = CommandType.Text);
        #endregion

        #region Static
        public static IDatabaseProvider From(string providerName, string connectionString)
        {
            return DatabaseProviderFactory.ToDatabaseProvider(providerName, connectionString);
        }
        internal static List<string> GetUniqueColumnNames(IDataReader reader)
        {
            var result = new List<string>();
            var occurrences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);

                if (occurrences.TryGetValue(columnName, out int count))
                {
                    count++;
                    occurrences[columnName] = count;
                    columnName = $"{columnName}_{count}";
                }
                else
                {
                    occurrences[columnName] = 0;
                }

                result.Add(columnName);
            }

            return result;
        }
        #endregion
    }
}
