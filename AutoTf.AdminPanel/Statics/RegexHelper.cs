using System.Text.RegularExpressions;

namespace AutoTf.AdminPanel.Statics;

public class RegexHelper
{
    public  const string AuthHostPattern = "(?:http|https)://\\d{1,3}.\\d{1,3}.\\d{1,3}.\\d{1,3}(?::\\d{1,6})?";
    public const string DomainsPattern = "((?:[a-z0-9-]+\\.)*)([a-z0-9-]+\\.[a-z]{2,})";
    
    public static bool ValidateAuthHost(string host)
    {
        return Regex.IsMatch(host, AuthHostPattern);
    }
    
    /// <summary>
    /// Key is subdomain, value is root domain
    /// </summary>
    public static KeyValuePair<string, string>? ExtractDomains(string host)
    {
        MatchCollection matches = Regex.Matches(host, RegexHelper.DomainsPattern);
        
        if (matches.Count != 1)
            return null;

        if (matches[0].Groups.Count != 3)
            return null;

        return new KeyValuePair<string, string>(matches[0].Groups[1].Value.TrimEnd('.'), matches[0].Groups[2].Value);
    }
}