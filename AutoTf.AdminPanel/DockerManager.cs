using Docker.DotNet;
using Docker.DotNet.Models;

namespace AutoTf.AdminPanel;

public class DockerManager
{
    private readonly DockerClient _dockerClient;

    public DockerManager()
    {
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
    }

    public async Task<List<ContainerListResponse>> GetContainers()
    {
        IList<ContainerListResponse>? containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters()
        {
            All = true
        });
        return containers.ToList();
    }
    
    public async Task StartContainer(string containerId)
    {
        await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
    }

    public async Task StopContainer(string containerId)
    {
        await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
    }
}