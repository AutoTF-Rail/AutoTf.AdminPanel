using Microsoft.Extensions.Hosting;

namespace AutoTf.AdminPanel.Models.Interfaces;

public interface IPleskManager : IHostedService
{
    List<string> Records { get; }

    /// <summary>
    /// Creates a subdomain in plesk and automatically issues a lets encrypt certificate for it.
    /// </summary>
    Result<object> CreateSubdomain(string subDomain, string rootDomain, string email, string authentikHost);

    Result<object> DeleteSubDomain(string rootDomain, string subDomain);
    Result<object> UpdateAuthHost(string rootDomain, string subDomain, string newAuthHost);
    Result<object> UpdateAuthHost(string domain, string newAuthHost);
    Result<string> GetAuthHost(string rootDomain, string subDomain);
    Result<string> GetAuthHost(string domain);
    void ReloadNginx();
    void UpdateCache();
}