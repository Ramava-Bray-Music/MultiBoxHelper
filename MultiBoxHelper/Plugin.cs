using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using Dalamud.Game.Config;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using MultiBoxHelper.Windows;

namespace MultiBoxHelper;

/// <summary>
/// Multibox Helper plugin
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/mbh";
    private const string BardModeCommand = "/bardmode";
    private const string CloneModeCommand = "/clonemode";

    private bool bardModeEnabled = false;

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

        // you might normally want to embed resources and load them from the manifest stream
        //var file = new FileInfo(Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png"));

        // ITextureProvider takes care of the image caching and dispose
        //var goatImage = Service.TextureProvider.GetTextureFromFile(file);

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
            HelpMessage = "Force Clone mode to be on."
        });

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
        SetCloneMode();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();

    public void OnLogin()
    {
        var pc = Service.ClientState.LocalPlayer;

        if (isClone(pc))
        {
            Service.Log.Debug("Clone detected.");
            SetCloneMode();
        }
        else
        {
            SetDefaultMode();
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


    private void ToggleBardMode()
    {
        if (bardModeEnabled)
        {
            SetDefaultMode();
        }
        else
        {
            SetBardMode();
        }
    }

    private void SetBardMode()
    {
        bardModeEnabled = true;

        MuteSound(Configuration.BardMuteSound);
        LowerGraphics(Configuration.BardLowGraphicsMode);
        DisablePenumbra(Configuration.BardDisablePenumbra);
        Service.GameConfig.Set(SystemConfigOption.DisplayObjectLimitType, (uint)Configuration.BardObjectLimit);
    }

    private void SetDefaultMode()
    {
        bardModeEnabled = false;

        MuteSound(Configuration.DefaultMuteSound);
        LowerGraphics(Configuration.DefaultLowGraphicsMode);
        DisablePenumbra(Configuration.DefaultDisablePenumbra);
        Service.GameConfig.Set(SystemConfigOption.DisplayObjectLimitType, (uint)Configuration.DefaultObjectLimit);
    }

    private void SetCloneMode()
    {
        // so we can just go to default mode from clone mode directly
        bardModeEnabled = true;

        MuteSound(Configuration.CloneMuteSound);
        LowerGraphics(Configuration.CloneLowGraphicsMode);
        DisablePenumbra(Configuration.CloneDisablePenumbra);
        Service.GameConfig.Set(SystemConfigOption.DisplayObjectLimitType, (uint)Configuration.CloneObjectLimit);
    }

    private static void MuteSound(bool mute = true)
    {
        if (mute)
        {
            Service.Log.Debug("Attempting to mute");
            Service.GameConfig.System.Set("SoundMaster", 0);
        }
        else
        {
            Service.GameConfig.System.Set("SoundMaster", 100);
        }
    }

    private static void DisablePenumbra(bool disable = true)
    {
        if (disable)
        {
            Service.CommandManager.ProcessCommand("/penumbra disable");
        }
        else
        {
            Service.CommandManager.ProcessCommand("/penumbra enable");
        }
    }

    private static void LowerGraphics(bool lower = true)
    {
        if (lower)
        {
            Service.CommandManager.ProcessCommand("/btb gfxlow on");
        }
        else
        {
            Service.CommandManager.ProcessCommand("/btb gfxlow off");
        }
    }
}
