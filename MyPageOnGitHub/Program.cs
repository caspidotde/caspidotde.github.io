using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using MyPageOnGitHub;
using MyPageOnGitHub.Code;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddCoreServices();

builder.Services.AddMudServices();
builder.Services.AddMudMarkdownServices();
// Optionally if the default clipboard functionality fails it is possible to add a custom service
// NB! MauiClipboardService is just an example
// builder.Services.AddMudMarkdownClipboardService<MauiClipboardService>();



await builder.Build().RunAsync();

