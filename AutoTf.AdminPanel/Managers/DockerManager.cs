using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoTf.AdminPanel.Managers;

public class DockerManager
{
    public readonly DockerClient Client;

    public DockerManager()
    {
#if RELEASE
        Client = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
#endif
    }

    // TODO: Cache?
    public async Task<List<ContainerListResponse>> GetAll()
    {
        #if DEBUG
        return new List<ContainerListResponse>();
        #endif
        IList<ContainerListResponse>? containers = await Client.Containers.ListContainersAsync(new ContainersListParameters()
        {
            All = true
        });
        
        // This is honestly easier than figuring out the filter argument from the docker package.
        return containers.Where(x => x.Labels.ContainsKey("app.id") && x.Labels["app.id"] == "central-server-app").ToList();
    }

    public long GetContainerSize(ContainerListResponse container)
    {
        return GetDirectorySize(Path.Combine("/etc/AutoTf/CentralServer/", container.Names.First().TrimStart('/')));
    }

    public async Task<long> GetContainerSize(string containerId)
    {
        ContainerListResponse? container = await GetContainerById(containerId);
        
        if (container == null)
            return -1000;

        return GetContainerSize(container);
    }
    
    private long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
            return -2000;

        long size = 0;
        foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            try
            {
                size += new FileInfo(file).Length;
            }
            catch
            {
                // ignored
            }
        }
        
        return size;
    }

    public async Task<bool> ContainerExists(string id)
    {
        return (await GetAll()).Any(x => x.ID == id);
    }

    public async Task<ContainerListResponse?> GetContainerByName(string name)
    {
        ContainerListResponse? containerListResponse = (await GetAll()).FirstOrDefault(x => x.Names.Any(y => y.Contains(name.ToLower())));

        if (containerListResponse == null)
        {
            return null;
        }

        return containerListResponse;
    }

    public async Task<ContainerListResponse?> GetContainerById(string id)
    {
        ContainerListResponse? containerListResponse = (await GetAll()).FirstOrDefault(x => x.ID == id);

        if (containerListResponse == null)
        {
            return null;
        }

        return containerListResponse;
    }

    public async Task<bool> ContainerRunning(string id)
    {
        ContainerInspectResponse container = await Client.Containers.InspectContainerAsync(id);
        
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
        
        return await Client.Containers.CreateContainerAsync(new CreateContainerParameters()
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
                },
                Binds = new List<string>()
                {
                    $"/etc/AutoTf/CentralServer/{parameters.ContainerName}:/Data"
                }
            },
            NetworkingConfig = new NetworkingConfig()
            {
                EndpointsConfig = networks
            },
            Env = new List<string>()
            {
                $"evuName={parameters.EvuName}",
                $"containerName={parameters.ContainerName}"
            },
        });
    }
    
    public async Task<bool> StartContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return false;
        
        return await Client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
    }

    public async Task<bool> StopContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return false;
        
        return await Client.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
    }

    public async Task<bool> KillContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return false;
        
        if (!await ContainerRunning(containerId))
            return false;
        
        await Client.Containers.KillContainerAsync(containerId, new ContainerKillParameters());

        return true;
    }

    public async Task<bool> DeleteContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return false;
        
        if (await ContainerRunning(containerId))
            return false;

        ContainerListResponse container = (await GetContainerById(containerId))!;
        await Client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
        
        string dir = $"/etc/AutoTf/CentralServer/{container.Names.First()}";
        if(Directory.Exists(dir))
            Directory.Delete(dir);

        return true;
    }

    public async Task<NetworkResponse?> GetNetwork(string name)
    {
        return (await Client.Networks.ListNetworksAsync()).FirstOrDefault(x => x.Name == name);
    }

    public async Task<List<string>> GetNetworks()
    {
        return (await Client.Networks.ListNetworksAsync()).Select(x => x.Name).ToList();
    }

    public async Task<int> GetTrainCount(string id)
    {
        ContainerListResponse? container = await GetContainerById(id);
        
        if (container == null)
            return 0;

        KeyValuePair<string, EndpointSettings>? network = container.NetworkSettings.Networks.FirstOrDefault();

        if (network == null)
            return 0;

        return await HttpHelper.SendGet<int>($"http://{network.Value.Value.IPAddress}:8080/sync/device/trainCount");
    }
}