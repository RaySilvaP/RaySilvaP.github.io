using Frontend;
using Frontend.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5156") };

builder.Services.AddScoped(sp => client);
builder.Services.AddBlazorBootstrap();
builder.Services.AddTransient<HttpService>();
builder.Services.AddTransient<TokenService>();

await builder.Build().RunAsync();
