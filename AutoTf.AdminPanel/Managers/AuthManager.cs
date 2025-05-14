using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Enums;
using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using AutoTf.AdminPanel.Statics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Group = AutoTf.AdminPanel.Models.Requests.Authentik.Group;
using Timer = System.Timers.Timer;

namespace AutoTf.AdminPanel.Managers;

public class AuthManager : IAuthManager
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

    public async Task<Result<TransactionalCreationResponse>>CreateProxy(CreateProxyRequest request)
    {
        try
        {
            request.Name = request.Name.ToLower();
            
            CreateAppWithProviderModel model = new CreateAppWithProviderModel
            {
                App =
                {
                    Name = $"Managed application for {request.Name}",
                    Slug = Regex.Replace(request.Name.ToLower(), "[^a-z]", ""),
                    OpenInNewTab = false,
                    MetaLaunchUrl = request.LaunchUrl.ToLower(),
                    PolicyEngineMode = "any",
                    Group = "AutoTF-Managed"
                },
                Provider =
                {
                    Name = $"Managed provider for {request.Name}",
                    AuthenticationFlow = null,
                    AuthorizationFlow = request.AuthorizationFlow,
                    InvalidationFlow = request.InvalidationFlow,
                    PropertyMappings = [],
                    InternalHost = request.InternalHost.ToLower(),
                    ExternalHost = request.ExternalHost.ToLower(),
                    InternalHostSslValidation = true,
                    Certificate = null,
                    SkipPathRegex = "",
                    BasicAuthEnabled = false,
                    BasicAuthUserAttribute = "",
                    BasicAuthPasswordAttribute = "",
                    Mode = "proxy",
                    InterceptHeaderAuth = true,
                    CookieDomain = "",
                    JwtFederationSources = [],
                    JwtFederationProviders = [],
                    AccessTokenValidity = "hours=24",
                    ProviderModel = "authentik_providers_proxy.proxyprovider"
                },
                PolicyBindings = request.PolicyBindings
            };

            HttpContent content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            TransactionalCreationResponse? result = await ApiHttpHelper.SendPut<TransactionalCreationResponse>($"{_credentials.AuthUrl}/api/v3/core/transactional/applications/", content, _apiKey, true);
            
            if(result == null)
                return Result.Fail<TransactionalCreationResponse>(ResultCode.InternalServerError, "Failed to create the proxy.");
            
            return Result.Ok(result);
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when creating a application with proxy:");
            Console.WriteLine(e.ToString());

            return Result.Fail<TransactionalCreationResponse>(ResultCode.InternalServerError, "An unexpected error occurred while creating the proxy.");
        }
    }

    public async Task<Result<string>> GetOutposts()
    {
        return await ApiHttpHelper.SendGet($"{_credentials.AuthUrl}/api/v3/outposts/instances/?ordering=name&page=1&page_size=200&search=", _apiKey);
    }

    public async Task<Result<OutpostModel>> GetOutpost(string id)
    {
        return await ApiHttpHelper.SendGet<OutpostModel>($"{_credentials.AuthUrl}/api/v3/outposts/instances/{id}/", _apiKey);
    }

    public async Task<Result<string>> UpdateOutpost(string id, OutpostModel config)
    {
        HttpContent content = new StringContent(JsonSerializer.Serialize(config), Encoding.UTF8, "application/json");

        return await ApiHttpHelper.SendPut($"{_credentials.AuthUrl}/api/v3/outposts/instances/{id}/", content, _apiKey);
    }

    public async Task<Result<List<Flow>>> GetAuthorizationFlows()
    {
        Result<FlowPaginationRequest> result = await ApiHttpHelper.SendGet<FlowPaginationRequest>($"{_credentials.AuthUrl}/api/v3/flows/instances/?designation=authorization&ordering=slug", _apiKey);

        if (!result.IsSuccess || result.Value?.Results == null)
        {
            return Result.Fail<List<Flow>>(result.ResultCode, result.Error);
        }

        return Result.Ok(result.Value.Results);
    }

    public async Task<Result<List<Flow>>> GetInvalidationFlows()
    {
        Result<FlowPaginationRequest> result = await ApiHttpHelper.SendGet<FlowPaginationRequest>($"{_credentials.AuthUrl}/api/v3/flows/instances/?designation=invalidation&ordering=slug", _apiKey);
        
        if (!result.IsSuccess || result.Value?.Results == null)
        {
            return Result.Fail<List<Flow>>(result.ResultCode, result.Error);
        }

        return Result.Ok(result.Value.Results);
    }

    public async Task<Result<List<Group>>> GetGroups()
    {
        Result<GroupPaginationRequest> result = await ApiHttpHelper.SendGet<GroupPaginationRequest>($"{_credentials.AuthUrl}/api/v3/core/groups/?include_users=false&ordering=name", _apiKey);
        
        if (!result.IsSuccess || result.Value?.Results == null)
        {
            return Result.Fail<List<Group>>(result.ResultCode, result.Error);
        }

        return Result.Ok(result.Value.Results);
    }

    public async Task<Result<ProviderPaginationResult>> GetProviders()
    {
        Result<ProviderPaginationResult> result = await ApiHttpHelper.SendGet<ProviderPaginationResult>($"{_credentials.AuthUrl}/api/v3/providers/proxy/?application__isnull=false&ordering=name&page=1&page_size=200&search=", _apiKey);
        
        if (!result.IsSuccess || result.Value?.Results == null)
        {
            return result;
        }

        // If there are no results, we still want to return the pagination metadata
        if (!result.Value.Results.Any())
            return result;

        ProviderPaginationResult filtered = new ProviderPaginationResult
        {
            Pagination = result.Value.Pagination,
            Results = result.Value.Results.Where(x => x.Name.Contains("Managed provider for")).ToList()
        };

        return Result.Ok(filtered);
    }

    public async Task<Result<Provider>> GetProvider(string pk)
    {
        Result<ProviderPaginationResult> result = await GetProviders();
        
        if (!result.IsSuccess || result.Value?.Results == null)
        {
            return Result.Fail<Provider>(result.ResultCode, result.Error);
        }
        
        if (!result.Value.Results.Any())
            return Result.Fail<Provider>(ResultCode.NotFound, "Did not find any providers.");

        Provider? provider = result.Value.Results.FirstOrDefault(x => x.Pk != null && x.Pk == pk);

        if (provider == null)
            return Result.Fail<Provider>(ResultCode.NotFound, $"Could not find a provider by the given id {pk}.");

        return Result.Ok(provider);
    }

    public async Task<Result<ApplicationPaginationResult>> GetApplications()
    {
        Result<ApplicationPaginationResult> result = await ApiHttpHelper.SendGet<ApplicationPaginationResult>($"{_credentials.AuthUrl}/api/v3/core/applications/?ordering=name&page=1&page_size=200&search=&superuser_full_list=true", _apiKey);
        
        if (!result.IsSuccess || result.Value?.Results == null)
        {
            return result;
        }

        // If there are no results, we still want to return the pagination metadata
        if (!result.Value.Results.Any())
            return result;

        ApplicationPaginationResult filtered = new ApplicationPaginationResult
        {
            Pagination = result.Value.Pagination,
            Results = result.Value.Results.Where(x => x.Name.Contains("Managed application for ")).ToList()
        };

        return Result.Ok(filtered);
    }

    public async Task<Result<string>> DeleteProvider(string id)
    {
        return await ApiHttpHelper.SendDelete($"{_credentials.AuthUrl}/api/v3/providers/proxy/{id}/", _apiKey);
    }

    public async Task<Result<string>> DeleteApplication(string slug)
    {
        return await ApiHttpHelper.SendDelete($"{_credentials.AuthUrl}/api/v3/core/applications/{slug}/", _apiKey);
    }

    public async Task<Result<string>> GetProviderIdByExternalHost(string externalHost)
    {
        Result<ProviderPaginationResult> result = await GetProviders();
        
        if (!result.IsSuccess || result.Value?.Results == null)
        {
            return Result.Fail<string>(result.ResultCode, result.Error);
        }

        externalHost = externalHost.ToLower();
        Provider? provider = result.Value.Results.FirstOrDefault(x => x.ExternalHost.ToLower() == externalHost);
        
        if (provider == null)
            return Result.Fail<string>(ResultCode.NotFound, $"Could not find a provider by the given external host \"{externalHost}\".");
        
        if (string.IsNullOrEmpty(provider.Pk))
            return Result.Fail<string>(ResultCode.InternalServerError, "Provider found but had an invalid ID.");

        return Result.Ok(provider.Pk);
    }

    public async Task<Result<string>> GetApplicationSlugByLaunchUrl(string launchUrl)
    {
        Result<ApplicationPaginationResult> result = await GetApplications();
        
        if (!result.IsSuccess || result.Value?.Results == null)
        {
            return Result.Fail<string>(result.ResultCode, result.Error);
        }

        launchUrl = launchUrl.ToLower();
        Application? app = result.Value.Results.FirstOrDefault(x => x.LaunchUrl != null && x.LaunchUrl.ToLower() == launchUrl);
       
        if (app == null)
            return Result.Fail<string>(ResultCode.NotFound, $"Could not find a application by the given launch url \"{launchUrl}\".");
        
        return Result.Ok(app.Slug);
    }

    public async Task<Result<string>> AssignToOutpost(string outpostId, string providerPk)
    {
        // TODO: Check for existance
        Result<OutpostModel> result = await GetOutpost(outpostId);

        if (!result.IsSuccess || result.Value == null)
        {
            return Result.Fail<string>(result.ResultCode, result.Error);
        }
        
        // TODO: Check for provider existance 
        result.Value!.Providers.Add(providerPk);

        return await UpdateOutpost(outpostId, result.Value);
    }

    public async Task<Result<string>> UnassignFromOutpost(string outpostId, string providerPk)
    {
        // TODO: Check for existance
        Result<OutpostModel> result = await GetOutpost(outpostId);
        
        if (!result.IsSuccess || result.Value == null)
        {
            return Result.Fail<string>(result.ResultCode, result.Error);
        }
        
        // TODO: Check for provider existance 
        result.Value.Providers.Remove(providerPk);

        return await UpdateOutpost(outpostId, result.Value);
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