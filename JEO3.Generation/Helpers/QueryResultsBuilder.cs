using System.Collections.Concurrent;
using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Generation
{
    public static class QueryResultsBuilder
    {
        #region All Results Helper
        public static async Task<IReadOnlyList<QueryGenerationResult>> GetAllTablesResult(this DatabaseContext context, QueryGenerationOptions options, CancellationToken token)
        {
            ConcurrentBag<QueryGenerationResult> results = [];

            await Parallel.ForEachAsync(context.Tables, async (table, token) =>
            {
                var gen = new DiagramDrivenGraphGenerator();
                QueryGenerationResult jarvisResult = gen.Generate(table, options);
                if (jarvisResult == null) return;
                jarvisResult.Coverage = CoverageCalculator.CalculateCoverage(table, context.Tables, context.Relations);
                results.Add(jarvisResult);
            });

            var ret = results.OrderBy(v => v.Tracker.Root.Table.TablePath).ToList();

            return ret;
        }
        #endregion
    }
}
