<!-- Import-Batch mit SimpleInjector und Factory Pattern -->

#### Import-Batch mit SimpleInjector und Factory Pattern realisieren

````csharp

container.Register<ImportJob01>(Lifestyle.Transient);
container.Register<ImportJob02>(Lifestyle.Transient);
...

container.RegisterInstance<IImportFactory>(new ImportFactory(container) { { "job01", typeof(ImportJob01) }, { "job02", typeof(ImportJob02) }, ... });

var factory = container.GetInstance<IImportFactory>();
using (Scope scope = AsyncScopedLifestyle.BeginScope(container))
{
    var import = factory.CreateImportJob(importParameter.ToUpper());
    foreach (var file in import.GetImportFiles()) { import.ProcessFile(file); }
    import.PostProcess();
}

public interface IImportFactory { IBaseImportJob CreateImportJob(string name); }

public sealed class ImportFactory : Dictionary<string, Type>, IImportFactory
{
    private readonly Container container;
    public ImportFactory(Container container) { this.container = container; }

    public void Register<T>(string name) where T : IBaseImportJob
    {
        this.container.Register(typeof(T));
        this.Add(name, typeof(T));
    }

    public IBaseImportJob CreateImportJob(string name) => (IBaseImportJob)this.container.GetInstance(this[name]);
}

````