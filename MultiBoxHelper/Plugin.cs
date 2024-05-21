using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Game.Config;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using MultiBoxHelper.Settings;
using MultiBoxHelper.Windows;
using System;

namespace MultiBoxHelper;

/// <summary>
/// Multibox Helper plugin
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/mbh";
    private const string BardModeCommand = "/bardmode";
    private const string CloneModeCommand = "/clonemode";

    private Mode currentMode = Mode.Default;
    public Mode CurrentMode
    {
        get
        {
            return currentMode;
        }
        set
        {
            currentMode = value;
            SettingsManager.SetMode(Configuration[currentMode]);
        }
    }

    private void ToggleBardMode()
    {
        // Check for Default here because we want to count Clone as Bard for this function
        if (CurrentMode != Mode.Default)
        {
            CurrentMode = Mode.Default;
        }
        else
        {
            CurrentMode = Mode.Bard;
        }
    }

    public Configuration Configuration
    {
        get; init;
    }

    public readonly WindowSystem WindowSystem = new("MultiBoxHelper");

    private ConfigWindow ConfigWindow
    {
        get; init;
    }

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(pluginInterface);

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Display Multibox Helper configuration"
        });

        Service.CommandManager.AddHandler(BardModeCommand, new CommandInfo(OnBardCommand)
        {
            HelpMessage = "Toggle Bard Mode (lower settings to improve performance)"
        });

        Service.CommandManager.AddHandler(CloneModeCommand, new CommandInfo(OnCloneCommand)
        {
            HelpMessage = "Force Clone Mode to be on."
        });

#if DEBUG
        Service.CommandManager.AddHandler("/datadump", new CommandInfo(OnDataDump)
        {
            HelpMessage = "Dump diagnostic or testing data we're currently needing."
        });
#endif

        pluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        //pluginInterface.UiBuilder.OpenMainUi += ToggleConfigUI;

        // Get notified for login and logout events
        Service.ClientState.Login += OnLogin;

        // For testing purposes at times
        //Service.GameConfig.Changed += GameConfig_Changed;
    }

#if DEBUG
    /// <summary>
    /// Create a log message to help us determine what setting actually changed for testing.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void GameConfig_Changed(object? sender, ConfigChangeEvent e)
    {
        Service.Log.Debug("Config change to: {0}", e.Option.ToString());
    }
#endif

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        Service.CommandManager.RemoveHandler(CommandName);
    }

    /// <summary>
    /// Handle /mbh
    /// </summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our configuration
        ToggleConfigUI();
    }

    /// <summary>
    /// Handle /bardmode
    /// </summary>
    /// <param name="command"></param>
    /// <param name="arguments"></param>
    private void OnBardCommand(string command, string arguments)
    {
        ToggleBardMode();
    }

    /// <summary>
    /// Force clone mode on /clonemode
    /// </summary>
    /// <param name="command"></param>
    /// <param name="arguments"></param>
    private void OnCloneCommand(string command, string arguments)
    {
        CurrentMode = Mode.Clone;
    }

#if DEBUG
    private void OnDataDump(string command, string arguments)
    {
        if (arguments.Equals("fps"))
        {
            uint fpsLimit, fpsInActive, fpsDownAfk;

            Service.GameConfig.TryGet(SystemConfigOption.Fps, out fpsLimit);
            Service.GameConfig.TryGet(SystemConfigOption.FPSDownAFK, out fpsDownAfk);
            Service.GameConfig.TryGet(SystemConfigOption.FPSInActive, out fpsInActive);

            Service.Log.Debug($"[FPS] fps limit: {fpsLimit}, fps afk: {fpsDownAfk}, fps inactive: {fpsInActive}");
        }
    }
#endif

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();

    public void OnLogin()
    {
        var pc = Service.ClientState.LocalPlayer;

        if (isClone(pc))
        {
            Service.Log.Debug("Clone detected.");
            CurrentMode = Mode.Clone;
        }
        else
        {
            CurrentMode = Mode.Default;
        }
    }

    public static void OnLogout()
    {
        Service.Log.Debug("Logout event happpens.");
    }

    private bool isClone(PlayerCharacter? pc)
    {
        if (pc != null && pc.HomeWorld != null && pc.HomeWorld.GameData != null)
        {
            Service.Log.Debug($"Checking for {pc.Name.TextValue} @ {pc.HomeWorld.GameData.Name.RawString}...");
            if (Configuration.CloneList.TryGetValue(pc.HomeWorld.Id, out var value))
            {
                Service.Log.Debug("Have world in list.");
                if (value.Contains(pc.Name.TextValue))
                {
                    return true;
                }
            }
        }

        return false;
    }







}
