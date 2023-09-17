using Dalamud.Interface;
using ImGuiNET;

namespace FindersFilter;

public static class ImExt
{

    public static bool IconSelectable(FontAwesomeIcon icon, bool selected)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var flag = ImGui.Selectable(icon.ToIconString(), selected);
        ImGui.PopFont();
        return flag;
    }
    
}
