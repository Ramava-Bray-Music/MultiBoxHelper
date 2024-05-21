using Dalamud.Game.Config;

namespace MultiBoxHelper.Settings;

/// <summary>
/// Static class for holding our helper functions to change settings.
/// </summary>
public static class SettingsManager
{
    /// <summary>
    /// Master function to access all the helpers at once for current mode.
    /// </summary>
    public static void SetMode(ModeConfiguration config)
    {
        MuteSound(config.MuteSound);
        LowerGraphics(config.LowGraphicsMode);
        DisablePenumbra(config.DisablePenumbra);
        Service.GameConfig.Set(SystemConfigOption.DisplayObjectLimitType, (uint)config.ObjectLimit);
        SetFps(config);
    }

    /// <summary>
    /// Adjust FPS settings
    /// </summary>
    /// <param name="config">configuration for current mode</param>
    public static void SetFps(ModeConfiguration config)
    {
        Service.GameConfig.Set(SystemConfigOption.Fps, config.Fps);
        Service.GameConfig.Set(SystemConfigOption.FPSInActive, config.FpsDownInactive);
        Service.GameConfig.Set(SystemConfigOption.FPSDownAFK, config.FpsDownAFK);
    }

    /// <summary>
    /// Mute all sound
    /// </summary>
    /// <param name="mute"></param>
    public static void MuteSound(bool mute = true)
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

    /// <summary>
    /// Disable Penumbra plugin
    /// </summary>
    /// <param name="disable"></param>
    public static void DisablePenumbra(bool disable = true)
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

    /// <summary>
    /// Lower graphics settings
    /// </summary>
    /// <param name="lower"></param>
    /// TODO: Major rewrite
    public static void LowerGraphics(bool lower = true)
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
