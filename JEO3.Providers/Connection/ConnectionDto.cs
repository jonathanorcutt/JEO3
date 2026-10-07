namespace JEO3.Providers.Dto
{
    public sealed class ConnectionDto : IConnectionDto
    {
        public string ConnectionString { get; private init; } = string.Empty;
        public string ProviderName { get; private init; } = string.Empty;
        public DatabaseProviderType ProviderType { get; private init; } = DatabaseProviderType.Unknown;
        public string Server { get; private init; } = string.Empty;
        public string Database { get; private init; } = string.Empty;
        public string UserId { get; private init; } = string.Empty;
        public string Password { get; private init; } = string.Empty;

        internal ConnectionDto(DatabaseProviderType providerType, string connectionString, string providerName, string server, string database, string userId, string password = "")
        {
            this.ConnectionString = connectionString;
            this.ProviderName = providerName;
            this.ProviderType = providerType;
            this.Server = server;
            this.Database = database;
            this.UserId = userId;
            this.Password = password;
        }
    }
}
