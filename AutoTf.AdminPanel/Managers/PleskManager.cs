using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Enums;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Statics;
using Timer = System.Timers.Timer;

namespace AutoTf.AdminPanel.Managers;

public class PleskManager : IPleskManager
{
    private Timer _timer = new Timer(TimeSpan.FromMinutes(5));

    public List<string> Records { get; private set; } = [];


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Run(UpdateCache, cancellationToken);
        StartCacheTimer();
    }

    private void StartCacheTimer()
    {
        _timer.AutoReset = true;
        _timer.Elapsed += (_, _) => UpdateCache();
        _timer.Start();
    }
    
    /// <summary>
    /// Creates a subdomain in plesk and automatically issues a lets encrypt certificate for it.
    /// </summary>
    public Result CreateSubdomain(string subDomain, string rootDomain, string email, string authentikHost)
    {
        subDomain = subDomain.ToLower();
        rootDomain = rootDomain.ToLower();
        
        string result = CommandExecuter.ExecuteCommand(
            $"plesk bin subdomain --create {subDomain} -domain {rootDomain} -admin-description \"Externally managed by AutoTF\"");

        if (!result.Contains("SUCCESS: Creation of"))
            return Result.Fail(ResultCode.InternalServerError, $"Failed while creating subdomain \"{subDomain}.{rootDomain}\".");

        if (!IssueCertificate(subDomain, rootDomain, email))
            return Result.Fail(ResultCode.InternalServerError, $"Failed while issuing the certificate for \"{subDomain}.{rootDomain}\".");

        PointToAuthentik(subDomain, rootDomain, authentikHost);
        Records.Add($"{subDomain}.{rootDomain}");
        
        return Result.Ok();
    }

    public Result DeleteSubDomain(string rootDomain, string subDomain)
    {
        subDomain = subDomain.ToLower();
        rootDomain = rootDomain.ToLower();
        string result = CommandExecuter.ExecuteCommand($"plesk bin subdomain --remove {subDomain} -domain {rootDomain}");

        if (!result.Contains("SUCCESS: Removal of"))
            return Result.Fail(ResultCode.InternalServerError, $"Failed while removing subdomain \"{subDomain}.{rootDomain}\".");

        string certResult =
            CommandExecuter.ExecuteCommand($"plesk bin certificate --remove 'Lets Encrypt {subDomain}.{rootDomain}' -domain {rootDomain}");

        if (!certResult.Contains("was successfully removed"))
            return Result.Fail(ResultCode.InternalServerError, $"Failed while removing the certificate for \"{subDomain}.{rootDomain}\".");

        Task.Run(UpdateCache);
        ReloadNginx();
        
        return Result.Ok();
    }

    public Result UpdateAuthHost(string rootDomain, string subDomain, string newAuthHost)
    {
        return UpdateAuthHost($"{subDomain}.{rootDomain}", newAuthHost);
    }

    public Result UpdateAuthHost(string domain, string newAuthHost)
    {
        if (!RegexHelper.ValidateAuthHost(newAuthHost))
            return Result.Fail(ResultCode.InternalServerError, $"Failed while validating the new auth host \"{newAuthHost}\".");
        
        string file = $"/var/www/vhosts/system/{domain}/conf/vhost_nginx.conf";
        
        if (!File.Exists(file))
            return Result.Fail(ResultCode.NotFound, $"Could not find the domain \"{domain}\".");

        string fileContent = File.ReadAllText(file);
        fileContent = Regex.Replace(fileContent, RegexHelper.AuthHostPattern, newAuthHost);
        File.WriteAllText(file, fileContent);
        
        return Result.Ok();
    }

    public Result<string> GetAuthHost(string rootDomain, string subDomain)
    {
        return GetAuthHost($"{subDomain}.{rootDomain}");
    }

    public Result<string> GetAuthHost(string domain)
    {
        string file = $"/var/www/vhosts/system/{domain}/conf/vhost_nginx.conf";
        
        if (!File.Exists(file))
            return Result<string>.Fail(ResultCode.NotFound, $"Could not find the domain \"{domain}\".");
        
        string fileContent = File.ReadAllText(file);
        Match match = Regex.Match(fileContent, RegexHelper.AuthHostPattern);

        if (!match.Success)
            return Result<string>.Fail(ResultCode.NotFound, $"Could not find the auth host in the data for domain \"{domain}\".");

        return Result<string>.Ok(match.Value);
    }

    public void ReloadNginx()
    {
        CommandExecuter.ExecuteCommand("systemctl reload nginx");
    }

    private void PointToAuthentik(string subDomain, string rootDomain, string authentikHost)
    {
        string dir = $"/var/www/vhosts/system/{subDomain}.{rootDomain}/conf";
        Directory.CreateDirectory(dir);
        File.WriteAllText($"{dir}/vhost_nginx.conf", AssembleAuthentikConfig(authentikHost));
    }

    private bool IssueCertificate(string subDomain, string rootDomain, string email)
    {
        string result = CommandExecuter.ExecuteCommand($"plesk bin extension --exec letsencrypt cli.php -d {subDomain}.{rootDomain} -m {email}");

        return string.IsNullOrEmpty(result.Trim());
    }

    private string AssembleAuthentikConfig(string authentikHost) // http://172.20.0.4:9000
    {
        return "location ~ ^/.* {" +
                   $"proxy_pass {authentikHost};" +
                   "proxy_set_header Upgrade $http_upgrade;" +
                   "proxy_set_header Connection \"Upgrade\";" +
                   "proxy_set_header Host $host;" +
                   "proxy_set_header X-Real-IP $remote_addr;" +
                   "proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;" +
                   "proxy_set_header X-Forwarded-Proto $scheme;" +
                   "proxy_cache_bypass $http_upgrade;" +
               "}";
    }

    public void UpdateCache()
    {
        Console.WriteLine("Updating plesk site cache.");
        
        string[] result = CommandExecuter.ExecuteCommand("plesk bin subdomain -l").Split(Environment.NewLine);

        ConcurrentBag<string> managedDomains = new ConcurrentBag<string>();
        
        Parallel.ForEach(result, subDomain =>
        {
            if (IsManaged(subDomain))
                managedDomains.Add(subDomain);
        });

        Records = managedDomains.ToList();
        
        Console.WriteLine($"Finished updating plesk site cache with {Records.Count} sites.");
    }

    private bool IsManaged(string subDomain)
    {
        try
        {
            string[] result = CommandExecuter.ExecuteCommand($"plesk bin subdomain -i {subDomain}").Split(Environment.NewLine);
            string adminComment = result.First(x => x.Contains("Description for the administrator:"));

            // weird plesk bug?
            return adminComment.Contains("Externally managed by AutoTF") || adminComment.Contains("Externally");
        }
        catch
        {
            return false;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Dispose();
        return Task.CompletedTask;
    }
}