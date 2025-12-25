<!-- C# Console App -->

##### Console App mit Serilog und OpenTelemetry Sink konfigurieren

````csharp
var builder = Host.CreateApplicationBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

Log.Logger = new LoggerConfiguration()
   .Enrich.FromLogContext()
   .ReadFrom.Configuration(configuration)
   .WriteTo.OpenTelemetry(x =>
   {
       x.Endpoint = "http://<<seqServer>>:5341/ingest/otlp/v1/logs";
       x.Protocol = OtlpProtocol.HttpProtobuf;
       x.Headers = new Dictionary<string, string> { ["X-Seq-ApiKey"] = "xxxYYzz" };
       x.ResourceAttributes = new Dictionary<string, object>
       {
           ["service.name"] = "MyApp",
            ...
       };
   })
   .CreateLogger();

builder.Configuration.AddConfiguration(configuration);

// ILoggerFactory für Programm.cs
builder.Services.AddSingleton<ILoggerFactory>(new LoggerFactory().AddSerilog(dispose: true));

// DI mit ILogger<T> für die jeweiligen Services etc. weitere Klassen
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

var container = new Container();
container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

container.Register(...);

builder.Services.AddSimpleInjector(container);

var host = builder.Build().UseSimpleInjector(container);

host.Run();

````

###### Arbeiten mit BackgroundService

````csharp

builder.Services.AddHostedService<SomeService>();
var app = builder.Build();
app.Run();

public class SomeService: BackgroundService
{
    public SomeService() { ... }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Immer wieder die Aufgabe ausführen bis der Dienst gestoppt wird
        while (!stoppingToken.IsCancellationRequested)
        {
            DoWork();
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

}


````

###### oder mit einer IHostedService und man kann Start und Stop explizit steuern

````csharp

builder.Services.AddHostedService<ExportService>();
var host = builder.Build();
// await host.StartAsync();
// await host.StopAsync();
// oder einfach die Applikation ausführen
host.Run();

public class ExportService: IHostedService
{
    public ExportService() { ... }
    public async Task StartAsync(CancellationToken cancellationToken) { ... }
    public async Task StopAsync(CancellationToken cancellationToken) { ... }
}


````

###### oder mit einer IHostedLifecycleService und plötzlich hat man sogar Zwischenstufen von Start und Stop

````csharp

builder.Services.AddHostedService<SomeService>();
var app = builder.Build();
app.Run();

public class SomeService: IHostedLifecycleService
{
    public SomeService() { ... }
    public Task StartAsync(CancellationToken cancellationToken) { return Task.CompletedTask; }
    public Task StopAsync(CancellationToken cancellationToken) { return Task.CompletedTask; }
    public Task StartedAsync(CancellationToken cancellationToken) { return Task.CompletedTask; }
    public Task StartingAsync(CancellationToken cancellationToken) { return Task.CompletedTask; }
    public Task StoppedAsync(CancellationToken cancellationToken) { return Task.CompletedTask; }
    public Task StoppingAsync(CancellationToken cancellationToken) { return Task.CompletedTask; }
}

````