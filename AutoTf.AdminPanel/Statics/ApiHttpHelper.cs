using System.Net.Http.Headers;
using System.Text.Json;

namespace AutoTf.AdminPanel.Statics;

public class ApiHttpHelper
{
    public static async Task<bool> SendDelete(string endpoint, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
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
    
    public static async Task<T?> SendGet<T>(string endpoint, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
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
    
    public static async Task<T?> SendPost<T>(string endpoint, HttpContent content, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
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
    
    public static async Task<T?> SendPatch<T>(string endpoint, HttpContent content, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
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
    
    public static async Task<string?> SendPatch(string endpoint, HttpContent content, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
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
    
    public static async Task<string?> SendPut(string endpoint, HttpContent content, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            HttpResponseMessage response = await client.PutAsync(endpoint, content);
            
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }
    
    public static async Task<T?> SendPut<T>(string endpoint, HttpContent content, string apiKey, bool reThrow = false, int timeoutSeconds = 5)
    {
        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            
            HttpResponseMessage response = await client.PutAsync(endpoint, content);
            
            return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            if(reThrow)
                throw;

            return default;
        }
    }
}