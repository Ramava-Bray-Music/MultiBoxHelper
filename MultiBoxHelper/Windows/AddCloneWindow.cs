using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MultiBoxHelper.Windows
{
    public class AddCloneWindow : Window, IDisposable
    {
        private Plugin plugin;

        private string world = string.Empty;
        private string name = string.Empty;

        // We give this window a constant ID using ###
        // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
        // and the window ID will always be "###XYZ counter window" for ImGui
        public AddCloneWindow(Plugin plugin) : base("Add a clone character###MultiBoxHelperAddCloneWindow")
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.Popup | ImGuiWindowFlags.Modal;

            this.plugin = plugin;
        }
        public void Dispose() { }

        public void Reset()
        {
            name = string.Empty;
            world = string.Empty;
        }

        public override void PreDraw()
        {
        }

        public override void Draw()
        {
            
            //Service.LogPosition("add window", Position);

            // Always center this window when appearing
            //var center = ImGui.GetWindowViewport().GetCenter();
            //this.Position = center;

            ImGui.BeginGroup();
            ImGui.InputText("World Name", ref world, 45);
            ImGui.InputText("Character Name", ref name, 45);

            var buttonSize = new Vector2(120, 0);
            if (ImGui.Button("Add", buttonSize))
            {
                Service.Log.Debug(string.Format("Should be adding: {0}:{1}", world, name));
                IsOpen = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", buttonSize))
            {
                IsOpen = false;
            }
            ImGui.SetItemDefaultFocus();
            ImGui.EndGroup();
        }


    }
}
