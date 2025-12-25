using BlazorLayout;
using BlazorLayout.Authentification;
using BlazorLayout.Extensions;
using BlazorLayout.Gateways;
using BlazorLayout.Localization;
using BlazorLayout.Stores;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Localization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// =======================
// CONFIGURATION
// =======================
builder.Configuration.AddJsonFile(
    "appsettings.json",
    optional: false,
    reloadOnChange: false);


// =======================
// LOCALIZATION
// =======================
builder.Services.AddLocalization();
builder.Services.AddSingleton<IStringLocalizer>(
    sp => sp.GetRequiredService<IStringLocalizer<LocalizationResource>>()
);


// =======================
// AUTHENTICATION
// =======================
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, FakeAuthenticationStateProvider>();


// =======================
// STORES & API
// =======================
builder.Services.AddSingleton<UserByEmailStore>();
builder.Services.AddSingleton<ShrinkageApi>();


// =======================
// HTTP CLIENT (API GATEWAY)
// =======================
var apiUrl = builder.Configuration.GetValue<string>("ShrinkageGrpcClientApiConfig:Url");

if (string.IsNullOrWhiteSpace(apiUrl))
{
    throw new InvalidOperationException("ShrinkageGrpcClientApiConfig:Url is missing");
}

builder.Services.AddHttpClient(HttpClients.ApiGateway, client =>
{
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromMinutes(10);

});


// =======================
await builder.Build().RunAsync();
