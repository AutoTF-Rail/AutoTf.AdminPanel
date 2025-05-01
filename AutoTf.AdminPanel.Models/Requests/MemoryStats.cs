namespace AutoTf.AdminPanel.Models.Requests;

public class MemoryStats
{
    public double MemoryUsageMb { get; set; }
    public double MemoryLimitMb { get; set; }
    public double MemoryPercentage { get; set; }
}