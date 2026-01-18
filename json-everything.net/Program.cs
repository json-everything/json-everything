using Blazored.LocalStorage;
using JsonEverythingNet;
using JsonEverythingNet.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddSingleton<DataManager>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddScoped<EditorOptions>();

var host = builder.Build();
var client = host.Services.GetService<HttpClient>();

// Initialize theme before the UI renders so editors get the correct initial theme
var localStorage = host.Services.GetRequiredService<ILocalStorageService>();
var savedTheme = await localStorage.GetItemAsync<string>("theme");
var isDarkMode = savedTheme != "light"; // default to dark when missing

var themeService = host.Services.GetRequiredService<ThemeService>();
themeService.SetTheme(isDarkMode);

var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
await jsRuntime.InvokeVoidAsync("eval", isDarkMode
	? "document.documentElement.removeAttribute('data-bs-theme')"
	: "document.documentElement.setAttribute('data-bs-theme', 'light')");

await host.RunAsync();