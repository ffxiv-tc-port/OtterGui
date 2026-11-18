using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;

namespace OtterGui.Table;

public class ColumnString<TItem> : Column<TItem>
{
    public ColumnString()
        => Flags &= ~ImGuiTableColumnFlags.NoResize;

    public    string FilterValue = string.Empty;
    protected Regex? FilterRegex;

    public virtual string ToName(TItem item)
        => item!.ToString() ?? string.Empty;

    public override int Compare(TItem lhs, TItem rhs)
        => string.Compare(ToName(lhs), ToName(rhs), StringComparison.InvariantCulture);

    // 與 using ImRaii.PushStyle(FrameRounding, 0) 推彈相同，每幀不配置 Raii 物件。
    public override bool DrawFilter()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
        try
        {
            return DrawFilterInput();
        }
        finally
        {
            ImGui.PopStyleVar();
        }
    }

    private bool DrawFilterInput()
    {
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var tmp = FilterValue;
        if (!ImGui.InputTextWithHint(FilterLabel, Label, ref tmp, 256) || tmp == FilterValue)
            return false;

        FilterValue = tmp;
        try
        {
            FilterRegex = new Regex(FilterValue, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
        catch
        {
            FilterRegex = null;
        }

        return true;
    }

    public override bool FilterFunc(TItem item)
    {
        if (FilterValue.Length == 0)
            return true;

        var name = ToName(item);
        return FilterRegex?.IsMatch(name) ?? name.Contains(FilterValue, StringComparison.OrdinalIgnoreCase);
    }

    public override void DrawColumn(TItem item, int _)
    {
        ImGui.TextUnformatted(ToName(item));
    }
}
