<!-- Hue API -->

###### (thanks to Michiel Post - https://github.com/michielpost/Q42.HueApi)

##### Register a new Hue application and get the app key
###### using Q42.HueApi

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

##### Huetility - Example to turn on a light
###### using HueApi V2
````csharp
LocalHueApi client = new LocalHueApi(Model.BridgeIP, Model.AppKey);
var Lights = await client.GetLightsAsync();
...
var light = Lights.Data.FirstOrDefault(l => l.Metadata.Name == "Hue white lamp 1");
var req = new UpdateLight();

if (light.On.IsOn)
    req.TurnOff();
else
    req.TurnOn();

var result = await client.UpdateLightAsync(light.Id, req);
````
