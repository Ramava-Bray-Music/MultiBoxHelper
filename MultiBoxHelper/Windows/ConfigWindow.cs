using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using MultiBoxHelper.Settings;
using System;
using System.Numerics;
using World = Lumina.Excel.GeneratedSheets.World;

namespace MultiBoxHelper.Windows;

/// <summary>
/// Configuration UI
/// </summary>
public class ConfigWindow : Window, IDisposable
{
    //private Configuration configuration;
    private readonly Plugin plugin;
    private readonly Configuration config;
    private Vector2 iconButtonSize = new(16);

    // selected entry in clone list
    private string selectedClone = string.Empty;
    private uint selectedWorld = 0;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Multibox Helper Settings###Multibox Helper Settings")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

        //configuration = config;
        this.plugin = plugin;
        this.config = plugin.Configuration;
    }

    public void Dispose()
    {
       
    }

    /// <summary>
    /// Draw the window
    /// </summary>
    public override void Draw()
    {
        // Character list
        ImGui.BeginGroup();
        {
            if (ImGui.BeginChild("clone_list", ImGuiHelpers.ScaledVector2(240, 300) - iconButtonSize with
            {
                X = 0
            }, true))
            {
                DrawCharacterList();
            }
            ImGui.EndChild();

            // Need to add some buttons below

            // Add current player character
            if (ImGuiComponents.IconButton(FontAwesomeIcon.User))
            {
                if (Service.ClientState != null)
                {
                    AddClone(Service.ClientState.LocalPlayer);
                }
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Add current character");
            }
            ImGui.SameLine();

            // add target character
            if (ImGuiComponents.IconButton(FontAwesomeIcon.DotCircle))
            {
                AddClone(Service.Targets.Target);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Add current target");
            }
            ImGui.SameLine();

            if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash))
            {
                // Delete character from list
                RemoveClone(selectedWorld, selectedClone);
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
                ImGui.Checkbox("Mute all sound###DefaultMuteSound", ref config[Mode.Default].MuteSound);
                ImGui.Checkbox("Disable Penumbra###DefaultDisablePenumbra", ref config[Mode.Default].DisablePenumbra);
                ImGui.Checkbox("Use low graphics mode###DefaultLowGraphicsMode", ref config[Mode.Default].LowGraphicsMode);

                ImGui.NewLine();
                ImGui.Text("Visible character limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###DefaultObjectLimit", ref config[Mode.Default].ObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.NewLine();
                ImGui.Text("FPS limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###DefaultFps", ref config[Mode.Default].Fps, "None\0Full refresh rate\01/2 of refresh rate\01/4 of refresh rate\0\0");
                ImGui.Checkbox("Reduce FPS while window is inactive###DefaultFpsDownInactive", ref config[Mode.Default].FpsDownInactive);
                ImGui.Checkbox("Reduce FPS while AFK###DefaultFpsDownAfk", ref config[Mode.Default].FpsDownAFK);

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Bard Mode"))
            {
                ImGui.Text("Settings for when Bard Mode is toggled.");
                ImGui.Checkbox("Mute all sound###BardMuteSound", ref config[Mode.Bard].MuteSound);
                ImGui.Checkbox("Disable Penumbra###BardDisablePenumbra", ref config[Mode.Bard].DisablePenumbra);
                ImGui.Checkbox("Use low graphics mode###efaultLowGraphicsMode", ref config[Mode.Bard].LowGraphicsMode);

                ImGui.NewLine();
                ImGui.Text("Visible character limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###BardObjectLimit", ref config[Mode.Bard].ObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.NewLine();
                ImGui.Text("FPS limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###BardFps", ref config[Mode.Bard].Fps, "None\0Full refresh rate\01/2 of refresh rate\01/4 of refresh rate\0\0");
                ImGui.Checkbox("Reduce FPS while window is inactive###BardFpsDownInactive", ref config[Mode.Bard].FpsDownInactive);
                ImGui.Checkbox("Reduce FPS while AFK###BardFpsDownAfk", ref config[Mode.Bard].FpsDownAFK);

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Clone Settings"))
            {
                ImGui.Text("Settings for all clones.");
                ImGui.Checkbox("Mute all sound###CloneMuteSound", ref config[Mode.Clone].MuteSound);
                ImGui.Checkbox("Disable Penumbra###CloneDisablePenumbra", ref config[Mode.Clone].DisablePenumbra);
                ImGui.Checkbox("Use low graphics mode###efaultLowGraphicsMode", ref config[Mode.Clone].LowGraphicsMode);

                ImGui.NewLine();
                ImGui.Text("Visible character limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###CloneObjectLimit", ref config[Mode.Clone].ObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.NewLine();
                ImGui.Text("FPS limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###CloneFps", ref config[Mode.Clone].Fps, "None\0Full refresh rate\01/2 of refresh rate\01/4 of refresh rate\0\0");
                ImGui.Checkbox("Reduce FPS while window is inactive###CloneFpsDownInactive", ref config[Mode.Clone].FpsDownInactive);
                ImGui.Checkbox("Reduce FPS while AFK###CloneFpsDownAfk", ref config[Mode.Clone].FpsDownAFK);

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
            config.Save();
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

        foreach (var (worldId, list) in config.CloneCharacterList)
        {
            var world = Service.Data.GetExcelSheet<World>()?.GetRow(worldId);
            if (world == null)
                continue;

            ImGui.TextDisabled(world.Name.RawString);
            ImGui.Separator();

            foreach (var clone in list.ToArray())
            {
                if (ImGui.Selectable($"{clone}##{world.Name.RawString}", (selectedClone == clone && selectedWorld == worldId)))
                {
                    selectedClone = clone;
                    selectedWorld = worldId;
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Selectable($"Remove '{clone} @ {world.Name.RawString}'"))
                    {
                        RemoveClone(worldId, clone);
                    }
                    ImGui.EndPopup();
                }
            }
        }
        ImGui.EndListBox();

    }

    /// <summary>
    /// Remove clone from configuration
    /// </summary>
    /// <param name="worldId">world identifier</param>
    /// <param name="clone">character name</param>
    private void RemoveClone(uint worldId, string clone)
    {
        if (plugin != null)
        {
            config.RemoveClone(worldId, clone);
            selectedClone = string.Empty;
            selectedWorld = 0;
        }
    }

    /// <summary>
    /// Add a clone to the list from a game object
    /// </summary>
    /// <param name="pc">player character object</param>
    private void AddClone(GameObject? clone)
    {
        if (clone != null && clone is PlayerCharacter pc)
        {
            config.AddClone(pc);
            selectedClone = pc.Name.TextValue;
            selectedWorld = pc.HomeWorld.Id;
        }
    }
}
