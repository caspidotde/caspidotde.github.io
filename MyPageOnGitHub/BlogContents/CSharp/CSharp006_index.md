<!-- IHttpClientFactory und named HttpClient -->

##### IHttpClientFactory 

````csharp

builder.Services.AddHttpClient();

public class MyService(
    IHttpClientFactory httpClientFactory,
    ILogger<MyService> logger)
{
    public async Task DoSomeStuffAsync()
    {
        // Create the client
        HttpClient client = httpClientFactory.CreateClient();
        
        var request = new MyRequest { /* ... */ };
        var myUri = new Uri("https://example.com/api/some-endpoint");
        try
        {
            var response = await httpClient.PostAsJsonAsync<MyRequest>(myUri, request);
        }
        catch (Exception ex)
        {
            logger.LogError("Error posting my request: {Error}", ex);
        }
    }
}


````


##### named HttpClient 

````csharp

builder.Services
    .AddHttpClient("myapi", config => config.BaseAddress = new Uri("https://example.com/"))
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler()
        {
            UseDefaultCredentials = false
        };
    });

public class MyService(
    IHttpClientFactory httpClientFactory,
    ILogger<MyService> logger)
{
    public async Task DoSomeStuffAsync()
    {
        // Create the named client
        var httpClient = httpClientFactory.CreateClient("myapi");
        var myUri = "api/user";        
        var request = new MyRequest { /* ... */ };

        try
        {
            var response = await httpClient.PostAsJsonAsync<MyRequest>(myUri, request);
        }
        catch (Exception ex)
        {
            logger.LogError("Error posting my request: {Error}", ex);
        }
    }
}

````
