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
using System.Linq;

namespace MultiBoxHelper;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/mbh";
    private const string BardModeCommand = "/bardmode";
    private const string CloneModeCommand = "/clonemode";

    private bool bardModeEnabled = false;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("MultiBoxHelper");

    private ConfigWindow ConfigWindow { get; init; }
    public AddCloneWindow AddCloneWindow { get; init; }

    //private MainWindow MainWindow { get; init; }

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
        AddCloneWindow = new AddCloneWindow(this);
        //MainWindow = new MainWindow(this, goatImage);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(AddCloneWindow);
        //WindowSystem.AddWindow(MainWindow);

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
        //pluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Get notified for login and logout events
        Service.ClientState.Login += OnLogin;
        //Service.ClientState.Logout += OnLogout;

        // Checking to see some things about graphic settings.
        //Service.GameConfig.Changed += GameConfig_Changed;
    }

    private void OnCloneCommand(string command, string arguments)
    {
        // Just force it
        Service.Log.Debug("Forcing Clone Mode!");
        SetCloneMode();
    }

    private void OnBardCommand(string command, string arguments)
    {
        ToggleBardMode();
    }

    private void GameConfig_Changed(object? sender, ConfigChangeEvent e)
    {
        Service.Log.Debug("Config change to: {0}", e.Option.ToString());
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        //MainWindow.Dispose();

        Service.CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our configuration
        ToggleConfigUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    //public void ToggleMainUI() => MainWindow.Toggle();

    public void OnLogin()
    {
        var pc = Service.ClientState.LocalPlayer;

        Service.Log.Debug("Login event occurred.");
        if (pc != null && pc.HomeWorld != null && pc.HomeWorld.GameData != null)
        {
            Service.Log.Debug("{0} @ {1} ({2})", pc.Name, pc.HomeWorld.GameData.Name, pc.NameId.ToString());
        }

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

    public void OnLogout()
    {
        Service.Log.Debug("Logout event happpens.");
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

    private bool isClone(PlayerCharacter? pc)
    {
        if (pc != null && pc.HomeWorld != null && pc.HomeWorld.GameData != null)
        {
            var key = string.Format("{0}:{1}", pc.HomeWorld.GameData.Name, pc.Name);
            Service.Log.Debug($"Checking for {key}...");
            if (Configuration.CloneCharacters != null && Configuration.CloneCharacters.Contains(key))
            {
                return true;
            }
        }

        return false;
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
        MuteSound(Configuration.CloneMuteSound);
        LowerGraphics(Configuration.CloneLowGraphicsMode);
        DisablePenumbra(Configuration.CloneDisablePenumbra);
        Service.GameConfig.Set(SystemConfigOption.DisplayObjectLimitType, (uint)Configuration.CloneObjectLimit);
    }

    private static void MuteSound(bool mute = true)
    {
        //Service.GameConfig.Set(SystemConfigOption.IsSoundDisable, mute);
        if (mute)
        {
            Service.Log.Debug("Attempting to mute");
            //Service.GameConfig.Set(SystemConfigOption.IsSndMaster, false);
            Service.GameConfig.System.Set("IsSndMaster", 0);
        }
        else
        {
            Service.GameConfig.System.Set("IsSndMaster", 1);
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
