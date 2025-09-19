#### Register a new Hue application and get the app key
###### using Q42.HueApi (thanks to Michiel Post - https://github.com/michielpost/Q42.HueApi)

````csharp
using Microsoft.Extensions.Configuration;
using Q42.HueApi;
using Q42.HueApi.Interfaces;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

string ip = configuration.GetValue<string>("appSettings:HueBridgeIp")!;
string appName = configuration.GetValue<string>("appSettings:HueAppName")!;
string deviceName = configuration.GetValue<string>("appSettings:HueDeviceName")!;

ILocalHueClient localHueClient = new LocalHueClient(ip);
string? appKey = await localHueClient.RegisterAsync(appName, deviceName);

Console.WriteLine($"AppKey: {appKey}");
````


