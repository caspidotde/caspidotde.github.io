<!-- Huetility -->

#### Huetility - Example to turn on a light
###### using HueApi V2 (thanks to Michiel Post - https://github.com/michielpost/Q42.HueApi)
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