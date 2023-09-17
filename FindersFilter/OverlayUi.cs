using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace FindersFilter;

public class OverlayUi : IDisposable
{
    private readonly Configuration configuration;

    public OverlayUi(Configuration configuration)
    {
        this.configuration = configuration;
    }

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
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, hSpace);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, hSpace);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, hSpace);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.One);

        if (ImGui.Begin("FindersFilterOverlay",
                        ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoScrollWithMouse |
                        ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground))
        {
            FilterToggleButton(c => c.PartyMakeupFilter, "Show only parties you can join");
        }
        
        ImGui.PopStyleVar(6);
        ImGui.End();
    }

    private void FilterToggleButton(Expression<Func<Configuration, bool>> expr, string info)
    {
        var target = (MemberExpression)expr.Body;
        var prop = (PropertyInfo)target.Member;
        var b = (bool)prop.GetValue(configuration)!;
        
        ImGui.SameLine();
        var style = ImGui.GetStyle().Colors;
        ImGui.PushStyleColor(ImGuiCol.Text, b ? style[(int)ImGuiCol.CheckMark] : style[(int)ImGuiCol.TextDisabled]);
        if (ImExt.IconSelectable(FontAwesomeIcon.DoorOpen, b))
        {
            prop.SetValue(configuration, !b);
        }

        ImGui.PopStyleColor(1);

        if (ImGui.IsItemHovered())
        {
            // b ? checkmark : ffxiv cross mark
            ImGui.SetTooltip($"{(b ? '\u2713' : '\ue04c')} {info}");
        }
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
