using System.Diagnostics;
using JEO3.Core;
using JEO3.Logging;
using JEO3.Providers;

namespace JEO3.Monitor.Services.API
{
    internal class SqlInfrastructureBuilder
    {
        private MonitorSettings _settings { get; set; } = new();
        private const string MonitorDbName = "Monitor";
       

        internal SqlInfrastructureBuilder(MonitorSettings settings)
        {
            _settings = settings;
        }

        internal async Task Scaffold()
        {
            try
            {
                if (!await ImplementSchema()) { return; }
                if (!await PurgeHistoricalData(24)) { return; }
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
            }
        }

        private async Task<bool> ImplementSchema()
        {
            DateTime now = DateTime.Now;
            if (_settings.ConnectionString.Contains(MonitorDbName) == false)
            {
                throw new Exception($"Error: Monitor database {MonitorDbName} not found in connection string. Exiting..");
            }

            // Create Database
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_settings.ConnectionString);
            builder.InitialCatalog = "master";
            string masterConnString = builder.ConnectionString;

            var masterProvider = DatabaseProvider.From(_settings.ProviderName, masterConnString);
            await masterProvider.ExecuteNonQuery(string.Format(SqlScaffoldingQueries.CreateDatabaseQuery, MonitorDbName));

            // --------------------------------------------

            // Update Monitor Instance Properties
            DateTime startTime = DateTime.UtcNow;
            _settings.TimestampLastActive = startTime;
            _settings.TimestampLastRegistered = startTime;
            // Create Schema Names
            var queries = new string[]
            {
                //string.Format(SqlScaffoldingQueries.DropDatabaseQuery, MonitorDbName),
                string.Format(SqlScaffoldingQueries.CreateDatabaseQuery, MonitorDbName)
            }.ToList();
            foreach (var query in queries)
            {
                try
                {
                    await masterProvider.ExecuteNonQuery(query);
                }
                catch (Exception ex)
                {
                    await ExceptionUtility.LogExceptionAsync(ex);
                    return false;
                }
            }

            queries = Array.Empty<string>().ToList();
            queries = queries.Concat(new string[] { "Dev", "Prod", "Configuration" }.Select(v => string.Format(SqlScaffoldingQueries.CreateSchemaNameQuery, v))).ToList();

            // Create Tables
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateTablesQuery, EnvironmentResolver.DevelopmentPrefix) }).ToList();
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateTablesQuery, EnvironmentResolver.ProductionPrefix) }).ToList();
            // Create Procedure HIGH
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateProcessProcedureHigh, EnvironmentResolver.DevelopmentPrefix) }).ToList();
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateProcessProcedureHigh, EnvironmentResolver.ProductionPrefix) }).ToList();
            // Create Procedure MEDIUM
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateProcessProcedureMedium, EnvironmentResolver.DevelopmentPrefix) }).ToList();
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateProcessProcedureMedium, EnvironmentResolver.ProductionPrefix) }).ToList();
            // Create Procedure LOW
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateProcessProcedureLow, EnvironmentResolver.DevelopmentPrefix) }).ToList();
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateProcessProcedureLow, EnvironmentResolver.ProductionPrefix) }).ToList();
            // Views
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateTableRowCountsView, EnvironmentResolver.DevelopmentPrefix) }).ToList();
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.CreateTableRowCountsView, EnvironmentResolver.ProductionPrefix) }).ToList();
            // Exceptions
            queries = queries.Concat(new[] {SqlScaffoldingQueries.CreateExceptionLogProcedure }).ToList();

            // Ensure Instance Entry
            queries = queries.Concat(new[] { string.Format(SqlScaffoldingQueries.InsertUpdateMonitorInstanceQuery, _settings.UserName, _settings.MachineName, _settings.ApiBaseUrl, _settings.TimestampLastRegistered, _settings.TimestampLastActive) }).ToList();

            // EXECUTE Everything In (2)
            var provider = DatabaseProvider.From(_settings.ProviderName, _settings.ConnectionString);

            foreach (var query in queries)
            {
                try
                {
                    DateTime qnow = DateTime.Now;
                    await provider.ExecuteNonQuery(query);


                    var qlog = (DateTime.Now - qnow).TotalSeconds;
                    Trace.WriteLine($"{query.Substring(0, query.Length > 100 ? 100 : query.Length)} - Elapsed {qlog}sec");
                }
                catch (Exception ex)
                {
                    await ExceptionUtility.LogExceptionAsync(ex);
                    return false;
                }
            }


            var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
            Trace.WriteLine($"GetFlatContext - Elapsed {log}sec");


            return true;
        }
        public async Task<bool> PurgeHistoricalData(int hours)
        {
            try
            {
                DateTime now = DateTime.Now;
                var provider = DatabaseProvider.From(_settings.ProviderName, _settings.ConnectionString);
                await provider.ExecuteNonQuery(string.Format(SqlScaffoldingQueries.PurgeDataQuery, EnvironmentResolver.DevelopmentPrefix, hours));
                await provider.ExecuteNonQuery(string.Format(SqlScaffoldingQueries.PurgeDataQuery, EnvironmentResolver.ProductionPrefix, hours));

                var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
                Trace.WriteLine($"GetFlatContext - Elapsed {log}sec");

                return true;
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
                return false;
            }
        }
    }
}
