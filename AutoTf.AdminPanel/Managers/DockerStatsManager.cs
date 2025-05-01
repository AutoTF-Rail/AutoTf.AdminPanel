using System.Collections.Concurrent;
using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet.Models;
using MemoryStats = AutoTf.AdminPanel.Models.Requests.MemoryStats;
using NetworkStats = AutoTf.AdminPanel.Models.Requests.NetworkStats;

namespace AutoTf.AdminPanel.Managers;

public class DockerStatsManager
{
    private readonly DockerManager _docker;

    public DockerStatsManager(DockerManager docker)
    {
        _docker = docker;
    }

    public async Task<MemoryStats?> Memory(string containerId)
    {
        ContainerStatsResponse? response = await _docker.GetContainerStats(containerId);

        if (response == null)
            return null;

        return Memory(response);
    }

    public async Task<double?> Cpu(string containerId)
    {
        ContainerStatsResponse? response = await _docker.GetContainerStats(containerId);

        if (response == null)
            return null;

        return Cpu(response);
    }

    public async Task<NetworkStats?> Network(string containerId)
    {
        ContainerStatsResponse? response = await _docker.GetContainerStats(containerId);

        if (response == null)
            return null;

        return Network(response);
    }

    public async Task<ContainerStats?> Stats(string containerId)
    {
        ContainerStatsResponse? response = await _docker.GetContainerStats(containerId);

        if (response == null)
            return null;

        NetworkStats network = Network(response);
        MemoryStats memory = Memory(response);
        double cpuUsage = Cpu(response);

        return new ContainerStats()
        {
            Network = network,
            Memory = memory,
            CpuUsage = cpuUsage
        };
    }

    public async Task<MemoryStats> Memory()
    {
        List<ContainerListResponse> containers = await _docker.GetContainers();

        ConcurrentBag<ContainerStatsResponse> stats = new ConcurrentBag<ContainerStatsResponse>();
        
        await Parallel.ForEachAsync(containers, async (container, _) =>
        {
            stats.Add((await _docker.GetContainerStats(container.ID))!);
        });
        
        MemoryStats finalStats = new MemoryStats
        {
            MemoryLimitMb = -1
        };

        Parallel.ForEach(stats, x =>
        {
            MemoryStats memoryStats = Memory(x);
            finalStats.MemoryPercentage += memoryStats.MemoryPercentage;
            finalStats.MemoryUsageMb += memoryStats.MemoryUsageMb;

            if (Math.Abs(finalStats.MemoryLimitMb - -1) < .01)
                finalStats.MemoryLimitMb = memoryStats.MemoryLimitMb;
        });

        return finalStats;
    }
    
    #region Core
    
    private float Safe(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 0.0f : value;
    
    private MemoryStats Memory(ContainerStatsResponse response)
    {
        float memoryUsageBytes = response.MemoryStats.Usage;
        float memoryLimitBytes = response.MemoryStats.Limit;
        
        float memoryUsageMb = memoryUsageBytes / (1024 * 1024);
        float memoryLimitMb = memoryLimitBytes / (1024 * 1024);
        float memoryPercentage = (memoryUsageBytes / memoryLimitBytes) * 100;

        MemoryStats stats = new MemoryStats()
        {
            MemoryUsageMb = Safe(memoryUsageMb),
            MemoryLimitMb = Safe(memoryLimitMb),
            MemoryPercentage = Safe(memoryPercentage)
        };

        return stats;
    }

    private double Cpu(ContainerStatsResponse response)
    {
        ulong cpuDelta = response.CPUStats.CPUUsage.TotalUsage - response.PreCPUStats.CPUUsage.TotalUsage;
        ulong systemDelta = response.CPUStats.SystemUsage - response.PreCPUStats.SystemUsage;
        uint cpuCount = response.CPUStats.OnlineCPUs;

        double cpuPercent = 0;
        
        if (systemDelta > 0 && cpuDelta > 0)
        {
            cpuPercent = (double)cpuDelta / systemDelta * cpuCount * 100;
        }

        return cpuPercent;
    }
    
    private NetworkStats Network(ContainerStatsResponse response)
    {
        ulong totalRx = 0;
        ulong totalTx = 0;

        if (response.Networks == null)
            return new NetworkStats();
            
        foreach (Docker.DotNet.Models.NetworkStats? net in response.Networks.Values)
        {
            if (net == null)
                continue;

            totalRx += net.RxBytes;
            totalTx += net.TxBytes;
        }

        return new NetworkStats()
        {
            TotalReceived = totalRx,
            TotalSend = totalTx
        };
    }
    
    #endregion
}