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
using Group = AutoTf.AdminPanel.Models.Requests.Authentik.Group;
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

    public async Task<TransactionalCreationResponse?> CreateProxy(CreateProxyRequest request)
    {
        try
        {
            CreateAppWithProviderModel model = new CreateAppWithProviderModel();
            model.App.Name = $"Managed application for {request.Name}";
            model.App.Slug = Regex.Replace(request.Name.ToLower(), "[^a-z]", "");
            model.App.OpenInNewTab = false;
            model.App.MetaLaunchUrl = request.LaunchUrl.ToLower();
            model.App.PolicyEngineMode = "any";
            model.App.Group = "AutoTF-Managed";

            model.Provider.Name = $"Managed provider for {request.Name}";
            model.Provider.AuthenticationFlow = null;
            model.Provider.AuthorizationFlow = request.AuthorizationFlow;
            model.Provider.InvalidationFlow = request.InvalidationFlow;
            model.Provider.PropertyMappings = [];
            model.Provider.InternalHost = request.InternalHost.ToLower();
            model.Provider.ExternalHost = request.ExternalHost.ToLower();
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

            return await ApiHttpHelper.SendPut<TransactionalCreationResponse>($"{_credentials.AuthUrl}/api/v3/core/transactional/applications/", content, _apiKey, true);
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

    public async Task<List<Flow>> GetAuthorizationFlows()
    {
        try
        {
            FlowPaginationRequest? result = await ApiHttpHelper.SendGet<FlowPaginationRequest>($"{_credentials.AuthUrl}/api/v3/flows/instances/?designation=authorization&ordering=slug",
                _apiKey, true);
            
            if (result == null)
                return new List<Flow>();

            return result.Results;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when getting all authorization flows:");
            Console.WriteLine(e.ToString());
        }

        return new List<Flow>();
    }

    public async Task<List<Flow>> GetInvalidationFlows()
    {
        try
        {
            FlowPaginationRequest? result = await ApiHttpHelper.SendGet<FlowPaginationRequest>($"{_credentials.AuthUrl}/api/v3/flows/instances/?designation=invalidation&ordering=slug",
                _apiKey, true);
            
            if (result == null)
                return new List<Flow>();

            return result.Results;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when getting all invalidation flows:");
            Console.WriteLine(e.ToString());
        }

        return new List<Flow>();
    }

    public async Task<List<Group>> GetGroups()
    {
        try
        {
            GroupPaginationRequest? result = await ApiHttpHelper.SendGet<GroupPaginationRequest>($"{_credentials.AuthUrl}/api/v3/core/groups/?include_users=false&ordering=name",
                _apiKey, true);
            
            if (result == null)
                return new List<Group>();

            return result.Results;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when getting all groups:");
            Console.WriteLine(e.ToString());
        }

        return new List<Group>();
    }

    public async Task<ProviderPaginationResult?> GetProviders()
    {
        try
        {
            ProviderPaginationResult? providerPaginationResult = await ApiHttpHelper.SendGet<ProviderPaginationResult>($"{_credentials.AuthUrl}/api/v3/providers/proxy/?application__isnull=false&ordering=name&page=1&page_size=20&search=",
                _apiKey, true);
            
            if (providerPaginationResult == null)
                return null;

            if (!providerPaginationResult.Results.Any())
                return providerPaginationResult;

            providerPaginationResult.Results =
                providerPaginationResult.Results.Where(x => x.Name.Contains("Managed provider for")).ToList();

            return providerPaginationResult;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when getting all providers:");
            Console.WriteLine(e.ToString());
        }

        return null;
    }

    public async Task<ApplicationPaginationResult?> GetApplications()
    {
        try
        {
            ApplicationPaginationResult? result = await ApiHttpHelper.SendGet<ApplicationPaginationResult>($"{_credentials.AuthUrl}/api/v3/core/applications/",
                _apiKey, true);
            
            if (result == null)
                return null;

            if (!result.Results.Any())
                return result;

            result.Results =
                result.Results.Where(x => x.Name.Contains("Managed application for ")).ToList();

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when getting all applications:");
            Console.WriteLine(e.ToString());
        }

        return null;
    }

    public async Task<bool> DeleteProvider(string id)
    {
        try
        {
            return await ApiHttpHelper.SendDelete($"{_credentials.AuthUrl}/api/v3/providers/proxy/{id}/", _apiKey, true);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong when deleting provider {id}:");
            Console.WriteLine(e.ToString());
        }

        return false;
    }

    public async Task<bool> DeleteApplication(string id)
    {
        try
        {
            return await ApiHttpHelper.SendDelete($"{_credentials.AuthUrl}/api/v3/providers/proxy/{id}/", _apiKey, true);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong when deleting provider {id}:");
            Console.WriteLine(e.ToString());
        }

        return false;
    }

    public async Task<string?> GetProviderIdByExternalHost(string externalHost)
    {
        ProviderPaginationResult? providers = await GetProviders();
        
        if (providers == null || !providers.Results.Any())
            return null;

        Provider? provider = providers.Results.FirstOrDefault(x => x.ExternalHost.ToLower() == externalHost.ToLower());
       
        if (provider == null)
            return null;
        
        return provider.Pk;
    }

    public async Task<string?> GetApplicationIdByLaunchUrl(string launchUrl)
    {
        ApplicationPaginationResult? providers = await GetApplications();
        
        if (providers == null || !providers.Results.Any())
            return null;

        Application? app = providers.Results.FirstOrDefault(x => x.LaunchUrl.ToLower() == launchUrl.ToLower());
       
        if (app == null)
            return null;
        
        return app.Pk;
    }

    public async Task<string?> AssignToOutpost(string outpostId, string providerPk)
    {
        // TODO: Check for existance
        OutpostModel? outpostModel = await GetOutpost(outpostId);
        
        if (outpostModel == null)
            return $"Could not find outpost {outpostId}.";
        
        // TODO: Check for provider existance 
        outpostModel.Providers.Add(providerPk);

        return await UpdateOutpost(outpostId, outpostModel);
    }

    public async Task<string?> UnassignFromOutpost(string outpostId, string providerPk)
    {
        // TODO: Check for existance
        OutpostModel? outpostModel = await GetOutpost(outpostId);
        
        if (outpostModel == null)
            return $"Could not find outpost {outpostId}.";
        
        // TODO: Check for provider existance 
        outpostModel.Providers.Remove(providerPk);

        return await UpdateOutpost(outpostId, outpostModel);
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