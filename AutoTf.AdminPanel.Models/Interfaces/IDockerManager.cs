using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace AutoTf.AdminPanel.Models.Interfaces;

public interface IDockerManager
{
    DockerClient Client { get; }
    Task<List<ContainerListResponse>> GetAll();
    Result<float> GetContainerSize(ContainerListResponse container);
    Task<Result<float>> GetContainerSize(string containerId);
    Task<Result<float>> GetContainerSizeGb(string containerId);
    Task<bool> ContainerExists(string id);
    Task<Result<ContainerListResponse>> GetContainerByName(string name);
    Task<Result<ContainerListResponse>> GetContainerById(string id);
    Task<Result<ContainerInspectResponse>> InspectContainerById(string id);
    Task<bool> ContainerRunning(string id);
    Task<Result<CreateContainerResponse>> CreateContainer(CreateContainer parameters);
    Task<Result<object>> StartContainer(string containerId);
    Task<Result<object>> StopContainer(string containerId);
    Task<Result<object>> KillContainer(string containerId);
    Task<Result<object>> DeleteContainer(string containerId);
    Task<NetworkResponse?> GetNetwork(string name);
    Task<List<string>> GetNetworks();
    Task<string?> GetContainerNetworkIp(string containerId, string networkId);
    Task<Result<int>> GetTrainCount(string id);
    Task<Result<int>> GetAllowedTrainsCount(string id);
    Task<Result<object>> UpdateAllowedTrains(string id, int allowedTrains);
}