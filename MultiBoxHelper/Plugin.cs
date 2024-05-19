using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.MultiBoxHelper.Windows;
using System;
using Dalamud.Logging;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Config;

namespace Dalamud.MultiBoxHelper;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/mbh";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("MultiBoxHelper");

    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    [PluginService]
    private DalamudPluginInterface PluginInterface { get; init; }

    [PluginService]
    private ICommandManager CommandManager { get; init; }

    private IClientState ClientState { get; init; }

    private IPluginLog Logger { get; init; }

    private IGameConfig GameConfig { get; init; }

    /// <summary>
    /// Gets the Dalamud client state.
    /// </summary>
    //[PluginService]
    //private IClientState ClientState { get; set; } = null!;

    /// <summary>
    /// Gets the Dalamud command manager.
    /// </summary>
    //[PluginService]
    //private IGameInteropProvider Interop { get; set; } = null!;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager,
        [RequiredVersion("1.0")] ITextureProvider textureProvider,
        [RequiredVersion("1.0")] IClientState clientState,
        [RequiredVersion("1.0")] IPluginLog pluginLog,
        [RequiredVersion("1.0")] IGameConfig gameConfig)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ClientState = clientState;
        Logger = pluginLog;
        GameConfig = gameConfig;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        // you might normally want to embed resources and load them from the manifest stream
        var file = new FileInfo(Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png"));

        // ITextureProvider takes care of the image caching and dispose
        var goatImage = textureProvider.GetTextureFromFile(file);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImage);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Get notified for login and logout events
        ClientState.Login += OnLogin;
        ClientState.Logout += OnLogout;

        // Checking to see some things about graphic settings.
        GameConfig.Changed += GameConfig_Changed;
    }

    private void GameConfig_Changed(object? sender, Game.Config.ConfigChangeEvent e)
    {
        //Logger.Debug("Config change to: {0}", e.Option.ToString());
        
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    public void OnLogin()
    {
        PlayerCharacter? pc = ClientState.LocalPlayer;

        Logger.Debug("Login event occurred.");
        if (pc != null)
        {
            Logger.Debug("{0} @ {1} ({2})", pc.Name, pc.HomeWorld.GameData.Name, pc.NameId.ToString());
        }
    }

    public void OnLogout()
    {
        Logger.Debug("Logout event happpens.");
    }

    

}
