using System.Reflection;

namespace MyPageOnGitHub.Code;

public enum MarkdownResourceType : byte
{
    Blog,
    Programming
}

public interface IMarkdownService
{
    Task<string> GetBlogEntryAsync(MarkdownResourceType resourceType);
}

public class MarkdownService: IMarkdownService
{
    public Task<string> GetBlogEntryAsync(MarkdownResourceType resourceType)
    {
        var resourceDirectory = resourceType switch
        { 
            MarkdownResourceType.Blog => "Blog",
            MarkdownResourceType.Programming => "Programming"
        };

        var resourceName = resourceType switch
        {
            MarkdownResourceType.Blog => "Blog001_index",
            MarkdownResourceType.Programming => "Programming001_index",
            _ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, $"Unknown {nameof(MarkdownResourceType)}: {resourceType}")
        };

        return GetMarkdownAsync(resourceDirectory, resourceName);
    }

    private async Task<string> GetMarkdownAsync(string directory, string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.BlogContents.{directory}.{name}.md";
        // var resourceName = "MyPageOnGitHub.BlogContents.Blog.Blog001_index.md";

        // resources should be build as embedded resource, see their Peroperties

        await using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Markdown resource not found");
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync()
            .ConfigureAwait(false);
    }
}
