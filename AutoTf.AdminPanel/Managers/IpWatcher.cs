using AutoTf.AdminPanel.Models;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace AutoTf.AdminPanel.Managers;

public class IpWatcher : IHostedService
{
    private readonly DockerManager _docker;
    private readonly PleskManager _plesk;
    private readonly Credentials _credentials;

    private string _latestAuthIp = string.Empty; 
    
    private Timer? _currentTimer;

    public IpWatcher(DockerManager docker, PleskManager plesk, IOptions<Credentials> credentials)
    {
        _docker = docker;
        _plesk = plesk;

        _credentials = credentials.Value;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await CheckForNewIp();
        StartTimer();
    }

    private void StartTimer()
    {
        _currentTimer = new Timer(TimeSpan.FromMinutes(10));
        _currentTimer.Elapsed += async (_, _) => await CheckForNewIp();
        _currentTimer.Start();
    }

    private async Task CheckForNewIp()
    {
        string? containerIp = await _docker.GetContainerNetworkIp(_credentials.AuthServerContainerId, _credentials.AuthDefaultNetworkId);

        if (containerIp == null)
        {
            Console.WriteLine("Could not retreive authentik IP.");
            return;
        }

        if (_latestAuthIp == containerIp)
            return;

        _latestAuthIp = containerIp;
        
        Console.WriteLine($"Found new authentik IP {_latestAuthIp}.");

        List<string> pleskRecords = _plesk.Records;

        int matched = 0;

        Parallel.ForEach(pleskRecords, domain =>
        {
            string? currentHost = _plesk.GetAuthHost(domain); // http://xx.xx.xx.xx:9000
            
            if (currentHost == null || currentHost.Contains(_latestAuthIp))
                return;

            matched++;
            _plesk.UpdateAuthHost(domain, $"http://{_latestAuthIp}:9000");
        });
        
        _plesk.ReloadNginx();
        Console.WriteLine($"Updated {matched} containers to match new authentik IP.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _currentTimer?.Dispose();
        return Task.CompletedTask;
    }
}