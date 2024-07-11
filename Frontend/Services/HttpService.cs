using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Shared.Models;

namespace Frontend.Services;

public class HttpService(HttpClient client, TokenService tokenService)
{
    private readonly HttpClient _client = client;
    private readonly TokenService _tokenService = tokenService;

    public async Task<bool> LoginAsync(string username, string password)
    {
        string json = JsonSerializer.Serialize(new
        {
            username,
            password
        });

        using StringContent jsonContent = new(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/login");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Content = jsonContent;
        try
        {
            var response = await _client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                await _tokenService.StoreTokenAsync(token);
                return true;
            }
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CheckAuthorizationAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/auth");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<ProjectDto>?> GetProjectsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            return await _client.GetFromJsonAsync<List<ProjectDto>>($"/projects?page={page}&pageSize={pageSize}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public async Task<ProjectDto?> GetProjectAsync(string id)
    {
        try
        {
            return await _client.GetFromJsonAsync<ProjectDto>($"/projects/{id}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public async Task<bool> PostProjectAsync(ProjectDto project)
    {
        string json = JsonSerializer.Serialize(project);
        var token = await _tokenService.GetTokenAsync();

        using StringContent jsonContent = new(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/projects");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = jsonContent;
        try
        {
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadFromJsonAsync<ProjectDto>();
            project.Id = content.Id;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public async Task<bool> PutProjectAsync(ProjectDto project)
    {
        var token = await _tokenService.GetTokenAsync();
        string json = JsonSerializer.Serialize(project);
        using StringContent jsonContent = new(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Put, "/projects");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = jsonContent;
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public async Task<bool> PutProjectImageAsync(string id, Image image)
    {
        var token = await _tokenService.GetTokenAsync();
        string json = JsonSerializer.Serialize(new { id, image });
        using StringContent jsonContent = new(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Put, "/projects/images");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = jsonContent;
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public async Task<bool> DeleteProjectAsync(string id)
    {
        var token = await _tokenService.GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/projects/{id}");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public async Task<Image?> GetImageAsync(string id)
    {
        try
        {
            var image = await _client.GetFromJsonAsync<Image>($"/images/{id}");
            return image;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> PostImageAsync(Image image, string projectId)
    {
        string json = JsonSerializer.Serialize(new { projectId, image });
        var token = await _tokenService.GetTokenAsync();

        using StringContent jsonContent = new(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/images");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = jsonContent;
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public async Task<bool> PostThumbnailAsync(Image image, string projectId)
    {
        string json = JsonSerializer.Serialize(new { projectId, image });
        var token = await _tokenService.GetTokenAsync();

        using StringContent jsonContent = new(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/images/thumbnails");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = jsonContent;
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }
}