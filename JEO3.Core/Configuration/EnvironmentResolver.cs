using Microsoft.Extensions.Configuration;

namespace JEO3.Core
{
    public static class EnvironmentResolver
    {
        public const string DevelopmentPrefix = "Dev";
        public const string ProductionPrefix = "Prod";

        public static string ResolveTablePrefix(EnvironmentType environment) => environment == EnvironmentType.Development ? DevelopmentPrefix : ProductionPrefix;
        public static EnvironmentType ResolveEnvironment(IConfiguration configuration)
        {
            var urls = configuration["urls"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (string.IsNullOrWhiteSpace(urls)) return EnvironmentType.Production;

            var list = GetSanitizedUriList(urls);
            var httpUrl = urls.Split(';', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(u => u.StartsWith("http://", StringComparison.OrdinalIgnoreCase));
            if (httpUrl is null) return EnvironmentType.Production;

            var port = new Uri(httpUrl).Port;
            return port < 1000 ? EnvironmentType.Production : EnvironmentType.Development;
        }

        public static string ResolveURI(IConfiguration configuration, EnvironmentType environment)
        {
            var urls = configuration["urls"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            var list = GetSanitizedUriList(urls);

            if (list.Count == 0)
            {
                return environment == EnvironmentType.Production ? "http://localhost:82" : "http://localhost:5164";
            }

            var uri = environment == EnvironmentType.Production
                ? list.FirstOrDefault(u => u.Port < 1000) ?? list.First()
                : list.FirstOrDefault(v => v.Port > 1000 && v.AbsoluteUri.Contains("http:")) ?? list.First();

            return environment == EnvironmentType.Production ? uri.AbsoluteUri : uri.AbsoluteUri.Replace(uri.Port.ToString(), (uri.Port + 1).ToString());
        }
        private static List<Uri> GetSanitizedUriList(string? urls)
        {
            if (string.IsNullOrWhiteSpace(urls))
            {
                return new List<Uri>();
            }

            return urls.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Where(v => v != null)
                .Select(v => v.TrimStart('/', '\\').TrimEnd('/', '\\').Trim())
                .Select(v => new Uri(v)).ToList();
        }
    }
}
