using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FindersFilter.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Configuration config) : base(
        "FindersFilter"
    )
    {
        this.configuration = config;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var configValue = this.configuration.PartyMakeupFilter;
        if (ImGui.Checkbox("Filter PF entries based on current party", ref configValue))
        {
            this.configuration.PartyMakeupFilter = configValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.configuration.Save();
        }
    }
}
