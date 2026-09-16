using CustomerManagement.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using CustomerManagement.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUri = new Uri(
    builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl not found."));

builder.Services.AddScoped<HttpClient>(sp =>
{
    var handler = new ApiAuthorizationHandler(
        sp.GetRequiredService<CustomAuthStateProvider>(),
        sp.GetRequiredService<NavigationManager>(),
        apiBaseUri)
    {
        InnerHandler = new HttpClientHandler()
    };

    return new HttpClient(handler)
    {
        BaseAddress = apiBaseUri
    };
});
builder.Services.AddMudServices();
builder.Services.AddScoped<CustomerApiService>();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<CustomAuthStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddScoped<AuthApiService>();

await builder.Build().RunAsync();
