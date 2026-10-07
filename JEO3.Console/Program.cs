using System.Configuration;
using JEO3.Code;
using JEO3.Engine;
using JEO3.Generation;
using JEO3.Generation.Models;
using JEO3.Providers;

namespace JEO3.Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Note - User Principals/Roles/Permissions/Synonymns - not loaded by default - not needed for portfolio demo..

            // Get Provider
            var baseProv = ConfigurationManager.ConnectionStrings[1].ToDatabaseProvider() ?? throw new Exception("Connection string not found.");

            // Create Context
            var ctx = await DatabaseContextFactory.GetContext(baseProv);

            // Get Query Generation Results
            var options = QueryGenerationOptions.GetDefaultGenerationOptions(GenerationMethod.PlanDrivenExecution);
            var results = await ctx.GetAllTablesResult(options, new());

            // See Generated Queries
            var queries = results.Select(v => v.GeneratedQuery).ToList();

            // Create Code Generator
            var gen = new CodeContextGenerator(ctx, "JEO3", results.ToList());

            // Create Db Context Code
            string dbContextResult = gen.Generate();
            var stop = true;
        }
    }
}