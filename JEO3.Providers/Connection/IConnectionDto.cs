namespace JEO3.Providers
{
    public interface IConnectionDto
    {
        string ConnectionString { get; }
        string ProviderName { get; }
        DatabaseProviderType ProviderType { get; }
        string Server { get; }
        string Database { get; }
        string UserId { get; }

        // string Password { get; }
    }
}
