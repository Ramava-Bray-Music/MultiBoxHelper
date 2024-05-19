using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace MultiBoxHelper.Windows;

public class ConfigWindow : Window, IDisposable
{
    //private Configuration configuration;
    private readonly Plugin plugin;
    private Vector2 iconButtonSize = new(16);

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Multibox Helper Configuration###MultiBoxHelperConfiguration")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        //configuration = plugin.Configuration;
        this.plugin = plugin;
    }

    public void Dispose() { }


    public override void PreDraw()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 400),
            MaximumSize = ImGuiHelpers.MainViewport.Size * 1 / ImGuiHelpers.GlobalScale * 0.95f
        };
    }

    public override void Draw()
    {
        // Character list
        ImGui.BeginGroup();
        {
            if (ImGui.BeginChild("clone_list", ImGuiHelpers.ScaledVector2(240, 0) - iconButtonSize with { X = 0 }, true))
            {
                DrawCharacterList();
            }
            ImGui.EndChild();

            // Need to add some buttons below
            // see: https://github.com/Caraxi/Honorific/blob/master/ConfigWindow.cs


            if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus))
            {
                // Add a character
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Add a clone character");
            }
            ImGui.SameLine();

            if (ImGuiComponents.IconButton(FontAwesomeIcon.PencilAlt))
            {
                // Edit character name/world
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Edit character name or world");
            }
            ImGui.SameLine();

            if (ImGuiComponents.IconButton(FontAwesomeIcon.Trash))
            {
                // Delete character from list
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

                ImGui.Text("\nVisible character limit:");
                ImGui.Combo("###DefaultObjectLimit", ref plugin.Configuration.DefaultObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");


                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Bard Mode"))
            {
                ImGui.Text("Settings for when Bard Mode is toggled.");
                ImGui.Checkbox("Mute all sound###BardMuteSound", ref plugin.Configuration.BardMuteSound);
                ImGui.Checkbox("Disable Penumbra###BardDisablePenumbra", ref plugin.Configuration.BardDisablePenumbra);
                ImGui.Checkbox("Use low graphics mode###efaultLowGraphicsMode", ref plugin.Configuration.BardLowGraphicsMode);

                ImGui.Text("\nVisible character limit:");
                ImGui.Combo("###BardObjectLimit", ref plugin.Configuration.BardObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Clone Settings"))
            {
                ImGui.Text("Settings for all clones.");
                ImGui.Checkbox("Mute all sound###CloneMuteSound", ref plugin.Configuration.CloneMuteSound);
                ImGui.Checkbox("Disable Penumbra###CloneDisablePenumbra", ref plugin.Configuration.CloneDisablePenumbra);
                ImGui.Checkbox("Use low graphics mode###efaultLowGraphicsMode", ref plugin.Configuration.CloneLowGraphicsMode);

                ImGui.Text("\nVisible character limit:");
                ImGui.Combo("###CloneObjectLimit", ref plugin.Configuration.CloneObjectLimit, "Maximum\0High\0Normal\0Low\0Minimum\0\0");

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
        ImGui.EndGroup();
        /*

        // can't ref a property, so use a local copy
        var configValue = configuration.SomePropertyToBeSavedAndWithADefault;
        if (ImGui.Checkbox("Random Config Bool", ref configValue))
        {
            configuration.SomePropertyToBeSavedAndWithADefault = configValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            configuration.Save();
        }

        var movable = configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            configuration.IsConfigWindowMovable = movable;
            configuration.Save();
        }
        */
    }

    private void DrawCharacterList()
    {
        ImGui.Text("Clone List");
    }
}
