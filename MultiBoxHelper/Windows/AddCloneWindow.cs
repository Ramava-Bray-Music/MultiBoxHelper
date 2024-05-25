using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBoxHelper.Windows;
public class AddCloneWindow : Window, IDisposable
{
    //private Configuration configuration;
    private readonly Plugin plugin;

    public AddCloneWindow(Plugin plugin) : base("Add a Clone Character###MBH - Add a Clone Window")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize
            | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.Popup | ImGuiWindowFlags.Modal;

        this.plugin = plugin;

       //this.Size = ImGuiHelpers.ScaledVector2(580, 500);
    }

    public override void Draw()
    {
        ImGui.BeginGroup();

        if (ImGui.Button("Close"))
        {
            IsOpen = false;
        }

        ImGui.EndGroup();
    }
    public void Dispose()
    {

    }
}
