using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace AutoTf.AdminPanel;

public static class HttpHelper
{
    /// <summary>
    /// Sends a GET request to the given endpoint and saves the video in the downloads folder
    /// </summary>
    /// <returns>The path to the file.</returns>
    public static async Task<string> SendGetVideo(string endpoint, string fileName, bool reThrow = true)
    {
        try
        {
            using HttpClient client = new HttpClient();
			
            HttpResponseMessage response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            byte[] videoBytes = await response.Content.ReadAsByteArrayAsync();
			
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadsFolder = Path.Combine(home, "Downloads");
            Directory.CreateDirectory(downloadsFolder);
			
            string tempFile = Path.Combine(downloadsFolder, $"{fileName}.mp4");
            await File.WriteAllBytesAsync(tempFile, videoBytes);
            Console.WriteLine($"Video saved to: {tempFile}");
			
            return tempFile;
        }
        catch (Exception)
        {
            Console.WriteLine($"An error occured while sending a get request to {endpoint}");
            
            if(reThrow)
                throw;
            return "";
        }
    }

    /// <summary>
    /// Sends a get request to the given endpoint and saves it in the downloads folder.
    /// </summary>
    /// <returns>The path to the file.</returns>
    public static async Task<string> SendGetFile(string endpoint, string fileName, bool reThrow = true)
    {
        try
        {
            using HttpClient client = new HttpClient();
			
            HttpResponseMessage response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            string[] content = JsonSerializer.Deserialize<string[]>(await response.Content.ReadAsStringAsync())!;
			
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string downloadsFolder = Path.Combine(home, "Downloads");
            Directory.CreateDirectory(downloadsFolder);
			
            string tempFile = Path.Combine(downloadsFolder, $"{fileName}.txt");
            await File.WriteAllTextAsync(tempFile, string.Join("\n", content));
            Console.WriteLine($"Logs saved to: {tempFile}");
            return tempFile;
        }
        catch (Exception)
        {
            Console.WriteLine($"An error occured while sending a get request to {endpoint}");
            
            if(reThrow)
                throw;
            return string.Empty;
        }
    }

    /// <summary>
    /// Sends a GET request to the given endpoint and returns it's content as a string array.
    /// </summary>
    public static async Task<string[]> SendGetStringArray(string endpoint, bool reThrow = true)
    {
        try
        {
            using HttpClient client = new HttpClient();
			
            HttpResponseMessage response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<string[]>(await response.Content.ReadAsStringAsync())!;
        }
        catch (Exception)
        {
            Console.WriteLine($"An error occured while sending a get request to {endpoint}");
            
            if(reThrow)
                throw;
            return [];
        }
    }

    /// <summary>
    /// Sends a GET request to the given endpoint and returns it's content as a string.
    /// </summary>
    public static async Task<string> SendGetString(string endpoint, bool reThrow = true)
    {
        try
        {
            using HttpClient client = new HttpClient();
			
            HttpResponseMessage response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception)
        {
            Console.WriteLine($"An error occured while sending a get request to {endpoint}");
            
            if(reThrow)
                throw;
            return "";
        }
    }

    public static async Task SendPost(string endpoint, HttpContent content, bool reThrow = true)
    {
        try
        {
            using HttpClient client = new HttpClient();
			
            HttpResponseMessage response = await client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception)
        {
            Console.WriteLine($"An error occured while sending a post request to {endpoint}");
            
            if(reThrow)
                throw;
        }
    }
	
    public static void OpenBrowser(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported platform.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open URL: {ex.Message}");
        }
    }
}