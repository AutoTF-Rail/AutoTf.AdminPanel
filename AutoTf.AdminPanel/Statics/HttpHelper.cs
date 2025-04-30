using System.Net.Http.Headers;
using System.Text.Json;

namespace AutoTf.AdminPanel.Statics;

public static class HttpHelper
{
    /// <summary>
    /// Sends a GET request to the given endpoint and returns it's content as the given type.
    /// </summary>
    public static async Task<T?> SendGet<T>(string endpoint, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            
            HttpResponseMessage response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }
    
    /// <summary>
    /// Sends a GET request to the given endpoint and returns it's content as a string.
    /// </summary>
    public static async Task<T?> SendPost<T>(string endpoint, HttpContent content, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }
    
    /// <summary>
    /// Sends a GET request to the given endpoint and returns it's content as a string.
    /// </summary>
    public static async Task<bool> SendPost(string endpoint, HttpContent content, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            
            return response.IsSuccessStatusCode;
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }

    public static async Task<bool> SendCloudflareDelete(string endpoint, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            HttpResponseMessage response = await client.DeleteAsync(endpoint);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            if (reThrow)
                throw;
            else
            {
                Console.WriteLine(ex.ToString());
            }
            
            return false;
        }
    }
    
    public static async Task<T?> SendGetCloudflare<T>(string endpoint, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            HttpResponseMessage response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }
    
    public static async Task<T?> SendPostCloudflare<T>(string endpoint, HttpContent content, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }
    
    public static async Task<T?> SendPatchCloudflare<T>(string endpoint, HttpContent content, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            HttpResponseMessage response = await client.PatchAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }
    
    public static async Task<string?> SendPatchCloudflare(string endpoint, HttpContent content, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            HttpResponseMessage response = await client.PatchAsync(endpoint, content);
            
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }
}