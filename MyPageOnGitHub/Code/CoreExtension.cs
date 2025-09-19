using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace MyPageOnGitHub.Code;

public static class CoreExtension
{
    public static WebAssemblyHostBuilder AddCore(this WebAssemblyHostBuilder @this)
    {
        @this.Logging
            .SetMinimumLevel(LogLevel.Information)
            .AddFilter("Microsoft", LogLevel.Warning)
            // .AddFilter("Microsoft", LogLevel.Information)
            .AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json")
            .Build();

        @this.Configuration.AddConfiguration(configuration);        

        return @this;
    }
}
