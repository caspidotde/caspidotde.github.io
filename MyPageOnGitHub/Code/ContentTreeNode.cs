namespace MyPageOnGitHub.Code;

public class ContentTreeNode
{
    public string Name { get; set; } = string.Empty;
    public IList<ContentTreeItem> Children { get; set; } = new List<ContentTreeItem>();
}
