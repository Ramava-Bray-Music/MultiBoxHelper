using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MultiBoxHelper.Settings;

/// <summary>
/// Configuration data for an individual mode
/// </summary>
[Serializable]
public class ModeConfig
{
    // Default mode
    public bool MuteSound = false;

    public bool DisablePenumbra = false;
    public bool LowGraphicsMode = false;
    public int ObjectLimit = (int)DisplayObjectLimit.Maximum;

    public int Fps = (int)FpsLimit.RefreshRate;
    public bool FpsDownAFK = true;
    public bool FpsDownInactive = true;

    public ModeConfig(Mode mode)
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
                LowGraphicsMode = true;
                ObjectLimit = (int)DisplayObjectLimit.Minimum;
                Fps = (int)FpsLimit.OneQuarterRefreshRate;
                FpsDownAFK = false;
                FpsDownInactive = false;
                break;
            case Mode.Bard:
                LowGraphicsMode = true;
                ObjectLimit = (int)DisplayObjectLimit.Normal;
                Fps = (int)FpsLimit.HalfRefreshRate;
                FpsDownAFK = false;
                FpsDownInactive = false;
                break;
        }
    }
}
