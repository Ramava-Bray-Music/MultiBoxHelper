using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace MultiBoxHelper;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool DefaultMuteSound = false;
    public bool DefaultDisablePenumbra = false;
    public bool DefaultLowGraphicsMode = false;
    public int DefaultObjectLimit = (int)DisplayObjectLimit.Maximum;

    public bool BardMuteSound = false;
    public bool BardDisablePenumbra = false;
    public bool BardLowGraphicsMode = false;
    public int BardObjectLimit = (int)DisplayObjectLimit.Normal;

    public bool CloneMuteSound = false;
    public bool CloneDisablePenumbra = false;
    public bool CloneLowGraphicsMode = false;
    public int CloneObjectLimit = (int)DisplayObjectLimit.Minimum;

    public List<Character>? CloneCharacters { get; set; } = [];

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
}
