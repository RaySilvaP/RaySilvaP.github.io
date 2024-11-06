using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Shared.Models;

namespace Frontend.Services;

public class HttpService(HttpClient client, TokenService tokenService, ILogger<HttpService> logger)
{
    private readonly HttpClient _client = client;
    private readonly TokenService _tokenService = tokenService;
    private readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly ILogger<HttpService> _logger = logger;

    public async Task<bool> LoginAsync(string username, string password)
    {
        string json = JsonSerializer.Serialize(new { username, password });
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/login")
        {
            Content = content
        };
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
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong");
            return false;
        }
    }

    public async Task<bool> CheckAuthorizationAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Head, "/auth");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong");
            return false;
        }
    }

    public async Task<(int, List<ProjectDto>?)> GetProjectsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var json = await _client.GetFromJsonAsync<JsonDocument>($"/projects?page={page}&pageSize={pageSize}");
            if (json == null)
                return (0, null);

            var projects = json.RootElement.GetProperty("projects").Deserialize<List<ProjectDto>>(_serializerOptions);
            var totalPages = json.RootElement.GetProperty("totalPages").GetInt32();
            return (totalPages, projects);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong");
            return (0, null);
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
            _logger.LogError(e, "Something went wrong");
            return null;
        }
    }

    public async Task<bool> PostProjectAsync(ProjectDto project)
    {
        string json = JsonSerializer.Serialize(project);
        var token = await _tokenService.GetTokenAsync();
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/projects")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<ProjectDto>();
            if (body == null)
                return false;
            else
            {
                project.Id = body.Id;
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong");
            return false;
        }
    }

    public async Task<bool> PutProjectAsync(ProjectDto project)
    {
        var token = await _tokenService.GetTokenAsync();
        string json = JsonSerializer.Serialize(project);
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, "/projects")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong");
            return false;
        }
    }

    public async Task<bool> PutProjectImageAsync(string id, Image image)
    {
        var token = await _tokenService.GetTokenAsync();
        string json = JsonSerializer.Serialize(new { id, image });
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, "/projects/images")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong");
            return false;
        }
    }

    public async Task<bool> DeleteProjectAsync(string id)
    {
        var token = await _tokenService.GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/projects/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong");
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
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong");
            return null;
        }
    }

    public async Task<bool> PostImageAsync(Image image, string projectId)
    {
        string json = JsonSerializer.Serialize(new { projectId, image });
        var token = await _tokenService.GetTokenAsync();
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/images")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong.");
            return false;
        }
    }

    public async Task<bool> PostThumbnailAsync(Image image, string projectId)
    {
        string json = JsonSerializer.Serialize(new { projectId, image });
        var token = await _tokenService.GetTokenAsync();
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/images/thumbnails")
        {
            Content = content
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Something went wrong.");
            return false;
        }
    }

    public async Task CheckConnectionAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Head, "/");
            await _client.SendAsync(request);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to connect to the server.");
        }
    }
}