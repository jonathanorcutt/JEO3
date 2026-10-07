using JEO3.Core;
using Microsoft.Extensions.Configuration;

namespace JEO3.Monitor
{
    public sealed class MonitorSettings
    {
        public const string MonitoringSectionName = "Monitoring";
        public const string ApiBaseUrlName = nameof(ApiBaseUrl);
        public const string ConnectionStringName = nameof(ConnectionString);

        public long Id { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ApiBaseUrl { get; private set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public int PollIntervalSeconds { get; set; } = 3;
        public int LookbackPaddingSeconds { get; set; } = 2;
        public int HistoryCapacity { get; set; } = 300;
        public int DeadlockCapacity { get; set; } = 50;
        public EnvironmentType EnvironmentType { get; set; }
        public string TablePrefix { get; set; } = string.Empty;
        public DateTime TimestampLastRegistered { get; set; }
        public DateTime TimestampLastActive { get; set; }
        public void Load(IConfiguration config)
        {
            EnvironmentType = EnvironmentResolver.ResolveEnvironment(config);
            ApiBaseUrl = EnvironmentResolver.ResolveURI(config, EnvironmentType);
            TablePrefix = EnvironmentResolver.ResolveTablePrefix(EnvironmentType);
            MachineName = Environment.MachineName;
            UserName = Environment.UserName;
        }
    }
}
