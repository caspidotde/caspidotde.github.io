<!-- C# Web Applikation -->

##### Web Applikation ist zunächst nicht anders als eine Console App

````csharp
var builder = WebApplication.CreateBuilder(args);

// add configuration, logging, services, DI etc.
.....

var app = builder.Build();

app.Run();

````


##### Oder doch anders, z.B. mit HealthChecks

````csharp

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

... 
builder.Services.AddHealthChecks()
	// Add a default liveness check to ensure app is responsive
	.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
	// Add custom health checks here, e.g. for database, external services etc.
	.AddCheck<SomeHealthCheck>("some_health_check");
....
// All health checks must pass for app to be considered ready to accept traffic after starting
app.MapHealthChecks("/health");
// Only health checks tagged with the "live" tag must pass for app to be considered alive
app.MapHealthChecks("/alive", new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") });


````

##### mit Authorization/Authentication

````csharp

using Microsoft.AspNetCore.Authentication;

... 

string corsPolicy = "_myCorsPolicy";

builder.Services.AddCors(options => { options.AddPolicy(corsPolicy, builder => { builder.WithOrigins("xyz""); }); });

// Don't forget to implement your own BasicAuthenticationHandler :)
builder.Services
    .AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddAuthorization(o => { o.AddPolicy("ApiUserPolicy", b => b.RequireRole("User")); });

...

app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();

````

##### mit OpenAPI und Scalar

````csharp

using Scalar.AspNetCore;

... 

builder.Services.AddOpenApi(options =>
{
    // manche Methoden in den Controllern haben keine HTTP-Methode
    // und sollten auch nicht in die Dokumentation einbezogen werden
    options.ShouldInclude = operation => operation.HttpMethod != null;
});

// auch möglich mit API-Versionierung
// var withApiVersioning = builder.Services.AddApiVersioning();
// builder.AddDefaultOpenApi(withApiVersioning);

if (app.Environment.IsDevelopment())
{
    // auch hier dürfen nur authorisierte Benutzer (Basic Auth) die API-Doku sehen

    app.MapOpenApi().RequireAuthorization("ApiUserPolicy");
    app.MapScalarApiReference(options =>
    {
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.RestSharp);

        // Disable default fonts to avoid download unnecessary fonts
        options.DefaultFonts = false;

        // options.DarkMode = false;
        // options.HideDarkModeToggle = false;
    }).RequireAuthorization("ApiUserPolicy");
}



````