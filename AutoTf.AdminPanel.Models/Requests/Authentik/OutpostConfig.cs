using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class OutpostConfig
{
    [JsonPropertyName("log_level")]
    public string LogLevel { get; set; }
    
    [JsonPropertyName("docker_labels")]
    public List<object> DockerLabels { get; set; }
    
    [JsonPropertyName("authentik_host")]
    public string AuthentikHost { get; set; }
    
    [JsonPropertyName("docker_network")]
    public object DockerNetwork { get; set; }
    
    [JsonPropertyName("container_image")]
    public object ContainerImage { get; set; }
    
    [JsonPropertyName("docker_map_ports")]
    public bool DockerMapPorts { get; set; }
    
    [JsonPropertyName("refresh_interval")]
    public string RefreshInterval { get; set; }
    
    [JsonPropertyName("kubernetes_replicas")]
    public int KubernetesReplicas { get; set; }
    
    [JsonPropertyName("kubernetes_namespace")]
    public string KubernetesNamespace { get; set; }
    
    [JsonPropertyName("authentik_host_browser")]
    public string AuthentikHostBrowser { get; set; }
    
    [JsonPropertyName("object_naming_template")]
    public string ObjectNamingTemplate { get; set; }
    
    [JsonPropertyName("authentik_host_insecure")]
    public bool AuthentikHostInsecure { get; set; }
    
    [JsonPropertyName("kubernetes_json_patches")]
    public object KubernetesJsonPatches { get; set; }
    
    [JsonPropertyName("kubernetes_service_type")]
    public string KubernetesServiceType { get; set; }
    
    [JsonPropertyName("kubernetes_image_pull_secrets")]
    public List<object> KubernetesImagePullSecrets { get; set; }
    
    [JsonPropertyName("kubernetes_ingress_class_name")]
    public object KubernetesImagePullSecret { get; set; }
    
    [JsonPropertyName("kubernetes_disabled_components")]
    public List<object> KubernetesDisabledComponents { get; set; }
    
    [JsonPropertyName("kubernetes_ingress_annotations")]
    public object KubernetesIngressAnnotations { get; set; }
    
    [JsonPropertyName("kubernetes_ingress_secret_name")]
    public string KubernetesIngressSecretName { get; set; }
}