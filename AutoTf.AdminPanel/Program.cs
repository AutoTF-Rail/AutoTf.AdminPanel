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
        builder.Services.AddSingleton<PleskManager>();
        builder.Services.AddSingleton<DockerStatsManager>();
        
        builder.Services.AddHostedSingleton<CloudflareManager>();
        builder.Services.AddHostedSingleton<AuthManager>();
        
        // stored in appsettings.Development.json or set manually in .env
        builder.Services.Configure<Credentials>(options =>
        {
            options.ClientId = builder.Configuration["ClientId"] ?? "key";
            options.Username = builder.Configuration["Username"] ?? "key";
            options.Password = builder.Configuration["Password"] ?? "key";
            options.AuthUrl = builder.Configuration["AuthUrl"] ?? "key";
            options.CloudflareZone = builder.Configuration["CloudflareZone"] ?? "key";
            options.CloudflareKey = builder.Configuration["CloudflareKey"] ?? "key";
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