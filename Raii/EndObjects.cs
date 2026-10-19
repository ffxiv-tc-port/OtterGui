using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
using OtterGui.Widgets;

namespace OtterGui.Raii;

// Most ImGui widgets with IDisposable interface that automatically destroys them
// when created with using variables.
public static partial class ImRaii
{
    // What every EndConditionally without success is returned as: it has nothing to end, so one object serves all.
    // It is disposed up front; a later Dispose returns at the first check and never writes to it.
    private static readonly IEndObject FailedEnd = CreateFailedEnd();

    private static int _disabledCount = 0;

    public static IEndObject CollapsingHeader(string label)
    {
        var ret = ImGui.CollapsingHeader(label);
        ImGui.PushID(label);
        return new EndUnconditionally(ImGui.PopID, ret);
    }

    public static IEndObject ItemWidth(float width)
    {
        ImGui.PushItemWidth(width);
        return new EndUnconditionally(ImGui.PopItemWidth, true);
    }

    public static IEndObject Child(string strId)
        => new EndUnconditionally(ImGui.EndChild, ImGui.BeginChild(strId));

    public static IEndObject Child(string strId, Vector2 size)
        => new EndUnconditionally(ImGui.EndChild, ImGui.BeginChild(strId, size));

    public static IEndObject Child(string strId, Vector2 size, bool border)
        => new EndUnconditionally(ImGui.EndChild, ImGui.BeginChild(strId, size, border));

    public static IEndObject Child(string strId, Vector2 size, bool border, ImGuiWindowFlags flags)
        => new EndUnconditionally(ImGui.EndChild, ImGui.BeginChild(strId, size, border, flags));

    public static IEndObject DragDropTarget()
        => Conditional(ImGui.EndDragDropTarget, ImGui.BeginDragDropTarget());

    public static IEndObject DragDropSource()
        => Conditional(ImGui.EndDragDropSource, ImGui.BeginDragDropSource());

    public static IEndObject DragDropSource(ImGuiDragDropFlags flags)
        => Conditional(ImGui.EndDragDropSource, ImGui.BeginDragDropSource(flags));

    public static IEndObject Popup(string id)
        => Conditional(ImGui.EndPopup, ImGui.BeginPopup(id));

    public static IEndObject Popup(string id, ImGuiWindowFlags flags)
        => Conditional(ImGui.EndPopup, ImGui.BeginPopup(id, flags));

    public static IEndObject ContextPopupItem()
        => Conditional(ImGui.EndPopup, ImGui.BeginPopupContextItem(default));

    public static IEndObject ContextPopupItem(string label)
        => Conditional(ImGui.EndPopup, ImGui.BeginPopupContextItem(label));

    public static IEndObject ContextPopupItem(string label, ImGuiPopupFlags flags)
        => Conditional(ImGui.EndPopup, ImGui.BeginPopupContextItem(label, flags));

    public static IEndObject ContextPopup(string id)
        => Conditional(ImGui.EndPopup, ImGui.BeginPopupContextWindow(id));

    public static IEndObject ContextPopup(string id, ImGuiPopupFlags flags)
        => Conditional(ImGui.EndPopup, ImGui.BeginPopupContextWindow(id, flags));

    public static IEndObject Combo(string label, string previewValue)
        => Conditional(ImGui.EndCombo, ImGui.BeginCombo(label, previewValue));

    public static IEndObject Combo(string label, string previewValue, ImGuiComboFlags flags)
        => Conditional(ImGui.EndCombo, ImGui.BeginCombo(label, previewValue, flags));

    public static IEndObject Group()
    {
        ImGui.BeginGroup();
        return new EndUnconditionally(ImGui.EndGroup, true);
    }

    public static IEndObject Tooltip()
    {
        ImGui.BeginTooltip();
        return new EndUnconditionally(ImGui.EndTooltip, true);
    }

    public static IEndObject ListBox(string label)
        => Conditional(ImGui.EndListBox, ImGui.BeginListBox(label));

    public static IEndObject ListBox(string label, Vector2 size)
        => Conditional(ImGui.EndListBox, ImGui.BeginListBox(label, size));

    public static IEndObject Table(string table, int numColumns)
        => Conditional(ImGui.EndTable, ImGui.BeginTable(table, numColumns));

    public static IEndObject Table(string table, int numColumns, ImGuiTableFlags flags)
        => Conditional(ImGui.EndTable, ImGui.BeginTable(table, numColumns, flags));

    public static IEndObject Table(string table, int numColumns, ImGuiTableFlags flags, Vector2 outerSize)
        => Conditional(ImGui.EndTable, ImGui.BeginTable(table, numColumns, flags, outerSize));

    public static IEndObject Table(string table, int numColumns, ImGuiTableFlags flags, Vector2 outerSize, float innerWidth)
        => Conditional(ImGui.EndTable, ImGui.BeginTable(table, numColumns, flags, outerSize, innerWidth));

    public static IEndObject TabBar(string label)
        => Conditional(ImGui.EndTabBar, ImGui.BeginTabBar(label));

    public static IEndObject TabBar(string label, ImGuiTabBarFlags flags)
        => Conditional(ImGui.EndTabBar, ImGui.BeginTabBar(label, flags));

    public static IEndObject TabItem(string label)
        => Conditional(ImGui.EndTabItem, ImGui.BeginTabItem(label));

    public static unsafe IEndObject TabItem(byte* label, ImGuiTabItemFlags flags)
        => Conditional(ImGui.EndTabItem, ImGui.BeginTabItem(label, flags));

    public static IEndObject TabItem(string label, ref bool open)
        => Conditional(ImGui.EndTabItem, ImGui.BeginTabItem(label, ref open));

    public static IEndObject TabItem(string label, ref bool open, ImGuiTabItemFlags flags)
        => Conditional(ImGui.EndTabItem, ImGui.BeginTabItem(label, ref open, flags));

    public static IEndObject TreeNode(string label)
        => Conditional(ImGui.TreePop, ImGui.TreeNodeEx(label));

    public static IEndObject TreeNode(string label, ImGuiTreeNodeFlags flags)
        => Conditional(flags.HasFlag(ImGuiTreeNodeFlags.NoTreePushOnOpen) ? Nop : ImGui.TreePop, ImGui.TreeNodeEx(label, flags));

    public static IEndObject Disabled()
    {
        ImGui.BeginDisabled();
        ++_disabledCount;
        return DisabledEnd();
    }

    public static IEndObject Disabled(bool disabled)
    {
        if (!disabled)
            return FailedEnd;

        ImGui.BeginDisabled();
        ++_disabledCount;
        return DisabledEnd();
    }

    public static IEndObject Enabled()
    {
        var oldCount = _disabledCount;
        return oldCount == 0 ? FailedEnd : EnableAll(oldCount);
    }

    // Separate method so the closure over oldCount is only allocated when something is actually re-enabled.
    private static IEndObject EnableAll(int oldCount)
    {
        void Restore()
        {
            _disabledCount += oldCount;
            while (--oldCount >= 0)
                ImGui.BeginDisabled();
        }

        for (; _disabledCount > 0; --_disabledCount)
            ImGui.EndDisabled();

        return new EndUnconditionally(Restore, true);
    }

    private static IEndObject DisabledEnd()
        => new EndUnconditionally(() =>
        {
            --_disabledCount;
            ImGui.EndDisabled();
        }, true);

    public static IEndObject FramedGroup(string label, uint borderColor = 0, uint headerColor = 0,
        FontAwesomeIcon headerPreIcon = FontAwesomeIcon.None)
    {
        Widget.BeginFramedGroup(label, Vector2.Zero, string.Empty, headerColor, headerPreIcon);
        return new EndUnconditionally(() => Widget.EndFramedGroup(borderColor), true);
    }

    public static IEndObject FramedGroup(string label, Vector2 minSize, string description = "", uint borderColor = 0, uint headerColor = 0,
        FontAwesomeIcon headerPreIcon = FontAwesomeIcon.None)
    {
        Widget.BeginFramedGroup(label, minSize, description, headerColor, headerPreIcon);
        return new EndUnconditionally(() => Widget.EndFramedGroup(borderColor), true);
    }

    // Exported interface for RAII.
    public interface IEndObject : IDisposable
    {
        public bool Success { get; }

        public static bool operator true(IEndObject i)
            => i.Success;

        public static bool operator false(IEndObject i)
            => !i.Success;

        public static bool operator !(IEndObject i)
            => !i.Success;

        public static bool operator &(IEndObject i, bool value)
            => i.Success && value;

        public static bool operator |(IEndObject i, bool value)
            => i.Success || value;

        // Empty end object.
        public static readonly IEndObject Empty = new EndConditionally(Nop, false);
    }

    // Use end-function regardless of success.
    // Used by Child, Group and Tooltip.
    private struct EndUnconditionally(Action endAction, bool success) : IEndObject
    {
        private Action EndAction { get; }              = endAction;
        public  bool   Success   { get; }              = success;
        public  bool   Disposed  { get; private set; } = false;

        public void Dispose()
        {
            if (Disposed)
                return;

            EndAction();
            Disposed = true;
        }
    }

    // Use end-function only on success.
    private struct EndConditionally(Action endAction, bool success) : IEndObject
    {
        private Action EndAction { get; }              = endAction;
        public  bool   Success   { get; }              = success;
        public  bool   Disposed  { get; private set; } = false;

        public void Dispose()
        {
            if (Disposed)
                return;

            if (Success)
                EndAction();
            Disposed = true;
        }
    }

    // A successful result must be its own object: its Disposed flag is what keeps a second Dispose from ending twice.
    private static IEndObject Conditional(Action endAction, bool success)
        => success ? new EndConditionally(endAction, true) : FailedEnd;

    private static IEndObject CreateFailedEnd()
    {
        IEndObject end = new EndConditionally(Nop, false);
        end.Dispose();
        return end;
    }

    // Used to avoid tree pops when flag for no push is set.
    private static void Nop()
    { }
}
