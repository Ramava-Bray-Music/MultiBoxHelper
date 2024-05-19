using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System;
using Dalamud.Logging;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Config;
using System.ComponentModel;
using MultiBoxHelper.Windows;

namespace MultiBoxHelper;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/mbh";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("MultiBoxHelper");

    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(pluginInterface);

        // you might normally want to embed resources and load them from the manifest stream
        var file = new FileInfo(Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png"));

        // ITextureProvider takes care of the image caching and dispose
        var goatImage = Service.TextureProvider.GetTextureFromFile(file);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImage);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        pluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Get notified for login and logout events
        Service.ClientState.Login += OnLogin;
        Service.ClientState.Logout += OnLogout;

        // Checking to see some things about graphic settings.
        Service.GameConfig.Changed += GameConfig_Changed;
    }

    private void GameConfig_Changed(object? sender, ConfigChangeEvent e)
    {
        //Service.Log.Debug("Config change to: {0}", e.Option.ToString());
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        Service.CommandManager.RemoveHandler(CommandName);
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
        var pc = Service.ClientState.LocalPlayer;

        Service.Log.Debug("Login event occurred.");
        if (pc != null && pc.HomeWorld != null && pc.HomeWorld.GameData != null)
        {
            Service.Log.Debug("{0} @ {1} ({2})", pc.Name, pc.HomeWorld.GameData.Name, pc.NameId.ToString());
        }
    }

    public void OnLogout()
    {
        Service.Log.Debug("Logout event happpens.");
    }

    private void RunSettingTest()
    {
        // Turn down object limit
        //GameConfig.Set(SystemConfigOption.DisplayObjectLimitType, (uint)DisplayObjectLimit.Minimum);

        // Mute the sound
        // GameConfig.Set(SystemConfigOption.IsSndMaster, false);

        // Run the /btb gfxlow on command
        // Also just run /penumbra disable


        //CommandManager.ProcessCommand("/penumbra disable");
    }

}
