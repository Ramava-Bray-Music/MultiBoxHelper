using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace MultiBoxHelper;
internal class LoginHandler : IDisposable
{
    private readonly Queue<Func<bool>> actionQueue = new();
    private readonly Dictionary<uint, Dictionary<uint, List<string>>> characters = [];

    private Queue<uint> datacenters = [];
    private Queue<uint> worlds = [];
    private uint currentDatacenter;
    private uint currentWorld;

    private Plugin plugin { get; init; }

    public static bool IsLoggedIn
    {
        get
        {
            if (Service.Condition.Any())
            {
                return true;
            }
            return false;
        }
    }

    public LoginHandler(Plugin plugin)
    {
        this.plugin = plugin;
        if (Service.Framework != null)
        {
            Service.Framework.Update += OnFrameworkUpdate;
        }

        if (Plugin.Configuration.AutoLogin)
        {
            Service.Log.Debug("AutoLogin enabled. Starting process.");
            BuildLists();
            InitiateAutoLogin();
        }
    }

    private void InitiateAutoLogin()
    {
        if (actionQueue == null) { return; }

        var autoLoginAnnouncement = new Notification() with
        {
            Content = "Starting AutoLogin Process.\nPress and hold shift to cancel.",
            Title = "Auto Login",
            InitialDuration = new TimeSpan(0, 0, Plugin.Configuration.AutoLoginDelay),
            Type = NotificationType.Info

        };

        //Service.Notifications.AddNotification(autoLoginAnnouncement);
        QueueDatacenter();

#if DEBUG
        //unsafe
        //{
        //    // Make sure we're on the title menu first.
        //    var titleMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("_TitleMenu", 1);
        //    if (titleMenu != null && titleMenu->IsVisible)
        //    {
        //        Service.Log.Debug("Already on title menu, forcing auto login process to start.");
        //        sw.Reset();
        //        if (actionQueue.Count > 0)
        //        {
        //            //actionQueue.Dequeue();
        //        }
        //    }
        //}
#endif
    }

    private void QueueDatacenter()
    {
        Service.Log.Debug("QueueDatacenter()");
                actionQueue.Enqueue(VariableDelay(10));

        actionQueue.Enqueue(OpenDataCenterMenu);
        actionQueue.Enqueue(VariableDelay(10));

        actionQueue.Enqueue(SelectDataCenter);
    }

    private bool NextWorld()
    {
        Service.Log.Debug("NextWorld()");
        actionQueue.Enqueue(VariableDelay(10));

        actionQueue.Enqueue(SelectWorld);
        return false;
    }

    private void QueueCharacters()
    {
        Service.Log.Debug("QueueCharacters()");

        // Not sure why this delay is there. Was 10
        actionQueue.Enqueue(VariableDelay(20));
        actionQueue.Enqueue(SelectCharacter);
    }

    private void QueueSelectYes()
    {
        Service.Log.Debug("QueueSelectYes()");
        actionQueue.Enqueue(VariableDelay(10));

        actionQueue.Enqueue(SelectYes);
    }

    private void BuildLists()
    {
        var worldSheet = Service.Data.Excel.GetSheet<World>();
        if (worldSheet == null)
        {
            return;
        }

        foreach (var worldId in Plugin.Configuration.CloneCharacterList.Keys)
        {
            var world = worldSheet.GetRow(worldId);
            if (world == null)
            {
                Service.Log.Debug($"Couldn't find world data for {worldId}");
                continue;
            }

            if (world.DataCenter == null)
            {
                Service.Log.Debug($"{worldId} didn't have a datacenter?");
                continue;
            }

            var dc = world.DataCenter.Value;
            if (dc == null)
            {
                continue;
            }

            if (!characters.TryGetValue(dc.RowId, out var list))
            {
                list = [];
                characters[dc.RowId] = list;
            }

            list.Add(worldId, Plugin.Configuration.CloneCharacterList[worldId]);
        }

        foreach (var dc in characters.Keys)
        {
            datacenters.Enqueue(dc);
        }

        Service.Log.Debug($"Have {datacenters.Count} datacenters. Currently have {worlds.Count} worlds.");
    }

    private void BuildWorldQueue(uint dc)
    {
        worlds.Clear();

        var list = characters[dc];
        if (list == null)
        {
            return;
        }

        foreach (var world in list.Keys)
        {
            worlds.Enqueue(world);
        }

        Service.Log.Debug($"have {worlds.Count} worlds for this datacenter");
    }

    // This seems like a bad idea to do this way?
    private readonly Stopwatch sw = new();
    // Frame based delay timing
    private uint Delay = 0;

    public void Dispose()
    {
    }

    private Func<bool> VariableDelay(uint frameDelay)
    {
        return () =>
        {
            Delay = frameDelay;
            return true;
        };
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (actionQueue == null) { return; }

        if (actionQueue.Count == 0)
        {
            if (sw.IsRunning)
            {
                sw.Stop();
                return;
            }
        }
        if (!sw.IsRunning)
        {
            sw.Restart();
        }

        // Cancel if the user is holding shift
        if (Service.KeyState[VirtualKey.SHIFT])
        {
            var autoLoginAnnouncement = new Notification() with
            {
                Content = "AutoLogin cancelled.",
                Title = "Auto Login",
                Type = NotificationType.Warning

            };

            Service.Notifications.AddNotification(autoLoginAnnouncement);
            actionQueue.Clear();
        }

        if (Delay > 0)
        {
            Delay -= 1;
            return;
        }

        // This seems to be a delay between steps, in case one of them is taking too long?
        // Made this 20 seconds instead of 10 for now. I have a feeling maybe?
        if (sw.ElapsedMilliseconds > 20000)
        {
            Service.Log.Debug("Over 20s time limit. Aborting.");
            actionQueue.Clear();
            return;
        }

        try
        {
            if (actionQueue.TryPeek(out var next))
            {
                if (next())
                {
                    actionQueue.Dequeue();
                    sw.Reset();
                } else
                {
                    Service.Log.Debug("Last action failed and we're stopping?");
                }
            }
        }
        catch (Exception ex)
        {
            Service.Log.Error($"Failed: {ex.Message}");
        }
    }

    public bool OpenDataCenterMenu()
    {
        unsafe
        {
            // Make sure we're on the title menu first.
            var titleMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("_TitleMenu", 1);
            if (titleMenu == null || titleMenu->IsVisible == false)
            {
                Service.Log.Debug("Not on title menu");
                return false;
            }
            Service.Log.Debug("Open DC Menu");

            GenerateCallback(titleMenu, 12);
            // Make sure we can get to the DC worldmap menu
            var dcMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("TitleDCWorldMap", 1);
            if (dcMenu == null)
            {
                //Service.Log.Debug("Not on DC menu");
                return false;
            }
            return true;
        }
    }

    public bool SelectDataCenter()
    {
        Service.Log.Debug($"SelectDataCenter: Have {datacenters.Count} to check");
        if (!datacenters.TryDequeue(out currentDatacenter))
        {
            Service.Log.Debug($"No more datacenters to try");

            // no more datacenters to try
            actionQueue.Clear();
            return false;
        }

        Service.Log.Debug($"Trying datacenter {currentDatacenter}");
        BuildWorldQueue(currentDatacenter);

        unsafe
        {
            var dcMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("TitleDCWorldMap", 1);
            if (dcMenu == null)
            {
                Service.Log.Debug("No DC menu in SelectDataCenter");
                return false;
            }

            GenerateCallback(dcMenu, 2, currentDatacenter);
            NextWorld();
            return true;
        }
    }


    public bool SelectWorld()
    {
        if (!worlds.TryDequeue(out currentWorld))
        {
            Service.Log.Debug($"Out of worlds, trying more datacenters");
            // no more worlds to try do we have any dcs to try?
            actionQueue.Clear();
            if (datacenters.Count > 0)
            {
                QueueDatacenter();
            }
            return false;
        }

        unsafe
        {
            // Hide the map if it's visible first
            var dcMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("TitleDCWorldMap", 1);
            if (dcMenu != null)
            {
                Service.Log.Debug("Hide DC Menu");
                dcMenu->IsVisible = false;
            }


            var worldMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("_CharaSelectWorldServer", 1);
            if (worldMenu == null)
            {
                Service.Log.Debug($"worldMenu for world id {currentWorld} was null");
                return NextWorld();
            }

            var stringArray = Framework.Instance()->UIModule->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder.StringArrays[1];
            if (stringArray == null)
            {
                Service.Log.Debug($"stringArray for world id {currentWorld} was null");
                return NextWorld();
            }

            var world = Service.Data.Excel.GetSheet<World>()?.GetRow(currentWorld);
            if (world == null || !world.IsPublic)
            {
                Service.Log.Debug($"world data for world id {currentWorld} was null or world was not public");
                return NextWorld();
            }

            var checkedWorldCount = 0;

            for (var i = 0; i < 16; i++)
            {
                if (stringArray->StringArray[i] == null)
                {
                    continue;
                }

                var worldName = MemoryHelper.ReadStringNullTerminated(new IntPtr(stringArray->StringArray[i])).Trim();
                if (worldName.Length == 0)
                {
                    continue;
                }
                checkedWorldCount++;
                if (worldName != world.Name.RawString)
                {
                    continue;
                }

                GenerateCallback(worldMenu, 9, 0, i);
                QueueCharacters();
                return true;
            }

            Service.Log.Debug($"Nothing found for world {currentWorld}, trying the next one.");
            return NextWorld();
        }
    }

    public bool SelectCharacter()
    {
        unsafe
        {
            // Select Character
            var characterListMenu = (AtkUnitBase*)Service.GameGui.GetAddonByName("_CharaSelectListMenu", 1);
            if (characterListMenu == null)
            {
                Service.Log.Debug($"No character list menu");

                return NextWorld();
            }

            // TODO: have to rewrite this whole thing to check names.
            //GenerateCallback(characterListMenu, 17, 0, tempCharacter ?? PluginConfig.CharacterSlot);

            var stringArray = Framework.Instance()->UIModule->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder.StringArrays[1];
            if (stringArray == null)
            {
                Service.Log.Debug($"No string array for characters");

                return NextWorld();
            }

            // Should probably try more than 16, but shouldn't matter for clone accounts
            for (var i = 0; i < 16; i++)
            {
                if (stringArray->StringArray[i] == null)
                {
                    continue;
                }

                var characterName = MemoryHelper.ReadStringNullTerminated(new IntPtr(stringArray->StringArray[i])).Trim();
                if (characterName.Length == 0)
                {
                    continue;
                }

                // validate against our list
                if (Plugin.Configuration.CloneCharacterList[currentWorld].Contains(characterName))
                {
                    GenerateCallback(characterListMenu, 17, 0, i);
                    QueueSelectYes();
                    return true;
                }
            }

            Service.Log.Debug($"Didn't find a character in {currentWorld}, trying the next");
            return NextWorld();
        }
    }

    public bool SelectYes()
    {
        unsafe
        {
            var dialog = (AtkUnitBase*)Service.GameGui.GetAddonByName("SelectYesno", 1);
            if (dialog == null)
            {
                return false;
            }

            // Yes is 0 and 1 is no? Probably the same for other dialogs, just button order.
            GenerateCallback(dialog, 0);
            dialog->IsVisible = false;

            return true;
        }
    }

    public bool Logout()
    {
        if (!IsLoggedIn)
        {
            return false;
        }

        unsafe
        {
            // Run the actual logout command
            Framework.Instance()->GetUiModule()->ExecuteMainCommand(23);
            return true;
        }
    }

    // Don't delete this yet. May have a use for the debug menu it's showing.
    /*
    private void DrawUI()
    {
        drawConfigWindow = drawConfigWindow && PluginConfig.DrawConfigUI();
#if DEBUG
        if (!drawDebugWindow) return;
        if (ImGui.Begin($"{this.Name} Debugging", ref drawDebugWindow))
        {
            if (ImGui.Button("Open Config")) drawConfigWindow = true;
            if (ImGui.Button("Clear Queue"))
            {
                actionQueue.Clear();
            }

            if (ImGui.Button("Test Step: Open Data Centre Menu")) actionQueue.Enqueue(OpenDataCenterMenu);
            if (ImGui.Button($"Test Step: Select Data Center [{PluginConfig.DataCenter}]")) actionQueue.Enqueue(SelectDataCentre);

            if (ImGui.Button($"Test Step: SELECT WORLD [{PluginConfig.World}]"))
            {
                actionQueue.Clear();
                actionQueue.Enqueue(SelectWorld);
            }

            if (ImGui.Button($"Test Step: SELECT CHARACTER [{PluginConfig.CharacterSlot}]"))
            {
                actionQueue.Clear();
                actionQueue.Enqueue(SelectCharacter);
            }

            if (ImGui.Button("Test Step: SELECT YES"))
            {
                actionQueue.Clear();
                actionQueue.Enqueue(SelectYes);
            }

            if (ImGui.Button("Logout"))
            {
                actionQueue.Clear();
                actionQueue.Enqueue(Logout);
                actionQueue.Enqueue(SelectYes);
                actionQueue.Enqueue(Delay5s);
            }



            if (ImGui.Button("Swap Character"))
            {
                tempDc = 9;
                tempWorld = 87;
                tempCharacter = 0;

                actionQueue.Enqueue(Logout);
                actionQueue.Enqueue(SelectYes);
                actionQueue.Enqueue(OpenDataCenterMenu);
                actionQueue.Enqueue(SelectDataCentre);
                actionQueue.Enqueue(SelectWorld);
                actionQueue.Enqueue(SelectCharacter);
                actionQueue.Enqueue(SelectYes);
                actionQueue.Enqueue(Delay5s);
                actionQueue.Enqueue(ClearTemp);
            }

            if (ImGui.Button("Full Run"))
            {
                actionQueue.Clear();
                actionQueue.Enqueue(OpenDataCenterMenu);
                actionQueue.Enqueue(SelectDataCentre);
                actionQueue.Enqueue(SelectWorld);
                actionQueue.Enqueue(SelectCharacter);
                actionQueue.Enqueue(SelectYes);
            }

            ImGui.Text("Current Queue:");
            foreach (var l in actionQueue.ToList())
            {
                ImGui.Text($"{l.Method.Name}");
            }
        }
        ImGui.End();
#endif
    }
    */

    public unsafe static void GenerateCallback(AtkUnitBase* unitBase, params object[] values)
    {
        if (unitBase == null)
        {
            Service.Log.Debug("Null unitbase");
            throw new Exception("Null UnitBase");
        }

        var atkValues = (AtkValue*)Marshal.AllocHGlobal(values.Length * sizeof(AtkValue));
        if (atkValues == null)
        {
            Service.Log.Debug("atkvalues null");
            return;
        }
        
        try
        {
            for (var i = 0; i < values.Length; i++)
            {
                var v = values[i];
                switch (v)
                {
                    case uint uintValue:
                        atkValues[i].Type = ValueType.UInt;
                        atkValues[i].UInt = uintValue;
                        break;
                    case int intValue:
                        atkValues[i].Type = ValueType.Int;
                        atkValues[i].Int = intValue;
                        break;
                    case float floatValue:
                        atkValues[i].Type = ValueType.Float;
                        atkValues[i].Float = floatValue;
                        break;
                    case bool boolValue:
                        atkValues[i].Type = ValueType.Bool;
                        atkValues[i].Byte = (byte)(boolValue ? 1 : 0);
                        break;
                    case string stringValue:
                        {
                            atkValues[i].Type = ValueType.String;
                            var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                            var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length + 1);
                            Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length);
                            Marshal.WriteByte(stringAlloc, stringBytes.Length, 0);
                            atkValues[i].String = (byte*)stringAlloc;
                            break;
                        }
                    default:
                        throw new ArgumentException($"Unable to convert type {v.GetType()} to AtkValue");
                }
            }
            unitBase->FireCallback(values.Length, atkValues);
        }
        finally
        {
            for (var i = 0; i < values.Length; i++)
            {
                if (atkValues[i].Type == ValueType.String)
                {
                    Marshal.FreeHGlobal(new IntPtr(atkValues[i].String));
                }
            }
            Marshal.FreeHGlobal(new IntPtr(atkValues));
        }
    }
}

