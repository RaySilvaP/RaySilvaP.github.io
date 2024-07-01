using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

    public async Task<List<Project>?> GetProjectsAsync(int page = 1, int pageSize = 10)
    {
        var token = await _tokenService.GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/projects?page={page}&pageSize={pageSize}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        try
        {
            var response = await _client.SendAsync(request);
            return await response.Content.ReadFromJsonAsync<List<Project>>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }

    public async Task<bool> PostProjectAsync(Project project)
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
            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
        catch
        {
            return false;
        }
    }
}