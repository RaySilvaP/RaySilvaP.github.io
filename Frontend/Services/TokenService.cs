using Microsoft.JSInterop;

namespace Frontend.Services;

public class TokenService(IJSRuntime JS)
{
    private readonly IJSRuntime _JS = JS;

    public async Task StoreTokenAsync(string token)
    {
        token = token.Replace(@"""", "");
        await _JS.InvokeVoidAsync("storeToken", token);
    }

    public async Task<string> GetTokenAsync()
        => await _JS.InvokeAsync<string>("getToken");
}