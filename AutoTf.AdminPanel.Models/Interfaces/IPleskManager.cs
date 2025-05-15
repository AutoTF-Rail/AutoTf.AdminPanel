using Microsoft.Extensions.Hosting;

namespace AutoTf.AdminPanel.Models.Interfaces;

public interface IPleskManager : IHostedService
{
    List<string> Records { get; }

    /// <summary>
    /// Creates a subdomain in plesk and automatically issues a lets encrypt certificate for it.
    /// </summary>
    Result CreateSubdomain(string subDomain, string rootDomain, string email, string authentikHost);

    Result DeleteSubDomain(string rootDomain, string subDomain);
    Result UpdateAuthHost(string rootDomain, string subDomain, string newAuthHost);
    Result UpdateAuthHost(string domain, string newAuthHost);
    Result<string> GetAuthHost(string rootDomain, string subDomain);
    Result<string> GetAuthHost(string domain);
    void ReloadNginx();
    void UpdateCache();
}