using Dalamud.Data;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using FindersFilter.Windows;

namespace FindersFilter;

public class Dalamud
{
    public static void Initialize(DalamudPluginInterface pluginInterface) => pluginInterface.Create<Dalamud>();

    [PluginService]
    public static DalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    public static CommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    public static GameGui GameGui { get; private set; } = null!;

    [PluginService]
    public static PartyFinderGui PartyFinderGui { get; private set; } = null!;

    [PluginService]
    public static DataManager DataManager { get; private set; } = null!;
}

// ReSharper disable once UnusedType.Global
public sealed class Plugin : IDalamudPlugin
{
    public string Name => "FindersFilter";

    private readonly WindowSystem windowSystem = new("FindersFilter");
    private ConfigWindow ConfigWindow { get; init; }
    private readonly OverlayUi overlayUi;
    public Configuration Configuration { get; set; }
    private readonly IFilter currentFilter;

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        Dalamud.Initialize(pluginInterface);

        this.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream

        ConfigWindow = new ConfigWindow(Configuration);
        overlayUi = new OverlayUi(Configuration);
        currentFilter = new JoinableMakeupFilter(Configuration);

        windowSystem.AddWindow(ConfigWindow);

        Dalamud.CommandManager.AddHandler("/ffilter", new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the FindersFilter configuration window"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        Dalamud.PartyFinderGui.ReceiveListing += OnReceiveListing;
    }

    private void OnReceiveListing(PartyFinderListing listing, PartyFinderListingEventArgs args)
    {
        args.Visible = currentFilter.AcceptsListing(listing);
    }

    public void Dispose()
    {
        this.windowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        overlayUi.Dispose();

        Dalamud.PartyFinderGui.ReceiveListing -= OnReceiveListing;

        Dalamud.CommandManager.RemoveHandler("/ffilter");
    }

    private void DrawUI()
    {
        windowSystem.Draw();
        overlayUi.Draw();
    }

    private void OnCommand(string command, string args)
    {
        DrawConfigUI();
    }

    public void DrawConfigUI()
    {
        ConfigWindow.IsOpen = true;
    }
}
