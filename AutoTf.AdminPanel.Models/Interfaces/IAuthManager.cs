using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Models.Requests.Authentik;
using Microsoft.Extensions.Hosting;

namespace AutoTf.AdminPanel.Models.Interfaces;

public interface IAuthManager : IHostedService
{
    public Task<Result<TransactionalCreationResponse>> CreateProxy(CreateProxyRequest request);

    public Task<Result<string>> GetOutposts();

    public Task<Result<OutpostModel>> GetOutpost(string id);

    public Task<Result<string>> UpdateOutpost(string id, OutpostModel config);

    public Task<Result<List<Flow>>> GetAuthorizationFlows();

    public Task<Result<List<Flow>>> GetInvalidationFlows();

    public Task<Result<List<Group>>> GetGroups();

    public Task<Result<ProviderPaginationResult>> GetProviders();

    public Task<Result<ApplicationPaginationResult>> GetApplications();

    public Task<Result<string>> DeleteProvider(string id);

    public Task<Result<string>> DeleteApplication(string slug);

    public Task<Result<string>> GetProviderIdByExternalHost(string externalHost);

    public Task<Result<string>> GetApplicationSlugByLaunchUrl(string launchUrl);

    public Task<Result<string>> AssignToOutpost(string outpostId, string providerPk);

    public Task<Result<string>> UnassignFromOutpost(string outpostId, string providerPk);
}