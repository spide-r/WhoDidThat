/*
 * Attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Game/ActionEffect.cs
 */

using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace WhoDidThat.Toolbox;

#pragma warning disable 8618
// ReSharper disable UnusedAutoPropertyAccessor.Local
internal class Service {

    [PluginService]
    [RequiredVersion("1.0")]
    internal static IDataManager DataManager { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static IChatGui ChatGui { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static IObjectTable ObjectTable { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static IPartyList PartyList { get; private set; }

    [PluginService]
    [RequiredVersion("1.0")]
    internal static IClientState ClientState { get; private set; }
    
    [PluginService]
    [RequiredVersion("1.0")]
    internal static DalamudPluginInterface DalamudPluginInterface { get; private set; }
    
    [PluginService]
    [RequiredVersion("1.0")]
    internal static IFramework Framework { get; private set; }
    
    [PluginService]
    [RequiredVersion("1.0")]
    internal static ICondition Condition { get; private set; }

    
    [PluginService]
    [RequiredVersion("1.0")]
    internal static IGameInteropProvider GameInteropProvider { get; private set; }
    
    [PluginService]
    [RequiredVersion("1.0")]
    internal static IPluginLog PluginLog { get; private set; }

    internal static void Initialize(DalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Service>();
    }
}
#pragma warning restore 8618
