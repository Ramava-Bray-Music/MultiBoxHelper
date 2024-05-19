using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;

namespace MultiBoxHelper;

// disable nullable warnings as all of these are injected. if they're missing, we have serious issues.
#pragma warning disable CS8618
public class Service
{
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IDutyState DutyState { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] public static IGameConfig GameConfig { get; private set; } = null!;
    //[PluginService] public static ICondition Condition { get; private set; } = null!;
    //[PluginService] public static IFramework Framework { get; private set; } = null!;
    //[PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
    //[PluginService] public static IGameGui GameGui { get; private set; } = null!;
}
