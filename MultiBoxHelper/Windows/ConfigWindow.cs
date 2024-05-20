using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using World = Lumina.Excel.GeneratedSheets.World;

namespace MultiBoxHelper.Windows;

public class ConfigWindow : Window, IDisposable
{
    //private Configuration configuration;
    private readonly Plugin plugin;
    private Vector2 iconButtonSize = new(16);

    //private string world = string.Empty;
    //private string clone = string.Empty;

    private string selectedCharacter = string.Empty;
    private uint selectedWorld = 0;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Multibox Helper Configuration###MultiBoxHelperConfiguration")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

        //configuration = plugin.Configuration;
        this.plugin = plugin;
    }

    public void Dispose()
    {
    }


    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 300),
            MaximumSize = new Vector2(800, 600)
        };
    }

    public override void Draw()
    {
        // Character list
        ImGui.BeginGroup();
        {
            if (ImGui.BeginChild("clone_list", ImGuiHelpers.ScaledVector2(240, 300) - iconButtonSize with { X = 0 }, true))
            {
                DrawCharacterList();
            }
            ImGui.EndChild();

            // Need to add some buttons below
            if (ImGuiComponents.IconButton(FontAwesomeIcon.User))
            {
                // Add current player character
                if (Service.ClientState != null && Service.ClientState.LocalPlayer != null)
                {
                    plugin.Configuration.AddClone(Service.ClientState.LocalPlayer);
                }
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Add current character");
            }
            ImGui.SameLine();

            if (ImGuiComponents.IconButton(FontAwesomeIcon.DotCircle))
            {
                // add target character
                if (Service.Targets.Target is PlayerCharacter pc)
                {
                    plugin.Configuration.AddClone(pc);
                }
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Add current target");
            }
            ImGui.SameLine();

            if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash))
            {
                // TODO: Delete character from list
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Delete clone from list");
            }
            iconButtonSize = ImGui.GetItemRectSize() + ImGui.GetStyle().ItemSpacing;

        }
        ImGui.EndGroup();

        // Keep this on the same line, so that the character list is a bar on the left
        ImGui.SameLine();

        ImGui.BeginGroup();
        // Show tabs for configuration options
        ImGuiTabBarFlags tabBarFlags = ImGuiTabBarFlags.None;
        if (ImGui.BeginTabBar("SettingsTabBar", tabBarFlags))
        {
            if (ImGui.BeginTabItem("Default"))
            {
                ImGui.Text("Default settings for when it's not specified otherwise.");
                ImGui.Checkbox("Mute all sound###DefaultMuteSound", ref plugin.Configuration.DefaultMuteSound);
                ImGui.Checkbox("Disable Penumbra###DefaultDisablePenumbra", ref plugin.Configuration.DefaultDisablePenumbra);
                ImGui.Checkbox("Use low graphics mode###efaultLowGraphicsMode", ref plugin.Configuration.DefaultLowGraphicsMode);

                ImGui.NewLine();
                ImGui.Text("Visible character limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###DefaultObjectLimit", ref plugin.Configuration.DefaultObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");


                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Bard Mode"))
            {
                ImGui.Text("Settings for when Bard Mode is toggled.");
                ImGui.Checkbox("Mute all sound###BardMuteSound", ref plugin.Configuration.BardMuteSound);
                ImGui.Checkbox("Disable Penumbra###BardDisablePenumbra", ref plugin.Configuration.BardDisablePenumbra);
                ImGui.Checkbox("Use low graphics mode###efaultLowGraphicsMode", ref plugin.Configuration.BardLowGraphicsMode);

                ImGui.NewLine();
                ImGui.Text("Visible character limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###BardObjectLimit", ref plugin.Configuration.BardObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Clone Settings"))
            {
                ImGui.Text("Settings for all clones.");
                ImGui.Checkbox("Mute all sound###CloneMuteSound", ref plugin.Configuration.CloneMuteSound);
                ImGui.Checkbox("Disable Penumbra###CloneDisablePenumbra", ref plugin.Configuration.CloneDisablePenumbra);
                ImGui.Checkbox("Use low graphics mode###efaultLowGraphicsMode", ref plugin.Configuration.CloneLowGraphicsMode);

                ImGui.NewLine();
                ImGui.Text("Visible character limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###CloneObjectLimit", ref plugin.Configuration.CloneObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.SameLine(200);
        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Save, "Save Changes"))
        {
            plugin.Configuration.Save();
            IsOpen = false;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Save and Close");
        }

        ImGui.EndGroup();
    }

    private void DrawCharacterList()
    {
        ImGui.Text("Clone List");
        ImGui.PushItemWidth(225);
        ImGui.BeginListBox(string.Empty, new Vector2(225, 235));
        
        foreach (var (worldId, list) in plugin.Configuration.CloneList)
        {
            var world = Service.Data.GetExcelSheet<World>()?.GetRow(worldId);
            if (world == null) continue;

            ImGui.TextDisabled(world.Name.RawString);
            ImGui.Separator();

            foreach (var clone in list.ToArray())
            {
                if (ImGui.Selectable($"{clone}##{world.Name.RawString}", (selectedCharacter == clone && selectedWorld == worldId)))
                {
                    selectedCharacter = clone;
                    selectedWorld = worldId;
                }
                
                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Selectable($"Remove '{clone} @ {world.Name.RawString}'"))
                    {
                        plugin.Configuration.RemoveClone(worldId, clone);
                        selectedCharacter = string.Empty;
                        selectedWorld = 0;
                    }
                    ImGui.EndPopup();
                }
            }
        }
        ImGui.EndListBox();

    }
}
