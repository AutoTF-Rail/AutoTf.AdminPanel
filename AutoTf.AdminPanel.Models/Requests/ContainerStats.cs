namespace AutoTf.AdminPanel.Models.Requests;

public class ContainerStats
{
    public NetworkStats Network { get; set; }
    public MemoryStats Memory { get; set; }
    public double CpuUsage { get; set; }
}