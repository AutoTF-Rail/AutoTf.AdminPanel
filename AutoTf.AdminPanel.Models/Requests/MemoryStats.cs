namespace AutoTf.AdminPanel.Models.Requests;

public class MemoryStats
{
    public float MemoryUsageMb { get; set; }
    public float MemoryLimitMb { get; set; }
    public float MemoryPercentage { get; set; }
}