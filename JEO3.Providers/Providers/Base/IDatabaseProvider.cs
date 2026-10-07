using System.Data;
using JEO3.Providers.Catalogs;

namespace JEO3.Providers
{
    public interface IDatabaseProvider
    {
        IConnectionDto Connection { get; }
        ISchemaQueryCatalog SchemaCatalog { get; }

        Task<int?> ExecuteNonQuery(string query, CommandType commandType = CommandType.Text);
        Task<DataSet> GetDataset(string query, CommandType commandType = CommandType.Text);
        Task<DataTable> GetDataTable(string query, CommandType commandType = CommandType.Text);
        Task<IReadOnlyList<T>> GetInstances<T>(string query, CommandType commandType = CommandType.Text) where T : class, new();
        Task<IReadOnlyList<T>> GetInstances<T>(SchemaQueryType type, CommandType commandType = CommandType.Text) where T : class, new();
    }
}