using System.Configuration;
using JEO3.Core;

namespace JEO3.Providers
{
    /// <summary>
    /// Only Way To Create A Database Provider
    /// </summary>
    public static class DatabaseProviderFactory
    {
        public static IDatabaseProvider ToDatabaseProvider(this ConnectionStringSettings connSetting)
            => connSetting == null ? throw new ArgumentNullException(nameof(connSetting)) : ToDatabaseProvider(connSetting.ProviderName, connSetting.ConnectionString);
        public static List<IDatabaseProvider> ToDatabaseProviderList(this List<ConnectionStringSettings> connSettings)
            => connSettings == null ? throw new ArgumentNullException(nameof(connSettings))
            : connSettings.Where(v => v.ConnectionString != null && v.ConnectionString != string.Empty && v.ConnectionString != "*" && v.ConnectionString != SQLConstants.DefaultSqlExpressConnection)
                .Select(v => v.ToDatabaseProvider()).ToList();
        public static IList<IDatabaseProvider> ToDatabaseProviderList(this ConnectionStringSettingsCollection connSettingList)
        {
            var connectionList = new List<IDatabaseProvider>();
            foreach (ConnectionStringSettings connSetting in connSettingList)
            {
                var provider = ToDatabaseProvider(connSetting.ProviderName, connSetting.ConnectionString);
                connectionList.Add(provider);
            }
            return connectionList;
        }
        internal static IDatabaseProvider ToDatabaseProvider(string providerName, string connectionString)
        {
            var dto = ConnectionFactory.GetConnectionDto(connectionString, providerName);
            switch (dto.ProviderType)
            {
                case DatabaseProviderType.MSSQL:
                    return new SqlServerDatabaseProvider(dto);
                case DatabaseProviderType.Oracle:
                    return new OracleDatabaseProvider(dto);
                case DatabaseProviderType.Postgres:
                    return new PostgresDatabaseProvider(dto);
                default:
                    throw new NotImplementedException($"Database provider '{providerName}' has not been implemented in JEO3.");
            }
        }
    }
}