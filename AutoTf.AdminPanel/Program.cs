using AutoTf.AdminPanel.Extensions;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models;

namespace AutoTf.AdminPanel;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddSingleton<DockerManager>();
        builder.Services.AddSingleton<DockerStatsManager>();
        builder.Services.AddSingleton<ManageManager>();
        
        builder.Services.AddHostedSingleton<ServerStatsCacheManager>();
        builder.Services.AddHostedSingleton<DockerCacheManager>();
        builder.Services.AddHostedSingleton<CloudflareManager>();
        builder.Services.AddHostedSingleton<AuthManager>();
        builder.Services.AddHostedSingleton<PleskManager>();
        builder.Services.AddHostedSingleton<IpWatcher>();
        
        // stored in appsettings.Development.json or set manually in .env
        builder.Services.Configure<Credentials>(options =>
        {
            options.ClientId = builder.Configuration["ClientId"] ?? "key";
            options.Username = builder.Configuration["Username"] ?? "key";
            options.Password = builder.Configuration["Password"] ?? "key";
            options.AuthUrl = builder.Configuration["AuthUrl"] ?? "key";
            options.CloudflareZone = builder.Configuration["CloudflareZone"] ?? "key";
            options.CloudflareKey = builder.Configuration["CloudflareKey"] ?? "key";

            options.DefaultConfig = new ServerConfig()
            {
                DefaultDnsType = builder.Configuration["DefaultDnsType"] ?? "key",
                DefaultTarget = builder.Configuration["DefaultTarget"] ?? "key",
                DefaultProxySetting = bool.Parse(builder.Configuration["DefaultProxySetting"] ?? "false"),
                DefaultTtl = int.Parse(builder.Configuration["DefaultTtl"] ?? "3600"),

                DefaultNetwork = builder.Configuration["DefaultNetwork"] ?? "key",
                DefaultAdditionalNetwork = builder.Configuration["DefaultAdditionalNetwork"] ?? "key",
                DefaultImage = builder.Configuration["DefaultImage"] ?? "key",

                DefaultAuthorizationFlow = builder.Configuration["DefaultAuthorizationFlow"] ?? "key",
                DefaultInvalidationFlow = builder.Configuration["DefaultInvalidationFlow"] ?? "key",

                DefaultOutpost = builder.Configuration["DefaultOutpost"] ?? "key",

                DefaultCertificateEmail = builder.Configuration["DefaultCertificateEmail"] ?? "key",

                DefaultAuthentikHost = builder.Configuration["DefaultAuthentikHost"] ?? "key",
            };
            
            options.AuthServerContainerId = builder.Configuration["AuthServerContainerId"] ?? "key";
            options.AuthDefaultNetworkId = builder.Configuration["AuthDefaultNetworkId"] ?? "key";
        });

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.MapControllers();
        
        app.UseDefaultFiles();
        app.UseStaticFiles();
        
        #if DEBUG
        app.Run("http://0.0.0.0:837");
        #endif
        app.Run("http://172.17.0.1:837");
    }
}