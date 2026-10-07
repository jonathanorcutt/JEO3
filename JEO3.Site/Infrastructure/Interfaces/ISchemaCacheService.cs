using JEO3.Providers;
using JEO3.Schema;

namespace JEO3.Site.Infrastructure
{
    public interface ISchemaCacheService
    {
        Task<DatabaseContext> GetDatabaseSchemaAsync(IDatabaseProvider provider);
    }
}
