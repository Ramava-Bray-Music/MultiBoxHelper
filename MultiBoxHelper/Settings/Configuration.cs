using Dalamud.Configuration;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using MultiBoxHelper.Ipc;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Linq;

namespace MultiBoxHelper.Settings;

/// <summary>
/// Configuration data for the plugin
/// </summary>
[Serializable]
public class Configuration : IPluginConfiguration
{
    public const string ConfigurationChangedIPC = "MultiBoxHelper.ConfigurationChanged";

    // Only parameter is current Version number to make sure one of the clients isn't using an old one.
    private readonly ICallGateProvider<bool, int, string> configurationChangedEvent = Service.PluginInterface.GetIpcProvider<bool, int, string>(ConfigurationChangedIPC);
    private readonly ICallGateSubscriber<bool, int, string> configurationChanged = Service.PluginInterface.GetIpcSubscriber<bool, int, string>(ConfigurationChangedIPC);

    // Update when this changes
    public int Version { get; set; } = 2024052502;

    public ModeConfiguration DefaultModeConfiguration { get; set; } = new ModeConfiguration(Mode.Default);
    public ModeConfiguration BardModeConfiguration { get; set; } = new ModeConfiguration(Mode.Bard);
    public ModeConfiguration CloneModeConfiguration { get; set; } = new ModeConfiguration(Mode.Clone);

    /// <summary>
    /// Override to allow access to mode configurations by index.
    /// </summary>
    /// <param name="mode">mode to access</param>
    /// <returns>configuration for requested mode</returns>
    public ModeConfiguration this[Mode mode]
    {
        get
        {
            return mode switch
            {
                Mode.Clone => CloneModeConfiguration,
                Mode.Bard => BardModeConfiguration,
                _ => DefaultModeConfiguration
            };
        }
    }

    public bool AutoLogin = false;
    // Delay before loggin in automatically in seconds.
    public int AutoLoginDelay = 20;

    // Some default values for adding clones by name (and eventually auto login stuff)
    public uint LastUsedDataCenter;
    public uint LastUsedWorld;

    /// <summary>
    /// List of clone characters
    /// </summary>
    public Dictionary<uint, List<string>> CloneCharacterList { get; set; } = [];

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
        try
        {
            // Trigger event to sync
            IpcHandles.SyncAllSettings();
        }
        catch (IpcNotReadyError)
        {
            Service.Log.Error("Unable to send configuration change event to other clients.");
        }
    }

    /// <summary>
    /// Add a clone character to the list based on a PlayerCharacter GameObject.
    /// </summary>
    /// <param name="character"></param>
    public bool AddClone(PlayerCharacter character)
    {
        return AddClone(character.HomeWorld.Id, character.Name.TextValue);
    }

    public bool AddClone(uint world, string clone)
    {
        // TODO: Should be providing feedback if any of this fails.
        // Need to make sure there's a sublist for that world first
        if (!CloneCharacterList.TryGetValue(world, out var value))
        {
            value = ([]);
            CloneCharacterList[world] = value;
        }

        // Validate character exists


        if (!value.Contains(clone))
        {
            value.Add(clone);
        }

        // Not sure we can fail yet
        return true;
    }

    /// <summary>
    /// Remove a clone from the list by name and world id.
    /// </summary>
    /// <param name="world">id of the world</param>
    /// <param name="clone">name of the clone</param>
    public void RemoveClone(uint world, string clone)
    {
        var list = CloneCharacterList[world];
        if (list == null)
            return;

        list.Remove(clone);

        // Remove the world character list if it's empty
        if (list.Count == 0)
        {
            CloneCharacterList.Remove(world);
        }
    }

    /// <summary>
    /// Save the current graphics settings to the selected mode
    /// </summary>
    /// <param name="mode"></param>
    public void SaveGraphicsSettings(Mode mode)
    {
        this[mode].SaveCurrentGraphicsSettings();
    }

    /// <summary>
    /// Helper function for resetting graphics defaults
    /// </summary>
    /// <param name="mode"></param>
    public void ResetGraphicsSettings(Mode mode)
    {
        this[mode].GraphicsSettings = ModeConfiguration.GetGraphicsDefaults(mode);
    }

}
