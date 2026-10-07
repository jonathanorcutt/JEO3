using System.Configuration;
using System.Runtime.ExceptionServices;
using JEO3.Logging;

namespace JEO3.Site.Extensions
{
    internal static class RegistrationExtensions
    {
        internal static List<ConnectionStringSettings> PopulateConnectionStrings(this WebApplicationBuilder builder)
        {
            // ── DYNAMICALLY EXTRACT STRUCTURED JSON CONNECTION STRINGS ───────────
            var connectionSettingsList = new List<ConnectionStringSettings>();
            var connStringsSection = builder.Configuration.GetSection("ConnectionStrings");

            if (connStringsSection.Exists())
            {
                foreach (var child in connStringsSection.GetChildren())
                {
                    // Fall back to standard primitive string if it isn't a complex object layout
                    string connectionString = child["ConnectionString"] ?? child.Value ?? string.Empty;
                    string providerName = child["ProviderName"] ?? string.Empty;

                    connectionSettingsList.Add(new ConnectionStringSettings(child.Key, connectionString, providerName));
                }
            }
            return connectionSettingsList;
        }
        internal static void RegisterPersonalStuff(this WebApplication app)
        {
            // TIMELAPSE PROJECT
            app.MapGet("/videos/video.webm", (string filename) =>
            {
                var path = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "assets", filename);
                return Results.File(path, "video/webm");
            }).AllowAnonymous();

            // TIMELAPSE PROJECT
            app.MapGet("/videos/matrix.webm", (string filename) =>
            {
                var path = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "assets", filename);
                return Results.File(path, "video/webm");
            }).AllowAnonymous();
        }
        internal static void RegisterExceptionHandling(this WebApplication app, string connString)
        {
            ExceptionUtility.Register(connString);
            AppDomain.CurrentDomain.FirstChanceException += new EventHandler<FirstChanceExceptionEventArgs>((o, e) =>
            {
                ExceptionUtility.LogException(e.Exception);
            });
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((o, e) =>
            {
                ExceptionUtility.LogException((Exception)e.ExceptionObject);
            });
        }
    }
}
