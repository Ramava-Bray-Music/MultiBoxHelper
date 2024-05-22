using Lumina;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MultiBoxHelper.Settings;

/// <summary>
/// Configuration data for an individual mode
/// </summary>
[Serializable]
public class ModeConfiguration
{
    // Default mode
    public bool MuteSound = false;

    public bool DisablePenumbra = false;
    public bool ChangeGraphicsMode = false;
    public int ObjectLimit = (int)DisplayObjectLimit.Maximum;

    public GraphicsConfiguration GraphicsSettings = [];

    // FPS related settings
    public int Fps = (int)FpsLimit.RefreshRate;
    public bool FpsDownAFK = true;
    public bool FpsDownInactive = true;

    public ModeConfiguration(Mode mode)
    {
        SetDefaults(mode);
    }

    public void SetDefaults(Mode mode = Mode.Default)
    {
        switch (mode)
        {

            case Mode.Clone:
                MuteSound = true;
                DisablePenumbra = true;
                ChangeGraphicsMode = false;
                ObjectLimit = (int)DisplayObjectLimit.Minimum;
                Fps = (int)FpsLimit.OneQuarterRefreshRate;
                FpsDownAFK = false;
                FpsDownInactive = false;
                break;
            case Mode.Bard:
                ObjectLimit = (int)DisplayObjectLimit.Normal;
                Fps = (int)FpsLimit.HalfRefreshRate;
                FpsDownAFK = false;
                FpsDownInactive = false;
                break;
        }
        GraphicsSettings = GetGraphicsDefaults(mode);
    }

    public static GraphicsConfiguration GetGraphicsDefaults(Mode mode = Mode.Default)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "MultiBoxHelper.Data.MaximumGraphicsSettings.json";
        if (mode != Mode.Default)
        {
            resourceName = "MultiBoxHelper.Data.BardGraphicsSettings.json";
        }

        if (assembly != null)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {

                    using (var reader = new StreamReader(stream))
                    {
                        var jsonFile = reader.ReadToEnd();
                        var data = JsonConvert.DeserializeObject<GraphicsConfiguration>(jsonFile);
                        if (data != null)
                        {
                            return data;
                        }
                    }
                }
            }
        }

        Service.Log.Debug($"Failed to load json data from {resourceName}");
        // Something went wrong somewhere
        return [];
    }

    public void SaveCurrentGraphicsSettings()
    {
        foreach (var setting in GraphicsSettings.Keys)
        {
            if (Service.GameConfig.System.TryGet(setting, out uint value))
            {
                GraphicsSettings[setting] = value;
            }
        }
    }
}
