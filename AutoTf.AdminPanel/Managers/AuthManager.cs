using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using AutoTf.AdminPanel.Statics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace AutoTf.AdminPanel.Managers;

public class AuthManager : IHostedService
{
    private readonly Credentials _credentials;
    private Timer? _currentTimer;

    private string _apiKey = string.Empty;

    public AuthManager(IOptions<Credentials> credentials)
    {
        _credentials = credentials.Value;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await GrabNewToken();
        StartTimer(300);
    }

    public async Task<string?> CreateProxy(CreateProxyRequest request)
    {
        try
        {
            CreateAppWithProviderModel model = new CreateAppWithProviderModel();
            model.App.Name = request.Name;
            model.App.Slug = Regex.Replace(request.LaunchUrl.ToLower(), "[^a-z]", "");
            model.App.OpenInNewTab = false;
            model.App.MetaLaunchUrl = request.LaunchUrl;
            model.App.PolicyEngineMode = "any";
            model.App.Group = "AutoTF-Managed";

            model.Provider.Name = $"Managed provider for {request.Name}";
            model.Provider.AuthenticationFlow = null;
            model.Provider.AuthorizationFlow = request.AuthorizationFlow;
            model.Provider.InvalidationFlow = request.InvalidationFlow;
            model.Provider.PropertyMappings = [];
            model.Provider.InternalHost = request.InternalHost;
            model.Provider.ExternalHost = request.ExternalHost;
            model.Provider.InternalHostSslValidation = true;
            model.Provider.Certificate = null;
            model.Provider.SkipPathRegex = "";
            model.Provider.BasicAuthEnabled = false;
            model.Provider.BasicAuthUserAttribute = "";
            model.Provider.BasicAuthPasswordAttribute = "";
            model.Provider.Mode = "proxy";
            model.Provider.InterceptHeaderAuth = true;
            model.Provider.CookieDomain = "";
            model.Provider.JwtFederationSources = [];
            model.Provider.JwtFederationProviders = [];
            model.Provider.AccessTokenValidity = "hours=24";
            model.Provider.ProviderModel = "authentik_providers_proxy.proxyprovider";

            model.PolicyBindings = request.PolicyBindings;

            HttpContent content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            return await ApiHttpHelper.SendPut($"{_credentials.AuthUrl}/api/v3/core/transactional/applications/", content, _apiKey, true);
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when creating a application with proxy:");
            Console.WriteLine(e.ToString());
        }

        return null;
    }

    public async Task<string?> GetOutposts()
    {
        try
        {
            return await ApiHttpHelper.SendGet($"{_credentials.AuthUrl}/api/v3/outposts/instances/?ordering=name&page=1&page_size=200&search=",
                _apiKey, true);
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when getting all outposts:");
            Console.WriteLine(e.ToString());
        }

        return null;
    }

    public async Task<OutpostModel?> GetOutpost(string id)
    {
        try
        {
            return await ApiHttpHelper.SendGet<OutpostModel>($"{_credentials.AuthUrl}/api/v3/outposts/instances/{id}/",
                _apiKey, true) ?? null;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong when getting outpost {id}:");
            Console.WriteLine(e.ToString());
        }

        return null;
    }

    public async Task<string?> UpdateOutpost(string id, OutpostModel config)
    {
        try
        {
            HttpContent content = new StringContent(JsonSerializer.Serialize(config), Encoding.UTF8, "application/json");
            
            return await ApiHttpHelper.SendPut($"{_credentials.AuthUrl}/api/v3/outposts/instances/{id}/", content, _apiKey, true);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong when getting outpost {id}:");
            Console.WriteLine(e.ToString());
        }

        return null;
    }

    public async Task<string?> GetProviders()
    {
        try
        {
            return await ApiHttpHelper.SendGet($"{_credentials.AuthUrl}/api/v3/providers/proxy/?application__isnull=false&ordering=name&page=1&page_size=20&search=",
                _apiKey, true);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong when getting all providers:");
            Console.WriteLine(e.ToString());
        }

        return null;
    }
    
    
    #region Core

    private async Task GrabNewToken()
    {
        Console.WriteLine("Trying to grab a new API token.");
        
        List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
        
        headers.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
        // Provider client ID
        headers.Add(new KeyValuePair<string, string>("client_id", _credentials.ClientId));
        // Service account username
        headers.Add(new KeyValuePair<string, string>("username", _credentials.Username));
        // Service account password
        headers.Add(new KeyValuePair<string, string>("password", _credentials.Password));
        // Scope, needs to be added under Advanced procotol settings in the provider.
        headers.Add(new KeyValuePair<string, string>("scope", "goauthentik.io/api"));
        
        HttpContent content = new FormUrlEncodedContent(headers);

        TokenRequestModel response = await HttpHelper.SendPost<TokenRequestModel>($"{_credentials.AuthUrl}/application/o/token/", content, false) ?? new TokenRequestModel();
        
        if (response.ExpiresIn == 0)
        {
            Console.WriteLine("Cannot get token from authentik instance.");
            Environment.Exit(1);
            return;
        }

        _apiKey = response.AccessToken;
        
        Console.WriteLine("Successfully retrieved API token from authentik instance.");
    }

    private void StartTimer(int responseExpiresIn)
    {
        // Requests a new token 10 seconds before the current one expires.
        _currentTimer = new Timer(TimeSpan.FromSeconds(responseExpiresIn) - TimeSpan.FromSeconds(10));
        _currentTimer.Elapsed += async (_, _) => await GrabNewToken();
        _currentTimer.Start();
    }
    
    #endregion

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _currentTimer?.Dispose();
        return Task.CompletedTask;
    }
}