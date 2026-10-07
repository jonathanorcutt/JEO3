namespace JEO3.Site.Components.Treeview.Commented;

public enum CheckState { Unchecked, Checked, Indeterminate }

/// <summary>Wraps a caller-supplied TItem with the state the tree needs (expansion, checks, hierarchy).</summary>
public class TreeNode<TItem>
{
    public required TItem Data { get; init; }
    public string Text { get; set; } = "";
    public string? Icon { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsLoading { get; set; }
    public bool HasChildren { get; set; }
    public bool ChildrenLoaded { get; set; }
    public bool IsSelected { get; set; }
    public bool IsVisible { get; set; } = true; // false when filtered out
    public bool MatchesFilter { get; set; } = true;
    public CheckState Check { get; set; } = CheckState.Unchecked;
    public TreeNode<TItem>? Parent { get; set; }
    public List<TreeNode<TItem>> Children { get; set; } = new();
    public int Level => Parent is null ? 0 : Parent.Level + 1;
    public string Id { get; } = Guid.NewGuid().ToString("N");

    public IEnumerable<TreeNode<TItem>> Descendants()
    {
        foreach (var child in Children)
        {
            yield return child;
            foreach (var d in child.Descendants()) yield return d;
        }
    }

    public IEnumerable<TreeNode<TItem>> AncestorsAndSelf()
    {
        var node = this;
        while (node is not null) { yield return node; node = node.Parent; }
    }
}

public enum DropPosition { Before, After, Inside }

public record TreeNodeMoveArgs<TItem>(TreeNode<TItem> Moved, TreeNode<TItem>? NewParent, TreeNode<TItem>? RelativeTo, DropPosition Position);
