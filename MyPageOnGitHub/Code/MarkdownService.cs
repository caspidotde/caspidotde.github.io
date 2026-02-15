using System.Reflection;
using System.Xml.Linq;

namespace MyPageOnGitHub.Code;

public enum MarkdownResourceType : byte
{
    Blog,
    Help,
    Gupta,
    CSharp,
    PHP
}

public interface IMarkdownService
{
    Task<ContentTree> GetContentTree();
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

    private string getResourceDirectory(MarkdownResourceType resourceType) => resourceType switch
    {
        MarkdownResourceType.Blog => "Blog",
        MarkdownResourceType.Help => "Help",
        MarkdownResourceType.Gupta => "Gupta",
        MarkdownResourceType.CSharp => "CSharp",
        MarkdownResourceType.PHP => "PHP",
        _ => throw new ArgumentOutOfRangeException(
            nameof(resourceType), resourceType, 
            $"Unknown {nameof(MarkdownResourceType)}: {resourceType}")
    };

    private string getResourceName(MarkdownResourceType resourceType, int itemIndex) => resourceType switch
    {
        MarkdownResourceType.Blog => string.Format("Blog{0}_index", itemIndex.ToString("D3")),
        MarkdownResourceType.Help => string.Format("Help{0}_index", itemIndex.ToString("D3")),
        MarkdownResourceType.Gupta => string.Format("Gupta{0}_index", itemIndex.ToString("D3")),
        MarkdownResourceType.CSharp => string.Format("CSharp{0}_index", itemIndex.ToString("D3")),
        MarkdownResourceType.PHP => string.Format("PHP{0}_index", itemIndex.ToString("D3")),
        _ => throw new ArgumentOutOfRangeException(
            nameof(resourceType), resourceType, 
            $"Unknown {nameof(MarkdownResourceType)}: {resourceType}")
    };

    public async Task<string> GetBlogEntryAsync(MarkdownResourceType resourceType, int itemIndex)
    {
        if (itemIndex < 1 || itemIndex > 999)
        {
            return await GetBlogEntryAsync(resourceType, 1);
        }        

        var resourceDirectory = getResourceDirectory(resourceType);
        var resourceName = getResourceName(resourceType, itemIndex);
        return await GetMarkdownAsync(resourceType, resourceDirectory, resourceName);
    }

    public async Task<ContentTree> GetContentTree()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();

        ContentTree tree = new();

        foreach (var resourceName in resourceNames)
        {
            // _logger.LogInformation("Resource: {ResourceName}", resourceName);

            var parts = resourceName.Split('.');
            if (parts.Length >= 4 && parts[parts.Length - 1] == "md")
            {
                var item = new ContentTreeItem
                {
                    Category = parts[2],
                    Filename = parts[3],
                    Index = parts[3].Substring(parts[3].Length - 9, 3) is string indexStr && int.TryParse(indexStr, out int index) ? index : 0
                };

                // Skip Help files for now
                if (item.Category.StartsWith("Help", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!tree.Nodes.Any(n => n.Name.Equals(item.Category)))
                {
                    // _logger.LogWarning("New Category: {Category}", item.Category);
                    tree.Nodes.Add(new ContentTreeNode { Name = item.Category });
                }

                var node = tree.Nodes.First(n => n.Name.Equals(item.Category));

                await using var stream = assembly.GetManifestResourceStream(resourceName);
                using var reader = new StreamReader(stream);
                string firstLine = reader.ReadLine() ?? ""; // skip first line
                reader.Close();

                if (!string.IsNullOrEmpty(firstLine) && firstLine.StartsWith("<!-- "))
                {                    
                    firstLine = firstLine.TrimEnd('-','>',' ');
                    item.Title = firstLine[5..].Trim();
                }
                else
                {
                    item.Title = item.Filename;
                }                

                node.Children.Add(item);
            }
        }

       return tree;
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
            return await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {            
            return await GetBlogEntryAsync(resourceType, 1);
        }
    }
}
