using Dalamud.Interface.Utility;
using Dalamud.Bindings.ImGui;
using OtterGui.Extensions;
using OtterGui.Raii;
using OtterGui.Text;

namespace OtterGui.Table;

public static class Table
{
    public const float ArrowWidth = 10;
}

public class Table<T>
{
    protected          bool                   FilterDirty = true;
    protected          bool                   SortDirty   = true;
    protected readonly IReadOnlyCollection<T> Items;
    internal readonly  List<(T, int)>         FilteredItems;

    protected readonly string      Label;
    protected readonly Column<T>[] Headers;

    protected float ItemHeight  { get; set; }
    public    float ExtraHeight { get; set; } = 0;

    private int _currentIdx = 0;

    protected bool Sortable
    {
        get => Flags.HasFlag(ImGuiTableFlags.Sortable);
        set => Flags = value ? Flags | ImGuiTableFlags.Sortable : Flags & ~ImGuiTableFlags.Sortable;
    }

    protected int SortIdx = -1;

    public ImGuiTableFlags Flags = ImGuiTableFlags.RowBg
      | ImGuiTableFlags.Sortable
      | ImGuiTableFlags.BordersOuter
      | ImGuiTableFlags.ScrollY
      | ImGuiTableFlags.ScrollX
      | ImGuiTableFlags.PreciseWidths
      | ImGuiTableFlags.SizingFixedFit
      | ImGuiTableFlags.BordersInnerV
      | ImGuiTableFlags.NoBordersInBodyUntilResize;

    public int TotalItems
        => Items.Count;

    public int CurrentItems
        => FilteredItems.Count;

    public int TotalColumns
        => Headers.Length;

    public int VisibleColumns { get; private set; }

    public Table(string label, IReadOnlyCollection<T> items, params Column<T>[] headers)
    {
        Label          = label;
        Items          = items;
        Headers        = headers;
        FilteredItems  = new List<(T, int)>(Items.Count);
        VisibleColumns = Headers.Length;
    }

    // Draw／DrawItem／表頭迴圈的 ID 與樣式直接推彈（與 ImRaii.PushId／PushStyle 推彈相同），每幀不配置 Raii 物件。
    // ImRaii.Style 的靜態堆疊只供 ImRaii.DefaultStyle（教學視窗）用，表格內不會呼叫到。
    private Action<(T, int)>? _drawItem;

    public void Draw(float itemHeight)
    {
        ItemHeight = itemHeight;
        ImGui.PushID(Label);
        try
        {
            UpdateFilter();
            DrawTableInternal();
        }
        finally
        {
            ImGui.PopID();
        }
    }

    public bool WouldBeVisible(T value)
        => Headers.All(header => header.FilterFunc(value));

    protected virtual void DrawFilters()
        => throw new NotImplementedException();

    protected virtual void PreDraw()
    { }

    private void SortInternal()
    {
        if (!Sortable)
            return;

        var sortSpecs = ImGui.TableGetSortSpecs();
        SortDirty |= sortSpecs.SpecsDirty;

        if (!SortDirty)
            return;

        SortIdx = sortSpecs.Specs.ColumnIndex;

        if (Headers.Length <= SortIdx)
            SortIdx = 0;

        var header = Headers[SortIdx];
        if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending)
            SortBy(header, true);
        else if (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending)
            SortBy(header, false);
        else
            SortIdx = -1;
        SortDirty            = false;
        sortSpecs.SpecsDirty = false;
    }

    // 捕捉 header 的閉包放在這裡：留在 SortInternal 會在方法入口配置，沒排序的每一幀也配置一次。
    private void SortBy(Column<T> header, bool ascending)
    {
        header.PreSort();
        if (ascending)
            FilteredItems.StableSort((a, b) => header.Compare(a.Item1, b.Item1));
        else
            FilteredItems.StableSort((a, b) => header.CompareInv(a.Item1, b.Item1));
        header.PostSort();
    }

    private void UpdateFilter()
    {
        if (!FilterDirty)
            return;

        FilteredItems.Clear();
        var idx = 0;
        foreach (var item in Items)
        {
            if (WouldBeVisible(item))
                FilteredItems.Add((item, idx));
            idx++;
        }

        FilterDirty = false;
        SortDirty   = true;
    }

    private void DrawItem((T, int) pair)
    {
        var column = 0;
        ImGui.PushID(_currentIdx);
        _currentIdx = pair.Item2;
        try
        {
            foreach (var header in Headers)
            {
                ImGui.PushID(column++);
                try
                {
                    if (ImGui.TableNextColumn())
                        header.DrawColumn(pair.Item1, pair.Item2);
                }
                finally
                {
                    ImGui.PopID();
                }
            }
        }
        finally
        {
            ImGui.PopID();
        }
    }

    private void DrawTableInternal()
    {
        using var table = ImUtf8.Table("Table"u8, Headers.Length, Flags,
            ImGui.GetContentRegionAvail() - ExtraHeight * Vector2.UnitY * ImGuiHelpers.GlobalScale);
        if (!table)
            return;

        PreDraw();
        ImGui.TableSetupScrollFreeze(1, 1);

        foreach (var header in Headers)
            ImGui.TableSetupColumn(header.Label, header.Flags, header.Width);

        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        var i = 0;
        VisibleColumns = 0;
        foreach (var header in Headers)
        {
            ImGui.PushID(i);
            try
            {
                if (ImGui.TableGetColumnFlags(i).HasFlag(ImGuiTableColumnFlags.IsEnabled))
                    ++VisibleColumns;
                if (!ImGui.TableSetColumnIndex(i++))
                    continue;

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
                try
                {
                    ImGui.TableHeader(string.Empty);
                    ImGui.SameLine();
                }
                finally
                {
                    ImGui.PopStyleVar();
                }

                if (header.DrawFilter())
                    FilterDirty = true;
            }
            finally
            {
                ImGui.PopID();
            }
        }

        SortInternal();
        _currentIdx = 0;
        ImGuiClip.ClippedDraw(FilteredItems, _drawItem ??= DrawItem, ItemHeight);
    }
}
