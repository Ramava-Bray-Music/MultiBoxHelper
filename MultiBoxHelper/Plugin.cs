using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Game.Config;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using MultiBoxHelper.IPC;
using MultiBoxHelper.Settings;
using MultiBoxHelper.Windows;
using System.Diagnostics;

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

    // This is a good place for it, I think?
    internal static MultiboxManager MultiboxManager { get; private set; } = new MultiboxManager();

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

            // Do we want both? Not sure which is better yet.
            Service.ToastGui.ShowNormal($"Enabled {currentMode} mode.");
            Service.ChatGui.PrintError($"Enabled {currentMode} mode.", "Multibox Helper");

            Service.Log.Info($"Enabling {currentMode} mode");
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

    public static Configuration Configuration
    {
        get; private set;
    } = new Configuration();

    public readonly WindowSystem WindowSystem = new("MultiBoxHelper");

    private ConfigWindow ConfigWindow
    {
        get; init;
    }

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();

        var config = pluginInterface.GetPluginConfig();
        if (config != null)
        {
            Configuration = (Configuration)config;
        }
        Configuration.Initialize(pluginInterface);

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        _ = Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Display Multibox Helper configuration"
        });

        _ = Service.CommandManager.AddHandler(BardModeCommand, new CommandInfo(OnBardCommand)
        {
            HelpMessage = "Toggle Bard Mode (lower settings to improve performance)"
        });

        _ = Service.CommandManager.AddHandler(CloneModeCommand, new CommandInfo(OnCloneCommand)
        {
            HelpMessage = "Force Clone Mode to be on."
        });

#if DEBUG
        _ = Service.CommandManager.AddHandler("/datadump", new CommandInfo(OnDataDump)
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

#if DEBUG
        // For testing purposes at times
        Service.GameConfig.Changed += GameConfig_Changed;
#endif
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        if (MultiboxManager != null)
        {
            MultiboxManager.Dispose();
        }
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
    public bool watchConfigChanges = false;

    /// <summary>
    /// Create a log message to help us determine what setting actually changed for testing.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameConfig_Changed(object? sender, ConfigChangeEvent e)
    {
        if (watchConfigChanges)
        {
            Service.Log.Debug("Config change to: {0}", e.Option.ToString());
        }

    }

    private void OnDataDump(string command, string arguments)
    {
        // Add data dump for whatever we're researching here.
    }
#endif

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();

    public void OnLogin()
    {
        var pc = Service.ClientState.LocalPlayer;

        if (IsClone(pc))
        {
            CurrentMode = Mode.Clone;
        }
        else
        {
            CurrentMode = Mode.Default;
        }
    }

    private static bool IsClone(PlayerCharacter? pc)
    {
        if (pc != null && pc.HomeWorld != null && pc.HomeWorld.GameData != null)
        {
            // Check for list for home world
            if (Configuration != null && Configuration.CloneCharacterList.TryGetValue(pc.HomeWorld.Id, out var value))
            {
                // check for specific character
                if (value.Contains(pc.Name.TextValue))
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal static void UpdateConfiguration(Configuration config)
    {
        Configuration = config;
        // TODO: Check for things we might need to do when this changes.
    }
}
