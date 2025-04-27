namespace AutoTf.AdminPanel.Extensions;

public static class ServiceExtension
{
    // This method is used to create a singleton that is also a hostedservice at the same time.
    // Singleton cause we need it in controllers, and hostedservice because it has to do something on startup (e.g. sync)
    public static void AddHostedSingleton<THostedService>(this IServiceCollection collection) 
        where THostedService : class, IHostedService
    {
        collection.AddSingleton<THostedService>();
        collection.AddHostedService(provider => provider.GetRequiredService<THostedService>());
    }
	
    public static void AddHostedSingleton<TInterface, TService>(this IServiceCollection collection) 
        where TInterface : class, IHostedService
        where TService : class, TInterface
    {
        collection.AddSingleton<TInterface, TService>();
        collection.AddHostedService(provider => provider.GetRequiredService<TInterface>());
    }
}