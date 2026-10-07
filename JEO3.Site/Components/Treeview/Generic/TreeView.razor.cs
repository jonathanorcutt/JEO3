using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;

namespace JEO3.Site.Components.Treeview.Commented;

public partial class TreeView<TItem> : ComponentBase, IAsyncDisposable
{
    // ---- Public parameters -------------------------------------------------

    [Parameter, EditorRequired] public IEnumerable<TItem> Items { get; set; } = Array.Empty<TItem>();
    [Parameter, EditorRequired] public Func<TItem, string> TextSelector { get; set; } = x => x?.ToString() ?? "";
    [Parameter] public Func<TItem, string?>? IconSelector { get; set; }
    [Parameter] public Func<TItem, bool>? HasChildrenSelector { get; set; }
    [Parameter] public Func<TItem, IEnumerable<TItem>>? ChildrenSelector { get; set; } // eager, in-memory
    [Parameter] public Func<TItem, Task<IEnumerable<TItem>>>? LoadChildrenAsync { get; set; } // lazy, async

    [Parameter] public bool ShowCheckboxes { get; set; }
    [Parameter] public bool ShowSearchBox { get; set; } = true;
    [Parameter] public bool AllowDragDrop { get; set; }
    [Parameter] public bool AllowMultiSelect { get; set; }
    [Parameter] public bool AutoExpandOnFilter { get; set; } = true;
    [Parameter] public double RowHeight { get; set; } = 28;
    [Parameter] public double IndentPx { get; set; } = 18;
    [Parameter] public string EmptyText { get; set; } = "No items";
    [Parameter] public string SearchPlaceholder { get; set; } = "Filter…";
    [Parameter] public RenderFragment<TreeNode<TItem>>? NodeTemplate { get; set; }

    [Parameter] public EventCallback<TreeNode<TItem>> OnNodeSelected { get; set; }
    [Parameter] public EventCallback<TreeNode<TItem>> OnNodeExpanded { get; set; }
    [Parameter] public EventCallback<TreeNode<TItem>> OnCheckChanged { get; set; }
    [Parameter] public EventCallback<TreeNodeMoveArgs<TItem>> OnNodeMoved { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = default!;

    // ---- Internal state ------------------------------------------------------

    private readonly List<TreeNode<TItem>> _roots = new();
    private readonly List<TreeNode<TItem>> _visibleFlat = new();
    private readonly Dictionary<string, ElementReference> _checkboxRefs = new();
    private readonly HashSet<TreeNode<TItem>> _selected = new();
    private ElementReference _containerRef;
    private Virtualize<TreeNode<TItem>>? _virtualize;
    private TreeNode<TItem>? _focusedNode;
    private TreeNode<TItem>? _dropTarget;
    private DropPosition _dropPosition;
    private string _filterText = "";
    private string _typeAheadBuffer = "";
    private DateTime _lastTypeAhead = DateTime.MinValue;
    private bool _preventDefaultOnKeydown = true;

    protected override void OnParametersSet()
    {
        if (_roots.Count == 0 && Items.Any()) BuildRoots();
    }

    private void BuildRoots()
    {
        _roots.Clear();
        foreach (var item in Items) _roots.Add(CreateNode(item, null));
        RecomputeVisible();
    }

    private TreeNode<TItem> CreateNode(TItem data, TreeNode<TItem>? parent)
    {
        var node = new TreeNode<TItem>
        {
            Data = data,
            Text = TextSelector(data),
            Icon = IconSelector?.Invoke(data),
            Parent = parent,
            HasChildren = HasChildrenSelector?.Invoke(data) ?? ChildrenSelector?.Invoke(data)?.Any() ?? false
        };

        // eager children (small trees) get materialized immediately
        if (ChildrenSelector is not null && LoadChildrenAsync is null)
        {
            foreach (var child in ChildrenSelector(data)) node.Children.Add(CreateNode(child, node));
            node.ChildrenLoaded = true;
            node.HasChildren = node.Children.Count > 0;
        }
        return node;
    }

    // ---- Expand / lazy load ---------------------------------------------------

    private async Task ToggleExpandAsync(TreeNode<TItem> node)
    {
        if (!node.HasChildren) return;

        if (!node.IsExpanded && !node.ChildrenLoaded && LoadChildrenAsync is not null)
        {
            node.IsLoading = true;
            RecomputeVisible();
            StateHasChanged();
            try
            {
                var kids = await LoadChildrenAsync(node.Data);
                foreach (var k in kids) node.Children.Add(CreateNode(k, node));
                node.ChildrenLoaded = true;
                node.HasChildren = node.Children.Count > 0;
            }
            finally { node.IsLoading = false; }
        }

        node.IsExpanded = !node.IsExpanded;
        RecomputeVisible();
        if (node.IsExpanded) await OnNodeExpanded.InvokeAsync(node);
    }

    // ---- Selection --------------------------------------------------------

    private async Task SelectNodeAsync(TreeNode<TItem> node)
    {
        if (!AllowMultiSelect)
        {
            foreach (var n in _selected) n.IsSelected = false;
            _selected.Clear();
        }
        node.IsSelected = !node.IsSelected || !AllowMultiSelect;
        if (node.IsSelected) _selected.Add(node); else _selected.Remove(node);
        _focusedNode = node;
        await OnNodeSelected.InvokeAsync(node);
    }

    // ---- Tri-state checkboxes (parent reflects children, children inherit parent) --

    private async Task ToggleCheckAsync(TreeNode<TItem> node)
    {
        var newState = node.Check == CheckState.Checked ? CheckState.Unchecked : CheckState.Checked;
        SetCheckRecursive(node, newState);
        PropagateCheckUpward(node.Parent);
        await OnCheckChanged.InvokeAsync(node);
        await SyncIndeterminateVisualsAsync();
    }

    private void SetCheckRecursive(TreeNode<TItem> node, CheckState state)
    {
        node.Check = state;
        foreach (var child in node.Children) SetCheckRecursive(child, state);
    }

    private void PropagateCheckUpward(TreeNode<TItem>? parent)
    {
        while (parent is not null)
        {
            var states = parent.Children.Select(c => c.Check).Distinct().ToList();
            parent.Check = states.Count == 1 ? states[0] : CheckState.Indeterminate;
            parent = parent.Parent;
        }
    }

    private Action<ElementReference> CheckboxRefCapture(TreeNode<TItem> node) => el => _checkboxRefs[node.Id] = el;

    private async Task SyncIndeterminateVisualsAsync()
    {
        var pairs = _visibleFlat.Where(n => _checkboxRefs.ContainsKey(n.Id)).ToList();
        if (pairs.Count == 0) return;
        var refs = pairs.Select(n => _checkboxRefs[n.Id]).ToArray();
        var states = pairs.Select(n => n.Check == CheckState.Indeterminate).ToArray();
        await JS.InvokeVoidAsync("awesomeTreeView.setIndeterminate", refs, states);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await JS.InvokeVoidAsync("awesomeTreeView.init");
        await SyncIndeterminateVisualsAsync();
    }

    // ---- Filtering (text match bubbles ancestors visible, auto-expands path) ------

    private async Task OnFilterInputAsync(ChangeEventArgs e)
    {
        _filterText = e.Value?.ToString() ?? "";
        ApplyFilter();
        await Task.CompletedTask;
    }

    private async Task ClearFilterAsync()
    {
        _filterText = "";
        ApplyFilter();
        await Task.CompletedTask;
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(_filterText))
        {
            foreach (var n in AllNodes()) { n.MatchesFilter = true; n.IsVisible = true; }
            RecomputeVisible();
            return;
        }

        var term = _filterText.Trim();
        foreach (var n in AllNodes()) n.MatchesFilter = n.Text.Contains(term, StringComparison.OrdinalIgnoreCase);

        foreach (var n in AllNodes()) n.IsVisible = false;
        foreach (var n in AllNodes().Where(n => n.MatchesFilter))
        {
            foreach (var ancestor in n.AncestorsAndSelf())
            {
                ancestor.IsVisible = true;
                if (AutoExpandOnFilter && ancestor != n) ancestor.IsExpanded = true;
            }
        }
        RecomputeVisible();
    }

    private IEnumerable<TreeNode<TItem>> AllNodes()
    {
        foreach (var r in _roots) { yield return r; foreach (var d in r.Descendants()) yield return d; }
    }

    private RenderFragment HighlightedText(TreeNode<TItem> node) => builder =>
    {
        if (string.IsNullOrWhiteSpace(_filterText) || !node.MatchesFilter) { builder.AddContent(0, node.Text); return; }
        var idx = node.Text.IndexOf(_filterText, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) { builder.AddContent(0, node.Text); return; }
        builder.AddContent(0, node.Text[..idx]);
        builder.OpenElement(1, "mark");
        builder.AddContent(2, node.Text.Substring(idx, _filterText.Length));
        builder.CloseElement();
        builder.AddContent(3, node.Text[(idx + _filterText.Length)..]);
    };

    // ---- Flatten visible nodes for the Virtualize list ----------------------------

    private void RecomputeVisible()
    {
        _visibleFlat.Clear();
        void Walk(IEnumerable<TreeNode<TItem>> nodes)
        {
            foreach (var n in nodes)
            {
                if (!n.IsVisible) continue;
                _visibleFlat.Add(n);
                if (n.IsExpanded) Walk(n.Children);
            }
        }
        Walk(_roots);
        _virtualize?.RefreshDataAsync();
    }

    // ---- Keyboard navigation: arrows, expand/collapse, space=check, type-ahead ----

    private async Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        if (_focusedNode is null && _visibleFlat.Count > 0) _focusedNode = _visibleFlat[0];
        if (_focusedNode is null) return;
        var idx = _visibleFlat.IndexOf(_focusedNode);

        switch (e.Key)
        {
            case "ArrowDown":
                if (idx < _visibleFlat.Count - 1) _focusedNode = _visibleFlat[idx + 1];
                break;
            case "ArrowUp":
                if (idx > 0) _focusedNode = _visibleFlat[idx - 1];
                break;
            case "ArrowRight":
                if (_focusedNode.HasChildren && !_focusedNode.IsExpanded) await ToggleExpandAsync(_focusedNode);
                else if (_focusedNode.Children.Count > 0) _focusedNode = _focusedNode.Children[0];
                break;
            case "ArrowLeft":
                if (_focusedNode.IsExpanded) await ToggleExpandAsync(_focusedNode);
                else if (_focusedNode.Parent is not null) _focusedNode = _focusedNode.Parent;
                break;
            case "Enter":
                await SelectNodeAsync(_focusedNode);
                break;
            case " ":
                if (ShowCheckboxes) await ToggleCheckAsync(_focusedNode);
                break;
            case "Home":
                _focusedNode = _visibleFlat.FirstOrDefault();
                break;
            case "End":
                _focusedNode = _visibleFlat.LastOrDefault();
                break;
            default:
                if (e.Key.Length == 1) { await TypeAheadAsync(e.Key); return; }
                break;
        }
        StateHasChanged();
    }

    private Task TypeAheadAsync(string key)
    {
        _typeAheadBuffer = (DateTime.UtcNow - _lastTypeAhead).TotalMilliseconds > 700 ? key : _typeAheadBuffer + key;
        _lastTypeAhead = DateTime.UtcNow;
        var match = _visibleFlat.FirstOrDefault(n => n.Text.StartsWith(_typeAheadBuffer, StringComparison.OrdinalIgnoreCase));
        if (match is not null) { _focusedNode = match; StateHasChanged(); }
        return Task.CompletedTask;
    }

    // ---- Drag & drop reordering / reparenting --------------------------------

    private TreeNode<TItem>? _dragged;

    private void OnDragStart(TreeNode<TItem> node) => _dragged = node;

    private void OnDragOverRow(DragEventArgs e, TreeNode<TItem> node)
    {
        if (_dragged is null || _dragged == node || IsDescendantOf(node, _dragged)) { _dropTarget = null; return; }
        _dropTarget = node;
        // top third = before, bottom third = after, middle = inside (reparent)
        var ratio = e.OffsetY / RowHeight;
        _dropPosition = ratio < 0.25 ? DropPosition.Before : ratio > 0.75 ? DropPosition.After : DropPosition.Inside;
    }

    private bool IsDescendantOf(TreeNode<TItem> candidate, TreeNode<TItem> ancestor) =>
        candidate.AncestorsAndSelf().Contains(ancestor);

    private async Task OnDropRowAsync(TreeNode<TItem> target)
    {
        if (_dragged is null || _dropTarget is null) return;
        var moved = _dragged;
        var oldParentList = moved.Parent?.Children ?? _roots;
        oldParentList.Remove(moved);

        TreeNode<TItem>? newParent;
        List<TreeNode<TItem>> newList;
        int insertIdx;

        if (_dropPosition == DropPosition.Inside)
        {
            newParent = target;
            newList = target.Children;
            insertIdx = newList.Count;
            target.HasChildren = true;
            target.ChildrenLoaded = true;
            target.IsExpanded = true;
        }
        else
        {
            newParent = target.Parent;
            newList = newParent?.Children ?? _roots;
            insertIdx = newList.IndexOf(target) + (_dropPosition == DropPosition.After ? 1 : 0);
        }

        moved.Parent = newParent;
        newList.Insert(Math.Clamp(insertIdx, 0, newList.Count), moved);

        _dragged = null;
        _dropTarget = null;
        RecomputeVisible();
        await OnNodeMoved.InvokeAsync(new TreeNodeMoveArgs<TItem>(moved, newParent, target, _dropPosition));
    }

    // ---- Public API for host pages -------------------------------------------

    public IReadOnlyList<TItem> GetCheckedItems() =>
        AllNodes().Where(n => n.Check == CheckState.Checked).Select(n => n.Data).ToList();

    public void ExpandAll() { foreach (var n in AllNodes().Where(n => n.HasChildren)) n.IsExpanded = true; RecomputeVisible(); StateHasChanged(); }
    public void CollapseAll() { foreach (var n in AllNodes()) n.IsExpanded = false; RecomputeVisible(); StateHasChanged(); }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
