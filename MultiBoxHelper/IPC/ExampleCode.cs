using Dalamud.Plugin.Ipc.Exceptions;
using Dalamud.Plugin.Ipc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumina.Excel.GeneratedSheets2;

namespace MultiBoxHelper.Ipc;
public class Teleporter
{
    private readonly ICallGateSubscriber<uint, byte, bool> teleportIpc = Service.PluginInterface.GetIpcSubscriber<uint, byte, bool>("Teleport");
    private readonly ICallGateSubscriber<bool> showChatMessageIpc = Service.PluginInterface.GetIpcSubscriber<bool>("Teleport.ChatMessage");

    public void Teleport(Aetheryte aetheryte)
    {
        try
        {
            var didTeleport = teleportIpc.InvokeFunc(aetheryte.RowId, (byte)aetheryte.SubRowId);
            var showMessage = showChatMessageIpc.InvokeFunc();

            if (!didTeleport)
            {
                UserError("Cannot teleport in this situation.");
            }
            else if (showMessage)
            {
                Service.ChatGui.Print($"Teleporting to {aetheryte.PlaceName.Value?.Name ?? "Unable to read name"}.", "Teleport");
            }
        }
        catch (IpcNotReadyError)
        {
            Service.Log.Error("Teleport IPC not found");
            UserError("To use the teleport function, you must install the 'Teleporter' plugin");
        }
    }

    private static void UserError(string error)
    {
        Service.ChatGui.PrintError(error);
        Service.ToastGui.ShowError(error);
    }
}
