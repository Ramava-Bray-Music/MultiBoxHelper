using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MultiBoxHelper.Settings;

[Serializable]
public class GraphicsConfiguration : Dictionary<string, uint>
{
    public GraphicsConfiguration()
    {
    }
}
