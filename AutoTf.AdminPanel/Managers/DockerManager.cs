using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Enums;
using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Docker.DotNet;
using Docker.DotNet.Models;

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

    public Result<float> GetContainerSize(ContainerListResponse container)
    {
        return GetDirectorySize(Path.Combine("/etc/AutoTf/CentralServer/", container.Names.First().TrimStart('/')));
    }

    public async Task<Result<float>> GetContainerSizeGb(string containerId)
    {
        Result<float> containerSize = await GetContainerSize(containerId);
        
        if (!containerSize.IsSuccess)
            return containerSize;
        
        return Result.Ok(MathF.Round((float)(containerSize.Value / (1024.0 * 1024.0 * 1024.0)), 2));
    }

    public async Task<Result<float>> GetContainerSize(string containerId)
    {
        Result<ContainerListResponse> container = await GetContainerById(containerId);
        
        if (!container.IsSuccess || container.Value == null)
            return Result.Fail<float>(ResultCode.NotFound, $"Could not find container {containerId}.");

        return GetContainerSize(container.Value);
    }
    
    private Result<float> GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
            return Result.Fail<float>(ResultCode.NotFound, $"Could not find directory {path}.");

        float size = 0;
        
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
        
        return Result.Ok(size);
    }

    public async Task<bool> ContainerExists(string id)
    {
        return (await GetAll()).Any(x => x.ID == id);
    }

    public async Task<Result<ContainerListResponse>> GetContainerByName(string name)
    {
        ContainerListResponse? containerListResponse = (await GetAll()).FirstOrDefault(x => x.Names.Any(y => y.Contains(name.ToLower())));

        if (containerListResponse == null)
        {
            return Result.Fail<ContainerListResponse>(ResultCode.NotFound, $"Could not find container \"{name}\".");
        }

        return Result.Ok(containerListResponse);
    }

    public async Task<Result<ContainerListResponse>> GetContainerById(string id)
    {
        ContainerListResponse? containerListResponse = (await GetAll()).FirstOrDefault(x => x.ID == id);

        if (containerListResponse == null)
        {
            return Result.Fail<ContainerListResponse>(ResultCode.NotFound, $"Could not find container {id}.");
        }

        return Result.Ok(containerListResponse);
    }

    public async Task<Result<ContainerInspectResponse>> InspectContainerById(string id)
    {
        if (!await ContainerExists(id))
        {
            return Result.Fail<ContainerInspectResponse>(ResultCode.NotFound, $"Could not find container {id}.");
        }

        return Result.Ok(await Client.Containers.InspectContainerAsync(id));
    }

    public async Task<bool> ContainerRunning(string id)
    {
        ContainerInspectResponse container = await Client.Containers.InspectContainerAsync(id);
        
        return container.State.Status == "running";
    }

    public async Task<Result<CreateContainerResponse>> CreateContainer(CreateContainer parameters)
    {
        try
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
                    return Result.Fail<CreateContainerResponse>(ResultCode.BadRequest, "Ports were empty");
                
                string hostPort = portMapping.Key;
                string containerPort = portMapping.Value;

                exposedPorts[containerPort + "/tcp"] = new EmptyStruct();

                if (!portBindings.ContainsKey(containerPort + "/tcp"))
                    portBindings[containerPort + "/tcp"] = new List<PortBinding>();
                
                portBindings[containerPort + "/tcp"].Add(new PortBinding { HostPort = hostPort });
            }
            
            return Result<CreateContainerResponse>.Ok(await Client.Containers.CreateContainerAsync(new CreateContainerParameters()
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
                    $"containerName={parameters.ContainerName}",
                    $"allowedTrainsCount={parameters.AllowedTrainsCount}"
                },
            }));
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong when creating a container:");
            Console.WriteLine(e.ToString());

            return Result.Fail<CreateContainerResponse>(ResultCode.InternalServerError, "An unexpected error occurred while creating the container.");
        }
    }
    
    public async Task<Result<object>> StartContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return Result.Fail<object>(ResultCode.NotFound, $"Could not find container {containerId}.");

        bool result = await Client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
        
        if (result)
            return Result.Ok();

        return Result.Fail<object>(ResultCode.InternalServerError, $"Could not start container {containerId}.");
    }

    public async Task<Result<object>> StopContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return Result.Fail<object>(ResultCode.NotFound, $"Could not find container {containerId}.");
        
        bool result = await Client.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        
        if (result)
            return Result.Ok();

        return Result.Fail<object>(ResultCode.InternalServerError, $"Could not stop container {containerId}.");
    }

    public async Task<Result<object>> KillContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return Result.Fail<object>(ResultCode.NotFound, $"Could not find container {containerId}.");

        if (!await ContainerRunning(containerId))
            return Result.Ok();

        await Client.Containers.KillContainerAsync(containerId, new ContainerKillParameters());

        return Result.Ok();
    }

    public async Task<Result<object>> DeleteContainer(string containerId)
    {
        if (!await ContainerExists(containerId))
            return Result.Fail<object>(ResultCode.NotFound, $"Could not find container {containerId}.");
        
        if (await ContainerRunning(containerId))
            return Result.Fail<object>(ResultCode.InternalServerError, $"Could not delete container {containerId} because it is still running.");

        Result<ContainerListResponse> container = await GetContainerById(containerId);
        
        if (!container.IsSuccess || container.Value == null)
            return Result.Fail<object>(container.ResultCode, container.Error);
        
        await Client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
        
        string dir = $"/etc/AutoTf/CentralServer/{container.Value.Names.First()}";
        
        if(Directory.Exists(dir))
            Directory.Delete(dir, true);

        return Result.Ok();
    }

    public async Task<NetworkResponse?> GetNetwork(string name)
    {
        return (await Client.Networks.ListNetworksAsync()).FirstOrDefault(x => x.Name == name);
    }

    public async Task<List<string>> GetNetworks()
    {
        return (await Client.Networks.ListNetworksAsync()).Select(x => x.Name).ToList();
    }

    public async Task<string?> GetContainerNetworkIp(string containerId, string networkId)
    {
        EndpointSettings? network = (await Client.Containers.InspectContainerAsync(containerId)).NetworkSettings.Networks.Values.FirstOrDefault(x => x.NetworkID == networkId);
        
        return network?.IPAddress;
    }

    public async Task<Result<int>> GetTrainCount(string id)
    {
        Result<ContainerListResponse> container = await GetContainerById(id);
        
        if (!container.IsSuccess || container.Value == null)
            return Result.Ok(0);

        KeyValuePair<string, EndpointSettings>? network = container.Value.NetworkSettings.Networks.FirstOrDefault();

        if (network == null)
            return Result.Ok(0);

        return Result<int>.Ok(await HttpHelper.SendGet<int>($"http://{network.Value.Value.IPAddress}:8080/sync/device/trainCount"));
    }

    public async Task<Result<int>> GetAllowedTrainsCount(string id)
    {
        Result<ContainerListResponse> container = await GetContainerById(id);
        
        if (!container.IsSuccess || container.Value == null)
            return Result.Ok(0);

        KeyValuePair<string, EndpointSettings>? network = container.Value.NetworkSettings.Networks.FirstOrDefault();

        if (network == null)
            return Result.Ok(0);

        return Result<int>.Ok( await HttpHelper.SendGet<int>($"http://{network.Value.Value.IPAddress}:8080/sync/device/allowedTrainsCount"));
    }

    public async Task<Result<object>> UpdateAllowedTrains(string id, int allowedTrains)
    {
        if (!await ContainerExists(id))
            return Result.Fail<object>(ResultCode.NotFound, $"Could not find container {id}.");
        
        Result<ContainerListResponse> container = (await GetContainerById(id));
        
        if (!container.IsSuccess || container.Value == null)
            return Result.Fail<object>(ResultCode.NotFound, $"Could not find container {id}.");
        
        string dir = $"/etc/AutoTf/CentralServer/{container.Value.Names.First()}";

        Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(Path.Combine(dir, "allowedTrainsCount"), allowedTrains.ToString());

        return Result.Ok();
    }
}