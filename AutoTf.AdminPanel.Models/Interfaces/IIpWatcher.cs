using Microsoft.Extensions.Hosting;

namespace AutoTf.AdminPanel.Models.Interfaces;

public interface IIpWatcher : IHostedService
{
    public string GetAuthIp();
}