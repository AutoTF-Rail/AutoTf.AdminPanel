namespace AutoTf.AdminPanel;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddSingleton<DockerManager>();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run("http://0.0.0.0:80");
    }
}