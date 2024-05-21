using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBoxHelper.Settings;
public enum FpsLimit : int
{
    None = 0,
    RefreshRate = 1,
    HalfRefreshRate = 2,
    OneQuarterRefreshRate = 3
}
