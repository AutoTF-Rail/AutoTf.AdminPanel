using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using AutoTf.AdminPanel.Statics;
using Timer = System.Timers.Timer;

namespace AutoTf.AdminPanel.Managers;

public class PleskManager : IHostedService
{
    private const string _authHostPattern = "(?:http|https)://\\d{1,3}.\\d{1,3}.\\d{1,3}.\\d{1,3}(?::\\d{1,6})?";
    
    private Timer _timer = new Timer(TimeSpan.FromMinutes(5));

    public List<string> Records { get; private set; } = [];


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.Run(UpdateCache, cancellationToken);
        StartCacheTimer();
    }

    public bool ValidateAuthHost(string host)
    {
        return Regex.IsMatch(host, _authHostPattern);
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
    public bool CreateSubdomain(string subDomain, string rootDomain, string email, string authentikHost)
    {
        subDomain = subDomain.ToLower();
        rootDomain = rootDomain.ToLower();
        
        string result = CommandExecuter.ExecuteCommand(
            $"plesk bin subdomain --create {subDomain} -domain {rootDomain} -admin-description \"Externally managed by AutoTF\"");

        if (!result.Contains("SUCCESS: Creation of"))
            return false;

        if (!IssueCertificate(subDomain, rootDomain, email))
            return false;

        PointToAuthentik(subDomain, rootDomain, authentikHost);
        Records.Add($"{subDomain}.{rootDomain}");
        return true;
    }

    public bool DeleteSubDomain(string rootDomain, string subDomain)
    {
        subDomain = subDomain.ToLower();
        rootDomain = rootDomain.ToLower();
        string result = CommandExecuter.ExecuteCommand($"plesk bin subdomain --remove {subDomain} -domain {rootDomain}");

        if (!result.Contains("SUCCESS: Removal of"))
            return false;

        string certResult =
            CommandExecuter.ExecuteCommand($"plesk bin certificate --remove 'Lets Encrypt {subDomain}.{rootDomain}' -domain {rootDomain}");

        if (!certResult.Contains("was successfully removed"))
            return false;

        CommandExecuter.ExecuteCommand("systemctl reload nginx");
        return true;
    }

    public string? GetAuthHost(string rootDomain, string subDomain)
    {
        string file = $"/var/www/vhosts/system/{subDomain}.{rootDomain}/conf/vhost_nginx.conf";
        
        if (!File.Exists(file))
            return null;
        
        string fileContent = File.ReadAllText(file);
        Match match = Regex.Match(fileContent, _authHostPattern);

        if (!match.Success)
            return null;

        return match.Value;
    }
    private void PointToAuthentik(string subDomain, string rootDomain, string authentikHost)
    {
        string dir = $"/var/www/vhosts/system/{subDomain}.{rootDomain}/conf";
        Directory.CreateDirectory(dir);
        File.WriteAllText($"{dir}/vhost_nginx.conf", AssembleAuthentikConfig(authentikHost));
        CommandExecuter.ExecuteCommand("systemctl reload nginx");
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