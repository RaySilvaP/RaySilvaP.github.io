using Microsoft.JSInterop;

namespace Frontend.Services;

public class TokenService(IJSRuntime JS)
{
    private readonly IJSRuntime _JS = JS;

    public async Task StoreToken(string token)
    {
        token = token.Replace(@"""", "");
        await _JS.InvokeVoidAsync("storeToken", token);
    }

    public async Task<string> GetToken()
        => await _JS.InvokeAsync<string>("getToken");
}