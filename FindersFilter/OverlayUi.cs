using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace FindersFilter;

public class OverlayUi : IDisposable
{
    public void Draw()
    {
        // figure out where the PF window is, or quit if not visible
        short pfX;
        short pfY;

        unsafe
        {
            var pfAddon = GetPartyFinderAddon();
            if (pfAddon == null || !pfAddon->IsVisible) return;

            pfX = pfAddon->X;
            pfY = pfAddon->Y;
        }

        // Offsets are found experimentally. The window should begin next to the "Party Finder" title.
        ImGui.SetNextWindowPos(new Vector2(pfX + 100, pfY + 13));

        // Make the window transparent and have smaller components
        // courtesy of Marketbuddy: https://github.com/chalkos/Marketbuddy/blob/3b54a3f9ab343cb3ccd3ae533abfb25cb87bcaa0/Marketbuddy/PluginUI.cs#L49 
        var hSpace = new Vector2(1, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, hSpace);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, hSpace);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, hSpace);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.One);

        if (ImGui.Begin("FindersFilterOverlay",
                        ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollWithMouse |
                        ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground))
        {
            var b = false;
            ImGui.Checkbox("Show only parties you can join", ref b);
        }

        ImGui.PopStyleVar(5);
        ImGui.End();
    }

    public static unsafe AtkUnitBase* GetPartyFinderAddon()
    {
        var addon = Dalamud.GameGui.GetAddonByName("LookingForGroup");
        if (addon != IntPtr.Zero) return (AtkUnitBase*)addon;
        return null;
    }

    public void Dispose()
    {
    }
    
}
