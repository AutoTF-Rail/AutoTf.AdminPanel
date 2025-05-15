using System.Collections.Concurrent;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Enums;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet.Models;
using MemoryStats = AutoTf.AdminPanel.Models.Requests.MemoryStats;
using NetworkStats = AutoTf.AdminPanel.Models.Requests.NetworkStats;

namespace AutoTf.AdminPanel.Managers;

public class DockerStatsManager : IDockerStatsManager
{
    private readonly DockerCacheManager _docker;
    private readonly ServerStatsCacheManager _server;

    public DockerStatsManager(DockerCacheManager docker, ServerStatsCacheManager server)
    {
        _docker = docker;
        _server = server;
    }

    public async Task<ContainerStats> Stats()
    {
        List<ContainerListResponse> containers = await _docker.GetAll();
        
        NetworkStats network = new NetworkStats();
        MemoryStats memory = new MemoryStats()
        {
            MemoryLimitMb = -1
        };
        
        double cpuUsage = 0.1f;

        IEnumerable<Task> statsTasks = containers.Select(container =>
        {
            ContainerStatsResponse? stat = _docker.GetCachedStats(container.ID);

            if (stat == null)
                return Task.CompletedTask;
            
            NetworkStats currNet = Network(stat);
            MemoryStats currMem = Memory(stat);
            double currCpu = Cpu(stat);

            network.TotalReceived += currNet.TotalReceived;
            network.TotalSend += currNet.TotalSend;

            memory.MemoryPercentage += currMem.MemoryPercentage;
            memory.MemoryUsageMb += currMem.MemoryUsageMb;
            
            if(Math.Abs(memory.MemoryLimitMb - -1) < .01)
                memory.MemoryLimitMb += currMem.MemoryLimitMb;

            cpuUsage += currCpu;
            return Task.CompletedTask;
        });

        await Task.WhenAll(statsTasks);
        
        return new ContainerStats(network, memory, cpuUsage, _server.GetLatestStats());
    }

    public async Task<MemoryStats> Memory()
    {
        List<ContainerListResponse> containers = await _docker.GetAll();

        ConcurrentBag<ContainerStatsResponse> stats = new ConcurrentBag<ContainerStatsResponse>();
        
        Parallel.ForEach(containers,  (container, _) =>
        {
            ContainerStatsResponse? containerStatsResponse = _docker.GetCachedStats(container.ID);
            if (containerStatsResponse == null)
                return;
            
            stats.Add(containerStatsResponse);
        });
        
        return Memory(stats);
    }

    public async Task<double> Cpu()
    {
        List<ContainerListResponse> containers = await _docker.GetAll();

        ConcurrentBag<ContainerStatsResponse> stats = new ConcurrentBag<ContainerStatsResponse>();
        
        Parallel.ForEach(containers, (container, _) =>
        {
            ContainerStatsResponse? containerStatsResponse = _docker.GetCachedStats(container.ID);
            if (containerStatsResponse == null)
                return;
            
            stats.Add(containerStatsResponse);
        });

        return Cpu(stats);
    }

    public async Task<NetworkStats> Network()
    {
        List<ContainerListResponse> containers = await _docker.GetAll();

        ConcurrentBag<ContainerStatsResponse> stats = new ConcurrentBag<ContainerStatsResponse>();
        
        Parallel.ForEach(containers, (container, _) =>
        {
            ContainerStatsResponse? containerStatsResponse = _docker.GetCachedStats(container.ID);
            if (containerStatsResponse == null)
                return;
            
            stats.Add(containerStatsResponse);
        });

        return Network(stats);
    }

    public Result<MemoryStats> Memory(string containerId)
    {
        ContainerStatsResponse? response = _docker.GetCachedStats(containerId);

        if (response == null)
            return Result<MemoryStats>.Fail(ResultCode.NotFound, $"Could not find container {containerId}.");

        return Result<MemoryStats>.Ok(Memory(response));
    }

    public Result<double> Cpu(string containerId)
    {
        ContainerStatsResponse? response = _docker.GetCachedStats(containerId);

        if (response == null)
            return Result<double>.Fail(ResultCode.NotFound, $"Could not find container {containerId}.");

        return Result<double>.Ok(Cpu(response));
    }

    public Result<NetworkStats> Network(string containerId)
    {
        ContainerStatsResponse? response = _docker.GetCachedStats(containerId);

        if (response == null)
            return Result<NetworkStats>.Fail(ResultCode.NotFound, $"Could not find container {containerId}.");

        return Result<NetworkStats>.Ok(Network(response));
    }

    public Result<ContainerStats> Stats(string containerId)
    {
        ContainerStatsResponse? response = _docker.GetCachedStats(containerId);

        if (response == null)
            return Result<ContainerStats>.Fail(ResultCode.NotFound, $"Could not find container {containerId}.");

        NetworkStats network = Network(response);
        MemoryStats memory = Memory(response);
        double cpuUsage = Cpu(response);

        return Result<ContainerStats>.Ok(new ContainerStats(network, memory, cpuUsage));
    }
    
    #region Core
    
    private float Safe(float value) => float.IsNaN(value) || float.IsInfinity(value) ? 0.0f : value;
    
    #region All
    
    private MemoryStats Memory(ConcurrentBag<ContainerStatsResponse> stats)
    {
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

    private double Cpu(ConcurrentBag<ContainerStatsResponse> stats)
    {
        double totalUsage = 0.0f;

        Parallel.ForEach(stats, x =>
        {
            double usage = Cpu(x);
            totalUsage += usage;
        });

        return totalUsage;
    }

    private NetworkStats Network(ConcurrentBag<ContainerStatsResponse> stats)
    {
        NetworkStats finalStats = new NetworkStats();

        Parallel.ForEach(stats, x =>
        {
            NetworkStats networkStats = Network(x);
            
            finalStats.TotalSend += networkStats.TotalSend;
            finalStats.TotalReceived += networkStats.TotalReceived;
        });

        return finalStats;
    }
    
    #endregion
    
    #region singular
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
    
    #endregion
}