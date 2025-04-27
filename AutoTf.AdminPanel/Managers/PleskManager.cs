using AutoTf.AdminPanel.Statics;

namespace AutoTf.AdminPanel.Managers;

public class PleskManager
{
    /// <summary>
    /// Creates a subdomain in plesk and automatically issues a lets encrypt certificate for it.
    /// </summary>
    public bool CreateSubdomain(string subDomain, string rootDomain, string email, string authentikHost)
    {
        string result = CommandExecuter.ExecuteCommand(
            $"plesk bin subdomain --create {subDomain} -domain {rootDomain} -admin-description \"Externally managed by AutoTF\"");

        if (!result.Contains("SUCCESS: Creation of"))
            return false;

        if (!IssueCertificate(subDomain, rootDomain, email))
            return false;

        PointToAuthentik(subDomain, rootDomain, authentikHost);
        return true;
    }

    public bool DeleteSubDomain(string rootDomain, string subDomain)
    {
        string result = CommandExecuter.ExecuteCommand($"plesk bin subdomain --remove {subDomain} -domain {rootDomain}");

        if (!result.Contains("SUCCESS: Removal of"))
            return false;

        string certResult =
            CommandExecuter.ExecuteCommand($"plesk bin certificate --remove \"Lets Encrypt {subDomain}.{rootDomain}\" -domain {rootDomain}");

        if (!certResult.Contains("was successfully removed"))
            return false;
        
        return true;
    }

    private void PointToAuthentik(string subDomain, string rootDomain, string authentikHost)
    {
        string dir = $"/var/www/vhosts/system/{subDomain}.{rootDomain}/conf";
        Directory.CreateDirectory(dir);
        File.WriteAllText($"{dir}/{subDomain}.{rootDomain}/conf/vhost_nginx.conf", AssembleAuthentikConfig(authentikHost));
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
}