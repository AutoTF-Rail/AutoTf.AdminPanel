using AutoTf.AdminPanel.Models.Requests;

namespace AutoTf.AdminPanel.Models.Interfaces;

public interface IDockerStatsManager
{
    Task<ContainerStats> Stats();
    Result<ContainerStats> Stats(string containerId);
    Task<MemoryStats> Memory();
    Result<MemoryStats> Memory(string containerId);
    Task<double> Cpu();
    Result<double> Cpu(string containerId);
    Task<NetworkStats> Network();
    Result<NetworkStats> Network(string containerId);
}