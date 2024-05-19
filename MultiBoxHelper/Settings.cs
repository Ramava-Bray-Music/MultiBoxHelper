using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBoxHelper;

[Serializable]
public class Settings
{
    public bool MuteSound = false;
    public bool DisablePenumbra = false;
    public bool LowGraphicsMode = false;
    public DisplayObjectLimit ObjectLimit = DisplayObjectLimit.Minimum;
}
