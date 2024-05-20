using Dalamud.Configuration;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace MultiBoxHelper;

/// <summary>
/// Configuration data for the plugin
/// </summary>
[Serializable]
public class Configuration : IPluginConfiguration
{
    // Update when this changes
    public int Version { get; set; } = 2024052001;

    // Default mode
    public bool DefaultMuteSound = false;
    public bool DefaultDisablePenumbra = false;
    public bool DefaultLowGraphicsMode = false;
    public int DefaultObjectLimit = (int)DisplayObjectLimit.Maximum;

    // Bard Mode
    public bool BardMuteSound = false;
    public bool BardDisablePenumbra = false;
    // could use a better middle ground for this
    public bool BardLowGraphicsMode = true;
    public int BardObjectLimit = (int)DisplayObjectLimit.Normal;

    // Clone mode
    public bool CloneMuteSound = true;
    public bool CloneDisablePenumbra = true;
    public bool CloneLowGraphicsMode = true;
    public int CloneObjectLimit = (int)DisplayObjectLimit.Minimum;

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
        if (!value.Contains(name)) {
            value.Add(name);
        }
    }

    internal void RemoveClone(uint world, string clone)
    {
        var list = CloneList[world];
        if (list == null) return;

        list.Remove(clone);

        // Remove the sublist if it's empty
        if (list.Count == 0)
        {
            CloneList.Remove(world);
        }
    }
}
