using System.Net.Http.Headers;
using System.Text.Json;
using AutoTf.AdminPanel.Models;
using AutoTf.AdminPanel.Models.Enums;

namespace AutoTf.AdminPanel.Statics;

public class ApiHttpHelper
{
    public static async Task<Result<string>> SendDelete(string endpoint, string apiKey, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        try
        {
            HttpResponseMessage response = await client.DeleteAsync(endpoint);

            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return Result.Ok(content);

            return Result.Fail<string>(Result.MapStatusToResultCode(response.StatusCode), content);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
        }
    }
    
    public static async Task<Result<string>> SendGet(string endpoint, string apiKey, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        try
        {
            HttpResponseMessage response = await client.GetAsync(endpoint);

            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return Result.Ok(content);

            return Result.Fail<string>(Result.MapStatusToResultCode(response.StatusCode), content);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
        }
    }
    
    public static async Task<Result<T>> SendGet<T>(string endpoint, string apiKey, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        try
        {
            HttpResponseMessage response = await client.GetAsync(endpoint);

            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                T? value = JsonSerializer.Deserialize<T>(content);

                if (value == null)
                    return Result.Fail<T>(ResultCode.InternalServerError, "Deserialization returned null.");

                return Result.Ok(value);
            }

            return Result.Fail<T>(Result.MapStatusToResultCode(response.StatusCode), content);
        }
        catch (Exception ex)
        {
            return Result.Fail<T>(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
        }
    }
    
    public static async Task<Result<T>> SendPost<T>(string endpoint, HttpContent content, string apiKey, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        try
        {
            HttpResponseMessage response = await client.PostAsync(endpoint, content);

            string result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                T? value = JsonSerializer.Deserialize<T>(result);

                if (value == null)
                    return Result.Fail<T>(ResultCode.InternalServerError, "Deserialization returned null.");

                return Result.Ok(value);
            }

            return Result.Fail<T>(Result.MapStatusToResultCode(response.StatusCode), result);
        }
        catch (Exception ex)
        {
            return Result.Fail<T>(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
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
    
    public static async Task<Result<string>> SendPatch(string endpoint, HttpContent content, string apiKey, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        try
        {
            HttpResponseMessage response = await client.PatchAsync(endpoint, content);

            string result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return Result.Ok(result);
            }

            return Result.Fail<string>(Result.MapStatusToResultCode(response.StatusCode), result);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
        }
    }
    
    public static async Task<Result<string>> SendPut(string endpoint, HttpContent content, string apiKey, int timeoutSeconds = 5)
    {
        using HttpClient client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        
        try
        {
            HttpResponseMessage response = await client.PutAsync(endpoint, content);

            string result = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return Result.Ok(result);
            }

            return Result.Fail<string>(Result.MapStatusToResultCode(response.StatusCode), result);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(ResultCode.InternalServerError, $"Exception occurred: {ex.Message}");
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