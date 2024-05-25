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
    private readonly ApiVersion apiVersion;
    private readonly GetEnabledState getEnabledState;
    
    public bool IsAvailable { get; private set; }
    public bool IsEnabled { get; private set; }

    public IpcPenumbra()
    {
        //Logger = logger;
        apiVersion = new(Service.PluginInterface);
        getEnabledState = new(Service.PluginInterface);

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

            

            IsAvailable = penumbraAvailable;
            IsEnabled = penumbraAvailable && getEnabledState.Invoke();
        }
        catch
        {
            IsAvailable = penumbraAvailable;
            IsEnabled = penumbraAvailable;
        }
    }
}
