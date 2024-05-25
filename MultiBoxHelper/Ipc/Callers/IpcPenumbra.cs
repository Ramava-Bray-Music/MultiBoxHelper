using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using Microsoft.Extensions.Logging;
using Penumbra.Api.IpcSubscribers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBoxHelper.Ipc.Callers;
public sealed class IpcPenumbra : IIpcCaller
{
    //private ILogger Logger { get; init; }
    private readonly ApiVersion penumbraApiVersion;
    private readonly GetEnabledState penumbraEnabled;

    public bool APIAvailable { get; private set; }

    public IpcPenumbra()
    {
        //Logger = logger;
        penumbraApiVersion = new(Service.PluginInterface);
        penumbraEnabled = new(Service.PluginInterface);

        CheckAPI();
    }

    public void CheckAPI()
    {
        bool penumbraAvailable = false;
        try
        {
            var penumbraVersion = (Service.PluginInterface.InstalledPlugins
                .FirstOrDefault(p => string.Equals(p.InternalName, "Penumbra", StringComparison.OrdinalIgnoreCase))
                ?.Version ?? new Version(0, 0, 0, 0));
            penumbraAvailable = penumbraVersion >= new Version(1, 0, 1, 0);

            penumbraAvailable &= penumbraEnabled.Invoke();
            APIAvailable = penumbraAvailable;
        }
        catch
        {
            APIAvailable = penumbraAvailable;
        }
    }
}
