using Dalamud.Bindings.ImGui;

namespace OtterGui.Table;

public class Column<TItem>
{
    public string                Label = string.Empty;
    public ImGuiTableColumnFlags Flags = ImGuiTableColumnFlags.NoResize;

    public virtual float Width
        => -1f;

    private string? _filterLabelSource;
    private string  _filterLabel = string.Empty;

    // 同 $"##{Label}Filter"；表頭每幀讀它，Label 換成別的字串才重組。
    public string FilterLabel
    {
        get
        {
            var label = Label;
            if (label is null || !ReferenceEquals(label, _filterLabelSource))
            {
                _filterLabel       = $"##{label}Filter";
                _filterLabelSource = label;
            }

            return _filterLabel;
        }
    }

    public virtual bool DrawFilter()
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(Label);
        return false;
    }

    public virtual bool FilterFunc(TItem item)
        => true;

    public virtual int Compare(TItem lhs, TItem rhs)
        => 0;

    public virtual void DrawColumn(TItem item, int idx)
    { }

    public virtual void PreSort()
    { }

    public virtual void PostSort()
    { }

    public int CompareInv(TItem lhs, TItem rhs)
        => Compare(rhs, lhs);
}
