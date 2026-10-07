using JEO3.Core;
using JEO3.Engine;
using JEO3.Logging;
using JEO3.Providers;
using JEO3.Schema;
using Microsoft.Extensions.Caching.Hybrid;

namespace JEO3.Site.Infrastructure
{
    public sealed class SchemaCacheService : ISchemaCacheService
    {
        private readonly HybridCache _cache;
        public SchemaCacheService(HybridCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }
        public async Task<DatabaseContext?> GetDatabaseSchemaAsync(IDatabaseProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(IDatabaseProvider));
            if (provider.Connection == null || provider.Connection.ConnectionString == SQLConstants.DefaultSqlExpressConnection) throw new ArgumentNullException(nameof(IDatabaseProvider));

            try
            {
                // Include a server-discriminating component in the key so two providers pointing
                // at the same database name on different servers get separate cache entries.
                // IConnectionDto only exposes ConnectionString, so hash it for a stable key.
                string cacheKey = $"SchemaMetadata_{provider.Connection.Database}_{Math.Abs(provider.Connection.ConnectionString.GetHashCode())}";
                var ctx = await _cache.GetOrCreateAsync(cacheKey, async token =>
                {
                    var x = await StagingContextFactory.GetFlatContext(provider);
                    return x;
                });

                DatabaseContext context = DatabaseContextFactory.GetContext(ctx, provider);
                return context;
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex);
                throw;
            }
        }
    }
}