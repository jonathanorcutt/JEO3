using System.Configuration;
using System.Data.Common;
using System.Text.RegularExpressions;
using JEO3.Providers.Dto;
using Microsoft.Data.SqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace JEO3.Providers
{
    public static class ConnectionFactory
    {
        // Root
        internal static DatabaseProviderType GetDatabaseProviderType(this IDatabaseProvider provider)
            => GetDatabaseProviderType(provider.Connection.ProviderName);
        internal static DatabaseProviderType GetDatabaseProviderType(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider Name cannot be null or empty.", nameof(providerName));

            // Normalize provider name for clean evaluation
            string normalizedProvider = providerName.ToLowerInvariant();

            string[] mssqlNames = new string[3] { "sqlclient", "microsoft", "mssql" };
            string[] oracleNames = new string[1] { "oracle" };
            string[] postgresNames = new string[2] { "npgsql", "postgresql" };

            if (mssqlNames.Any(v => normalizedProvider.Contains(v)))
            {
                return DatabaseProviderType.MSSQL;
            }
            else if (oracleNames.Any(v => normalizedProvider.Contains(v)))
            {
                return DatabaseProviderType.Oracle;
            }
            else if (postgresNames.Any(v => normalizedProvider.Contains(v)))
            {
                return DatabaseProviderType.Postgres;
            }

            return DatabaseProviderType.Unknown;
        }

        internal static IConnectionDto GetConnectionDto(string? connectionString, string? providerName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider Name cannot be null or empty.", nameof(providerName));

            DatabaseProviderType type = GetDatabaseProviderType(providerName);
            switch (type)
            {
                case DatabaseProviderType.MSSQL:
                    return GetConnectionDtoSql(connectionString, providerName);
                case DatabaseProviderType.Oracle:
                    return GetConnectionDtoOracle(connectionString, providerName);
                case DatabaseProviderType.Postgres:
                    return GetConnectionDtoPostgres(connectionString, providerName);
                default:
                    // Fallback (Generic key-value grabber)
                    return GetConnectionDtoDb(connectionString, providerName);
            }

            throw new NotImplementedException($"Database provider '{providerName}' has not been implemented in JEO3.");
        }

        // Implementations
        private static IConnectionDto GetConnectionDtoSql(string? connectionString, string? providerName)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return new ConnectionDto(DatabaseProviderType.MSSQL, connectionString, providerName, builder.DataSource, builder.InitialCatalog, builder.UserID, builder.Password);
        }
        private static IConnectionDto GetConnectionDtoOracle(string? connectionString, string? providerName)
        {
            var builder = new OracleConnectionStringBuilder(connectionString);

            // Oracle utilizes Service Names or SIDs to specify the database instance.
            // Look for inline patterns like (SERVICE_NAME=MyDb) or (SID=MyDb)
            var serviceNameMatch = Regex.Match(builder.DataSource, @"SERVICE_NAME\s*=\s*([a-zA-Z0-9_\-\.]+)", RegexOptions.IgnoreCase);
            var sidMatch = Regex.Match(builder.DataSource, @"SID\s*=\s*([a-zA-Z0-9_\-\.]+)", RegexOptions.IgnoreCase);
            var database = string.Empty;

            if (serviceNameMatch.Success)
            {
                database = serviceNameMatch.Groups[1].Value;
            }
            else if (sidMatch.Success)
            {
                database = sidMatch.Groups[1].Value;
            }
            else
            {
                // If it is just a plain TNS network alias (e.g., DataSource=PROD_DB), use the alias name
                database = builder.DataSource;
            }

            var dto = new ConnectionDto(DatabaseProviderType.Oracle, connectionString, providerName, builder.DataSource, database, builder.UserID, builder.Password);
            return dto;
        }
        private static IConnectionDto GetConnectionDtoPostgres(string? connectionString, string? providerName)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return new ConnectionDto(DatabaseProviderType.Postgres, connectionString, providerName, builder.Host, builder.Database, builder.Username, builder.Password);
        }
        private static IConnectionDto GetConnectionDtoDb(string? connectionString, string? providerName)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
            var server = GetConnectionStringBuilderValue(builder, "Server", "Data Source", "Host");
            var database = GetConnectionStringBuilderValue(builder, "Database", "Initial Catalog", "Service Name");
            var dto = new ConnectionDto(DatabaseProviderType.Unknown, connectionString, providerName, server, database, string.Empty, string.Empty);
            return dto;
        }

        // Helper
        private static string GetConnectionStringBuilderValue(DbConnectionStringBuilder builder, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (builder.TryGetValue(key, out object? value) && value != null)
                {
                    return value.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        // Extensions
        internal static IConnectionDto ToConnectionDto(this ConnectionStringSettings connectionSetting)
            => GetConnectionDto(connectionSetting?.ConnectionString, connectionSetting?.ProviderName);
        public static List<IConnectionDto> ToConnectionDtoList(this List<ConnectionStringSettings> connectionSettings)
            => connectionSettings.Select(v => v.ToConnectionDto()).ToList();
    }
}
