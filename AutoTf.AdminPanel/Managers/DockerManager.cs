using AutoTf.AdminPanel.Models.Requests;
using AutoTf.AdminPanel.Statics;
using Docker.DotNet;
using Docker.DotNet.Models;
using MemoryStats = AutoTf.AdminPanel.Models.Requests.MemoryStats;
using NetworkStats = Docker.DotNet.Models.NetworkStats;

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

    public async Task<MemoryStats?> GetMemoryStats(string id)
    {
        ContainerStatsResponse? response = await GetContainerStats(id);

        if (response == null)
            return null;
        
        float memoryUsageBytes = response.MemoryStats.Usage;
        float memoryLimitBytes = response.MemoryStats.Limit;
        
        float memoryUsageMb = memoryUsageBytes / (1024 * 1024);
        float memoryLimitMb = memoryLimitBytes / (1024 * 1024);
        float memoryPercentage = (memoryUsageBytes / memoryLimitBytes) * 100;

        MemoryStats stats = new MemoryStats()
        {
            MemoryUsageMb = memoryUsageMb,
            MemoryLimitMb = memoryLimitMb,
            MemoryPercentage = memoryPercentage
        };

        return stats;
    }

    public async Task<double?> GetCpuUsage(string id)
    {
        ContainerStatsResponse? response = await GetContainerStats(id);

        if (response == null)
            return null;
        
        ulong cpuDelta = response.CPUStats.CPUUsage.TotalUsage - response.PreCPUStats.CPUUsage.TotalUsage;
        ulong systemDelta = response.CPUStats.SystemUsage - response.PreCPUStats.SystemUsage;
        uint cpuCount = response.CPUStats.OnlineCPUs;

        double cpuPercent = 0;
        
        if (systemDelta > 0 && cpuDelta > 0)
        {
            cpuPercent = ((double)cpuDelta / systemDelta) * cpuCount * 100;
        }

        return cpuPercent;
    }

    public async Task<AutoTf.AdminPanel.Models.Requests.NetworkStats?> GetNetworkStats(string id)
    {
        ContainerStatsResponse? response = await GetContainerStats(id);

        if (response == null)
            return null;
        
        ulong totalRx = 0;
        ulong totalTx = 0;

        foreach (NetworkStats? net in response.Networks.Values)
        {
            totalRx += net.RxBytes;
            totalTx += net.TxBytes;
        }

        return new Models.Requests.NetworkStats()
        {
            TotalReceived = totalRx,
            TotalSend = totalTx
        };
    }

    public async Task<ContainerStatsResponse?> GetContainerStats(string id)
    {
        if (!await ContainerExists(id))
            return null;

        CancellationTokenSource cts = new CancellationTokenSource();
        ContainerStatsResponse stats = null;
        
        Progress<ContainerStatsResponse> progress = new Progress<ContainerStatsResponse>(s =>
        {
            stats = s;
            cts.Cancel(); // Stop streaming after the first result
        });

        try
        {
            await _dockerClient.Containers.GetContainerStatsAsync(id, new ContainerStatsParameters { Stream = true }, progress, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected, we canceled after getting the first stat
        }
        
        cts.Dispose();

        return stats;
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