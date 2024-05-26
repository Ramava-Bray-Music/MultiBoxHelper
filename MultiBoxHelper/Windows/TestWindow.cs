using Dalamud.Hooking;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace MultiBoxHelper.Windows;
public class TestWindow : Window, IDisposable
{
    //private Configuration configuration;
    private readonly Plugin plugin;
    public delegate void TitleMenuDelegate(int a1);
    public Hook<TitleMenuDelegate>? TitleMenuHook;

    public void TitleMenu(int a1)
    {
        Service.Log.Debug($"TitleMenu {a1}");
    }

    public TestWindow(Plugin plugin) : base("Debugging Tests")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

        this.plugin = plugin;

        //this.Size = ImGuiHelpers.ScaledVector2(580, 500); 

    }

    public override void Draw()
    {
        ImGui.BeginGroup();

        if (ImGui.Button("Open World Menu"))
        {
            try
            {
               
                if (!OpenDataCenterMenu())
                {
                    Service.Log.Debug("Failed to open DC Menu");
                }
            }
            catch (Exception e)
            {
                Service.Log.Debug("EXCEPTION! " + e.Message);
            }
        }

        ImGui.EndGroup();
    }
    public void Dispose()
    {



    }

    public unsafe bool OpenDataCenterMenu()
    {
        var addon = (AtkUnitBase*)Service.GameGui.GetAddonByName("_TitleMenu", 1);
        Service.Log.Debug($"Handle: {addon->ID}");
        if (addon == null || addon->IsVisible == false) return false;
        Service.Log.Debug("Trying menu command");
        GenerateCallback(addon, 12);
        var nextAddon = (AtkUnitBase*)Service.GameGui.GetAddonByName("TitleDCWorldMap", 1);
        if (nextAddon == null) return false;
        return true;
    }

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
