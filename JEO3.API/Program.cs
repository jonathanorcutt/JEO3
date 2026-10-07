using JEO3.API.Endpoints;
using JEO3.Core;
using JEO3.Monitor;
using JEO3.Providers;
using JEO3.Providers.Extensions;
using Microsoft.Extensions.Caching.Hybrid;
using Scalar.AspNetCore;

namespace JEO3.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthorization();
            builder.Services.AddOpenApi();

            // CORs
            builder.Services.AddCors(o => o.AddPolicy("dashboard", p =>
                p.WithOrigins("http://localhost", "https://localhost") // JEO3.Site's actual 80/443 origin(s), no port suffix needed for default ports
                 .AllowAnyMethod()
                 .AllowAnyHeader()));

            // Monitor Target Connection
            var connectionSettingsList = RegistrationExtensions.PopulateConnectionStrings(builder);
            var connStr = builder.Configuration.GetSection(MonitorSettings.MonitoringSectionName)[MonitorSettings.ConnectionStringName];
            var monitorTarget = connectionSettingsList.FirstOrDefault() ?? throw new InvalidOperationException("ConnectionStringSettings for 'MonitoringDatabase' are missing.");
            builder.Services.AddSingleton<IDatabaseProvider>(sp => monitorTarget.ToDatabaseProvider());

            // Monitoring
            builder.Services.AddSingleton<MonitorStateStore>();
            builder.Services.AddSingleton<SqlCollectionService>();
            builder.Services.AddHostedService<SqlCollectionService>(sp => sp.GetRequiredService<SqlCollectionService>());
            builder.Services.Configure<MonitorSettings>(builder.Configuration.GetSection(MonitorSettings.MonitoringSectionName));
            builder.Services.PostConfigure<MonitorSettings>(options =>
            {
                options.Load(builder.Configuration);
                EntityDescriptor.DefaultSchemaOverride = EnvironmentResolver.ResolveTablePrefix(options.EnvironmentType);
            });

            builder.Services.AddHybridCache(options =>
            {
                options.MaximumPayloadBytes = 1024 * 1024 * 1000;       // 1000MB headroom for complex metadata cross-references
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(4),               // 4 Second Behind Polling - Evict Delta Cache
                    LocalCacheExpiration = TimeSpan.FromSeconds(4)
                };
            });

            // Options For Initial Snapshot Retention
            var permanentEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(365),
                LocalCacheExpiration = TimeSpan.FromDays(365)
            };

            var app = builder.Build();

            if (app.Environment.IsDevelopment() || true)    // On Internet - Remove
            {                
                app.MapOpenApi();                           // Exposes the raw /openapi/v1.json specification document
                app.MapScalarApiReference(options =>
                {
                    options.WithTitle("JEO3 Monitor Telemetry API");
                });
                app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
            }

            // Exception Handling - Coupled With Monitoring Introduced.. Revisit
            app.RegisterExceptionHandling(connStr);

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseCors("dashboard");

            // Monitor Endpoints
            app.MapMonitorEndpoints(permanentEntryOptions);

            app.Run();
        }
    }
}
