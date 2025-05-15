using System.Diagnostics;
using AutoTf.AdminPanel.Models;

namespace AutoTf.AdminPanel.Managers;

public class ServerStatsCacheManager : IHostedService
{
    private SystemStats _latestStats = new();
    private CancellationTokenSource? _cts;
    
    public SystemStats GetLatestStats()
    {
        return _latestStats;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                float cpuUsage = await GetCpuUsageAsync();
                (float usedMem, float totalMem) = GetMemoryUsage();

                _latestStats = new SystemStats
                {
                    CpuUsagePercent = cpuUsage,
                    UsedMemoryMb = usedMem,
                    TotalMemoryMb = totalMem
                };

                await Task.Delay(1250, _cts.Token);
            }
        }, _cts.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    private async Task<float> GetCpuUsageAsync()
    {
        float cpuUsage = await GetCpuUsageLinuxAsync();
        return cpuUsage;
    }

    private async Task<float> GetCpuUsageLinuxAsync()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = "-c \"mpstat 1 1 | awk '/^Average/ { print 100 - $NF }'\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(startInfo);
        
        if (process == null)
            return 0;
            
        string output = await process.StandardOutput.ReadToEndAsync();
        if (float.TryParse(output, out float cpuUsage))
        {
            return cpuUsage;
        }

        return 0;
    }

    private (float used, float total) GetMemoryUsage()
    {
        string[] memInfo = File.ReadAllLines("/proc/meminfo");

        float totalMemory = float.Parse(memInfo.First(x => x.StartsWith("MemTotal")).Split(':')[1].Trim().Replace(" kB", ""));
        float availableMemory = float.Parse(memInfo.First(x => x.StartsWith("MemAvailable")).Split(':')[1].Trim().Replace(" kB", ""));

        float usedMemory = totalMemory - availableMemory;

        return (usedMemory / 1024, totalMemory / 1024); // in MB
    }
}