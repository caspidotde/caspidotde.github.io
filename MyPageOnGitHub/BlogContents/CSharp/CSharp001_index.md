<!-- C# Console App -->

##### Console App ohne HostApplicationBuilder: IConfiguration, Serilog, OpenTelemetry
````csharp

// Konfiguration mit appsettings.json und UserSecrets: fast immer sinnvoll
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

// Serilog Logger konfigurieren, hier mit Console Sink und Konfiguration aus appsettings.json
Log.Logger = new LoggerConfiguration()
   .Enrich.FromLogContext()
   .WriteTo.Console()
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

// ServiceCollection konfigurieren: Microsoft.Extensions.DependencyInjection wird benötigt
var services = new ServiceCollection();
services.AddScoped<IConfiguration>(_ => configuration);
services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

// weitere Services registrieren
services.AddScoped/Singleton/Transient...<IMyService, MyService>();

// da Program gleich hier beginnt, muss der ServiceProvider gebaut werden
using var servicesProvider = services.BuildServiceProvider();
var myService = servicesProvider.GetRequiredService<IMyService>();

// Programm starten
myService.DoSomeStuff();

````

##### Console App mit HostApplicationBuilder: BackgroundService/IHostedService und DI mit SimpleInjector

````csharp
var builder = Host.CreateApplicationBuilder(args);

// ... 

builder.Configuration.AddConfiguration(configuration);

// ILoggerFactory für Programm.cs
builder.Services.AddSingleton<ILoggerFactory>(new LoggerFactory().AddSerilog(dispose: true));

// DI mit ILogger<T> für die jeweiligen Services etc. weitere Klassen
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

var container = new Container();
container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

container.Register(...);

builder.Services.AddSimpleInjector(container);

// Service registrieren: wenn der IHostedService/BackgroundService über DI aufgelöst werden soll
builder.Services.AddHostedService<MyService>();

// Service registrieren: wenn der IHostedService/BackgroundService manuell erstellt werden soll
// jobName = args[0];
// var loggerFactory = new LoggerFactory().AddSerilog(dispose: true);
// builder.Services.AddHostedService<MyService>(serviceProvider => new MyService(jobName, loggerFactory, configuration, container, ...));

var host = builder.Build().UseSimpleInjector(container);

host.Run();

````

###### BackgroundService

````csharp

builder.Services.AddHostedService<SomeService>();
var app = builder.Build();
// async
await app.RunAsync();
// oder einfach
// app.Run();


public class SomeService: BackgroundService
{
    public SomeService() { ... }

    // wenn app.RunAsync aufgerufen wird
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Immer wieder die Aufgabe ausführen bis der Dienst gestoppt wird
        while (!stoppingToken.IsCancellationRequested)
        {
            DoSomeWork();
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

}


````

###### IHostedService und man kann Start und Stop explizit steuern

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

###### IHostedLifecycleService: noch mehr Zwischenstufen von Start und Stop

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