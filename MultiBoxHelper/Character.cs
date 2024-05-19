using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBoxHelper;

[Serializable]
public class Character
{
    public string Name { get; set; } = string.Empty;
    public string HomeWorld { get; set; } = string.Empty;

    public string Id => string.Format("{0}:{1}", HomeWorld, Name);
}
