<!-- Factory Pattern für QueryHandler -->

#### Factory Pattern für QueryHandler

````csharp

var factory = _container.GetInstance<IQueryHandlerFactory>();
var handler = factory.CreateNew(_jobName);
await handler.HandleAsync();

public interface IQueryHandler { Task HandleAsync(); }
public interface IQueryHandlerFactory { IQueryHandler CreateNew(string name); }

public class QueryHandlerFactory: IQueryHandlerFactory
{
    readonly Container container;

    readonly Dictionary<string, InstanceProducer<IQueryHandler>> producers =
        new Dictionary<string, InstanceProducer<IQueryHandler>>(StringComparer.OrdinalIgnoreCase);

    public QueryHandlerFactory(Container container) { this.container = container; }

    IQueryHandler IQueryHandlerFactory.CreateNew(string name)
    {
        if (name == null) throw new MyNotFoundException("job is empty");
        if (!producers.ContainsKey(name)) throw new MyNotFoundException($"job {name} not found");
        return this.producers[name].GetInstance();
    }

    public void Register<TImplementation>(string name) where TImplementation : class, IQueryHandler
    {
        var producer = Lifestyle.Transient.CreateProducer<IQueryHandler, TImplementation>(container);
        this.producers.Add(name, producer);
    }
}


````