using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;

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
                await _tokenService.StoreToken(token);
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
        var token = await _tokenService.GetToken();
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
}