using BlazorLayout;
using BlazorLayout.Authentification;
using BlazorLayout.Localization;
using BlazorLayout.Stores;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Localization;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization();

builder.Services.AddSingleton<IStringLocalizer>(
    sp => sp.GetRequiredService<IStringLocalizer<LocalizationResource>>()
);

builder.Services.AddSingleton<UserByEmailStore>();

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationStateProvider,FakeAuthenticationStateProvider>();





builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();





// Package a Installer : Microsoft.Extensions.Localization