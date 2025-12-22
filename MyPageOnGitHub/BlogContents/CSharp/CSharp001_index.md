<!-- C# u.a. -->

##### Console App mit Serilog und OpenTelemetry Sink konfigurieren
###### 

````csharp
var builder = Host.CreateApplicationBuilder(args);

var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

Log.Logger = new LoggerConfiguration()
   .Enrich.FromLogContext()
   .ReadFrom.Configuration(configuration)
   .WriteTo.OpenTelemetry(x =>
   {
       x.Endpoint = "http://seqServer:5341/ingest/otlp/v1/logs";
       x.Protocol = OtlpProtocol.HttpProtobuf;
       x.Headers = new Dictionary<string, string> { ["X-Seq-ApiKey"] = "xxxYYzz" };
       x.ResourceAttributes = new Dictionary<string, object>
       {
           ["service.name"] = "SomeJob",
            ...
       };
   })
   .CreateLogger();
````

#### Import-Batch mit SimpleInjector und Factory Pattern realisieren
````csharp

container.Register<ImportJob01>(Lifestyle.Transient);
container.Register<ImportJob02>(Lifestyle.Transient);
...

container.RegisterInstance<IImportFactory>(new ImportFactory(container)
{
    { "job01", typeof(ImportJob01) },
    { "job02", typeof(ImportJob02) },
    ...
});

var factory = container.GetInstance<IImportFactory>();
using (Scope scope = AsyncScopedLifestyle.BeginScope(container))
{
    var import = factory.CreateImportJob(importParameter.ToUpper());

    foreach (var file in import.GetImportFiles())
    {
        import.ProcessFile(file);
    }

    import.PostProcess();
}

public interface IImportFactory
{
    IBaseImportJob CreateImportJob(string name);
}

public sealed class ImportFactory : Dictionary<string, Type>, IImportFactory
{
    private readonly Container container;

    public ImportFactory(Container container)
    {
        this.container = container;
    }

    public void Register<T>(string name) where T : IBaseImportJob
    {
        this.container.Register(typeof(T));
        this.Add(name, typeof(T));
    }

    public IBaseImportJob CreateImportJob(string name) => (IBaseImportJob)this.container.GetInstance(this[name]);
}

````