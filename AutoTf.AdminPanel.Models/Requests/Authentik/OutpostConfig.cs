using System.Text.Json.Serialization;

namespace AutoTf.AdminPanel.Models.Requests.Authentik;

public class OutpostConfig
{
    [JsonPropertyName("log_level")]
    public required string LogLevel { get; set; }
    
    [JsonPropertyName("docker_labels")]
    public List<object>? DockerLabels { get; set; }
    
    [JsonPropertyName("authentik_host")]
    public required string AuthentikHost { get; set; }
    
    [JsonPropertyName("docker_network")]
    public object? DockerNetwork { get; set; }
    
    [JsonPropertyName("container_image")]
    public object? ContainerImage { get; set; }
    
    [JsonPropertyName("docker_map_ports")]
    public bool DockerMapPorts { get; set; }
    
    [JsonPropertyName("refresh_interval")]
    public required string RefreshInterval { get; set; }
    
    [JsonPropertyName("kubernetes_replicas")]
    public int KubernetesReplicas { get; set; }
    
    [JsonPropertyName("kubernetes_namespace")]
    public required string KubernetesNamespace { get; set; }
    
    [JsonPropertyName("authentik_host_browser")]
    public required string AuthentikHostBrowser { get; set; }
    
    [JsonPropertyName("object_naming_template")]
    public required string ObjectNamingTemplate { get; set; }
    
    [JsonPropertyName("authentik_host_insecure")]
    public bool AuthentikHostInsecure { get; set; }
    
    [JsonPropertyName("kubernetes_json_patches")]
    public object? KubernetesJsonPatches { get; set; }
    
    [JsonPropertyName("kubernetes_service_type")]
    public required string KubernetesServiceType { get; set; }
    
    [JsonPropertyName("kubernetes_image_pull_secrets")]
    public required List<object> KubernetesImagePullSecrets { get; set; }
    
    [JsonPropertyName("kubernetes_ingress_class_name")]
    public object? KubernetesIngressClassName { get; set; }
    
    [JsonPropertyName("kubernetes_disabled_components")]
    public required List<object> KubernetesDisabledComponents { get; set; }
    
    [JsonPropertyName("kubernetes_ingress_annotations")]
    public required object KubernetesIngressAnnotations { get; set; }
    
    [JsonPropertyName("kubernetes_ingress_secret_name")]
    public required string KubernetesIngressSecretName { get; set; }
}