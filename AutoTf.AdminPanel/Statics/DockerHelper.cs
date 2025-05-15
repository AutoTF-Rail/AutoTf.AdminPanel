using AutoTf.AdminPanel.Models.Interfaces;
using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet.Models;

namespace AutoTf.AdminPanel.Statics;

public static class DockerHelper
{
    public static async Task<Dictionary<string, EndpointSettings>> ConfigureNetwork(CreateContainer parameters, IDockerManager dockerManager)
    {
        Dictionary<string, EndpointSettings> dict = new Dictionary<string, EndpointSettings>();
        NetworkResponse? defaultNetwork = await dockerManager.GetNetwork(parameters.DefaultNetwork);
        IEnumerable<string> containersInNetwork = (await dockerManager.GetAll()).Where(x => x.NetworkSettings.Networks.ContainsKey(parameters.DefaultNetwork)).Select(x => x.NetworkSettings.Networks[parameters.DefaultNetwork].IPAddress);

        if (defaultNetwork == null)
            throw new Exception("Could not find network.");

        string newIp = parameters.DefaultIp;

        if (string.IsNullOrEmpty(newIp))
            newIp = GetFreeIp(defaultNetwork, containersInNetwork); 
        
        dict.Add(parameters.DefaultNetwork!, new EndpointSettings
        {
            IPAddress = newIp,
            Gateway = defaultNetwork.IPAM.Config.First().Gateway,
        });

        if (string.IsNullOrEmpty(parameters.AdditionalNetwork)) 
            return dict;
        
        // Configure additional network
        NetworkResponse? additionalNetwork = await dockerManager.GetNetwork(parameters.AdditionalNetwork);
        if (additionalNetwork == null)
            throw new Exception("Could not find additional network.");
        IEnumerable<string> containersInAdditionalNetwork = (await dockerManager.GetAll()).Where(x => x.NetworkSettings.Networks.ContainsKey(parameters.AdditionalNetwork)).Select(x => x.NetworkSettings.Networks[parameters.AdditionalNetwork].IPAddress);

        string additionalIp = GetFreeIp(additionalNetwork, containersInAdditionalNetwork);
        dict.Add(parameters.AdditionalNetwork, new EndpointSettings
        {
            IPAddress = additionalIp,
            Gateway = defaultNetwork.IPAM.Config.First().Gateway
        });

        return dict;
    }

    private static string GetFreeIp(NetworkResponse network, IEnumerable<string> usedIps)
    {
        string? subnet = network.IPAM.Config.FirstOrDefault()?.Subnet;
        if (string.IsNullOrEmpty(subnet))
            throw new Exception("Missing subnet in network config");

        string baseIp = subnet.Split('.')[0] + "." + subnet.Split('.')[1] + "." + subnet.Split('.')[2];

        for (int i = 2; i < 255; i++) 
        {
            string candidate = $"{baseIp}.{i}";
            if (!usedIps.Contains(candidate))
                return candidate;
        }

        throw new Exception("No free IPs found in the subnet.");
    }
}