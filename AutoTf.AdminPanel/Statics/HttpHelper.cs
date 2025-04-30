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
}