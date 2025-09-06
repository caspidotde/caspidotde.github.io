namespace MyPageOnGitHub.Code;

public static class CoreServicesExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection @this) =>
        @this
            .AddSingleton<IMarkdownService>(new MarkdownService());
            // .AddScoped<IThemeService, ThemeService>()
            // .AddBlazoredLocalStorage();
}
