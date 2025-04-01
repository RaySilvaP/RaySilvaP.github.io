using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Text.Json;
using Frontend.Exceptions;
using Shared.Models;
using BlazorBootstrap;

namespace Frontend.Services;

public class HttpService(HttpClient client, TokenService tokenService, ILogger<HttpService> logger)
{
    private readonly HttpClient _client = client;
    private readonly TokenService _tokenService = tokenService;
    private readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly ILogger<HttpService> _logger = logger;

    public async Task LoginAsync(string username, string password)
    {
        string json = JsonSerializer.Serialize(new { username, password });
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/login");
        request.Content = content;

        var response = await _client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadAsStringAsync();
            await _tokenService.StoreTokenAsync(token);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AuthenticationFailException("Wrong credentials.");
        else
            throw new Exception("Something went wrong.");
    }

    public async Task<bool> CheckAuthorizationAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Head, "/auth");
        var response = await _client.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    public async Task<(int, List<Project>)> GetProjectsAsync(int page = 1, int pageSize = 10)
    {
        var json = await _client.GetFromJsonAsync<JsonDocument>($"/projects?page={page}&pageSize={pageSize}");
        if (json == null)
            return (0, []);

        var projects = json.RootElement.GetProperty("projects").Deserialize<List<Project>>(_serializerOptions);
        var totalPages = json.RootElement.GetProperty("totalPages").GetInt32();
        projects ??= [];

        return (totalPages, projects);
    }

    public async Task<Project> GetProjectAsync(string id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/projects/{id}");
        var response = await _client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var project = await response.Content.ReadFromJsonAsync<Project>();
            if (project == null)
                throw new Exception("Invalid response.");

            return project;
        }
        else
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }

    }

    public async Task PostProjectAsync(Project project)
    {
        string json = JsonSerializer.Serialize(project);
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/projects");
        request.Content = content;

        var response = await _client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var resProject = await response.Content.ReadFromJsonAsync<Project>();
            if (resProject == null)
                throw new Exception("Invalid response.");

            project.Id = resProject.Id;
        }
        else
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }
    }

    public async Task PutProjectAsync(Project project)
    {
        string json = JsonSerializer.Serialize(project);
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, "/projects");
        request.Content = content;

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }
    }

    public async Task PutProjectImageAsync(string id, Image image)
    {
        string json = JsonSerializer.Serialize(new { id, image });
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, "/projects/images");
        request.Content = content;

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }
    }

    public async Task DeleteProjectAsync(string id)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/projects/{id}");

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }
    }

    public async Task<Image> GetImageAsync(string id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/images/{id}");

        var response = await _client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var image = await response.Content.ReadFromJsonAsync<Image>();
            if (image == null)
                throw new Exception("Invalid response.");

            return image;
        }
        else
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }
    }

    public async Task PostImageAsync(Image image, string projectId)
    {
        string json = JsonSerializer.Serialize(new { projectId, image });
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/images");
        request.Content = content;

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }
    }

    public async Task PostThumbnailAsync(Image image, string projectId)
    {
        string json = JsonSerializer.Serialize(new { projectId, image });
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/images/thumbnails");
        request.Content = content;

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new Exception(message);
        }
    }

    public async Task CheckConnectionAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Head, "/");
        await _client.SendAsync(request);
    }
}
