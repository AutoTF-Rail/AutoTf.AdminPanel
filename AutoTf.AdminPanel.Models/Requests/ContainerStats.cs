namespace AutoTf.AdminPanel.Models.Requests;

public class ContainerStats
{
    public ContainerStats(NetworkStats network, MemoryStats memory, double cpuUsage)
    {
        Network = network;
        Memory = memory;
        CpuUsage = cpuUsage;
    }
    public ContainerStats(NetworkStats network, MemoryStats memory, double cpuUsage, SystemStats systemStats)
    {
        Network = network;
        Memory = memory;
        CpuUsage = cpuUsage;
        SystemStats = systemStats;
    }

    public NetworkStats Network { get; set; }
    public MemoryStats Memory { get; set; }
    public double CpuUsage { get; set; }
    
    public SystemStats? SystemStats { get; set; }
}