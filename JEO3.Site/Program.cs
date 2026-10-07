using System.Configuration;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using JEO3.Core;
using JEO3.Logging;
using JEO3.Monitor;
using JEO3.Monitor.ECharts;
using JEO3.Monitor.Wmi;
using JEO3.Schema;
using JEO3.Site.Dashboard;
using JEO3.Site.Extensions;
using JEO3.Site.Infrastructure;
using JEO3.Site.Services;
using Microsoft.Extensions.Caching.Hybrid;
using Radzen;

namespace JEO3.Site
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddServerSideBlazor()
                .AddHubOptions(options =>
                {
                    options.MaximumParallelInvocationsPerClient = 10;
                    options.StreamBufferCapacity = 50;
                    options.MaximumReceiveMessageSize = 2140000000;
                    options.EnableDetailedErrors = true;
                    //options.ClientTimeoutInterval = new TimeSpan(0, 1, 0);
                })
               .AddCircuitOptions(options =>
               {
                   // Give heavy JS interactions (like loading huge SQL text or complex diagrams) more breathing room
                   options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(60);
               });

            var connectionSettingsList = builder.PopulateConnectionStrings();
            var connStr = builder.Configuration.GetSection(MonitorSettings.MonitoringSectionName)[MonitorSettings.ConnectionStringName];

            builder.Services.AddSingleton<IMonitorChartDataService, MonitorChartDataService>();
            builder.Services.AddSingleton<MonitorSnapshotService>();
            builder.Services.AddSingleton(connectionSettingsList);
            builder.Services.AddSingleton<ConnectionService>();
            builder.Services.AddHttpClient();

            // Caching
            builder.Services.AddScoped<ISchemaCacheService, SchemaCacheService>();
            builder.Services.AddHybridCache(options =>
            {
                options.MaximumPayloadBytes = 1024 * 1024 * 300; // 300MB headroom for complex metadata cross-references
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromHours(4),
                    LocalCacheExpiration = TimeSpan.FromHours(4)
                };
            })
            .AddSerializer<DatabaseContext, PayloadSerializer>();

            builder.Services.AddScoped<WSWorkspace>();
            builder.Services.AddScoped<IMonitorStateConsumer, WSWorkspace>();
            builder.Services.AddScoped<IServerMonitorService, SqlServerMonitorService>();

            // Rest
            builder.Services.AddScoped<DialogService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<TooltipService>();
            builder.Services.AddScoped<ContextMenuService>();
            builder.Services.AddScoped<ThemeService>();
            builder.Services.AddScoped<QueryStringThemeService>();
            builder.Services.AddLocalization();
            builder.Services.Configure<MonitorSettings>(builder.Configuration.GetSection(MonitorSettings.MonitoringSectionName));
            builder.Services.PostConfigure<MonitorSettings>(options =>
            {
                options.Load(builder.Configuration);
            });

            // Register typed HttpClient pointing to JEO3.API
            var baseUrl = builder.Configuration.GetSection(MonitorSettings.MonitoringSectionName)[MonitorSettings.ApiBaseUrlName];
            builder.Services.AddHttpClient<IMonitorApiClient, MonitorApiClient>(client =>
            {
                var environmentType = baseUrl == "https://api.jeo3.com" ? EnvironmentType.Production : EnvironmentResolver.ResolveEnvironment(builder.Configuration);
                client.BaseAddress = new Uri(baseUrl);
            });

            // WMI
            //builder.Services.AddSingleton<WmiMonitorStore>();
            //builder.Services.AddHostedService<WmiMonitorService>();

            // App
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.RegisterExceptionHandling(connStr);
            app.RegisterPersonalStuff();

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseAntiforgery();
            app.UseStaticFiles();
            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Kick off the monitor poller after a delay
            _ = Task.Run(async () =>
            {
                Trace.WriteLine($"PROGRAM.CS WAITING... {DateTime.Now.ToShortTimeString()}");
                await Task.Delay(20000); // second warm-up
                using var scope = app.Services.CreateScope();
                var data = scope.ServiceProvider.GetRequiredService<IMonitorChartDataService>();
                Trace.WriteLine($"PROGRAM.CS - MonitorChartDataService.EnsureStartedAsync()... {DateTime.Now.ToShortTimeString()}");
                await data.EnsureStartedAsync();
                Trace.WriteLine($"PROGRAM.CS - MonitorChartDataService.EnsureStartedAsync DONE!... {DateTime.Now.ToShortTimeString()}");
            });

            app.Run();
        }
    }
}