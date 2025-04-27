using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;

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
        
        // This is honestly easier than figuring out the filter argument from the docker package.
        return containers.Where(x => x.Labels.ContainsKey("com.docker.compose.project") && x.Labels["com.docker.compose.project"] == "centralserver").ToList();
    }

    public async Task<CreateContainerResponse> CreateContainer(CreateContainer parameters, Dictionary<string, EndpointSettings> networks)
    {
        
        Dictionary<string, EmptyStruct> exposedPorts = new Dictionary<string, EmptyStruct>();
        Dictionary<string, IList<PortBinding>> portBindings = new Dictionary<string, IList<PortBinding>>();
        
        foreach (KeyValuePair<string, string> portMapping in parameters.PortMappings)
        {
            string hostPort = portMapping.Key;
            string containerPort = portMapping.Value;

            exposedPorts[containerPort + "/tcp"] = new EmptyStruct();

            if (!portBindings.ContainsKey(containerPort + "/tcp"))
                portBindings[containerPort + "/tcp"] = new List<PortBinding>();
            
            portBindings[containerPort + "/tcp"].Add(new PortBinding { HostPort = hostPort });
        }
        
        return await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters()
        {
            Name = parameters.ContainerName,
            Image = parameters.Image,
            ExposedPorts = exposedPorts,
            HostConfig = new HostConfig
            {
                PortBindings = portBindings,
                RestartPolicy = new RestartPolicy()
                {
                    Name = RestartPolicyKind.UnlessStopped
                }
            },
            NetworkingConfig = new NetworkingConfig()
            {
                EndpointsConfig = networks
            },
            Env = new List<string>()
            {
                $"evuName={parameters.EvuName}"
            }
        });
    }
    
    public async Task<bool> StartContainer(string containerId)
    {
        return await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
    }

    public async Task<bool> StopContainer(string containerId)
    {
        return await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
    }

    public async Task DeleteContainer(string containerId)
    {
        await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
    }

    public async Task<NetworkResponse?> GetNetwork(string name)
    {
        return (await _dockerClient.Networks.ListNetworksAsync()).FirstOrDefault(x => x.Name == name);
    }
}