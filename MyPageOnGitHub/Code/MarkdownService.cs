using System.Reflection;

namespace MyPageOnGitHub.Code;

public enum MarkdownResourceType : byte
{
    Blog,
    Help,
    Programming,
    Songs
}

public interface IMarkdownService
{
    Task<string> GetBlogEntryAsync(MarkdownResourceType resourceType);
    Task<string> GetBlogEntryAsync(MarkdownResourceType resourceType, int itemIndex);
}

public class MarkdownService: IMarkdownService
{
    private readonly ILogger<MarkdownService> _logger;
    public MarkdownService(
        ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<MarkdownService>();
    }

    public Task<string> GetBlogEntryAsync(MarkdownResourceType resourceType)
    {
        var resourceDirectory = resourceType switch
        { 
            MarkdownResourceType.Blog => "Blog",
            MarkdownResourceType.Help => "Help",
            MarkdownResourceType.Programming => "Programming",
            MarkdownResourceType.Songs => "Songs"
        };

        var resourceName = resourceType switch
        {
            MarkdownResourceType.Blog => "Blog001_index",
            MarkdownResourceType.Help => "Help001_index",
            MarkdownResourceType.Programming => "Programming001_index",
            MarkdownResourceType.Songs => "Songs001_index",
            _ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, $"Unknown {nameof(MarkdownResourceType)}: {resourceType}")
        };

        return GetMarkdownAsync(resourceType, resourceDirectory, resourceName);
    }

    public Task<string> GetBlogEntryAsync(MarkdownResourceType resourceType, int itemIndex)
    {
        if (itemIndex < 1 || itemIndex > 999)
        {
            return GetBlogEntryAsync(resourceType);
        }

        var resourceDirectory = resourceType switch
        {
            MarkdownResourceType.Blog => "Blog",
            MarkdownResourceType.Help => "Help",
            MarkdownResourceType.Programming => "Programming",
            MarkdownResourceType.Songs => "Songs"
        };

        var resourceName = resourceType switch
        {
            MarkdownResourceType.Blog => string.Format("Blog{0}_index", itemIndex.ToString("D3")),
            MarkdownResourceType.Help => string.Format("Help{0}_index", itemIndex.ToString("D3")),
            MarkdownResourceType.Programming => string.Format("Programming{0}_index", itemIndex.ToString("D3")),
            MarkdownResourceType.Songs => string.Format("Songs{0}_index", itemIndex.ToString("D3")),
            _ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, $"Unknown {nameof(MarkdownResourceType)}: {resourceType}")
        };

        return GetMarkdownAsync(resourceType, resourceDirectory, resourceName);
    }


    private async Task<string> GetMarkdownAsync(MarkdownResourceType resourceType, string directory, string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.BlogContents.{directory}.{name}.md";

        // var resourceName = "MyPageOnGitHub.BlogContents.Blog.Blog001_index.md";

        // resources should be build as embedded resource, see their Peroperties

        try
        {
            await using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Markdown resource not found");
            using var reader = new StreamReader(stream);

            return await reader.ReadToEndAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return await GetBlogEntryAsync(resourceType);
        }
    }
}
