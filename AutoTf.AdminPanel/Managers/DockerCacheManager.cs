using System.Collections.Concurrent;
using Docker.DotNet.Models;

namespace AutoTf.AdminPanel.Managers;

public class DockerCacheManager : IHostedService
{
    private readonly DockerManager _docker;
    private readonly ConcurrentDictionary<string, ContainerStatsResponse> _statsCache = new();
    private readonly Dictionary<string, CancellationTokenSource> _streamTokens = new();

    public DockerCacheManager(DockerManager docker)
    {
        _docker = docker;
    }

    public async Task<List<ContainerListResponse>> GetAll()
    {
        return await _docker.GetAll();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            IList<ContainerListResponse> containers = await _docker.GetAll();
            foreach (ContainerListResponse? container in containers)
            {
                StartStream(container.ID);
            }
        }, cancellationToken);
        
        return Task.CompletedTask;
    }
    
    public ContainerStatsResponse? GetCachedStats(string containerId)
    {
        _statsCache.TryGetValue(containerId, out ContainerStatsResponse? stat);
        return stat;
    }

    private void StartStream(string containerId)
    {
        if (_streamTokens.ContainsKey(containerId))
            return;

        CancellationTokenSource cts = new CancellationTokenSource();
        _streamTokens[containerId] = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await _docker.Client.Containers.GetContainerStatsAsync(containerId,
                    new ContainerStatsParameters { Stream = true },
                    new Progress<ContainerStatsResponse>(stat =>
                    {
                        _statsCache[containerId] = stat;
                    }),
                    cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stats stream error for {containerId}: {ex.Message}");
            }
        }, cts.Token);
    }

    private void StopStream(string containerId)
    {
        if (_streamTokens.TryGetValue(containerId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            _streamTokens.Remove(containerId);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (string containerId in _streamTokens.Keys)
        {
            StopStream(containerId);
        }

        return Task.CompletedTask;
    }
}