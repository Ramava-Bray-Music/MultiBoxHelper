using Dalamud.Configuration;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MultiBoxHelper.Settings;

/// <summary>
/// Configuration data for the plugin
/// </summary>
[Serializable]
public class Configuration : IPluginConfiguration
{
    // Update when this changes
    public int Version { get; set; } = 2024052101;

    public ModeConfig Default { get; set; } = new ModeConfig(Mode.Default);
    public ModeConfig Bard { get; set; } = new ModeConfig(Mode.Bard);
    public ModeConfig Clone { get; set; } = new ModeConfig(Mode.Clone);

    public ModeConfig this[Mode mode]
    {
        get
        {
            return mode switch
            {
                Mode.Clone => Clone,
                Mode.Bard => Bard,
                _ => Default
            };
        }
    }

    /// <summary>
    /// List of clone characters
    /// </summary>
    public Dictionary<uint, List<string>> CloneList { get; set; } = [];

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        pluginInterface!.SavePluginConfig(this);
    }

    public void AddClone(PlayerCharacter character)
    {
        // TODO: Should be providing feedback if any of this fails.
        var world = character.HomeWorld.Id;
        var name = character.Name.TextValue;

        // Need to make sure there's a sublist for that world first
        if (!CloneList.TryGetValue(world, out var value))
        {
            value = ([]);
            CloneList[world] = value;
        }
        if (!value.Contains(name))
        {
            value.Add(name);
        }
    }

    internal void RemoveClone(uint world, string clone)
    {
        var list = CloneList[world];
        if (list == null)
            return;

        list.Remove(clone);

        // Remove the sublist if it's empty
        if (list.Count == 0)
        {
            CloneList.Remove(world);
        }
    }
}
