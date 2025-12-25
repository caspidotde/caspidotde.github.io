<!-- Minimal API -->

##### Minimal API

````csharp

public static IEndpointRouteBuilder MapNotificationAPI(this IEndpointRouteBuilder app)
{
    var callbacksGroup = app.MapGroup("/callbacks");

    callbacksGroup.MapGet("/ping", Pong)
        .WithName("Ping")
        .WithSummary("Ping pong")
        .WithDescription("Ping -> pong")
        .WithTags("callbacks");

    return app;
}

    
public static Ok<string> Pong()
{
    return TypedResults.Ok("Pong");
}


````

