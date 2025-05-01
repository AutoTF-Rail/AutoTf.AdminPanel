using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace AutoTf.AdminPanel.Managers;

public class DockerManager
{
    private readonly DockerClient _dockerClient;

    public DockerManager()
    {
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
    }

    // TODO: Cache?
    public async Task<List<ContainerListResponse>> GetContainers()
    {
        IList<ContainerListResponse>? containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters()
        {
            All = true
        });
        
        // This is honestly easier than figuring out the filter argument from the docker package.
        return containers.Where(x => x.Labels.ContainsKey("app.id") && x.Labels["app.id"] == "central-server-app").ToList();
    }

    public async Task<bool> ContainerExists(string id)
    {
        return (await GetContainers()).Any(x => x.ID == id);
    }

    public async Task<ContainerListResponse?> GetContainerByName(string name)
    {
        ContainerListResponse? containerListResponse = (await GetContainers()).FirstOrDefault(x => x.Names.Any(y => y.Contains(name.ToLower())));

        if (containerListResponse == null)
        {
            return null;
        }

        return containerListResponse;
    }

    public async Task<bool> ContainerRunning(string id)
    {
        ContainerInspectResponse container = await _dockerClient.Containers.InspectContainerAsync(id);
        
        return container.State.Status == "running";
    }

    public async Task<CreateContainerResponse> CreateContainer(CreateContainer parameters)
    {
        Dictionary<string,EndpointSettings> networks = await DockerHelper.ConfigureNetwork(parameters, this);
        parameters.ContainerName = parameters.ContainerName.ToLower();
        
        if (string.IsNullOrEmpty(parameters.ContainerName))
            parameters.ContainerName = parameters.EvuName.ToLower();

        Dictionary<string, EmptyStruct> exposedPorts = new Dictionary<string, EmptyStruct>();
        Dictionary<string, IList<PortBinding>> portBindings = new Dictionary<string, IList<PortBinding>>();
        
        foreach (KeyValuePair<string, string> portMapping in parameters.PortMappings)
        {
            if (portMapping.Key == "" || portMapping.Value == "")
                throw new Exception("Ports were empty.");
            
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
        if (!await ContainerExists(containerId))
            return false;
        
        return await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
    }

    public async Task<bool> StopContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return false;
        
        return await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
    }

    public async Task<bool> KillContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return false;
        
        if (!await ContainerRunning(containerId))
            return false;
        
        await _dockerClient.Containers.KillContainerAsync(containerId, new ContainerKillParameters());

        return true;
    }

    public async Task DeleteContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return;
        
        if (!await ContainerRunning(containerId))
            return;
        
        await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
    }

    public async Task<NetworkResponse?> GetNetwork(string name)
    {
        return (await _dockerClient.Networks.ListNetworksAsync()).FirstOrDefault(x => x.Name == name);
    }
}