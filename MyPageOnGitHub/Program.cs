using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using MyPageOnGitHub;
using MyPageOnGitHub.Code;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Logging
    .SetMinimumLevel(LogLevel.Information)
    .AddFilter("Microsoft", LogLevel.Warning)
    // .AddFilter("Microsoft", LogLevel.Information)
    .AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

/*
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .Build();

builder.Configuration.AddConfiguration(configuration);
*/

builder.Services.AddScoped(
    sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();

// MarkdownService
builder.Services
    .AddSingleton<IMarkdownService, MarkdownService>();
// .AddScoped<IThemeService, ThemeService>()
// .AddBlazoredLocalStorage();

builder.Services.AddMudMarkdownServices();
// Optionally if the default clipboard functionality fails it is possible to add a custom service
// NB! MauiClipboardService is just an example
// builder.Services.AddMudMarkdownClipboardService<MauiClipboardService>();

var serviceProvider = builder.Services.BuildServiceProvider();
// var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

var app = builder.Build();

// logger.LogInformation("AppDomain.CurrentDomain.BaseDirectory: {name}", AppDomain.CurrentDomain.BaseDirectory);
// logger.LogInformation("client environment: {name}", builder.HostEnvironment.Environment);
// logger.LogInformation(builder.Configuration["appSettings:Environment"] ?? "no appSettings found");

await app.RunAsync();


