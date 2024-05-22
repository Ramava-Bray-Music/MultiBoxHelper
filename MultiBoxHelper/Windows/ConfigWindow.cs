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

        this.Size = ImGuiHelpers.ScaledVector2(580, 480);
        /*this.SizeConstraints = new WindowSizeConstraints()
        {
            MaximumSize = new Vector2(580, 600),
            MinimumSize = new Vector2(550, 500)
        };*/
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
            if (ImGui.BeginChild("clone_list", ImGuiHelpers.ScaledVector2(240, -30), true))
            {
                DrawCharacterList();
            }
            ImGui.EndChild();

            // Need to add some buttons below
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.SameLine(20);
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
        if (ImGui.BeginChild("tabs", ImGuiHelpers.ScaledVector2(350, -30) , false))
        {
            DrawTabs();
        }
        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Spacing();
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
#if DEBUG
        //ImGui.BeginGroup();

        //ImGui.Checkbox("Log changes", ref plugin.watchConfigChanges);
        //ImGui.Combo("", ref plugin.watchingSettingsType, "Maximum\0High (Desktop)\0High (Laptop)\0Standard (Desktop)\0Standard (Laptop)\0\0");

        //if (
        //ImGui.Button("Dump Log"))
        //{
        //    plugin.DumpLog();
        //}

        //ImGui.EndGroup();
#endif
    }

    private void DrawTabs()
    {

        // Show tabs for configuration options
        ImGuiTabBarFlags tabBarFlags = ImGuiTabBarFlags.None;
        if (ImGui.BeginTabBar("SettingsTabBar", tabBarFlags))
        {
            if (ImGui.BeginTabItem("Default"))
            {
                ImGui.TextWrapped("Default settings for when it's not specified otherwise.");
                ImGui.Spacing();
                ImGui.Checkbox("Mute all sound###DefaultMuteSound", ref config[Mode.Default].MuteSound);
                ImGui.Checkbox("Disable Penumbra###DefaultDisablePenumbra", ref config[Mode.Default].DisablePenumbra);

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

                ImGui.NewLine();
                ImGui.Checkbox("Change graphics settings###DefaultChangeGraphicsMode", ref config[Mode.Default].ChangeGraphicsMode);
                if (ImGui.Button("Save Current"))
                {
                    plugin.Configuration.SaveGraphicsSettings(Mode.Default);
                }
                ImGui.SameLine();
                if (
                ImGui.Button("Reset to [Maximum]"))
                {
                    plugin.Configuration.ResetGraphicsSettings(Mode.Default);
                }


                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Bard Mode"))
            {
                ImGui.TextWrapped("Settings for when Bard Mode is toggled.");
                ImGui.Spacing();

                ImGui.Checkbox("Mute all sound###BardMuteSound", ref config[Mode.Bard].MuteSound);
                ImGui.Checkbox("Disable Penumbra###BardDisablePenumbra", ref config[Mode.Bard].DisablePenumbra);

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

                ImGui.NewLine();
                ImGui.Checkbox("Change graphics settings###BardChangeGraphicsMode", ref config[Mode.Bard].ChangeGraphicsMode);
                if (ImGui.Button("Save Current"))
                {
                    plugin.Configuration.SaveGraphicsSettings(Mode.Bard);
                }
                ImGui.SameLine();
                if (
                ImGui.Button("Reset to [extra low]"))
                {
                    plugin.Configuration.ResetGraphicsSettings(Mode.Bard);
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Clone Settings"))
            {
                ImGui.TextWrapped("Settings for all clones.");
                ImGui.Spacing();

                ImGui.Checkbox("Mute all sound###CloneMuteSound", ref config[Mode.Clone].MuteSound);
                ImGui.Checkbox("Disable Penumbra###CloneDisablePenumbra", ref config[Mode.Clone].DisablePenumbra);

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

                ImGui.NewLine();
                ImGui.Checkbox("Change graphics settings###CloneChangeGraphicsMode", ref config[Mode.Clone].ChangeGraphicsMode);
                if (ImGui.Button("Save Current"))
                {
                    plugin.Configuration.SaveGraphicsSettings(Mode.Clone);
                }
                ImGui.SameLine();
                if (
                ImGui.Button("Reset to [extra low]"))
                {
                    plugin.Configuration.ResetGraphicsSettings(Mode.Clone);
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DrawCharacterList()
    {
        var space = ImGui.GetContentRegionAvail();
        ImGui.Text("Clone List");
        ImGui.PushItemWidth(0);
        ImGui.BeginListBox(string.Empty, new Vector2(space.X, space.Y - 20));

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
