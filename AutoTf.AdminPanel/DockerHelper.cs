using AutoTf.AdminPanel.Models.Requests;
using Docker.DotNet.Models;

namespace AutoTf.AdminPanel;

public static class DockerHelper
{
    public static Dictionary<string, EndpointSettings> AssembleEndpoints(CreateContainer parameters)
    {
        Dictionary<string, EndpointSettings> dict = new Dictionary<string, EndpointSettings>();
        
        dict.Add(parameters.DefaultNetwork!, new EndpointSettings()
        {
            IPAddress = parameters.DefaultIp // "centralServerNetwork"
        });
        
        if (!string.IsNullOrEmpty(parameters.AdditionalNetwork))
            dict.Add(parameters.AdditionalNetwork, new EndpointSettings()); // authinstall_default

        return dict;
    }
}