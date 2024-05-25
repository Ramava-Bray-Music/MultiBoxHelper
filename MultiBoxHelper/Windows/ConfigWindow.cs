using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using MultiBoxHelper.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    // TODO: See if we really need this. Probably should fix our size code to work with it.
    private Vector2 iconButtonSize = new(16);

    // selected entry in clone list
    private string selectedClone = string.Empty;
    private uint selectedWorld = 0;

    private bool showAddCloneModal = false;

    // For manually adding characters, probably need to rename them
    public uint selectedAddDataCenter = Plugin.Configuration.LastUsedDataCenter;
    public uint selectedAddWorld = Plugin.Configuration.LastUsedWorld;
    public string selectedAddCharacterName = string.Empty;

    private Dictionary<string, IDalamudTextureWrap?> images { get; set; } = [];

    private readonly List<string> iconNames = [@"bard", @"default", @"clone"];

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Multibox Helper Settings###Multibox Helper Settings")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

        //configuration = config;
        this.plugin = plugin;

        this.Size = ImGuiHelpers.ScaledVector2(580, 500);
        /*this.SizeConstraints = new WindowSizeConstraints()
        {
            MaximumSize = new Vector2(580, 600),
            MinimumSize = new Vector2(550, 500)
        };*/

        foreach (var name in iconNames)
        {
            // Load images for buttons
            var file = new FileInfo(Path.Combine(Service.PluginInterface.AssemblyLocation.Directory?.FullName!, $"images/{name}.png"));

            // ITextureProvider takes care of the image caching and dispose
            images[name] = Service.Textures.GetTextureFromFile(file);
            if (images[name] == null)
            {
                Service.Log.Debug($"Couldn't load image for {name}.");
            }
        }

    }

    public void Dispose()
    {

    }

    /// <summary>
    /// Draw the window
    /// </summary>
    public override void Draw()
    {
        DrawAddCloneModal();
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
            if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus))
            {
                if (Service.ClientState != null)
                {
                    showAddCloneModal = true;
                    //plugin.AddCloneWindow.Position = this.Position;
                    //plugin.AddCloneWindow.IsOpen = true;
                }
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Add new character");
            }
            ImGui.SameLine();

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
        if (ImGui.BeginChild("tabs", ImGuiHelpers.ScaledVector2(350, -90), false))
        {
            DrawTabs();
        }
        ImGui.EndChild();

        if (ImGui.BeginChild("modes", ImGuiHelpers.ScaledVector2(0, 60), false))
        {
            DrawModeButtons();
        }
        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.SameLine(200);
        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Save, "Save Changes"))
        {
            IsOpen = false;
            Plugin.Configuration.Save();
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

    private void DrawModeButtons()
    {
        ModeButton(Mode.Default);
        ImGui.SameLine();
        ModeButton(Mode.Bard);
        ImGui.SameLine();
        ModeButton(Mode.Clone);
    }

    private void ModeButton(Mode mode)
    {
        var name = mode.ToString().ToLower();
        var disabled = (plugin.CurrentMode == mode);
        if (images[name] != null)
        {
            var size = new Vector2(images[name]!.Width, images[name]!.Height);
            if (disabled)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.ImageButton(images[name]!.ImGuiHandle, size))
            {
                plugin.CurrentMode = mode;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"Switch to {mode} mode");
            }

            if (disabled)
            {
                ImGui.EndDisabled();
            }
        }
    }

    private void DrawTabs()
    {

        // Show tabs for configuration options
        ImGuiTabBarFlags tabBarFlags = ImGuiTabBarFlags.None;
        if (ImGui.BeginTabBar("SettingsTabBar", tabBarFlags))
        {
            if (ImGui.BeginTabItem("default"))
            {
                ImGui.TextWrapped("Default settings for when it's not specified otherwise.");
                ImGui.Spacing();
                ImGui.Checkbox("Mute all sound###DefaultMuteSound", ref Plugin.Configuration[Mode.Default].MuteSound);
                ImGui.Checkbox("Disable Penumbra###DefaultDisablePenumbra", ref Plugin.Configuration[Mode.Default].DisablePenumbra);

                ImGui.NewLine();
                ImGui.Text("Visible character limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###DefaultObjectLimit", ref Plugin.Configuration[Mode.Default].ObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.NewLine();
                ImGui.Text("FPS limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###DefaultFps", ref Plugin.Configuration[Mode.Default].Fps, "None\0Full refresh rate\01/2 of refresh rate\01/4 of refresh rate\0\0");
                ImGui.Checkbox("Reduce FPS while window is inactive###DefaultFpsDownInactive", ref Plugin.Configuration[Mode.Default].FpsDownInactive);
                ImGui.Checkbox("Reduce FPS while AFK###DefaultFpsDownAfk", ref Plugin.Configuration[Mode.Default].FpsDownAFK);

                ImGui.NewLine();
                ImGui.Checkbox("Change graphics settings###DefaultChangeGraphicsMode", ref Plugin.Configuration[Mode.Default].ChangeGraphicsMode);
                if (ImGui.Button("Save Current"))
                {
                    Plugin.Configuration.SaveGraphicsSettings(Mode.Default);
                }
                ImGui.SameLine();
                if (
                ImGui.Button("Reset to [Maximum]"))
                {
                    Plugin.Configuration.ResetGraphicsSettings(Mode.Default);
                }


                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Bard Mode"))
            {
                ImGui.TextWrapped("Settings for when Bard Mode is toggled.");
                ImGui.Spacing();

                ImGui.Checkbox("Mute all sound###BardMuteSound", ref Plugin.Configuration[Mode.Bard].MuteSound);
                ImGui.Checkbox("Disable Penumbra###BardDisablePenumbra", ref Plugin.Configuration[Mode.Bard].DisablePenumbra);

                ImGui.NewLine();
                ImGui.Text("Visible character limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###BardObjectLimit", ref Plugin.Configuration[Mode.Bard].ObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.NewLine();
                ImGui.Text("FPS limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###BardFps", ref Plugin.Configuration[Mode.Bard].Fps, "None\0Full refresh rate\01/2 of refresh rate\01/4 of refresh rate\0\0");
                ImGui.Checkbox("Reduce FPS while window is inactive###BardFpsDownInactive", ref Plugin.Configuration[Mode.Bard].FpsDownInactive);
                ImGui.Checkbox("Reduce FPS while AFK###BardFpsDownAfk", ref Plugin.Configuration[Mode.Bard].FpsDownAFK);

                ImGui.NewLine();
                ImGui.Checkbox("Change graphics settings###BardChangeGraphicsMode", ref Plugin.Configuration[Mode.Bard].ChangeGraphicsMode);
                if (ImGui.Button("Save Current"))
                {
                    Plugin.Configuration.SaveGraphicsSettings(Mode.Bard);
                }
                ImGui.SameLine();
                if (
                ImGui.Button("Reset to [extra low]"))
                {
                    Plugin.Configuration.ResetGraphicsSettings(Mode.Bard);
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Clone Settings"))
            {
                ImGui.TextWrapped("Settings for all clones.");
                ImGui.Spacing();

                ImGui.Checkbox("Mute all sound###CloneMuteSound", ref Plugin.Configuration[Mode.Clone].MuteSound);
                ImGui.Checkbox("Disable Penumbra###CloneDisablePenumbra", ref Plugin.Configuration[Mode.Clone].DisablePenumbra);

                ImGui.NewLine();
                ImGui.Text("Visible character limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###CloneObjectLimit", ref Plugin.Configuration[Mode.Clone].ObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.NewLine();
                ImGui.Text("FPS limit:");
                ImGui.PushItemWidth(300);
                ImGui.Combo("###CloneFps", ref Plugin.Configuration[Mode.Clone].Fps, "None\0Full refresh rate\01/2 of refresh rate\01/4 of refresh rate\0\0");
                ImGui.Checkbox("Reduce FPS while window is inactive###CloneFpsDownInactive", ref Plugin.Configuration[Mode.Clone].FpsDownInactive);
                ImGui.Checkbox("Reduce FPS while AFK###CloneFpsDownAfk", ref Plugin.Configuration[Mode.Clone].FpsDownAFK);

                ImGui.NewLine();
                ImGui.Checkbox("Change graphics settings###CloneChangeGraphicsMode", ref Plugin.Configuration[Mode.Clone].ChangeGraphicsMode);
                if (ImGui.Button("Save Current"))
                {
                    Plugin.Configuration.SaveGraphicsSettings(Mode.Clone);
                }
                ImGui.SameLine();
                if (
                ImGui.Button("Reset to [extra low]"))
                {
                    Plugin.Configuration.ResetGraphicsSettings(Mode.Clone);
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

        foreach (var (worldId, list) in Plugin.Configuration.CloneCharacterList)
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

    private void DrawAddCloneModal()
    {
        if (showAddCloneModal)
        {
            ImGui.OpenPopup("Add a Clone");
        }

        if (!ImGui.BeginPopupModal("Add a Clone", ref showAddCloneModal, ImGuiWindowFlags.AlwaysAutoResize))
        {
            return;
        }

        var dcSheet = Service.Data.Excel.GetSheet<WorldDCGroupType>();
        if (dcSheet == null) return;
        var worldSheet = Service.Data.Excel.GetSheet<World>();
        if (worldSheet == null) return;

        var currentDc = dcSheet.GetRow(selectedAddDataCenter);
        if (currentDc == null)
        {
            selectedAddDataCenter = 0;
            return;
        }

        ImGui.Text("Add a clone character by name and world");

        if (ImGui.BeginCombo("Data Center", selectedAddDataCenter == 0 ? "Not Selected" : currentDc.Name.RawString))
        {
            foreach (var dc in dcSheet.Where(w => w.Region > 0 && w.Name.RawString.Trim().Length > 0))
            {
                if (ImGui.Selectable(dc.Name.RawString, dc.RowId == selectedAddDataCenter))
                {
                    selectedAddDataCenter = dc.RowId;
                    // TODO: Save this for next time (in config file)
                    //Save();
                }
            }
            ImGui.EndCombo();
        }

        if (currentDc.Region != 0)
        {

            var currentWorld = worldSheet.GetRow(selectedAddWorld);
            if (currentWorld == null || (selectedAddWorld != 0 && currentWorld.DataCenter.Row != selectedAddDataCenter))
            {
                selectedAddWorld = 0;
                return;
            }

            if (ImGui.BeginCombo("World", selectedAddWorld == 0 ? "Not Selected" : currentWorld.Name.RawString))
            {
                foreach (var w in worldSheet.Where(w => w.DataCenter.Row == selectedAddDataCenter && w.IsPublic))
                {
                    if (ImGui.Selectable(w.Name.RawString, w.RowId == selectedAddWorld))
                    {
                        selectedAddWorld = w.RowId;
                        // TODO: Save this as a default too?
                        //Save();
                    }
                }
                ImGui.EndCombo();
            }

            if (currentWorld.IsPublic)
            {
                ImGui.InputText("Character Name", ref selectedAddCharacterName, 45);
            }
        }


        if (ImGui.Button("Add"))
        {
            ImGui.CloseCurrentPopup();
            showAddCloneModal = false;

            if (AddClone(selectedAddWorld, selectedAddCharacterName))
            {
                // Save datacenter and world selection for next time.
                Plugin.Configuration.LastUsedDataCenter = selectedAddDataCenter;
                Plugin.Configuration.LastUsedWorld = selectedAddWorld;
                Plugin.Configuration.Save();
            }
            else
            {
                Service.Log.Error("Character does not exist.");
            }
        }
        ImGui.SameLine();

        if (ImGui.Button("Cancel"))
        {
            showAddCloneModal = false;
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
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
            Plugin.Configuration.RemoveClone(worldId, clone);
            selectedClone = string.Empty;
            selectedWorld = 0;

            // Save config changes
            Plugin.Configuration.Save();
        }
    }

    /// <summary>
    /// Add a clone to the list from a game object
    /// </summary>
    /// <param name="pc">player character object</param>
    private bool AddClone(GameObject? clone)
    {
        if (clone != null && clone is PlayerCharacter pc)
        {
            if (Plugin.Configuration.AddClone(pc))
            {
                selectedClone = pc.Name.TextValue;
                selectedWorld = pc.HomeWorld.Id;

                // Save config changes
                Plugin.Configuration.Save();

                return true;
            }
        }

        return false;
    }

    private bool AddClone(uint world, string clone)
    {
        if (Plugin.Configuration.AddClone(world, clone))
        {
            selectedWorld = world;
            selectedClone = clone;
            selectedAddCharacterName = string.Empty;

            // Save config changes
            Plugin.Configuration.Save();

            return true;
        }
        return false;
    }
}
