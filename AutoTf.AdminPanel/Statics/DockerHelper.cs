using System.Net;
using System.Net.Sockets;
using AutoTf.AdminPanel.Managers;
using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet.Models;

namespace AutoTf.AdminPanel.Statics;

public static class DockerHelper
{
    public static async Task<Dictionary<string, EndpointSettings>> ConfigureNetwork(CreateContainer parameters, DockerManager dockerManager)
    {
        Dictionary<string, EndpointSettings> dict = new Dictionary<string, EndpointSettings>();
        NetworkResponse? defaultNetwork = await dockerManager.GetNetwork(parameters.DefaultNetwork);

        if (defaultNetwork == null)
            throw new Exception("Could not find network.");

        string newIp = parameters.DefaultIp;

        if (string.IsNullOrEmpty(newIp))
            newIp = GetFreeIp(defaultNetwork); 
        
        dict.Add(parameters.DefaultNetwork!, new EndpointSettings()
        {
            IPAddress = newIp,
            Gateway = defaultNetwork.IPAM.Config.First().Gateway,
        });

        if (string.IsNullOrEmpty(parameters.AdditionalNetwork)) 
            return dict;
        
        // Configure additional network
        NetworkResponse? additionalNetwork = await dockerManager.GetNetwork(parameters.DefaultNetwork);
        if (additionalNetwork == null)
            throw new Exception("Could not find additional network.");

        string additionalIp = GetFreeIp(additionalNetwork);
        dict.Add(parameters.AdditionalNetwork, new EndpointSettings()
        {
            IPAddress = additionalIp,
            Gateway = defaultNetwork.IPAM.Config.First().Gateway
        });

        return dict;
    }

    private static string GetFreeIp(NetworkResponse network)
    {
        string? subnet = network.IPAM.Config.FirstOrDefault()?.Subnet;
        string? gateway = network.IPAM.Config.FirstOrDefault()?.Gateway;
        
        if (string.IsNullOrEmpty(subnet) || string.IsNullOrEmpty(gateway))
            throw new Exception("Invalid network config");
        
        string[] parts = subnet.Split('/');
        
        IPAddress baseIp = IPAddress.Parse(parts[0]);
        int prefix = int.Parse(parts[1]);
        
        
        HashSet<string> usedIps = new HashSet<string>(
            (network.Containers.Values.Select(c => c.IPv4Address?.Split('/')[0]))!
        );

        IEnumerable<IPAddress> allIps = GetAllIps(baseIp, prefix);
        foreach (IPAddress ip in allIps)
        {
            string ipStr = ip.ToString();
            
            if (ipStr == gateway) 
                continue; 
            
            if (!usedIps.Contains(ipStr))
                return ipStr;
        }

        throw new Exception("No IPs available in the network.");
    }
    
    private static IEnumerable<IPAddress> GetAllIps(IPAddress baseIp, int prefixLength)
    {
        if (baseIp.AddressFamily != AddressFamily.InterNetwork)
            throw new ArgumentException("Only IPv4 supported");

        uint ipAsUint = IpToUint(baseIp);
        uint mask = uint.MaxValue << (32 - prefixLength);
        uint networkAddress = ipAsUint & mask;
        uint broadcastAddress = networkAddress | ~mask;

        for (uint ip = networkAddress + 2; ip < broadcastAddress; ip++) // +2 to skip .0 and .1
        {
            yield return UintToIp(ip);
        }
    }
    
    private static uint IpToUint(IPAddress ip)
    {
        byte[] bytes = ip.GetAddressBytes();
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private static IPAddress UintToIp(uint ip)
    {
        byte[] bytes = BitConverter.GetBytes(ip);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return new IPAddress(bytes);
    }
}