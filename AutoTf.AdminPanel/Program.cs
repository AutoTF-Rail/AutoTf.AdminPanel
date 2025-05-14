using AutoTf.AdminPanel.Extensions;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Interfaces;

namespace AutoTf.AdminPanel;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddSingleton<IDockerManager, DockerManager>();
        builder.Services.AddSingleton<IDockerStatsManager, DockerStatsManager>();
        builder.Services.AddSingleton<ManageManager>();
        
        builder.Services.AddHostedSingleton<ServerStatsCacheManager>();
        builder.Services.AddHostedSingleton<DockerCacheManager>();
        builder.Services.AddHostedSingleton<ICloudflareManager, CloudflareManager>();
        builder.Services.AddHostedSingleton<IAuthManager, AuthManager>();
        builder.Services.AddHostedSingleton<IPleskManager, PleskManager>();
        builder.Services.AddHostedSingleton<IIpWatcher, IpWatcher>();
        
        // stored in appsettings.Development.json or set manually in .env
        builder.Services.Configure<Credentials>(builder.Configuration.GetSection("Credentials"));

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